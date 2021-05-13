CREATE TABLE `album_entries` (
  `path` varchar(600) NOT NULL,
  `album_id` int unsigned NOT NULL,
  `date_added_to_album` datetime NOT NULL
);

CREATE TABLE `albums` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(600) NOT NULL,
  PRIMARY KEY (`id`)
);

CREATE TABLE `media` (
  `path` varchar(600) NOT NULL,
  `date_added` datetime NOT NULL,
  `date_taken` datetime NOT NULL,
  `Separate` BOOLEAN NOT NULL,
  PRIMARY KEY (`path`)
);

CREATE TABLE `trash` (
  `path` varchar(600) NOT NULL,
  `album_id` int unsigned NOT NULL,
  `date_added_to_album` datetime NOT NULL
);