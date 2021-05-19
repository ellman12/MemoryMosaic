INSERT INTO albums (Name) values ("test_album");

select * from albums;
select * from album_entries;
select * from media;

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