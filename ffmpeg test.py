import subprocess
import dateutil.parser
# import ffmpeg

FILENAME = "C:\\Users\\Elliott\\Documents\\GitHub\\Photos-Storage-Server\\test.mp4"

results = subprocess.run(
        ["ffprobe", "-v", "0", "-print_format", "compact=print_section=0:nk=1", "-show_entries", "format_tags=creation_time", FILENAME],
    stdout = subprocess.PIPE
)
output = results.stdout.decode().strip()
video_datetime = dateutil.parser.isoparse(output)

print(video_datetime)