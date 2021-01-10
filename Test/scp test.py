# Early test created on Fri, Jan 1, 2021 to ensure that files can be shared over the network.
import os
from tkinter import filedialog
from tkinter import *

folderDialog = Tk()
folderDialog.withdraw() # Hide useless extra window.

testDir = filedialog.askdirectory(title="Select directory to send")
if (testDir == ""):
    quit()

os.system("scp -r " + testDir + " pi@192.168.1.52:./")