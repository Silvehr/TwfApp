-- APP_VERSION=1
-- DATABASE_VERSION=1

-- <TABLES>
CREATE TABLE IF NOT EXISTS "JokeObjects" (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	Author VARCHAR(255) NOT NULL COLLATE NOCASE,
	Name VARCHAR(255) NOT NULL COLLATE NOCASE UNIQUE,
	Description TEXT,
	Type INTEGER NOT NULL,
	CreationDate DATETIME NOT NULL,
	Editable BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS "DatabaseVersion" (
    DataName VARCHAR(255) PRIMARY KEY,
	MajorVersion INTEGER NOT NULL,
    MinorVersion INTEGER NOT NULL
);
-- </TABLES>

-- <MIGRATIONS>

-- </MIGRATIONS>

-- <INITIAL_DATA>
INSERT INTO "JokeObjects" (Author, Name, Description, Type, CreationDate, Editable) VALUES
	('Gall Anonim', 'Sosna Limba', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
	('Gall Anonim', 'Krowa', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
	('Gall Anonim', 'Śliwka w Czekoladzie', 'Podstawowa i niezmienna forma żartu kierownika', 1,'2023-06-29 00:00:00', 0),
	('Gall Anonim', 'Wektor', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
	('Gall Anonim', 'Baza', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0) 
	ON CONFLICT(Name) DO NOTHING;
-- </INITIAL_DATA>