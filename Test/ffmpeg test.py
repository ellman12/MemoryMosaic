# Test for generating a sorted dir for an mp4 file using ffmpeg.
import subprocess
import dateutil.parser
import datetime

def readVideoMetadata(filename):
    results = subprocess.run(["ffprobe", "-v", "0", "-print_format", "compact=print_section=0:nk=1", "-show_entries", "format_tags=creation_time", filename], stdout = subprocess.PIPE)
    output = results.stdout.decode().strip()
    video_datetime = dateutil.parser.isoparse(output)
    return video_datetime

FILENAME = "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Photos/VID_20201126_121225.mp4"

test = readVideoMetadata(FILENAME)

newDir = "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/" + str(test.year) + '/' + str(test.month) + '/' + "VID_20201126_121225.mp4"

print(test)
print(newDir)