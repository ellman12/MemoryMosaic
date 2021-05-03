# Pretty simple function that returns when a video file was taken using FFmpeg.
import subprocess
import dateutil.parser

def readVideoMetadata(dir):
    fileInfo = subprocess.run(["ffprobe", "-v", "0", "-print_format", "compact=print_section=0:nk=1", "-show_entries", "format_tags=creation_time", dir], stdout = subprocess.PIPE)
    output = fileInfo.stdout.decode().strip()
    videoDatetime = dateutil.parser.isoparse(output)
    return videoDatetime