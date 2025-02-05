INSERT INTO Clients (Id, Lname, Fname, Address, City, PostalCode, Type, AdditionalInformation, DateCreated)
VALUES
('22a85c85-7b0d-43cb-b7b2-0ec8b6b03c65', 'Dupont', 'Jean', '123 Rue de la Paix', 'Paris', '75001', 0, 'Client fidèle depuis 2010', '2025-02-05 21:00:00'),
('c51878f1-99ae-41ab-aa86-8bf899e7157d', 'Martin', 'Sophie', '45 Avenue des Champs-Élysées', 'Paris', '75008', 0, 'Préfère le contact par email', '2025-02-05 21:01:00'),
('396da0e3-4a92-4eb0-8331-ece34309e6bd', 'Dubois', 'Pierre', '78 Rue du Commerce', 'Lyon', '69002', 1, 'Entreprise de plomberie', '2025-02-05 21:02:00'),
('1b42006f-ece1-4d34-a3df-cf37e12c2ee5', 'Lefebvre', 'Marie', '15 Place Bellecour', 'Lyon', '69002', 0, 'Allergique aux arachides', '2025-02-05 21:03:00'),
('a933f6b5-cd21-41f0-b556-a7c85956f406', 'Moreau', 'Luc', '56 Rue de la République', 'Marseille', '13001', 0, 'Membre du programme de fidélité', '2025-02-05 21:04:00'),
('cfba0b7c-5f5e-4fec-adba-d2cff8b6d3df', 'Roux', 'Céline', '23 Quai des Belges', 'Marseille', '13001', 1, 'Boutique de vêtements', '2025-02-05 21:05:00'),
('ecf4ad11-aa61-412e-b83c-efc4ed21bd4e', 'Fournier', 'Antoine', '8 Rue du Vieux Marché', 'Rouen', '76000', 0, 'Intéressé par les nouveaux produits', '2025-02-05 21:06:00'),
('5fc7a3f8-1067-49c7-a111-29e75ea2c883', 'Vincent', 'Isabelle', '12 Place du Capitole', 'Toulouse', '31000', 0, 'Préfère être contactée le soir', '2025-02-05 21:07:00'),
('27550a6c-b07a-45d5-a062-d0e8f285d420', 'Mercier', 'Thomas', '34 Rue Nationale', 'Lille', '59000', 1, 'Société de consulting', '2025-02-05 21:08:00'),
('6d366547-d0f9-4cc9-ab53-6c46c5300529', 'Chevalier', 'Emma', '5 Cours Mirabeau', 'Aix-en-Provence', '13100', 0, 'Cliente VIP', '2025-02-05 21:09:00');

