BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Clients" (
	"Id"	TEXT NOT NULL,
	"Lname"	TEXT NOT NULL,
	"Fname"	TEXT NOT NULL,
	"Address"	TEXT NOT NULL,
	"City"	TEXT NOT NULL,
	"PostalCode"	TEXT NOT NULL,
	"Type"	INTEGER NOT NULL,
	"AdditionalInformation"	TEXT NOT NULL,
	"DateCreated"	TEXT NOT NULL,
	CONSTRAINT "PK_Clients" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "CompanySettings" (
	"Id"	TEXT NOT NULL,
	"NameCompany"	TEXT NOT NULL,
	"Logo"	TEXT NOT NULL,
	"TVA"	TEXT NOT NULL,
	"Siret"	TEXT NOT NULL,
	"RIB"	TEXT NOT NULL,
	"HeadOfficeAddress"	TEXT NOT NULL,
	"BillingAddress"	TEXT,
	"LegalStatus"	TEXT NOT NULL,
	"RMNumber"	TEXT NOT NULL,
	"RCS"	TEXT,
	"RegisteredCapital"	REAL,
	"CodeNAF"	TEXT,
	"ManagerName"	TEXT,
	"Phone"	TEXT NOT NULL,
	"Email"	TEXT,
	"WebSite"	TEXT,
	CONSTRAINT "PK_CompanySettings" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "InterestingReferences" (
	"Id"	TEXT NOT NULL,
	"Reference"	TEXT NOT NULL,
	"Price"	REAL NOT NULL,
	CONSTRAINT "PK_InterestingReferences" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Inventorys" (
	"Id"	TEXT NOT NULL,
	"Reference"	TEXT NOT NULL,
	"PartName"	TEXT NOT NULL,
	"Mark"	TEXT NOT NULL,
	"Details"	TEXT,
	"Picture"	TEXT,
	"Type"	TEXT,
	"Price"	REAL,
	"Quantity"	INTEGER,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "PK_Inventorys" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "ReferencesIgnored" (
	"Id"	TEXT NOT NULL,
	"Reference"	TEXT NOT NULL,
	CONSTRAINT "PK_ReferencesIgnored" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "SyncMetaDatas" (
	"Id"	TEXT NOT NULL,
	"TableName"	TEXT NOT NULL,
	"LastSyncTime"	TEXT NOT NULL,
	"SyncStatus"	TEXT NOT NULL,
	CONSTRAINT "PK_SyncMetaDatas" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Users" (
	"Id"	TEXT NOT NULL,
	"Lname"	TEXT NOT NULL,
	"Fname"	TEXT NOT NULL,
	"Email"	TEXT NOT NULL,
	"Login"	TEXT NOT NULL,
	"Picture"	TEXT,
	"Password"	TEXT NOT NULL,
	"Team"	TEXT NOT NULL,
	"Role"	INTEGER NOT NULL,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "PK_Users" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "VersionDatabases" (
	"Id"	TEXT NOT NULL,
	"Version"	TEXT NOT NULL,
	"DateVersion"	TEXT NOT NULL,
	"PathBackup"	TEXT NOT NULL,
	CONSTRAINT "PK_VersionDatabases" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Emails" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"Email"	TEXT NOT NULL,
	CONSTRAINT "PK_Emails" PRIMARY KEY("Id"),
	CONSTRAINT "FK_Emails_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "OtherVehicles" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"SerialNumber"	TEXT NOT NULL,
	"Type"	TEXT NOT NULL,
	"Mark"	TEXT NOT NULL,
	"Model"	TEXT NOT NULL,
	"StatusDataView"	INTEGER NOT NULL,
	"AdditionalInformation"	TEXT,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "PK_OtherVehicles" PRIMARY KEY("Id"),
	CONSTRAINT "FK_OtherVehicles_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Phones" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"Phone"	TEXT NOT NULL,
	CONSTRAINT "PK_Phones" PRIMARY KEY("Id"),
	CONSTRAINT "FK_Phones_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "ProfessionalClient" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"NameCompany"	TEXT NOT NULL,
	"Siret"	TEXT NOT NULL,
	"TVANumber"	TEXT NOT NULL,
	CONSTRAINT "FK_ProfessionalClient_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_ProfessionalClient" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Vehicles" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"Immatriculation"	TEXT NOT NULL,
	"Type"	TEXT NOT NULL,
	"Mark"	TEXT NOT NULL,
	"Model"	TEXT NOT NULL,
	"VIN"	TEXT NOT NULL,
	"AdditionalInformation"	TEXT NOT NULL,
	"CirculationDate"	TEXT NOT NULL,
	"KM"	INTEGER NOT NULL,
	"StatusDataView"	INTEGER NOT NULL,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "FK_Vehicles_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_Vehicles" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "AssociationSettingReferences" (
	"Id"	TEXT NOT NULL,
	"IdInterestingReferences"	TEXT NOT NULL,
	"EditionSettingId"	TEXT,
	CONSTRAINT "FK_AssociationSettingReferences_EditionSettings_EditionSettingId" FOREIGN KEY("EditionSettingId") REFERENCES "EditionSettings"("Id"),
	CONSTRAINT "FK_AssociationSettingReferences_InterestingReferences_IdInterestingReferences" FOREIGN KEY("IdInterestingReferences") REFERENCES "InterestingReferences"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_AssociationSettingReferences" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Quotes" (
	"Id"	TEXT NOT NULL,
	"IdClient"	TEXT NOT NULL,
	"QuoteNumber"	TEXT NOT NULL,
	"TotalAmount"	REAL,
	"Status"	INTEGER NOT NULL,
	"UserId"	TEXT,
	"DateAccepted"	TEXT,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "FK_Quotes_Users_UserId" FOREIGN KEY("UserId") REFERENCES "Users"("Id"),
	CONSTRAINT "FK_Quotes_Clients_IdClient" FOREIGN KEY("IdClient") REFERENCES "Clients"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_Quotes" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "MaintenanceAlerts" (
	"Id"	TEXT NOT NULL,
	"TypeMaintenace"	TEXT NOT NULL,
	"IdVehicle"	TEXT,
	"IdOtherVehicle"	TEXT,
	"DateMake"	TEXT NOT NULL,
	"DateAdded"	TEXT NOT NULL,
	CONSTRAINT "FK_MaintenanceAlerts_Vehicles_IdVehicle" FOREIGN KEY("IdVehicle") REFERENCES "Vehicles"("Id"),
	CONSTRAINT "FK_MaintenanceAlerts_OtherVehicles_IdOtherVehicle" FOREIGN KEY("IdOtherVehicle") REFERENCES "OtherVehicles"("Id"),
	CONSTRAINT "PK_MaintenanceAlerts" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "QuotesItems" (
	"Id"	TEXT NOT NULL,
	"IdQuote"	TEXT NOT NULL,
	"PartNumber"	TEXT NOT NULL,
	"PartName"	TEXT NOT NULL,
	"PartBrand"	TEXT,
	"Description"	TEXT,
	"Price"	REAL NOT NULL,
	"Quantity"	INTEGER NOT NULL,
	CONSTRAINT "FK_QuotesItems_Quotes_IdQuote" FOREIGN KEY("IdQuote") REFERENCES "Quotes"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_QuotesItems" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "HistoryParts" (
	"Id"	TEXT NOT NULL,
	"Description"	TEXT,
	"Discount"	REAL NOT NULL,
	"IdInvoice"	TEXT NOT NULL,
	"IdOtherVehicle"	TEXT,
	"IdVehicle"	TEXT,
	"KMMounted"	INTEGER,
	"PartBrand"	TEXT,
	"PartName"	TEXT NOT NULL,
	"PartNumber"	TEXT NOT NULL,
	"Price"	REAL NOT NULL,
	"Quantity"	REAL NOT NULL,
	CONSTRAINT "FK_HistoryParts_OtherVehicles_IdOtherVehicle" FOREIGN KEY("IdOtherVehicle") REFERENCES "OtherVehicles"("Id"),
	CONSTRAINT "FK_HistoryParts_Vehicles_IdVehicle" FOREIGN KEY("IdVehicle") REFERENCES "Vehicles"("Id"),
	CONSTRAINT "FK_HistoryParts_Invoices_IdInvoice" FOREIGN KEY("IdInvoice") REFERENCES "Invoices"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_HistoryParts" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "EditionSettings" (
	"Id"	TEXT NOT NULL,
	"PathSaveFile"	TEXT NOT NULL,
	"PathSaveInvociePrepare"	TEXT NOT NULL,
	"Picture"	TEXT,
	"PreloadedLine"	INTEGER NOT NULL,
	"RepairOrderSentenceBottom"	TEXT,
	"RepairOrderSentenceTop"	TEXT,
	"SentenceBottom"	TEXT,
	"SentenceInformationBottom"	TEXT,
	"SentenceInformationTop"	TEXT,
	"StartNumberInvoice"	TEXT NOT NULL,
	"TVA"	REAL,
	"TypeDesign"	INTEGER NOT NULL,
	CONSTRAINT "PK_EditionSettings" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Invoices" (
	"Id"	TEXT NOT NULL,
	"CustomerSuppliedPart"	INTEGER NOT NULL,
	"DateAdded"	TEXT NOT NULL,
	"IdOtherVehicle"	TEXT,
	"IdVehicle"	TEXT,
	"InvoiceNumber"	TEXT NOT NULL,
	"Observations"	TEXT,
	"OrderNumber"	TEXT NOT NULL,
	"PartReturnedCustomer"	INTEGER NOT NULL,
	"Payment"	INTEGER NOT NULL,
	"RepairType"	TEXT,
	"Status"	INTEGER NOT NULL,
	"TotalAmount"	REAL NOT NULL,
	"UserId"	TEXT,
	CONSTRAINT "FK_Invoices_OtherVehicles_IdOtherVehicle" FOREIGN KEY("IdOtherVehicle") REFERENCES "OtherVehicles"("Id"),
	CONSTRAINT "FK_Invoices_Users_UserId" FOREIGN KEY("UserId") REFERENCES "Users"("Id"),
	CONSTRAINT "FK_Invoices_Vehicles_IdVehicle" FOREIGN KEY("IdVehicle") REFERENCES "Vehicles"("Id"),
	CONSTRAINT "PK_Invoices" PRIMARY KEY("Id")
);
INSERT INTO "Clients" VALUES ('39F9E9CA-D9D7-487F-9231-C3EEF5EE2485','Chevalier','Emma','5 Cours Mirabeau','Aix-en-Provence','13100',0,'Clientse VIP','2025-02-05 21:09:00');
INSERT INTO "Clients" VALUES ('4E0B7F25-44C6-45FE-80DE-40D83C2F13EC','Dupont','Jean','123 Rue de la Paix','Paris','75001',0,'Clients fidèle depuis 2010','2025-02-05 21:00:00');
INSERT INTO "Clients" VALUES ('51D7F02B-B13D-4B4D-A29D-E68D23D392B6','Martin','Sophie','45 Avenue des Champs-Élysées','Paris','75008',0,'Préfère le contact par email','2025-02-05 21:01:00');
INSERT INTO "Clients" VALUES ('8C0B69A7-E9A6-4DBD-92C7-6035FBC5138F','Fournier','Antoine','8 Rue du Vieux Marché','Rouen','76000',0,'Intéressé par les nouveaux produits','2025-02-05 21:06:00');
INSERT INTO "Clients" VALUES ('946E0C84-187A-4AE3-B676-15C59E6786C4','Dubois','Pierre','78 Rue du Commerce','Lyon','69002',1,'Entreprise de plomberie','2025-02-05 21:02:00');
INSERT INTO "Clients" VALUES ('970B7937-8CE6-4C01-B114-89A2668B269B','Mercier','Thomas','34 Rue Nationale','Lille','59000',1,'Société de consulting','2025-02-05 21:08:00');
INSERT INTO "Clients" VALUES ('A0DA0054-63A3-43D8-8467-197C191391E1','Roux','Céline','23 Quai des Belges','Marseille','13001',1,'Boutique de vêtements','2025-02-05 21:05:00');
INSERT INTO "Clients" VALUES ('C1682A79-2B40-4F48-AD43-02A1DBAD71F1','Vincent','Isabelle','12 Place du Capitole','Toulouse','31000',0,'Préfère être contactée le soir','2025-02-05 21:07:00');
INSERT INTO "Clients" VALUES ('E305B18E-6B7D-4D0B-AB55-F11689537F6C','Lefebvre','Marie','15 Place Bellecour','Lyon','69002',0,'Allergique aux arachides','2025-02-05 21:03:00');
INSERT INTO "Clients" VALUES ('F52B9D4C-A89F-4FA2-AFF1-BB9986FDA0F8','Moreau','Luc','56 Rue de la République','Marseille','13001',0,'Membre du programme de fidélité','2025-02-05 21:04:00');

CREATE INDEX IF NOT EXISTS "IX_AssociationSettingReferences_EditionSettingId" ON "AssociationSettingReferences" (
	"EditionSettingId"
);
CREATE INDEX IF NOT EXISTS "IX_AssociationSettingReferences_IdInterestingReferences" ON "AssociationSettingReferences" (
	"IdInterestingReferences"
);
CREATE INDEX IF NOT EXISTS "IX_Emails_IdClient" ON "Emails" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_MaintenanceAlerts_IdOtherVehicle" ON "MaintenanceAlerts" (
	"IdOtherVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_MaintenanceAlerts_IdVehicle" ON "MaintenanceAlerts" (
	"IdVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_OtherVehicles_IdClient" ON "OtherVehicles" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_Phones_IdClient" ON "Phones" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_ProfessionalClient_IdClient" ON "ProfessionalClient" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_Quotes_IdClient" ON "Quotes" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_Quotes_UserId" ON "Quotes" (
	"UserId"
);
CREATE INDEX IF NOT EXISTS "IX_QuotesItems_IdQuote" ON "QuotesItems" (
	"IdQuote"
);
CREATE INDEX IF NOT EXISTS "IX_Vehicles_IdClient" ON "Vehicles" (
	"IdClient"
);
CREATE INDEX IF NOT EXISTS "IX_HistoryParts_IdInvoice" ON "HistoryParts" (
	"IdInvoice"
);
CREATE INDEX IF NOT EXISTS "IX_HistoryParts_IdOtherVehicle" ON "HistoryParts" (
	"IdOtherVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_HistoryParts_IdVehicle" ON "HistoryParts" (
	"IdVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_Invoices_IdOtherVehicle" ON "Invoices" (
	"IdOtherVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_Invoices_IdVehicle" ON "Invoices" (
	"IdVehicle"
);
CREATE INDEX IF NOT EXISTS "IX_Invoices_UserId" ON "Invoices" (
	"UserId"
);
COMMIT;
