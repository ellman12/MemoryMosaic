# MemoryMosaic

![Home Alt](https://github.com/ellman12/MemoryMosaic/assets/56001219/08b5e331-5008-4b00-a785-18f90491ee41)
![VideoPlayer](https://github.com/ellman12/MemoryMosaic/assets/56001219/bddf0c92-f68b-4769-8d79-d523be66f049)
![Memories](https://github.com/ellman12/MemoryMosaic/assets/56001219/7627ddaf-610c-4f7f-8d8c-1d4a22b1ba07)
![Import](https://github.com/ellman12/MemoryMosaic/assets/56001219/050aa952-cd9c-4835-be60-9163e288701e)

# The Ultimate Way to Store and Organize a Lifetime of Photos and Videos
MemoryMosaic (MM) is a free and open source replacement for Google Photos, powered by Blazor Server and PostgreSQL, intended to be used by a single person either on your computer, or on a server on the local network.

# Installation
1. Download `MemoryMosaic.zip` from the latest [Release](https://github.com/ellman12/MemoryMosaic/releases) and unzip it to wherever you want it.
2. In the `bin` folder, move `exiftool.exe`, `ffmpeg.exe`, and `ffprobe.exe` to `C:/Windows`. Delete the `bin` folder.
3. Download and install PostgreSQL 15.
4. In the `Initialization` folder, run `Initialization.exe`. This will setup MemoryMosaic for you.
5. In the `MemoryMosaic` folder, run `MemoryMosaic.exe`. This is the actual app. Feel free to make a shortcut to the `.exe`.

To learn more about MemoryMosaic, how it works, and how to use it, check out the [Wiki](https://github.com/ellman12/MemoryMosaic/wiki).

# Contributing
Feel free to open a PR or message me if you have ideas for MemoryMosaic or you spot a bug 🐛. 

## Building MemoryMosaic
1. Clone this repo.
2. Install the .NET 7 SDK.
3. If this is a debug build
	1. Enable the `#DEBUG` compiler flag.
	2. Set `$Debug` in `Constants.scss` to `true`.
4. If this is a release build
	1. Disable the `#DEBUG` compiler flag.
	2. Set `$Debug` in `Constants.scss` to `false`.
	3. Update `Program.Version` to the new version of MM.
5. Run the `build.sh` script.
