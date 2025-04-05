START TRANSACTION;

CREATE TABLE IF NOT EXISTS `Clients` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Address` VARCHAR(255) NOT NULL,
    `City` VARCHAR(255) NOT NULL,
    `PostalCode` VARCHAR(20) NOT NULL,
    `Type` INTEGER NOT NULL,
    `AdditionalInformation` TEXT,
    `DateCreated` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `CompanySettings` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `NameCompany` VARCHAR(255) NOT NULL,
    `Logo` TEXT,
    `TVA` VARCHAR(20),
    `Siret` VARCHAR(20) NOT NULL,
    `RIB` VARCHAR(64) NOT NULL,
    `HeadOfficeAddress` VARCHAR(255) NOT NULL,
    `BillingAddress` VARCHAR(255),
    `LegalStatus` VARCHAR(255) NOT NULL,
    `RMNumber` VARCHAR(255) NOT NULL,
    `RCS` VARCHAR(255),
    `RegisteredCapital` DECIMAL(15, 2),
    `CodeNAF` VARCHAR(50),
    `ManagerName` VARCHAR(255),
    `Phone` VARCHAR(20) NOT NULL,
    `Email` VARCHAR(255),
    `WebSite` VARCHAR(255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `EditionSettings` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `StartNumberInvoice` VARCHAR(255) NOT NULL,
    `PathSaveFile` VARCHAR(255) NOT NULL,
    `PathSaveInvociePrepare` VARCHAR(255) NOT NULL,
    `Picture` TEXT,
    `TypeDesign` INTEGER NOT NULL,
    `SentenceInformationBottom` TEXT,
    `SentenceInformationTop` TEXT,
    `SentenceBottom` TEXT,
    `RepairOrderSentenceTop` TEXT,
    `RepairOrderSentenceBottom` TEXT,
    `TVA` DECIMAL(5, 2),
    `PreloadedLine` INTEGER NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `InterestingReferences` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Reference` VARCHAR(255) NOT NULL,
    `Price` DECIMAL(15, 2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Inventorys` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Reference` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Details` TEXT,
    `Picture` TEXT,
    `Type` VARCHAR(255),
    `Price` DECIMAL(15, 2),
    `Quantity` INTEGER,
    `DateAdded` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `ReferencesIgnored` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Reference` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `SyncMetaDatas` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `TableName` VARCHAR(255) NOT NULL,
    `LastSyncTime` VARCHAR(255) NOT NULL,
    `SyncStatus` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    `Login` VARCHAR(255) NOT NULL,
    `Picture` TEXT,
    `Password` VARCHAR(255) NOT NULL,
    `Team` VARCHAR(255) NOT NULL,
    `Role` INTEGER NOT NULL,
    `DateAdded` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `VersionDatabases` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `Version` VARCHAR(255) NOT NULL,
    `DateVersion` VARCHAR(255) NOT NULL,
    `PathBackup` VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Emails` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `OtherVehicles` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `SerialNumber` VARCHAR(255) NOT NULL,
    `Type` VARCHAR(255) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `StatusDataView` INTEGER NOT NULL,
    `AdditionalInformation` TEXT,
    `DateAdded` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Phones` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `Phone` VARCHAR(20) NOT NULL,
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `ProfessionalClient` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `NameCompany` VARCHAR(255) NOT NULL,
    `Siret` VARCHAR(50) NOT NULL,
    `TVANumber` VARCHAR(50) NOT NULL,
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Vehicles` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `Immatriculation` VARCHAR(64) NOT NULL,
    `Type` VARCHAR(255) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `VIN` VARCHAR(255) NOT NULL,
    `AdditionalInformation` TEXT,
    `CirculationDate` VARCHAR(255) NOT NULL,
    `KM` INTEGER NOT NULL,
    `StatusDataView` INTEGER NOT NULL,
    `DateAdded` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AssociationSettingReferences` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdInterestingReferences` CHAR(36) NOT NULL,
    `EditionSettingId` CHAR(36),
    FOREIGN KEY (`EditionSettingId`) REFERENCES `EditionSettings`(`Id`),
    FOREIGN KEY (`IdInterestingReferences`) REFERENCES `InterestingReferences`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Quotes` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdClient` CHAR(36) NOT NULL,
    `QuoteNumber` VARCHAR(255) NOT NULL,
    `TotalAmount` DECIMAL(15, 2),
    `Status` INTEGER NOT NULL,
    `UserId` CHAR(36),
    `DateAccepted` VARCHAR(255),
    `DateAdded` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Invoices` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `InvoiceNumber` VARCHAR(255) NOT NULL,
    `OrderNumber` VARCHAR(255) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `Payment` INTEGER NOT NULL,
    `TotalAmount` DECIMAL(15, 2) NOT NULL,
    `Observations` TEXT,
    `RepairType` VARCHAR(255),
    `Status` INTEGER NOT NULL,
    `PartReturnedCustomer` INTEGER NOT NULL,
    `CustomerSuppliedPart` INTEGER NOT NULL,
    `UserId` CHAR(36),
    `DateAdded` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`),
    FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MaintenanceAlerts` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `TypeMaintenace` VARCHAR(255) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `DateMake` VARCHAR(255) NOT NULL,
    `DateAdded` VARCHAR(255) NOT NULL,
    FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`),
    FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `QuotesItems` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdQuote` CHAR(36) NOT NULL,
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Price` DECIMAL(15, 2) NOT NULL,
    `Quantity` INTEGER NOT NULL,
    FOREIGN KEY (`IdQuote`) REFERENCES `Quotes`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `HistoryParts` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `IdInvoice` CHAR(36) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Discount` DECIMAL(5, 2) NOT NULL,
    `Price` DECIMAL(15, 2) NOT NULL,
    `Quantity` DECIMAL(10, 2) NOT NULL,
    `KMMounted` INTEGER,
    FOREIGN KEY (`IdInvoice`) REFERENCES `Invoices`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`),
    FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX `IX_AssocSettingRef_EditionSettingId` ON `AssociationSettingReferences` (`EditionSettingId`);
CREATE INDEX `IX_AssocSettingRef_IdInterestingReferences` ON `AssociationSettingReferences` (`IdInterestingReferences`);
CREATE INDEX `IX_Emails_IdClient` ON `Emails` (`IdClient`);
CREATE INDEX `IX_HistoryParts_IdInvoice` ON `HistoryParts` (`IdInvoice`);
CREATE INDEX `IX_HistoryParts_IdOtherVehicle` ON `HistoryParts` (`IdOtherVehicle`);
CREATE INDEX `IX_HistoryParts_IdVehicle` ON `HistoryParts` (`IdVehicle`);
CREATE INDEX `IX_Invoices_IdOtherVehicle` ON `Invoices` (`IdOtherVehicle`);
CREATE INDEX `IX_Invoices_IdVehicle` ON `Invoices` (`IdVehicle`);
CREATE INDEX `IX_Invoices_UserId` ON `Invoices` (`UserId`);
CREATE INDEX `IX_MaintenanceAlerts_IdOtherVehicle` ON `MaintenanceAlerts` (`IdOtherVehicle`);
CREATE INDEX `IX_MaintenanceAlerts_IdVehicle` ON `MaintenanceAlerts` (`IdVehicle`);
CREATE INDEX `IX_OtherVehicles_IdClient` ON `OtherVehicles` (`IdClient`);
CREATE INDEX `IX_Phones_IdClient` ON `Phones` (`IdClient`);
CREATE INDEX `IX_ProfessionalClient_IdClient` ON `ProfessionalClient` (`IdClient`);
CREATE INDEX `IX_Quotes_IdClient` ON `Quotes` (`IdClient`);
CREATE INDEX `IX_Quotes_UserId` ON `Quotes` (`UserId`);
CREATE INDEX `IX_QuotesItems_IdQuote` ON `QuotesItems` (`IdQuote`);
CREATE INDEX `IX_Vehicles_IdClient` ON `Vehicles` (`IdClient`);

COMMIT;