-- Insertion de 10 véhicules dans la table Vehicles
INSERT INTO Vehicles (Id, IdClient, Immatriculation, Type, Mark, Model, VIN, AdditionalInformation, CirculationDate, KM, StatusDataView, DateAdded)
VALUES
    ('b6e24379-b37c-48c0-8321-9d0c3e64bfa9', '22a85c85-7b0d-43cb-b7b2-0ec8b6b03c65', 'AA-123-BB', 'Berline', 'Renault', 'Clio', 'VF1RB1N0H12345678', 'Véhicule principal', '2020-06-15', 45000, 0, '2025-02-05 21:10:00'),
    ('2d6f5564-802e-48d6-a894-0d63faf5d4e2', 'c51878f1-99ae-41ab-aa86-8bf899e7157d', 'BB-456-CC', 'SUV', 'Peugeot', '3008', 'VF3MRHNS0KS123456', 'Véhicule familial', '2019-03-22', 62000, 0, '2025-02-05 21:11:00'),
    ('84d8f1e8-7bb3-4c9a-bd0e-96121e3fbe00', '396da0e3-4a92-4eb0-8331-ece34309e6bd', 'CC-789-DD', 'Utilitaire', 'Citroën', 'Berlingo', 'VF7GBKFWC0J123456', 'Véhicule professionnel', '2021-09-10', 30000, 0, '2025-02-05 21:12:00'),
    ('e4bc34fa-2138-412f-8be5-669cedd87d5c', '1b42006f-ece1-4d34-a3df-cf37e12c2ee5', 'DD-012-EE', 'Citadine', 'Toyota', 'Yaris', 'JTDKG3EA60N123456', 'Véhicule secondaire', '2018-11-30', 75000, 0, '2025-02-05 21:13:00'),
    ('f5f5f5f5-f5f5-f5f5-f5f5-f5f5f5f5f5f5', 'a933f6b5-cd21-41f0-b556-a7c85956f406', 'EE-345-FF', 'Break', 'Volkswagen', 'Passat', 'WVWZZZ3CZLE123456', 'Véhicule de fonction', '2022-01-05', 25000, 0, '2025-02-05 21:14:00'),
    ('d9f83309-5331-44d5-aa06-4573c476840a', 'cfba0b7c-5f5e-4fec-adba-d2cff8b6d3df', 'FF-678-GG', 'Fourgon', 'Ford', 'Transit', 'WF0XXXTTGXNU12345', 'Véhicule de livraison', '2020-07-20', 50000, 0, '2025-02-05 21:15:00'),
    ('dd05e90f-2a75-4de2-b9c3-3221429cdbdf', 'ecf4ad11-aa61-412e-b83c-efc4ed21bd4e', 'GG-901-HH', 'Coupé', 'BMW', 'Série 4', 'WBA4J1C07LB123456', 'Véhicule de loisir', '2023-03-15', 10000, 0, '2025-02-05 21:16:00'),
    ('0a5a2031-75f7-4d9b-a270-af8254a2c58c', '5fc7a3f8-1067-49c7-a111-29e75ea2c883', 'HH-234-II', 'Monospace', 'Opel', 'Zafira', 'W0LPE8EE8G6123456', 'Véhicule familial', '2017-12-01', 90000, 0, '2025-02-05 21:17:00'),
    ('12e77c7a-f985-4246-94b6-a7cbf047839c', '27550a6c-b07a-45d5-a062-d0e8f285d420', 'II-567-JJ', 'Berline', 'Audi', 'A4', 'WAUZZZ8K9NA123456', 'Véhicule de représentation', '2021-11-10', 35000, 0, '2025-02-05 21:18:00'),
    ('78f342a8-fcaf-4e53-b00e-80a10323b694', '6d366547-d0f9-4cc9-ab53-6c46c5300529', 'JJ-890-KK', 'Cabriolet', 'Mercedes', 'Classe C', 'WDD2050421R123456', 'Véhicule de loisir', '2022-06-30', 15000, 0, '2025-02-05 21:19:00');

-- Insertion de 10 autres véhicules dans la table OtherVehicles
INSERT INTO OtherVehicles (Id, IdClient, SerialNumber, Type, Mark, Model, StatusDataView, AdditionalInformation, DateAdded)
VALUES
    ('ad450628-42a5-4616-8229-f78bac22b365', '22a85c85-7b0d-43cb-b7b2-0ec8b6b03c65', 'SN12345', 'Moto', 'Yamaha', 'MT-07', 0, 'Moto de loisir', '2025-02-05 21:20:00'),
    ('ed1d962e-b021-4662-b97a-2c36af264eee', 'c51878f1-99ae-41ab-aa86-8bf899e7157d', 'SN23456', 'Scooter', 'Piaggio', 'Vespa', 0, 'Scooter urbain', '2025-02-05 21:21:00'),
    ('6993a032-ace3-4c50-b413-cebebe548fcd', '396da0e3-4a92-4eb0-8331-ece34309e6bd', 'SN34567', 'Vélo électrique', 'Bosch', 'City', 0, 'Vélo pour déplacements professionnels', '2025-02-05 21:22:00'),
    ('73de567a-2e93-451d-8abb-8182f3ae834c', '1b42006f-ece1-4d34-a3df-cf37e12c2ee5', 'SN45678', 'Trottinette électrique', 'Xiaomi', 'Pro 2', 0, 'Trottinette pour courts trajets', '2025-02-05 21:23:00'),
    ('b028306c-e7ad-4a1f-8c80-d71d37c41645', 'a933f6b5-cd21-41f0-b556-a7c85956f406', 'SN56789', 'Quad', 'Yamaha', 'Grizzly', 0, 'Quad pour loisirs', '2025-02-05 21:24:00'),
    ('373ebbe3-624b-44d4-9a57-f96c63702d5f', 'cfba0b7c-5f5e-4fec-adba-d2cff8b6d3df', 'SN67890', 'Remorque', 'Lider', 'Robust', 0, 'Remorque pour transport de marchandises', '2025-02-05 21:25:00'),
    ('8bece5ba-1406-4d84-b4b8-54abd88b4763', 'ecf4ad11-aa61-412e-b83c-efc4ed21bd4e', 'SN78901', 'Jet Ski', 'Kawasaki', 'Ultra 310LX', 0, 'Jet Ski pour loisirs nautiques', '2025-02-05 21:26:00'),
    ('678de0fa-e0ab-4a0a-b00a-05dd54141cae', '5fc7a3f8-1067-49c7-a111-29e75ea2c883', 'SN89012', 'Camping-car', 'Fiat', 'Ducato', 0, 'Camping-car pour vacances', '2025-02-05 21:27:00'),
    ('97eaab7a-60e4-46b3-ad8f-77d894777ab5', '27550a6c-b07a-45d5-a062-d0e8f285d420', 'SN90123', 'Bateau', 'Bénéteau', 'Flyer 8', 0, 'Bateau de plaisance', '2025-02-05 21:28:00'),
    ('0c20e821-a9f5-42a8-a98d-085737a18329', '6d366547-d0f9-4cc9-ab53-6c46c5300529', 'SN01234', 'Vélo', 'Trek', 'FX 3 Disc', 0, 'Vélo de ville', '2025-02-05 21:29:00');


    -- Insertion des emails pour les clients
