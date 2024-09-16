
CREATE TABLE SyncMetaData (
                Id VARCHAR NOT NULL,
                TableName VARCHAR NOT NULL,
                LastSyncTimestamp DATE NOT NULL,
                LastSuccessfulSync DATE NOT NULL,
                SyncStatus VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE VersionBase (
                Id VARCHAR NOT NULL,
                Version INT NOT NULL,
                DateVersion VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE ActionsUser (
                Id VARCHAR NOT NULL,
                TypeAction VARCHAR NOT NULL,
                UserName VARCHAR NOT NULL,
                DateAction DATE NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Quotes (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                QuoteNumber VARCHAR NOT NULL,
                DateAdded VARCHAR NOT NULL,
                DateAccepted VARCHAR NOT NULL,
                Status VARCHAR NOT NULL,
                TotalAmount DOUBLE PRECISION NOT NULL,
                Note VARCHAR,
                PRIMARY KEY (Id)
);


CREATE TABLE QuotesItems (
                Id VARCHAR NOT NULL,
                IdQuote VARCHAR NOT NULL,
                PartNumber VARCHAR NOT NULL,
                PartName VARCHAR NOT NULL,
                PartBrand VARCHAR NOT NULL,
                Description VARCHAR,
                Price DOUBLE PRECISION NOT NULL,
                Quantity INT NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Inventorys (
                Id VARCHAR NOT NULL,
                Reference VARCHAR NOT NULL,
                PartName VARCHAR NOT NULL,
                Mark VARCHAR,
                Quantity INT NOT NULL,
                Price DOUBLE PRECISION,
                Type VARCHAR,
                Details VARCHAR,
                Picture VARCHAR,
                DateCreated DATE NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Clients (
                Id VARCHAR NOT NULL,
                Lname VARCHAR NOT NULL,
                Fname VARCHAR NOT NULL,
                Address VARCHAR NOT NULL,
                City VARCHAR NOT NULL,
                PostalCode VARCHAR NOT NULL,
                TypeClient INT NOT NULL,
                AdditionalInformation VARCHAR,
                DateCreated DATE NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE OtherVehicles (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                Type VARCHAR,
                Mark VARCHAR NOT NULL,
                Model VARCHAR,
                SerialNumber VARCHAR NOT NULL,
                ManufacturingYear VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Vehicles (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                Immatriculation VARCHAR NOT NULL,
                Type VARCHAR NOT NULL,
                Mark VARCHAR NOT NULL,
                Model VARCHAR NOT NULL,
                VIN VARCHAR NOT NULL,
                CirculationData VARCHAR NOT NULL,
                Km INT NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Invoices (
                Id VARCHAR NOT NULL,
                InvoiceNumber VARCHAR NOT NULL,
                OrderNumber VARCHAR NOT NULL,
                VehicleId VARCHAR,
                OtherVehicleId VARCHAR,
                PaymentMathod VARCHAR NOT NULL,
                TotalAmount DOUBLE PRECISION NOT NULL,
                Observations VARCHAR,
                Status VARCHAR NOT NULL,
                DateAdded DATE NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE HistoryPart (
                Id VARCHAR NOT NULL,
                IdInvoicie VARCHAR NOT NULL,
                IdVehicle VARCHAR,
                IdOtherVehicle VARCHAR,
                PartNumber VARCHAR NOT NULL,
                PartName VARCHAR NOT NULL,
                PartBrand VARCHAR NOT NULL,
                Description VARCHAR,
                Price DOUBLE PRECISION NOT NULL,
                Quantity INT NOT NULL,
                KmMounted INT,
                PRIMARY KEY (Id)
);


CREATE TABLE ClientsPros (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                NameCompany VARCHAR NOT NULL,
                Siret VARCHAR NOT NULL,
                TVANumber VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Phones (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                Phone VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


CREATE TABLE Emails (
                Id VARCHAR NOT NULL,
                IdClient VARCHAR NOT NULL,
                Email VARCHAR NOT NULL,
                PRIMARY KEY (Id)
);


ALTER TABLE QuotesItems ADD CONSTRAINT quotes_quotesitems_fk
FOREIGN KEY (IdQuote)
REFERENCES Quotes (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE Emails ADD CONSTRAINT clients_emails_fk
FOREIGN KEY (IdClient)
REFERENCES Clients (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE Phones ADD CONSTRAINT clients_phones_fk
FOREIGN KEY (IdClient)
REFERENCES Clients (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE ClientsPros ADD CONSTRAINT clients_clientspros_fk
FOREIGN KEY (IdClient)
REFERENCES Clients (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE Vehicles ADD CONSTRAINT clients_vehicles_fk
FOREIGN KEY (IdClient)
REFERENCES Clients (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE OtherVehicles ADD CONSTRAINT clients_othervehicles_fk
FOREIGN KEY (IdClient)
REFERENCES Clients (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE Invoices ADD CONSTRAINT othervehicles_invoices_fk
FOREIGN KEY (OtherVehicleId)
REFERENCES OtherVehicles (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE HistoryPart ADD CONSTRAINT othervehicles_historypart_fk
FOREIGN KEY (IdOtherVehicle)
REFERENCES OtherVehicles (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE Invoices ADD CONSTRAINT vehicles_invoices_fk
FOREIGN KEY (VehicleId)
REFERENCES Vehicles (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE HistoryPart ADD CONSTRAINT vehicles_historypart_fk
FOREIGN KEY (IdVehicle)
REFERENCES Vehicles (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

ALTER TABLE HistoryPart ADD CONSTRAINT invoices_historypart_fk
FOREIGN KEY (IdInvoicie)
REFERENCES Invoices (Id)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
