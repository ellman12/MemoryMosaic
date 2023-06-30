# Photos Storage Server

![image](https://user-images.githubusercontent.com/56001219/219803397-1193defe-a160-41a4-a654-2a168621fc36.png)
![image](https://user-images.githubusercontent.com/56001219/219803427-f8d839a4-5f92-42a2-a6b0-31dda7e21554.png)

## About
### **The Ultimate Way to Store, Sort, and Organize a Lifetime of Photos and Videos!**
Photos Storage Server (PSS or pss) is a free and open source replacement for Google Photos, powered by Blazor Server and PostgreSQL, intended to be used by a single person on a server hosted on the local network. The database manages your library of items, what albums you have, what is in your Trash, everything. The files themselves are stored on the local file system, while the database stores relative paths (also known as short paths or shortPaths) to where they are.<br>

## Features Not Found in Google Photos
* Folders, which act like the Archive feature in Google Photos, but without being limited to just one.
* Create snapshots of your library, allowing you to easily return your library to that point in time, or export to another server.
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
4. On the server, install .NET 7 and PostgreSQL (with all the default settings and whatnot). Then, `cd` into `Photos-Storage-Server/PSS/PSS-Init`, and run `dotnet run` and follow the steps and prompts. That should initialize the server automatically. Once PSS is initialized, you can `cd` into `Photos-Storage-Server/PSS/PSS` and use `dotnet run` to run PSS.<br>

## Getting Started
To learn more about PSS and how it works and how to use it, check out the [Wiki](https://github.com/ellman12/Photos-Storage-Server/wiki).

## Contributing
Feel free to open a PR or shoot me a message if you have ideas for PSS or you spot a bug 🐛. 
