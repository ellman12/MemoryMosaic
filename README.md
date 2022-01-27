# Photos Storage Server

## About
Improved clone(ish) of Google Photos developed for my personal use on the local network.<br>
PSS is a Blazor Server App that utilizes a PostgreSQL database. The database is for storing things like:
* What photos/videos are in the library
* What albums there are and what items are in them
* What items are in the trash

Since starting this project I've started to realize just how slow, bloated, and occasionally glitchy Google Photos is. Lots of slow, annoying animations and ads throughout the app, as well as some other buggy behavior sometimes. One goal I had/have with this is to make it as fast, simple, and streamlined as possible: no annoying animations that waste time, no weird behavior, etc.

For right now PSS will probably only work on a Windows machine but adding Linux support probably wouldn't be hard.

## Features
* Store your photos and videos on a local server as opposed to giving Google—or another cloud provider—access to all your media.
* Group items together in albums
  * Good use cases include making albums for vacation pictures, family/pet pictures, or other things.
* Put items in folders to keep them separate from your main library.
  * Like Photos' archiving but can have as many as you want as opposed to Photos' single 'Archive' option.
  * Best used for things like documents or school notes you don't want mixed in your main library.
* Easily backup your photos and videos, as well as your library (the database) to wherever you want. Also makes it easy to move your library to another location or another machine.

## How to Use and Setup
On the server, install .NET 6 and PostgreSQL (with all the default settings and whatnot). Then, cd into PSS-Init (Photos-Storage-Server/PSS/PSS-Init), and run<br>
```dotnet run``` <br>
and follow the steps/prompts. That should initialize the server automatically.

## Contributing
Feel free to open a PR or shoot me a message if you have ideas for PSS or you spot a bug 🐛. 