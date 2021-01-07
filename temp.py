from PIL import Image
from PIL import Tags
import re

def main_png(file: str) -> None:
    """
    Used for PNG images with EXIF data
    :param file: Path to a photo to be renamed
    :return: None
    """
    img = Image.open(file)
    print('Converting {}'.format(file))
    creation_date = re.compile(r'<photoshop:DateCreated>(\d)+-(\d)+-(\d)+T(\d)+:(\d)+:(\d)+</photoshop:DateCreated>')
    print(creation_date)
    before_time = 'blah'
    for tag, value in img.info.items():
        print(tag, value)
        match = creation_date.search(str(value))
        if match:
            before_time = match.group()

    no_tags = before_time.replace('<photoshop:DateCreated>', '').replace('</photoshop:DateCreated>', '')
    name = no_tags.replace(':', '.').replace('T', ' ')
    name += '.png'
    img.save('C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Photos/{}'.format(name))
    print('Saved {}'.format(name))


main_png("C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Capture 2020-12-26 21_00_56.png")