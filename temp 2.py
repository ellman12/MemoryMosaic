from PIL import Image
from PIL.Tags import Tags
import re


FILENAME = "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Capture 2020-12-26 21_00_56.png"

image = Image.open(FILENAME)
info = image._getexif()
time = 'blah'
try:
    for tag, value in info.items():
         key = TAGS.get(tag, tag)
         if isinstance(key, str):
            if key == 'DateTime':
                time = str(value)
            elif 'DateTime' in key:
                time = str(value)
            else:
                time = str(file)
