#######################################################################
# Purpose: Used to sort picture files.
#######################################################################
# Comments: Created on Monday, January 4, 2021, for sorting pictures and
# videos for the Photos Storage Server (PSS).
# Also adds them to the SQL database.
#######################################################################

import stripAndFormat

import os
import shutil
from tkinter import *
from tkinter import filedialog
import PIL.Image
from datetime import datetime
import logging

open('PSS File Sorter.log', 'w').close() # Temp

def printNewLogLine(): # Helps improve readability.
    logFile = open("PSS File Sorter.log", 'a')
    logFile.write("\n")
    logFile.close()

logging.basicConfig(filename='PSS File Sorter.log', encoding='utf-8', level=logging.INFO)
logging.info(str(datetime.now()) + " PSS File Sorter Starting\n")

# Constants
EXIF_DATETIME_TAG = 36867
MONTH_NAMES = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

# Get the directories to use.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.
unsortedDir = filedialog.askdirectory(title="Select directory to sort")
if (unsortedDir == ""):
    logging.info(str(datetime.now()) + " Cancelling unsortedDir selection...")
    exit()

destinationDir = filedialog.askdirectory(title="Where should the sorted files go?")
if (destinationDir == ""):
    logging.info(str(datetime.now()) + " Cancelling destinationDir selection...")
    exit()

for subDir, _, files in os.walk(unsortedDir):
    for file in files:
        originalFilePath = subDir + '/' + file
        newFilePath = destinationDir + '/' + file

        if (".jpg" in file) or (".jpeg" in file) or (".png" in file):
            logging.info(str(datetime.now()) + f' Examining picture "{file}"')
            currentImage = PIL.Image.open(originalFilePath)

            # if (".png" in file): # Only needed for .png files. Not sure if this actually works or not. https://stackoverflow.com/a/62456315
                # currentImage.load()

            exifData = currentImage._getexif() # Returns an array of data about this image. We only want the EXIF_DATETIME_TAG.

            if (exifData == None): # If no "date taken" data embedded in the file, look at the filename for a timestamp. If can't read or can't find, default to the time right now.
                logging.error(str(datetime.now()) + f' No date taken data detected for "{file}."')
                fileTakenDate = stripAndFormat.stripAndFormatTimestamp(file)
                if (fileTakenDate == -1):
                    logging.info(str(datetime.now()) + f' Unable to find date taken for "{file}". Using current time.')
                    fileTakenDate = datetime.now()

            elif (exifData != None) and (EXIF_DATETIME_TAG in exifData): # If the file does have "date taken" data embedded in the file.
                logging.info(str(datetime.now()) + f' "{file}" date taken data found.')
                fileTakenDate = datetime.strptime(exifData[EXIF_DATETIME_TAG], "%Y:%m:%d %H:%M:%S")
                logging.info(str(datetime.now()) + f' "{file}" was taken on {fileTakenDate}')

            else:
                logging.error(str(datetime.now()) + f' "{file}" caused an error or something idk.')
                fileTakenDate = stripAndFormat.stripAndFormatTimestamp(file)
                if (fileTakenDate == -1):
                    logging.info(str(datetime.now()) + f' Unable to find date taken for "{file}". Using current time.')
                    fileTakenDate = datetime.now()

            printNewLogLine()

        elif (".mp4" in file) or (".mkv" in file):
            logging.info(str(datetime.now()) + f' Examining video "{file}"')

            # Get date using that command, then generate its new dir

            printNewLogLine()