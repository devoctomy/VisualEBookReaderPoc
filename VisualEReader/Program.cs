using HtmlAgilityPack;
using System.Text;
using VersOne.Epub;

var id = Guid.NewGuid();

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
var image = await api.ImageGenerations.CreateImageAsync(response.ToString(), OpenAI_API.Models.Model.DALLE3);
var url = image.Data[0].Url;
var httpClient = new HttpClient();
var imageBytes = await httpClient.GetByteArrayAsync(url);
await File.WriteAllBytesAsync($"output/{id}/image.png", imageBytes);

Console.WriteLine("Generating audio");
var audioStream = await api.TextToSpeech.GetSpeechAsStreamAsync(truncatedParagraph);
var output = File.OpenWrite($"output/{id}/audio.mp3");
await audioStream.CopyToAsync(output);

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