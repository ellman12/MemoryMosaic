# Created: Saturday, January 16, 2021
# Purpose/comments: I have ~130 video clips from my Nintendo Switch that have their date taken metadata wrong.
# E.g., instead of March 20, 2018, it might say something like Jan 1, 1970.
# Not sure why this happens. Also not sure if this is a system-wide issue or if it's for specific games.
# Luckily, their filenames contain timestamps that I can strip and use to assign them the correct timestamp data.

import os
import shutil
from tkinter import *
from tkinter import filedialog
import PIL.Image
from datetime import datetime
import logging
import subprocess

from src.Functions.readVideoMetadata import *

# ffmpeg -i "C:/Users/Elliott/Documents/temp switch screenshots 1-10-2020/SUPER MARIO ODYSSEY/2018022016403700_s.mp4" -c copy -map 0 -metadata creation_time="2018-02-20T16:40:37" output.mp4

# Get the directories to use.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.
brokenDir = filedialog.askdirectory(title="Select directory to fix")
if (brokenDir == ""):
    exit()

fixedDir = filedialog.askdirectory(title="Select destination for fixed files")
if (fixedDir == ""):
    exit()

for subDir, _, files in os.walk(brokenDir):
    for file in files:
        originalFileLocation = subDir + '/' + file
        newFileLocation = fixedDir + '/' + file

        if (".mp4" in file):
            # originalTakenDate = readVideoMetadata(originalFileLocation)
            # print(f"Fixing date for {file} at {originalFileLocation} that was 'taken' on {originalTakenDate}")
            print(f"Fixing date for {file}")
            correctDate = file[0:14] # Cut off unnecessary chars. 2018021419102700_s.jpg
            correctDate = datetime.strptime(correctDate,'%Y%m%d%H%M%S')
            subprocess.run(["ffmpeg", "-i", f"{originalFileLocation}", "-c", "copy", "-map", "0", "-metadata", f"creation_time={correctDate}", f"{newFileLocation}"], stdout = subprocess.PIPE)
            # correctDate = correctDate.stdout.decode().strip()
            # correctDate = dateutil.parser.isoparse(correctDate)

            # print(f"{originalTakenDate} has been changed to {correctDate}\n")