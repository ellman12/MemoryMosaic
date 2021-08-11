INSERT INTO albums (Name) values ("test_album");

select * from albums;
select * from album_entries;
select * from media;
select * from album_entries_trash;
select * from media_trash;

UPDATE albums SET album_cover = "test lol" WHERE id = 2;
UPDATE albums SET album_cover = null WHERE id = 2;

UPDATE media SET path = "new path lol" WHERE path = "item1";
UPDATE album_entries SET path = "new path lol" WHERE path = "item1";

-- Works
SELECT a.path, a.album_id, a.date_added_to_album, m.date_taken FROM media AS m
INNER JOIN album_entries AS a
ON m.path = a.path
WHERE album_id=2;

-- Also works
SELECT a.path, m.date_taken, a.date_added_to_album FROM media AS m
INNER JOIN album_entries AS a
ON m.path = a.path
WHERE album_id=1;

-- Select only items from media that have 0 (false)
select * from media where Separate = "0";

-- Removing a single path from an album
DELETE FROM album_entries WHERE album_id = 4 AND path = "item6";

SELECT path FROM media WHERE uuid="0d986559-fa29-11eb-93b1-ec79492ad08a";