# VisualEBookReaderPoc

A proof of concept for an e-book reader that uses text to voice as well as image generation. The general idea is to take e-books to the next level, even beyond that of spoken audio books (such as kindle) without having to pay for narration fees. My idea is that you would 'play' an e-book on your TV, and be given pleasant (AI generated) audio narration along with intermittent AI generated imagery that is contextually correct to the content currently being narrated.

It is still a way off being perfect but I think this shows that the concept could work.  

Getting started.

1. Create open AI Api key and store it in USER level environment variable 'OpenAiApiKey'
2. Assure you are not on Free tier for OpenAi Api, if you are, this may not work, at time of writing you had to pay a minimum of $5 to be elevated to first non-free tier
3. Download an epub file and place it in data/ebooks/ (please see README.md in that folder for an example)
4. Run the application.

The following should be generated,

* 'output' directory
* directory for this specific session (unique guid) within 'output' directory.

and within session output directory,

* fulltext.txt = full, untruncated text of 'Chapter One'
* image.png = an image to represent shorttext.txt
* imageprompt.txt = a prompt used to generate image.png
* shorttext.txt = truncated version of fulltext.txt in order to keep below TTS 4096 max character limit
* audio.mp3 = audio narration of shorttext.txt

## Known Issues

If you use a different ebook other than the one suggested, you may need to tweak the xpath in order to find the correct chapter text, i.e.

```
var chapterOneParagraphs = html.DocumentNode.SelectNodes("/html/body/section[@class='chapter' and @title='Chapter One']/p");
```

The AI won't be able to hold state between images so you won't get consistency in what's being generated, each image will be as if it's come from the mind of someone entirely different.

Ideally this would all be done locally using an API based LLM, TTS and image generation (such as Stable Diffusion) as it is not feasible to run this on OpenAi services unless you have more money than sense.