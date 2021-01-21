#######################################################################
# Purpose: Used to sort picture files.
#######################################################################
# Comments: Created on Monday, January 4, 2021, for sorting pictures and
# videos for the Photos Storage Server (PSS).
# Also adds them to the SQL database.
#######################################################################

# TODO:
# Next thing to do should be allowing the user to enter a date/time if script can't figure it out on its own...

import os
import shutil
from tkinter import *
from tkinter import filedialog
import PIL.Image
from datetime import datetime
import logging
# import subprocess
import mysql.connector

# Functions, etc. that I've made for this main script.
from Functions.LogOutput import *
from Functions.readVideoMetadata import *
from Functions.stripAndFormatTimestamp import *

# Called after the fileTakenDate is determined. Determines where to copy the current file to.
def sortAndAddToDB():
    # Format it like this and then copy it there: C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Photos2020/11 November/VID_20201126_121225.mp4
    newFileFolderPath = destinationDir + '/' + str(fileTakenDate.year) + '/' + str(fileTakenDate.month) + " " + MONTH_NAMES[fileTakenDate.month - 1] + '/' # The folder containing the new item.
    newFileLocation = newFileFolderPath + file # The item's new full path.

    # If this dir doesn't exist, make it and then copy the item there.
    if not os.path.exists(newFileFolderPath):
        os.makedirs(newFileFolderPath)

    # This has been known to throw errors from time to time.
    # try:
    shutil.copyfile(originalFileLocation, newFileLocation)

    logInfo(f'"{file}" copied from "{originalFileLocation}" to "{newFileLocation}"')

    photosDBCursor.execute("INSERT INTO photos VALUES (%s, %s, %s, %s)", (newFileLocation, datetime.now(), fileTakenDate, MAIN_ALBUM_ID))
    photosDB.commit() # Apply changes.
    logInfo(f'"{file}" added to the database"')


open('src/PSS File Sorter.log', 'w').close() # TODO: Temp reset of this file on startup.

# Connect to the database.
# https://stackoverflow.com/a/53561512
photosDB = mysql.connector.connect(host="localhost", user="root", password="Ph0t0s_Server", database="photos_storage_server")
photosDBCursor = photosDB.cursor()

# Set up logging stuff.
logging.basicConfig(filename='src/PSS File Sorter.log', encoding='utf-8', level=logging.INFO)
logInfo("PSS File Sorter Starting\n")

# Constants
EXIF_DATETIME_TAG = 36867

# In this mode ALL items are displayed. Thus, every item added will for sure have this ID.
# TODO: For now I just want a basic photos viewer (not worrying about albums), so just using this.
MAIN_ALBUM_ID = 0
MONTH_NAMES = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

# Get the directories to use.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.
unsortedDir = filedialog.askdirectory(title="Select directory to sort")
if (unsortedDir == ""):
    logInfo("Cancelling unsortedDir selection...")
    exit()

destinationDir = filedialog.askdirectory(title="Where should the sorted files go?") # This is the main dir. The sorted month folders go inside this dir.
if (destinationDir == ""):
    logInfo("Cancelling destinationDir selection...")
    exit()

for subDir, _, files in os.walk(unsortedDir):
    for file in files:
        originalFileLocation = subDir + '/' + file

        if (".jpg" in file) or (".jpeg" in file) or (".png" in file):
            logInfo(f'Examining picture "{file}"')
            currentImage = PIL.Image.open(originalFileLocation)
            exifData = currentImage._getexif() # Returns an array of data about this image. We only want the EXIF_DATETIME_TAG.

            if (exifData == None): # If no "date taken" data embedded in the file, look at the filename for a timestamp. If can't read or can't find, default to the time right now.
                logWarning(f'No date taken data detected for "{file}."')
                fileTakenDate = stripAndFormatTimestamp(file)

            elif (exifData != None) and (EXIF_DATETIME_TAG in exifData): # If the file does have "date taken" data embedded in the file.
                logInfo(f'"{file}" date taken data found.')
                fileTakenDate = datetime.strptime(exifData[EXIF_DATETIME_TAG], "%Y:%m:%d %H:%M:%S")
                logInfo(f'"{file}" was taken on {fileTakenDate}')

            else:
                logError(f'Something happened while analyzing "{file}"') # I'm not sure why some files wouldn't fall under either of those 2 categories, so I guess this else clause is needed???
                fileTakenDate = stripAndFormatTimestamp(file)

            sortAndAddToDB()

        elif (".mp4" in file):
            logInfo(f'Examining video "{file}"')
            fileTakenDate = readVideoMetadata(originalFileLocation)

            if (fileTakenDate == None): # If can't find/doesn't have this data.
                logWarning(f'No taken date found in file "{file}". Searching in filename.')
                fileTakenDate = stripAndFormatTimestamp(file)
            elif (fileTakenDate != None):
                logInfo(f'"{file}" was taken on {fileTakenDate}.')
            else:
                logError(f'Something happened while analyzing "{file}"')
                fileTakenDate = stripAndFormatTimestamp(file)

            sortAndAddToDB()

        elif (".mkv" in file): # Unfortunately, I don't think it's possible to use readVideoMetadata() for MKV files. So, we can just use the filenames instead, since they really only come from OBS and they follow a common format.
            logInfo(f'Examining video "{file}"')
            fileTakenDate = stripAndFormatTimestamp(file)
            sortAndAddToDB()

        else:
            logWarning(f"Ignoring {file}...")

        printNewLogLine("src/PSS File Sorter.log")