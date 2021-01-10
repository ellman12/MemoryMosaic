CREATE TABLE photos (
	Directory VARCHAR(200) NOT NULL UNIQUE PRIMARY KEY,
    Date_Added DATETIME NOT NULL,
    Date_Taken DATETIME,
    Albums_List CHAR(75)
);