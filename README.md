# Photos Storage Server

## About
### **The Ultimate Way to Store, Sort, and Organize a Lifetime of Photos and Videos. Never Dig Through Folders Ever Again!**
Photos Storage Server (PSS or pss) is a free and open source locally-hosted replacement for Google Photos, intended to be used by a single person. The app itself is intended to run on a locally-hosted Windows (Linux isn't tested) server on the local network.<br><br>
PSS is a Blazor Server App that is powered by a PostgreSQL database. This database manages your library of items, what albums you have, what is in your Trash, everything. The files themselves are stored on the local file system, while the database stores relative paths (also known as short paths or shortPaths) to where they are.<br>

## Features Not Found in Google Photos
* PSS has Folders, which act like the Archive feature in Google Photos, but without being limited to just one.
  * They can easily transform into a normal Album and back.
  * Can be easily deleted and allow the items to return back to your Library.
  * Best used for things like documents or school notes you don't want mixed in your main library.
* PSS has the ability to create snapshots of your library, allowing you to easily return your library to that point in time.
* Items don't have to have a Date Taken value, like they do with Google Photos. This is useful for when you don't know when an item was taken, or you don't want it to have a Date Taken value, like a meme e.g.

### Features That Might Be Added Someday
* Multiple user accounts on one server, and sharing of items between them.
* Run PSS on a cloud provider instead of local only.

## Installation
1. Download the [ExifTool](https://exiftool.org/) Windows Executable. This is required for modifying Date Taken metadata.
   1. Change the name of the .exe from `exiftool(-k).exe` to `exiftool.exe`
   2. Add exiftool.exe to your system's `PATH` or move it to a folder already in the `PATH`, like `C:/Windows`.
2. Install [ffmpeg](https://www.ffmpeg.org/) and add it to your `PATH`.
3. Download the source code of PSS from the latest release, unzip it, and place it wherever.
4. On the server, install .NET 6 and PostgreSQL (with all the default settings and whatnot). Then, cd into PSS-Init (Photos-Storage-Server/PSS/PSS-Init), and run ```dotnet run``` and follow the steps and prompts. That should initialize the server automatically.<br>

## Remarks
PSS is designed to look and feel a lot like Google Photos, only a lot better. It fixes stuff I found annoying with Google Photos, like only being able to add selected items to one album at a time, lots of slow and pointless animations, etc. PSS is ideal for anyone who, like myself, was looking for a free replacement for Google Photos. To learn more about PSS and how it works and how to use it, check out the [Wiki](https://github.com/ellman12/Photos-Storage-Server/wiki).

## Contributing
Feel free to open a PR or shoot me a message if you have ideas for PSS or you spot a bug 🐛. 