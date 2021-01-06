# this example is sloppy but it's a definite proof of concept that this can work.
import datetime

filename = "Screenshot_20201028-141626_Messages.jpg"
stripped_timestamp = filename[11:19] + filename[21:26] # Strip the chars we don't want.
print(stripped_timestamp)

# https://stackoverflow.com/questions/2380013/converting-date-time-in-yyyymmdd-hhmmss-format-to-python-datetime
dt=datetime.datetime.strptime(stripped_timestamp,'%Y%m%d%H%M%S')
print(dt)