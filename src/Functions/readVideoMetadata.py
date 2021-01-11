# Pretty simple function that returns when a video file was taken using FFmpeg.
import subprocess
import dateutil.parser
import datetime

def readVideoMetadata(filename):
    results = subprocess.run(["ffprobe", "-v", "0", "-print_format", "compact=print_section=0:nk=1", "-show_entries", "format_tags=creation_time", filename], stdout = subprocess.PIPE)
    output = results.stdout.decode().strip()
    video_datetime = dateutil.parser.isoparse(output)
    return video_datetime