INSERT INTO Emails (Id, IdClient, Email)
VALUES
    ('eeafb3f2-982d-4e9b-92b1-782195426c47', '22a85c85-7b0d-43cb-b7b2-0ec8b6b03c65', 'jean.dupont@email.com'),
    ('4c021af5-5a82-4571-afbe-75ecb425a90c', 'c51878f1-99ae-41ab-aa86-8bf899e7157d', 'sophie.martin@email.com'),
    ('7f290760-547a-4c12-9eff-0ee0f90dba2c', '396da0e3-4a92-4eb0-8331-ece34309e6bd', 'pierre.dubois@email.com'),
    ('6c805708-9e94-4468-bfec-953096449d08', '1b42006f-ece1-4d34-a3df-cf37e12c2ee5', 'marie.lefebvre@email.com'),
    ('fa26e918-f333-4b37-91a3-b84be44675f9', 'a933f6b5-cd21-41f0-b556-a7c85956f406', 'luc.moreau@email.com'),
    ('cf9162f9-7e1c-4316-b47e-41a7cace8642', 'cfba0b7c-5f5e-4fec-adba-d2cff8b6d3df', 'celine.roux@email.com'),
    ('dd62eb6e-9762-4c15-81e4-695dc0af9b0a', 'ecf4ad11-aa61-412e-b83c-efc4ed21bd4e', 'antoine.fournier@email.com'),
    ('24635a85-bfed-47db-9f9f-b6690ad7c60d', '5fc7a3f8-1067-49c7-a111-29e75ea2c883', 'isabelle.vincent@email.com'),
    ('b3d37a18-38ca-4076-9e32-4dacf0bf3d80', '27550a6c-b07a-45d5-a062-d0e8f285d420', 'thomas.mercier@email.com'),
    ('6fee7fb9-6ea3-4d96-b4b9-781d4388db7a', '6d366547-d0f9-4cc9-ab53-6c46c5300529', 'emma.chevalier@email.com');

-- Insertion des numéros de téléphone pour les clients
INSERT INTO Phones (Id, IdClient, Phone)
VALUES
    ('2ba7a217-dff0-4246-ae5b-9989a87c52d1', '22a85c85-7b0d-43cb-b7b2-0ec8b6b03c65', '0123456789'),
    ('566339fc-a7b3-46ef-b733-6332827a0d54', 'c51878f1-99ae-41ab-aa86-8bf899e7157d', '0234567890'),
    ('8f838608-3223-48f6-b1b5-ce71803dccf0', '396da0e3-4a92-4eb0-8331-ece34309e6bd', '0345678901'),
    ('716bc7de-555d-473e-a498-ba6537a2397d', '1b42006f-ece1-4d34-a3df-cf37e12c2ee5', '0456789012'),
    ('4cfcb004-ba81-4a6b-9651-acfe73fbb541', 'a933f6b5-cd21-41f0-b556-a7c85956f406', '0567890123'),
    ('bbd8a2a6-99c1-4aa5-ab00-4d71824cf930', 'cfba0b7c-5f5e-4fec-adba-d2cff8b6d3df', '0678901234'),
    ('d483fad6-87e7-4a40-914c-baf5dff22ffa', 'ecf4ad11-aa61-412e-b83c-efc4ed21bd4e', '0789012345'),
    ('bcabc70e-37f7-42a5-b740-cbfdd62d1325', '5fc7a3f8-1067-49c7-a111-29e75ea2c883', '0890123456'),
    ('9c64b2b4-3398-4dbc-b751-274923df275e', '27550a6c-b07a-45d5-a062-d0e8f285d420', '0901234567'),
    ('1a44cac1-ce7f-4c63-b612-dca180049361', '6d366547-d0f9-4cc9-ab53-6c46c5300529', '0012345678');
