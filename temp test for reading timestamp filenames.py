# this example is sloppy but it's a definite proof of concept that this can work.
from datetime import datetime

# Android screenshots.
# filename = "Screenshot_20201028-141626_Messages.jpg"
# timestamp = filename[11:19] + filename[21:26] # Strip the chars we don't want.
# print(timestamp)

# # https://stackoverflow.com/questions/2380013/converting-date-time-in-yyyymmdd-hhmmss-format-to-python-datetime
# datetime = datetime.datetime.strptime(timestamp,'%Y%m%d%H%M%S')
# print(datetime)
# timestamp = "hi"
# OBS filenames.
# if (filename[4] == '-') and (filename[13] == '-') and (filename[16] == '-'):
    # print("hi")

filename = "2021-01-05 20-28-49.mkv"
print(filename)
timestamp = filename.replace('-', '').replace(' ', '')
timestamp = timestamp[:-4] # Remove file extension.
timestamp = datetime.strptime(timestamp,'%Y%m%d%H%M%S')
print(timestamp)
