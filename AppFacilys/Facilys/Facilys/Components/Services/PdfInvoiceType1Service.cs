using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facilys.Components.Services
{
    public class PdfInvoiceType1Service
    {
        public byte[] GenerateInvoicePdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
        {
            // Configuration de la licence (nécessaire une seule fois)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // En-tête
                    page.Header().Element(comp => DrawHeader(comp, invoice, managerInvoiceView, km));

                    // Contenu principal
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Informations client et véhicule
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(comp => DrawCustomerInfo(comp, managerInvoiceView, phones));
                            row.RelativeItem().Element(comp => DrawVehicleInfo(comp, managerInvoiceView));
                        });

                        // Tableau des services
                        column.Item().Element(comp => DrawServiceTable(comp, managerInvoiceView));

                        // Informations à prévoir
                        column.Item().Element(comp => DrawRequiredInformation(comp, managerInvoiceView));

                        // Total et mentions légales
                        column.Item().Element(comp => DrawTotalAndLegalMentions(comp, managerInvoiceView));
                    });

                    // Pied de page
                    page.Footer().AlignCenter().Text("MERCI DE VOTRE CONFIANCE !").Bold();
                });
            });

            return document.GeneratePdf();
        }

        private void DrawHeader(IContainer container, Invoices invoice, ManagerInvoiceViewModel company, int km)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Logo à gauche
                    row.RelativeItem().AlignLeft().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(company.CompanySettings.Logo))
                        {
                            try
                            {
                                var base64Image = company.CompanySettings.Logo.Split(',')[1];
                                var imageBytes = Convert.FromBase64String(base64Image);
                                col.Item().Width(80).Image(imageBytes);
                            }
                            catch
                            {
                                // Gestion d'erreur silencieuse si le logo ne peut être chargé
                            }
                        }

                        // Informations société
                        col.Item().Text(company.CompanySettings.HeadOfficeAddress.ToUpper()).FontSize(10);
                        col.Item().Text($"Téléphone : {company.CompanySettings.Phone}").FontSize(10);
                        col.Item().Text($"Email : {company.CompanySettings.Email}").FontSize(10);
                        col.Item().Text(company.CompanySettings.WebSite ?? "").FontSize(10);
                    });

                    // Titre et infos facture à droite
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("FACTURE").Bold().FontSize(16);
                        col.Item().Text($"Date : {invoice.DateAdded:dd-MM-yyyy}").FontSize(10);
                        col.Item().Text($"N° Facture : {invoice.InvoiceNumber}").FontSize(10);

                        if (invoice.Vehicle != null)
                        {
                            col.Item().Text($"Km : {km}").FontSize(10);
                        }
                    });
                });

                // Ligne de séparation
                column.Item().PaddingVertical(5).LineHorizontal(1);
            });
        }

        private void DrawCustomerInfo(IContainer container, ManagerInvoiceViewModel client, PhonesClients phones)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().AlignCenter().Text("CLIENT").Bold();
                column.Item().PaddingTop(5);

                var clientInfo = new[]
                {
                    ("Nom:", client.Client.Lname),
                    ("Prénom:", client.Client.Fname),
                    ("Rue:", client.Client.Address),
                    ("Code postal:", client.Client.PostalCode),
                    ("Ville:", client.Client.City),
                    ("Téléphone:", phones.Phone)
                };

                foreach (var (label, value) in clientInfo)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(60).Text(label).FontSize(10);
                        row.RelativeItem().Text(value).FontSize(10);
                    });
                }
            });
        }

        private void DrawVehicleInfo(IContainer container, ManagerInvoiceViewModel vehicle)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().AlignCenter().Text("VEHICULE").Bold();
                column.Item().PaddingTop(5);

                string[][] vehicleInfo = vehicle.Vehicle != null
                    ? new[]
                    {
                        new[] { "Marque:", vehicle.Vehicle.Mark },
                        new[] { "Modèle:", vehicle.Vehicle.Model },
                        new[] { "Immatriculation:", vehicle.Vehicle.Immatriculation },
                        new[] { "VIN:", vehicle.Vehicle.VIN },
                        new[] { "Type:", vehicle.Vehicle.Type },
                        new[] { "Mise en circulation:", vehicle.Vehicle.CirculationDate.ToString("dd/MM/yyyy") }
                    }
                    : new[]
                    {
                        new[] { "Marque:", vehicle.OtherVehicle.Mark },
                        new[] { "Modèle:", vehicle.OtherVehicle.Model },
                        new[] { "Numéro :", vehicle.OtherVehicle.SerialNumber },
                        new[] { "Type:", vehicle.OtherVehicle.Type }
                    };

                foreach (var info in vehicleInfo)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(60).Text(info[0]).FontSize(10);
                        row.RelativeItem().Text(info[1]).FontSize(10);
                    });
                }
            });
        }

        private void DrawServiceTable(IContainer container, ManagerInvoiceViewModel historyParts)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Description
                    columns.ConstantColumn(60); // Quantité
                    columns.ConstantColumn(75); // Prix Unitaire
                    columns.ConstantColumn(55); // Remise
                    columns.ConstantColumn(75); // Montant HT
                });

                // En-tête
                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Description").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Quantité").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Prix Unitaire").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Remise").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Montant HT").Bold().FontSize(10);
                });

                // Contenu
                foreach (var item in historyParts.HistoryParts)
                {
                    table.Cell().Border(1).Padding(3).AlignLeft().Text($"{item.Description}").FontSize(9);
                    table.Cell().Border(1).Padding(3).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().Border(1).Padding(3).AlignCenter().Text(item.Price.ToString("C")).FontSize(9);
                    table.Cell().Border(1).Padding(3).AlignCenter().Text($"{item.Discount} %").FontSize(9);
                    table.Cell().Border(1).Padding(3).AlignCenter().Text($"{item.Quantity * item.Price * (1 - item.Discount / 100):C}").FontSize(9);
                }
            });
        }

        private void DrawRequiredInformation(IContainer container, ManagerInvoiceViewModel invoiceInformation)
        {
            container.Border(1).Padding(3).Column(column =>
            {
                column.Item().AlignCenter().Text("INTERVENTION A PREVOIR").Bold();
                column.Item().PaddingTop(5).Text(invoiceInformation.InvoiceData.GeneralConditionInvoice).FontSize(9);
            });
        }

        private void DrawTotalAndLegalMentions(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.Column(column =>
            {
                // Section Total
                column.Item().AlignRight().Width(200).Padding(5).Column(totalCol =>
                {
                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text("SOUS-TOTAL HT :").Bold().FontSize(10);
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text(invoiceData.InvoiceData.HT.ToString("C")).Bold().FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text($"TAUX DE T.V.A :").FontSize(10);
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text($"{invoiceData.Edition.TVA}%").FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text("T.V.A :").FontSize(10);
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text(invoiceData.InvoiceData.TVA.ToString("C")).FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text("TOTAL TTC :").Bold().FontSize(10);
                        row.RelativeItem().Border(1).Padding(5).AlignCenter().Text(invoiceData.InvoiceData.TTC.ToString("C")).Bold().FontSize(10);
                    });
                });

                // Mentions légales
                column.Item().PaddingTop(10).Column(legalCol =>
                {
                    legalCol.Item().Text($"SIRET : {invoiceData.CompanySettings.Siret}").Bold().FontSize(9);
                    legalCol.Item().Text($"TVA : {invoiceData.CompanySettings.TVA}").Bold().FontSize(9);
                    legalCol.Item().Text($"APE : {invoiceData.CompanySettings.CodeNAF}").Bold().FontSize(9);
                });

                // Mentions en bas de page
                column.Item().PaddingTop(10).AlignCenter().Text(invoiceData.Edition.SentenceInformationBottom).FontSize(9);

                if (!string.IsNullOrEmpty(invoiceData.Edition.SentenceBottom))
                {
                    column.Item().PaddingTop(5).Background(Colors.Red.Lighten1).Padding(5)
                        .AlignCenter().Text(invoiceData.Edition.SentenceBottom).FontSize(10);
                }
            });
        }
    }
}