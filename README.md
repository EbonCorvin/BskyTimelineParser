# BskyTimelineParser

A Bluesky timeline parser library for **.NET Core**. It provides classes and methods that fetch your Liked Post, Following and Discover timeline, and convert the timeline to an array of .NET object. It also provides methods to verify the token, refresh your token, and get a new token with the identifies and password the user supplied. 

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Features

- Fetch your Liked Post, Following and Discover timeline.
- Convert the posts to readable and friendly .NET objects.
- Provide authentication function that help you to get and refresh your Bluesky token (No  more F12 on your browser!).

## Installation

You can just refer the libary project in your .NET Core application. You can also refer to the built DLL file in your project.

## Usage

**LoginHandler** provides static methods that can help you to do the authentication with Bluesky. When you're using the methods, please make sure to catch the exceptions so that you can know what exactly happened that prevent you from getting a valid token.

**BskyParser** is a class that fetch your timeline and convert your timeline to .NET object array. You need to create a new BskyParser object with a valid **BskyToken** and **TimelineTypes**.

Since Bluesky allows the API call to fetch the Liked Post of something other than the token owner, you can assign another **UserHande** after you created the object. Please note that it only works when you try to fetch the Liked Post.

Then you can started to call **Next()** to get the next page of timeline from Bleusky API. The library will handle the cursor for you, so you can keep calling Next() to move forward.
		
The **Post** object is a really plain object, it contains most of the information that you need to know for a post. 

**Media** represents the embedded media, like image, video and external link, its Url field will lead you to the file you need.

Please note that it is a bit tricky if you need to fetch a video from Bluesky's server, as the Url is actually a link to a playlist file, which contain links to another playlist of the different resolutions of the same video. 

Even if you fetched the playlist of the resolution you want for the video, a video is spliited into multiple small video clips, so you may need to download every of them and combine them in order to get the whole video you want. 

### Example

Please refer to the [TestConsoleApplication](samples/TestConsoleApplication) project to see how to call the library to get what you need.

More workable and more useful samples will be added to the samples folder in the future!

## Contributing

Um I don't know, who want to contriubute to this project?

## License

This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE.txt) file for details.

## Contact
Contact me via Telegram or Discord! My username is EbonCorvin.
Project Link: [https://github.com/EbonCorvin/BskyTimelineParser]
