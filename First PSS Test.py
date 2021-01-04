import datetime
import PIL.Image
from tkinter import filedialog
from tkinter import *
import shutil
import os.path
import mysql.connector

# Constants
EXIF_DATETIME_TAG = 36867
MONTH_NAMES = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

# Connect to the database.
photosDB = mysql.connector.connect(host="localhost", user="root", password="Ph0t0s_Server", database="photos_storage_server")
photosDBCursor = photosDB.cursor()

# Used for getting 2 folder choices.
folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.

# Used for timestamps.
currentTime = datetime.now()

# Get the directories to use. If either are blank quit the program.
dirToSort = filedialog.askdirectory(title="Select directory to sort")
if (dirToSort == ""):
    quit()

mainSortedDir = filedialog.askdirectory(title="Where should the sorted files go?")
if (mainSortedDir == ""):
    quit()

# Walk through the unsorted dir, and sort and copy files to the new dir, and also add them to the DB.
for dirPath, _, files in os.walk(dirToSort):
    for file in files:

        destinationPath = mainSortedDir + '/' + "test"
        newFileDir = destinationPath + '/' + file

        timestampStr = currentTime.strftime("%Y-%m-%d %H:%M:%S")
        photosDBCursor.execute("INSERT INTO photos VALUES (%s, %s, %s, %s)", (newFileDir, timestampStr, timestampStr, 69))

        photosDB.commit() # Apply changes.

        # Fetches and prints all the rows in the DB.
        photosDBCursor.execute("SELECT * FROM photos")
        rows = photosDBCursor.fetchall()
        for row in rows:
            print(row)
