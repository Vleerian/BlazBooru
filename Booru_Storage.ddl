CREATE TABLE "Image_Data" (
	"ID"	INTEGER,
	"Image"	TEXT,
	"MD5"	TEXT,
	"Original_Name"	TEXT,
	"Site_Origin"	TEXT,
	"Score"	INTEGER,
	PRIMARY KEY("ID" AUTOINCREMENT),
	UNIQUE("MD5"),
	UNIQUE("Image")
);|||CREATE TABLE "Tags_Data" (
	"Tag"	TEXT UNIQUE,
	"Type"	TEXT DEFAULT 'General',
	"Refs"	INTEGER DEFAULT 1,
	PRIMARY KEY("Tag")
);|||CREATE TABLE "Tag_Refs" (
	"Tag"	TEXT,
	"Image"	INTEGER,
	FOREIGN KEY("Image") REFERENCES "Image_Data"("ID"),
	FOREIGN KEY("Tag") REFERENCES "Tags_Data"("Tag"),
	UNIQUE("Tag","Image")
);|||CREATE TRIGGER del_dead_tags
AFTER UPDATE ON Tags_Data
FOR EACH ROW WHEN NEW.Refs = 0
BEGIN
	DELETE FROM Tags_Data WHERE Tag == NEW.Tag;
END;|||CREATE TRIGGER add_tag_data
BEFORE INSERT ON Tag_Refs
BEGIN
	INSERT OR IGNORE INTO Tags_Data (Tag, Refs) VALUES (NEW.Tag, 0);
END;|||CREATE TRIGGER update_tag_refs_insert
AFTER INSERT ON Tag_Refs
BEGIN
	UPDATE Tags_Data SET Refs = Refs + 1 WHERE Tag = NEW.Tag;
END;|||CREATE TRIGGER update_tag_refs_delete
AFTER DELETE ON Tag_Refs
BEGIN
	UPDATE Tags_Data SET Refs = Refs - 1 WHERE Tag = OLD.Tag;
END;|||CREATE VIEW Tags_View AS
SELECT Tag_Refs.Image, Tag_Refs.Tag, Tags_Data.Type, Tags_Data.Refs FROM Tag_Refs
INNER JOIN Tags_Data ON Tag_Refs.Tag = Tags_Data.Tag