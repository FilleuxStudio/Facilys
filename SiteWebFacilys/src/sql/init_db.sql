START TRANSACTION;

-- Suppression des tables existantes (optionnel, à utiliser avec précaution)
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS 
    `AssociationSettingReferences`,
    `HistoryParts`,
    `QuotesItems`,
    `MaintenanceAlerts`,
    `Invoices`,
    `Quotes`,
    `Emails`,
    `OtherVehicles`,
    `Phones`,
    `ProfessionalClient`,
    `Vehicles`,
    `InterestingReferences`,
    `Inventorys`,
    `ReferencesIgnored`,
    `SyncMetaDatas`,
    `Users`,
    `VersionDatabases`,
    `Clients`,
    `CompanySettings`,
    `EditionSettings`;
SET FOREIGN_KEY_CHECKS = 1;

-- Création des tables principales
CREATE TABLE IF NOT EXISTS `Clients` (
    `Id` CHAR(36) NOT NULL,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Address` TEXT NOT NULL,
    `City` VARCHAR(255) NOT NULL,
    `PostalCode` VARCHAR(20) NOT NULL,
    `Type` TINYINT NOT NULL,
    `AdditionalInformation` TEXT NOT NULL,
    `DateCreated` DATETIME NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `CompanySettings` (
    `Id` CHAR(36) NOT NULL,
    `NameCompany` VARCHAR(255) NOT NULL,
    `Logo` LONGTEXT NOT NULL,
    `TVA` VARCHAR(50) NOT NULL,
    `Siret` VARCHAR(50) NOT NULL,
    `RIB` VARCHAR(100) NOT NULL,
    `HeadOfficeAddress` TEXT NOT NULL,
    `BillingAddress` TEXT,
    `LegalStatus` VARCHAR(100) NOT NULL,
    `RMNumber` VARCHAR(50) NOT NULL,
    `RCS` VARCHAR(50),
    `RegisteredCapital` DECIMAL(15,2),
    `CodeNAF` VARCHAR(20),
    `ManagerName` VARCHAR(255),
    `Phone` VARCHAR(50) NOT NULL,
    `Email` VARCHAR(255),
    `WebSite` VARCHAR(255),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `EditionSettings` (
    `Id` CHAR(36) NOT NULL,
    `StartNumberInvoice` VARCHAR(50) NOT NULL,
    `PathSaveFile` TEXT NOT NULL,
    `PathSaveInvociePrepare` TEXT NOT NULL,
    `Picture` TEXT,
    `TypeDesign` TINYINT NOT NULL,
    `SentenceInformationBottom` TEXT,
    `SentenceInformationTop` TEXT,
    `SentenceBottom` TEXT,
    `RepairOrderSentenceTop` TEXT,
    `RepairOrderSentenceBottom` TEXT,
    `TVA` DECIMAL(5,2),
    `PreloadedLine` SMALLINT NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `InterestingReferences` (
    `Id` CHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL UNIQUE,
    `Price` DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Inventorys` (
    `Id` CHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Details` TEXT,
    `Picture` TEXT,
    `Type` VARCHAR(100),
    `Price` DECIMAL(10,2),
    `Quantity` INT,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IDX_Reference` (`Reference`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `ReferencesIgnored` (
    `Id` CHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL UNIQUE,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `SyncMetaDatas` (
    `Id` CHAR(36) NOT NULL,
    `TableName` VARCHAR(255) NOT NULL,
    `LastSyncTime` DATETIME NOT NULL,
    `SyncStatus` VARCHAR(100) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Email` VARCHAR(255) NOT NULL UNIQUE,
    `Login` VARCHAR(255) NOT NULL UNIQUE,
    `Picture` TEXT,
    `Password` VARCHAR(255) NOT NULL,
    `Team` VARCHAR(255) NOT NULL,
    `Role` TINYINT NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `VersionDatabases` (
    `Id` CHAR(36) NOT NULL,
    `Version` VARCHAR(50) NOT NULL,
    `DateVersion` DATETIME NOT NULL,
    `PathBackup` TEXT NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tables avec relations
CREATE TABLE IF NOT EXISTS `Emails` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Emails_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `OtherVehicles` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `SerialNumber` VARCHAR(255) NOT NULL,
    `Type` VARCHAR(100) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `StatusDataView` TINYINT NOT NULL,
    `AdditionalInformation` TEXT,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OtherVehicles_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Phones` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `Phone` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Phones_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `ProfessionalClient` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `NameCompany` VARCHAR(255) NOT NULL,
    `Siret` VARCHAR(50) NOT NULL,
    `TVANumber` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProfessionalClient_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Vehicles` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `Immatriculation` VARCHAR(20) NOT NULL,
    `Type` VARCHAR(100) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `VIN` VARCHAR(17) NOT NULL,
    `AdditionalInformation` TEXT NOT NULL,
    `CirculationDate` DATE NOT NULL,
    `KM` INT NOT NULL,
    `StatusDataView` TINYINT NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Vehicles_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tables avec relations complexes
CREATE TABLE IF NOT EXISTS `AssociationSettingReferences` (
    `Id` CHAR(36) NOT NULL,
    `IdInterestingReferences` CHAR(36) NOT NULL,
    `EditionSettingId` CHAR(36),
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AssociationSettingReferences_Interesting` FOREIGN KEY (`IdInterestingReferences`) 
        REFERENCES `InterestingReferences`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AssociationSettingReferences_Edition` FOREIGN KEY (`EditionSettingId`) 
        REFERENCES `EditionSettings`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Invoices` (
    `Id` CHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL,
    `OrderNumber` VARCHAR(50) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `Payment` TINYINT NOT NULL,
    `TotalAmount` DECIMAL(10,2) NOT NULL,
    `Observations` TEXT,
    `RepairType` VARCHAR(100),
    `Status` TINYINT NOT NULL,
    `PartReturnedCustomer` BOOLEAN NOT NULL,
    `CustomerSuppliedPart` BOOLEAN NOT NULL,
    `UserId` CHAR(36),
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Invoices_Users` FOREIGN KEY (`UserId`) 
        REFERENCES `Users`(`Id`),
    CONSTRAINT `FK_Invoices_Vehicles` FOREIGN KEY (`IdVehicle`) 
        REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_Invoices_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) 
        REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `MaintenanceAlerts` (
    `Id` CHAR(36) NOT NULL,
    `TypeMaintenace` VARCHAR(100) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `DateMake` DATETIME NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MaintenanceAlerts_Vehicles` FOREIGN KEY (`IdVehicle`) 
        REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_MaintenanceAlerts_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) 
        REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Quotes` (
    `Id` CHAR(36) NOT NULL,
    `IdClient` CHAR(36) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `QuoteNumber` VARCHAR(50) NOT NULL,
    `TotalAmount` DECIMAL(10,2),
    `Observations` TEXT,
    `Status` TINYINT NOT NULL,
    `UserId` CHAR(36),
    `DateAccepted` DATETIME,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Quotes_Clients` FOREIGN KEY (`IdClient`) 
        REFERENCES `Clients`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Quotes_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) 
        REFERENCES `OtherVehicles`(`Id`),
    CONSTRAINT `FK_Quotes_Users` FOREIGN KEY (`UserId`) 
        REFERENCES `Users`(`Id`),
    CONSTRAINT `FK_Quotes_Vehicles` FOREIGN KEY (`IdVehicle`) 
        REFERENCES `Vehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `HistoryParts` (
    `Id` CHAR(36) NOT NULL,
    `IdInvoice` CHAR(36) NOT NULL,
    `IdVehicle` CHAR(36),
    `IdOtherVehicle` CHAR(36),
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Discount` DECIMAL(5,2) NOT NULL,
    `Price` DECIMAL(10,2) NOT NULL,
    `Quantity` DECIMAL(10,2) NOT NULL,
    `KMMounted` INT,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_HistoryParts_Invoices` FOREIGN KEY (`IdInvoice`) 
        REFERENCES `Invoices`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_HistoryParts_Vehicles` FOREIGN KEY (`IdVehicle`) 
        REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_HistoryParts_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) 
        REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `QuotesItems` (
    `Id` CHAR(36) NOT NULL,
    `IdQuote` CHAR(36) NOT NULL,
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Price` DECIMAL(10,2) NOT NULL,
    `Quantity` INT NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_QuotesItems_Quotes` FOREIGN KEY (`IdQuote`) 
        REFERENCES `Quotes`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Création des index manquants
CREATE INDEX `IDX_Emails_Client` ON `Emails` (`IdClient`);
CREATE INDEX `IDX_OtherVehicles_Client` ON `OtherVehicles` (`IdClient`);
CREATE INDEX `IDX_Phones_Client` ON `Phones` (`IdClient`);
CREATE INDEX `IDX_ProfessionalClient_Client` ON `ProfessionalClient` (`IdClient`);
CREATE INDEX `IDX_Vehicles_Client` ON `Vehicles` (`IdClient`);
CREATE INDEX `IDX_AssociationRefs_Interesting` ON `AssociationSettingReferences` (`IdInterestingReferences`);
CREATE INDEX `IDX_AssociationRefs_Edition` ON `AssociationSettingReferences` (`EditionSettingId`);
CREATE INDEX `IDX_Invoices_User` ON `Invoices` (`UserId`);
CREATE INDEX `IDX_Invoices_Vehicle` ON `Invoices` (`IdVehicle`);
CREATE INDEX `IDX_Invoices_OtherVehicle` ON `Invoices` (`IdOtherVehicle`);
CREATE INDEX `IDX_Maintenance_Vehicle` ON `MaintenanceAlerts` (`IdVehicle`);
CREATE INDEX `IDX_Maintenance_OtherVehicle` ON `MaintenanceAlerts` (`IdOtherVehicle`);
CREATE INDEX `IDX_Quotes_Client` ON `Quotes` (`IdClient`);
CREATE INDEX `IDX_Quotes_User` ON `Quotes` (`UserId`);
CREATE INDEX `IDX_Quotes_Vehicle` ON `Quotes` (`IdVehicle`);
CREATE INDEX `IDX_Quotes_OtherVehicle` ON `Quotes` (`IdOtherVehicle`);
CREATE INDEX `IDX_HistoryParts_Invoice` ON `HistoryParts` (`IdInvoice`);
CREATE INDEX `IDX_HistoryParts_Vehicle` ON `HistoryParts` (`IdVehicle`);
CREATE INDEX `IDX_HistoryParts_OtherVehicle` ON `HistoryParts` (`IdOtherVehicle`);
CREATE INDEX `IDX_QuotesItems_Quote` ON `QuotesItems` (`IdQuote`);

-- Optimisations finales
SET SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO';
SET AUTOCOMMIT = 1;

COMMIT;