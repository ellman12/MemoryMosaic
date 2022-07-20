-- Create some dummy test data for testing.
INSERT INTO media (path, date_taken, uuid)
VALUES ('2022/10/filename1.jpg', now(), '56ff78b0-ef4d-11ec-8ea0-0242ac120002'),
       ('2021/11/filename2.png', now(), '56ff7b1c-ef4d-11ec-8ea0-0242ac120002'),
       ('2021/11/filename4.png', now(), '56ff7fcc-ef4d-11ec-8ea0-0242ac120002'),
       ('2021/11/filename3.png', now(), '56ff8116-ef4d-11ec-8ea0-0242ac120002'),
       ('2020/1/video1.mp4', now(), '56ff824c-ef4d-11ec-8ea0-0242ac120002');

INSERT INTO albums (name, last_updated, folder)
VALUES ('album1', now(), false),
       ('album3', now(), false),
       ('album2', now(), false),
       ('folder1', now(), true);

INSERT INTO collection_entries (uuid, collection_id)
VALUES ('56ff78b0-ef4d-11ec-8ea0-0242ac120002', 1),
('56ff7b1c-ef4d-11ec-8ea0-0242ac120002', 1),
('56ff7fcc-ef4d-11ec-8ea0-0242ac120002', 1),
('56ff8116-ef4d-11ec-8ea0-0242ac120002', 1),
('56ff824c-ef4d-11ec-8ea0-0242ac120002', 1),
('56ff7fcc-ef4d-11ec-8ea0-0242ac120002', 2),
('56ff8116-ef4d-11ec-8ea0-0242ac120002', 2),
('56ff824c-ef4d-11ec-8ea0-0242ac120002', 2);