CREATE TABLE Precincts (
	[PrecinctCode] smallint PRIMARY KEY NOT NULL,
	[PrecinctName] nvarchar(31),
	[LEG] tinyint NOT NULL,
	[CC] tinyint NOT NULL,
	[CG] tinyint NOT NULL,
	INDEX IX_Ids NONCLUSTERED (Id)
)

CREATE TABLE Results (
	[Id] int IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Year] smallint,
	[Election] nvarchar(15),
	[PrecinctName] nvarchar(31),
	[PrecinctCode] smallint,
	[Race] nvarchar(127),
	[LEG] tinyint,
	[CC] tinyint,
	[CG] tinyint,
	[Party] nvarchar(31),
	[CounterType] nvarchar(31),
	[Count] smallint
)

CREATE TABLE Measures (
	[Id] int PRIMARY KEY NOT NULL,
	[Year] smallint NOT NULL,
	[Name] nvarchar(50) NOT NULL,
	[Description] nvarchar(200),
	[BillNumber] smallint
)

CREATE TABLE DistrictResults (
	[MeasureId] int NOT NULL,
	[District] smallint NOT NULL,
	[Support] int NOT NULL,
	[Oppose] int NOT NULL,
	PRIMARY KEY(MeasureId, District)
)
