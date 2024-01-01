using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Web;
using VersOne.Epub;

const string coquiTTsServerBaseUrl = "http://localhost:5002";
const string automatic1111BaseUrl = "http://127.0.0.1:7860";
const bool useCoqui = false;
const bool useAutomatic111 = false;

var id = Guid.NewGuid();

Directory.CreateDirectory($"output/ttscache");
Directory.CreateDirectory($"output/{id}");

var openAiApiKey = Environment.GetEnvironmentVariable("OpenAiApiKey", EnvironmentVariableTarget.User);
var api = new OpenAI_API.OpenAIAPI(openAiApiKey);

Console.WriteLine("Loading epub");
var file = "data/ebooks/kafka-trial.epub"; // This should be changed to reflect the actual file you have put into data/ebooks folder. Mark as 'content' + 'copy always'
var ebook = await EpubReader.ReadBookAsync(file);

Console.WriteLine("Extracting paragraph content from 'Chapter One'");
var content = ebook.ReadingOrder[3].Content;
var html = new HtmlDocument();
html.LoadHtml(content);
var chapterOneParagraphs = html.DocumentNode.SelectNodes("/html/body/section[@class='chapter' and @title='Chapter One']/p");
var firstParagraph = chapterOneParagraphs[0];

Console.WriteLine("Truncating");
var normalised = NormaliseText(firstParagraph.InnerText);
var truncatedParagraph = TruncateText(normalised, 4096); // 4096 character limit for TTS
if (string.IsNullOrEmpty(truncatedParagraph))
{
    Console.WriteLine("Failed to truncate text.");
    return;
}

await File.WriteAllTextAsync($"output/{id}/fulltext.txt", firstParagraph.InnerText);
await File.WriteAllTextAsync($"output/{id}/shorttext.txt", truncatedParagraph);

Console.WriteLine("Generating image prompt from extracted content via ChatGpt");
var promptTemplate = await File.ReadAllTextAsync("data/prompts/imagegen.txt");
var prompt = promptTemplate.Replace("{content}", truncatedParagraph);
var chat = api.Chat.CreateConversation();
chat.Model = OpenAI_API.Models.Model.GPT4_Turbo;
chat.AppendUserInput(prompt);
var response = new StringBuilder();

await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
{
    response.Append(res.ToString());
}

Console.WriteLine("Generating image from image prompt");
await File.WriteAllTextAsync($"output/{id}/imageprompt.txt", response.ToString());
if(!useAutomatic111)
{
    var image = await api.ImageGenerations.CreateImageAsync(response.ToString(), OpenAI_API.Models.Model.DALLE3);
    var url = image.Data[0].Url;
    var httpClient = new HttpClient();
    var imageBytes = await httpClient.GetByteArrayAsync(url);
    await File.WriteAllBytesAsync($"output/{id}/image.png", imageBytes);
}    
else
{
    var image = await Automatic1111T2IAsync(response.ToString(), 20, CancellationToken.None);
    if(image != null)
    {
        await File.WriteAllBytesAsync($"output/{id}/image.png", image);
    }
}

var ttsSource = useCoqui ? "Coqui" : "OpenAi";
Console.WriteLine($"Generating audio using {ttsSource}");
string textHash = ComputeSha256Hash(truncatedParagraph);
var cachedAudioFilePath = $"output/ttscache/{textHash}.mp3";
var audioFilePath = $"output/{id}/{textHash}.mp3";
if(!File.Exists(cachedAudioFilePath))
{
    using var audioStream = useCoqui ? await CoquiTTSAsync(truncatedParagraph, CancellationToken.None) : await api.TextToSpeech.GetSpeechAsStreamAsync(truncatedParagraph);
    using var output = File.OpenWrite(cachedAudioFilePath);
    await audioStream.CopyToAsync(output);
}

File.Copy(cachedAudioFilePath, audioFilePath);

Console.WriteLine($"Operation complete, press any key to exit Results available in 'output/{id}'.");

string NormaliseText(string text)
{
    return text.Replace('\n', ' ');
}

string? TruncateText(string text, int maxLength)
{
    int curPos = text.IndexOf("  ");
    while(curPos < maxLength)
    {
        int nextPos = text.IndexOf("  ", curPos + 1);
        if(nextPos < maxLength)
        {
            curPos = nextPos;
        }
        else
        {
            var truncated = text.Substring(0, curPos);
            return truncated;
        }
    }

    return null;
}

async Task<byte[]?> Automatic1111T2IAsync(string prompt, int steps, CancellationToken cancellationToken)
{
    var payload = new
    {
        prompt,
        steps
    };
    var url = $"{automatic1111BaseUrl}/sdapi/v1/txt2img";
    var httpClient = new HttpClient();
    var result = await httpClient.PostAsync(url, JsonContent.Create(payload), cancellationToken);
    var response = await result.Content.ReadAsStringAsync();
    var responseJson = JObject.Parse(response);
    var imageBase64 = responseJson["images"].Value<JArray>()[0].Value<string>();
    return Convert.FromBase64String(imageBase64);
}

async Task<Stream> CoquiTTSAsync(string text, CancellationToken cancellationToken)
{
    var escapedText = HttpUtility.UrlEncode(text);
    var url = $"{coquiTTsServerBaseUrl}/api/tts?text={escapedText}&speaker_id=ED%0A&style_wav";
    var httpClient = new HttpClient();
    return await httpClient.GetStreamAsync(url, cancellationToken);
}

static string ComputeSha256Hash(string rawData)
{
    // Create a SHA256   
    using (SHA256 sha256Hash = SHA256.Create())
    {
        // ComputeHash - returns byte array  
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        // Convert byte array to a string   
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}