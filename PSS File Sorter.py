#######################################################################
# Purpose: Used to sort picture files.
#######################################################################
# Comments: Created on Monday, January 4, 2021, for sorting pictures and
# videos for the Photos Storage Server (PSS).
# Also adds them to the SQL database.
#######################################################################

import os.path
import shutil
from tkinter import *
from tkinter import filedialog
import PIL.Image
import datetime

# Constants
EXIF_DATETIME_TAG = 36867
MONTH_NAMES = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

# Get the directories to use.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.
unsortedDir = filedialog.askdirectory(title="Select directory to sort")
destinationDir = filedialog.askdirectory(title="Where should the sorted files go?")

for subDir, _, files in os.walk(unsortedDir):
    for file in files:

        originalFilePath = subDir + "/" + file
        newFilePath = destinationDir + "/" + file

        if (".jpg" in file) or (".jpeg" in file) or (".png" in file):
            currentImage = PIL.Image.open(originalFilePath)
            exifData = currentImage._getexif()

            if (exifData == None):
                print("temp no exif data")
                # TODO: Check multiple different ways if the file name has a timestamp in it. If so:
                #     write it to the file metadata first
                #     takenDate = file name timestamp
                # else: # If there isn't ANY time taken metadata
                #     takenDate = time right now

            elif (exifData != None) and (EXIF_DATETIME_TAG in exifData):
                fileTakenDate = datetime.datetime.strptime(exifData[EXIF_DATETIME_TAG], "%Y:%m:%d %H:%M:%S")


        # elif (".mp4" in file) or (".mkv" in file):
