import subprocess
import dateutil.parser
import datetime
# import ffmpeg

def readVideoMetadata(filename):
    results = subprocess.run(["ffprobe", "-v", "0", "-print_format", "compact=print_section=0:nk=1", "-show_entries", "format_tags=creation_time", filename], stdout = subprocess.PIPE)
    output = results.stdout.decode().strip()
    video_datetime = dateutil.parser.isoparse(output)
    return video_datetime

FILENAME = "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Photos/29e5607b885099d7ba2d3fbb88e3328a9ba92ecc~2.mp4"

test = readVideoMetadata(FILENAME)
test = datetime.datetime.strptime(str(test), "%Y:%m:%d %H:%M:%S")

print(test)