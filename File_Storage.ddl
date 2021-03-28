CREATE TABLE "Files" (
	"ID"	INTEGER,
	"Filename"	TEXT,
	"Uploader"	TEXT,
	"Date_Uploaded"	TEXT DEFAULT CURRENT_TIMESTAMP,
	"MD5"	TEXT,
	"Content_Type"	TEXT,
	"Data"	BLOB,
	PRIMARY KEY("ID" AUTOINCREMENT)
);|||