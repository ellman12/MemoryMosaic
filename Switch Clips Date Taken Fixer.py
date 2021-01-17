# Created: Saturday, January 16, 2021
# Purpose/comments: I have ~130 video clips from my Nintendo Switch that have their Date Taken metadata wrong.
# E.g., instead of March 20, 2018, it might say something like Jan 1, 1970.
# Not sure why this happens. Also not sure if this is a system-wide issue or if it's for specific games.
# Luckily, their filenames contain timestamps that I can strip and use to assign them the correct timestamp data.

import os
from tkinter import *
from tkinter import filedialog
from datetime import datetime
import subprocess

from src.Functions.readVideoMetadata import *

# Get the directories to use.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.
brokenDir = filedialog.askdirectory(title="Select directory to fix. Make sure to only fix clips for 1 game folder at a time!!")
if (brokenDir == ""):
    exit()

fixedDir = filedialog.askdirectory(title="Select destination for fixed files. Make sure to only fix clips for 1 game folder at a time!!")
if (fixedDir == ""):
    exit()

for subDir, _, files in os.walk(brokenDir):
    for file in files:
        originalFileLocation = subDir + '/' + file
        newFileLocation = fixedDir + '/' + file

        if (".mp4" in file): # Since only (some) MP4 files require fixing, we only want them.
            print(f"Fixing date for {file}")
            correctDate = file[0:14] # Cut off unnecessary chars. Example: 2018021419102700_s.jpg. There seems to be 2 extra zeroes after the second part of the timestamp-filename. Hmm...
            correctDate = datetime.strptime(correctDate,'%Y%m%d%H%M%S')
            subprocess.run(["ffmpeg", "-i", f"{originalFileLocation}", "-c", "copy", "-map", "0", "-metadata", f"creation_time={correctDate}", f"{newFileLocation}"], stdout = subprocess.PIPE)