# Some extremely basic functions that simplify outputting text to a .log file with a timestamp.
# Useful info about logging: https://docs.python.org/3/howto/logging.html

import logging
from datetime import datetime

# Output INFO to the .log file.
def logInfo(text: str):
    logging.info(str(datetime.now()) + " " + text)

# Output WARNING to the .log file.
def logWarning(text: str):
    logging.warning(str(datetime.now()) +  " " + text)

# Output ERROR to the .log file.
def logError(text: str):
    logging.error(str(datetime.now()) +  " " + text)

def printNewLogLine(fileDir): # Helps improve readability by adding a newline in the log file when needed.
    logFile = open(fileDir, 'a')
    logFile.write("\n")
    logFile.close()