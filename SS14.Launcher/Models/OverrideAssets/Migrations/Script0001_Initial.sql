CREATE TABLE OverrideAsset(
    Id INTEGER PRIMARY KEY,
    Name TEXT UNIQUE NOT NULL,
    OverrideName TEXT NOT NULL,
    Data BLOB NOT NULL
);
