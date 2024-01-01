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
* 'ttscache' directory within 'output' directory
* {sha256}.mp3 = Within 'ttscache' directory. audio narration of shorttext.txt, {sha256} is the sha256 for the text passed into the TTS service.
* directory for this specific session (unique guid) within 'output' directory.

and within session output directory,

* fulltext.txt = full, untruncated text of 'Chapter One'
* image.png = an image to represent shorttext.txt
* imageprompt.txt = a prompt used to generate image.png
* shorttext.txt = truncated version of fulltext.txt in order to keep below TTS 4096 max character limit
* {sha256}.mp3 = audio narration of shorttext.txt, {sha256} is the sha256 for the text passed into the TTS service.

## Known Issues

If you use a different ebook other than the one suggested, you may need to tweak the xpath in order to find the correct chapter text, i.e.

```
var chapterOneParagraphs = html.DocumentNode.SelectNodes("/html/body/section[@class='chapter' and @title='Chapter One']/p");
```

The AI won't be able to hold state between images so you won't get consistency in what's being generated, each image will be as if it's come from the mind of someone entirely different.

Ideally this would all be done locally using an API based LLM, TTS and image generation (such as Stable Diffusion) as it is not feasible to run this on OpenAi services unless you have more money than sense.

## Example (OpenAI)

Source epub:
https://www.epubbooks.com/book/560-trial

Truncated Text:
Someone must have been telling lies about Josef K., he knew he had done nothing wrong but, one morning, he was arrested.  Every day at eight in the morning he was brought his breakfast by  Mrs. Grubach's cook – Mrs. Grubach was his landlady – but today she didn't come.  That had never happened before.  K. waited a little while, looked from his pillow at the old woman who lived opposite and who was watching him with an inquisitiveness quite unusual for her, and finally, both hungry and disconcerted, rang the bell.  There was immediately a knock at the door and a man entered.  He had never seen the man in this house before.  He was slim but firmly built, his clothes were black and close–fitting, with many folds and pockets, buckles and buttons and a belt, all of which gave the impression of being very practical but without making it very clear what they were actually for.  "Who are you?" asked K., sitting half upright in his bed.  The man, however, ignored the question as if his arrival simply had to be accepted, and merely replied, "You rang?"  "Anna should have brought me my breakfast," said K.  He tried to work out who the man actually was, first in silence, just through observation and by thinking about it, but the man didn't stay still to be looked at for very long.  Instead he went over to the door, opened it slightly, and said to someone who was clearly standing immediately behind it, "He wants Anna to bring him his breakfast."  There was a little laughter in the neighbouring room, it was not clear from the sound of it whether there were several people laughing.  The strange man could not have learned anything from it that he hadn't known already, but now he said to K., as if making his report "It is not possible." "It would be the first time that's happened," said K., as he jumped out of bed and quickly pulled on his trousers.  "I want to see who that is in the next room, and why it is that Mrs. Grubach has let me be disturbed in this way."  It immediately occurred to him that he needn't have said this out loud, and that he must to some extent have acknowledged their authority by doing so, but that didn't seem important to him at the time.  That, at least, is how the stranger took it, as he said, "Don't you think you'd better stay where you are?"  "I want neither to stay here nor to be spoken to by you until you've introduced yourself."  "I meant it for your own good," said the stranger and opened the door, this time without being asked.  The next room, which K. entered more slowly than he had intended, looked at first glance exactly the same as it had the previous evening.  It was Mrs. Grubach's living room, over–filled with furniture, tablecloths, porcelain and photographs.  Perhaps there was a little more space in there than usual today, but if so it was not immediately obvious, especially as the main difference was the presence of a man sitting by the open window with a book from which he now looked up.  "You should have stayed in your room! Didn't Franz tell you?"  "And what is it you want, then?" said K., looking back and forth between this new acquaintance and the one named Franz, who had remained in the doorway.  Through the open window he noticed the old woman again, who had come close to the window opposite so that she could continue to see everything.  She was showing an inquisitiveness that really made it seem like she was going senile. "I want to see Mrs. Grubach … ," said K., making a movement as if tearing himself away from the two men – even though they were standing well away from him – and wanted to go.  "No," said the man at the window, who threw his book down on a coffee table and stood up.  "You can't go away when you're under arrest."  "That's how it seems," said K.  "And why am I under arrest?" he then asked.  "That's something we're not allowed to tell you.  Go into your room and wait there.  Proceedings are underway and you'll learn about everything all in good time.

Image:
![Man questioning another cloaked in practical yet ambiguous black attire with many folds, pockets, and buttons in a cluttered living room, old woman observing intently from the window across.](https://raw.githubusercontent.com/devoctomy/VisualEBookReaderPoc/main/VisualEReader/data/example/image.png)

Audio:
[Audio Sample](https://raw.githubusercontent.com/devoctomy/VisualEBookReaderPoc/main/VisualEReader/data/example/audio.mp3)

## Coqui TTS

By default this demo will use OpenAI for all AI functions. Coqui can be used in replacement, following the instructions at the following page

https://docs.coqui.ai/en/latest/docker_images.html

Once running, set 'useCoqui' to true in 'Program.cs'. You may need to check the base url too, which defaults to 'http://localhost:5002'.

> Personally I think this sounds awful, but it's free and still helps prove / refine the proof of concept.

## TTS Cache

All TTS is cached in the 'output/ttscache' folder and a copy made in the session folder for the content currently being generated.