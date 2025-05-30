START TRANSACTION;

CREATE TABLE IF NOT EXISTS `Clients` (
    `Id` VARCHAR(36) NOT NULL,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Address` TEXT NOT NULL,
    `City` VARCHAR(255) NOT NULL,
    `PostalCode` VARCHAR(20) NOT NULL,
    `Type` INT NOT NULL,
    `AdditionalInformation` TEXT NOT NULL,
    `DateCreated` DATETIME NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `CompanySettings` (
    `Id` VARCHAR(36) NOT NULL,
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
    `Id` VARCHAR(36) NOT NULL,
    `StartNumberInvoice` VARCHAR(50) NOT NULL,
    `PathSaveFile` TEXT NOT NULL,
    `PathSaveInvociePrepare` TEXT NOT NULL,
    `Picture` TEXT,
    `TypeDesign` INT NOT NULL,
    `SentenceInformationBottom` TEXT,
    `SentenceInformationTop` TEXT,
    `SentenceBottom` TEXT,
    `RepairOrderSentenceTop` TEXT,
    `RepairOrderSentenceBottom` TEXT,
    `TVA` DECIMAL(5,2),
    `PreloadedLine` INT NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `InterestingReferences` (
    `Id` VARCHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL,
    `Price` DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Inventorys` (
    `Id` VARCHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Details` TEXT,
    `Picture` TEXT,
    `Type` VARCHAR(100),
    `Price` DECIMAL(10,2),
    `Quantity` INT,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `ReferencesIgnored` (
    `Id` VARCHAR(36) NOT NULL,
    `Reference` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `SyncMetaDatas` (
    `Id` VARCHAR(36) NOT NULL,
    `TableName` VARCHAR(255) NOT NULL,
    `LastSyncTime` DATETIME NOT NULL,
    `SyncStatus` VARCHAR(100) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` VARCHAR(36) NOT NULL,
    `Lname` VARCHAR(255) NOT NULL,
    `Fname` VARCHAR(255) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    `Login` VARCHAR(255) NOT NULL,
    `Picture` TEXT,
    `Password` VARCHAR(255) NOT NULL,
    `Team` VARCHAR(255) NOT NULL,
    `Role` INT NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `VersionDatabases` (
    `Id` VARCHAR(36) NOT NULL,
    `Version` VARCHAR(50) NOT NULL,
    `DateVersion` DATETIME NOT NULL,
    `PathBackup` TEXT NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tables avec clés étrangères
CREATE TABLE IF NOT EXISTS `Emails` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_Emails_IdClient` (`IdClient`),
    CONSTRAINT `FK_Emails_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `OtherVehicles` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `SerialNumber` VARCHAR(255) NOT NULL,
    `Type` VARCHAR(100) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `StatusDataView` INT NOT NULL,
    `AdditionalInformation` TEXT,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_OtherVehicles_IdClient` (`IdClient`),
    CONSTRAINT `FK_OtherVehicles_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Phones` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `Phone` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_Phones_IdClient` (`IdClient`),
    CONSTRAINT `FK_Phones_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `ProfessionalClient` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `NameCompany` VARCHAR(255) NOT NULL,
    `Siret` VARCHAR(50) NOT NULL,
    `TVANumber` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_ProfessionalClient_IdClient` (`IdClient`),
    CONSTRAINT `FK_ProfessionalClient_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Vehicles` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `Immatriculation` VARCHAR(20) NOT NULL,
    `Type` VARCHAR(100) NOT NULL,
    `Mark` VARCHAR(255) NOT NULL,
    `Model` VARCHAR(255) NOT NULL,
    `VIN` VARCHAR(17) NOT NULL,
    `AdditionalInformation` TEXT NOT NULL,
    `CirculationDate` DATE NOT NULL,
    `KM` INT NOT NULL,
    `StatusDataView` INT NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_Vehicles_IdClient` (`IdClient`),
    CONSTRAINT `FK_Vehicles_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `AssociationSettingReferences` (
    `Id` VARCHAR(36) NOT NULL,
    `IdInterestingReferences` VARCHAR(36) NOT NULL,
    `EditionSettingId` VARCHAR(36),
    PRIMARY KEY (`Id`),
    INDEX `IX_AssociationSettingReferences_I` (`IdInterestingReferences`),
    INDEX `IX_AssociationSettingReferences_E` (`EditionSettingId`),
    CONSTRAINT `FK_AssociationSettingReferences_I` FOREIGN KEY (`IdInterestingReferences`) REFERENCES `InterestingReferences`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AssociationSettingReferences_E` FOREIGN KEY (`EditionSettingId`) REFERENCES `EditionSettings`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Invoices` (
    `Id` VARCHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL,
    `OrderNumber` VARCHAR(50) NOT NULL,
    `IdVehicle` VARCHAR(36),
    `IdOtherVehicle` VARCHAR(36),
    `Payment` INT NOT NULL,
    `TotalAmount` DECIMAL(10,2) NOT NULL,
    `Observations` TEXT,
    `RepairType` VARCHAR(100),
    `Status` INT NOT NULL,
    `PartReturnedCustomer` INT NOT NULL,
    `CustomerSuppliedPart` INT NOT NULL,
    `UserId` VARCHAR(36),
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_Invoices_UserId` (`UserId`),
    INDEX `IX_Invoices_IdVehicle` (`IdVehicle`),
    INDEX `IX_Invoices_IdOtherVehicle` (`IdOtherVehicle`),
    CONSTRAINT `FK_Invoices_Users` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    CONSTRAINT `FK_Invoices_Vehicles` FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_Invoices_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `MaintenanceAlerts` (
    `Id` VARCHAR(36) NOT NULL,
    `TypeMaintenace` VARCHAR(100) NOT NULL,
    `IdVehicle` VARCHAR(36),
    `IdOtherVehicle` VARCHAR(36),
    `DateMake` DATETIME NOT NULL,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_MaintenanceAlerts_IdVehicle` (`IdVehicle`),
    INDEX `IX_MaintenanceAlerts_IdOtherVehicle` (`IdOtherVehicle`),
    CONSTRAINT `FK_MaintenanceAlerts_Vehicles` FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_MaintenanceAlerts_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Quotes` (
    `Id` VARCHAR(36) NOT NULL,
    `IdClient` VARCHAR(36) NOT NULL,
    `IdVehicle` VARCHAR(36),
    `IdOtherVehicle` VARCHAR(36),
    `QuoteNumber` VARCHAR(50) NOT NULL,
    `TotalAmount` DECIMAL(10,2),
    `Observations` TEXT,
    `Status` INT NOT NULL,
    `UserId` VARCHAR(36),
    `DateAccepted` DATETIME,
    `DateAdded` DATETIME NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_Quotes_IdClient` (`IdClient`),
    INDEX `IX_Quotes_IdOtherVehicle` (`IdOtherVehicle`),
    INDEX `IX_Quotes_UserId` (`UserId`),
    INDEX `IX_Quotes_IdVehicle` (`IdVehicle`),
    CONSTRAINT `FK_Quotes_Clients` FOREIGN KEY (`IdClient`) REFERENCES `Clients`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Quotes_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`),
    CONSTRAINT `FK_Quotes_Users` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    CONSTRAINT `FK_Quotes_Vehicles` FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `HistoryParts` (
    `Id` VARCHAR(36) NOT NULL,
    `IdInvoice` VARCHAR(36) NOT NULL,
    `IdVehicle` VARCHAR(36),
    `IdOtherVehicle` VARCHAR(36),
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Discount` DECIMAL(5,2) NOT NULL,
    `Price` DECIMAL(10,2) NOT NULL,
    `Quantity` DECIMAL(8,2) NOT NULL,
    `KMMounted` INT,
    PRIMARY KEY (`Id`),
    INDEX `IX_HistoryParts_IdVehicle` (`IdVehicle`),
    INDEX `IX_HistoryParts_IdInvoice` (`IdInvoice`),
    INDEX `IX_HistoryParts_IdOtherVehicle` (`IdOtherVehicle`),
    CONSTRAINT `FK_HistoryParts_Vehicles` FOREIGN KEY (`IdVehicle`) REFERENCES `Vehicles`(`Id`),
    CONSTRAINT `FK_HistoryParts_Invoices` FOREIGN KEY (`IdInvoice`) REFERENCES `Invoices`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_HistoryParts_OtherVehicles` FOREIGN KEY (`IdOtherVehicle`) REFERENCES `OtherVehicles`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `QuotesItems` (
    `Id` VARCHAR(36) NOT NULL,
    `IdQuote` VARCHAR(36) NOT NULL,
    `PartNumber` VARCHAR(255) NOT NULL,
    `PartName` VARCHAR(255) NOT NULL,
    `PartBrand` VARCHAR(255),
    `Description` TEXT,
    `Price` DECIMAL(10,2) NOT NULL,
    `Quantity` INT NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_QuotesItems_IdQuote` (`IdQuote`),
    CONSTRAINT `FK_QuotesItems_Quotes` FOREIGN KEY (`IdQuote`) REFERENCES `Quotes`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Optimisations pour MariaDB
SET FOREIGN_KEY_CHECKS = 1;
SET SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO';

COMMIT;
