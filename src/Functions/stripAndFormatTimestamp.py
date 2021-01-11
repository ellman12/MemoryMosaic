from datetime import datetime
from Functions.LogOutput import *

# This is mainly for "PSS File Sorter.py"
# Pass in a filename that has a timestamp in it like this: 'Screenshot_20201028-141626_Messages.jpg'
# and this function will try and turn that into a standard timestamp like this: '2021-01-05 20:28:49'.
# Returns -1 if it isn't able to convert.
# This function is basically necessary for png files as they don't really work with exif I think?? Also helpful for .jpgs that might not have that embedded data.
def stripAndFormatTimestamp(filename):
    logInfo(f'Attempting to strip "{filename}"')

    if ("Screenshot_" in filename): # If Android screenshot. E.g., 'Screenshot_20201028-141626_Messages.jpg'
        logInfo(f'"{filename}" is an Android screenshot. Stripping...')
        timestamp = filename[11:19] + filename[20:26] # Strip the chars we don't want.
        timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S') # https://stackoverflow.com/questions/2380013/converting-date-time-in-yyyymmdd-hhmmss-format-to-python-datetime
        logInfo(f'"{filename}" was taken on {timestamp}')
        return timestamp

    elif ("IMG_" in filename) or ("VID_" in filename): # Some(?) pictures/videos taken with my phone have 'IMG_'/'VID_' in them. E.g., 'IMG_20201110_171155.jpg'
        logInfo(f'Stripping first 4 chars from "{filename}"...')
        timestamp = filename[4:12] + filename[13:19]
        timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
        logInfo(f'"{filename}" was taken on {timestamp}')
        return timestamp

    elif (filename[4] == '-') and (filename[13] == '-') and (filename[16] == '-') and (".mkv" in filename): # Check if an OBS-generated file. It would have '-' at these 3 indices.
        logInfo(f'"{filename}" is an OBS file. Stripping...')
        timestamp = filename.replace('-', '').replace(' ', '')
        timestamp = timestamp[:-4] # Remove file extension.
        timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
        logInfo(f'"{filename}" was taken on {timestamp}')
        return timestamp

    # elif (filename[8] == '_'): # A filename like this: '20201031_090459.jpg'. I think these come from (Android(?)) phones.
    #     logInfo(f'"{filename}" has a timestamp in the filename. Formatting...')
    #     timestamp = filename[0:8] + filename[9:15]
    #     timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
    #     logInfo(f'"{filename}" was taken on {timestamp}')
    #     return timestamp

    elif ("Capture" in filename) and (".png" in filename): # E.g., 'Capture 2020-05-16 21_04_54.png'
        logInfo(f'"{filename}" appears to be a Terraria capture. Formatting...')
        timestamp = filename[8:27] # Keep what we need and change '_' to ':'
        timestamp = timestamp.replace('_', ':')
        logInfo(f'"{filename}" was taken on {timestamp}')
        return timestamp

    # 1/11/2021 1:50 PM Commented out because idk if those filenames are actually timestamps.
    # elif ("Saved Clip" in filename) and (".png" in filename): # The Screen Clipper script generates these. E.g., 'Saved Clip 20201014103055.png'
    #     logInfo(f'"{filename}" appears to be a Screen Clip. Formatting...')
    #     timestamp = filename[11:25] # Keep what we need.
    #     timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
    #     logInfo(f'"{filename}" was taken on {timestamp}')
    #     return timestamp

    elif ("Screenshot " in filename) and (".png" in filename): # Snip & Sketch generates these filenames. E.g., 'Screenshot 2020-11-17 104051.png'
        logInfo(f'"{filename}" appears to be a Screen Clip. Formatting...')
        timestamp = filename[11:28] # Keep what we need.
        timestamp = timestamp.replace('-', '').replace(' ','')
        timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
        logInfo(f'"{filename}" was taken on {timestamp}')
        return timestamp

    else:
        logError(f" Unknown timestamp-filename format for {filename}. Returning -1")
        return -1