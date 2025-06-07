using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facilys.Components.Services
{
    public class PdfRepairOrderService
    {
        public byte[] GenerateRepairOrderPdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // En-tête
                    page.Header().Element(comp => DrawHeader(comp, invoice, managerInvoiceView));

                    // Contenu principal
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Informations client et véhicule
                        column.Item().Element(comp => DrawClientAndVehicleInfo(comp, managerInvoiceView, km, phones, emails));

                        // Tableau des services
                        column.Item().Element(comp => DrawServiceTable(comp, managerInvoiceView));

                        // Informations complémentaires
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(comp => DrawRequiredInformation(comp, managerInvoiceView));
                            row.ConstantItem(190).Element(comp => DrawTotal(comp, managerInvoiceView));
                        });

                        // Cases à cocher
                        column.Item().Element(comp => DrawCheckboxes(comp, managerInvoiceView));
                    });

                    // Pied de page
                    page.Footer().Element(comp => DrawFooter(comp, managerInvoiceView));
                });
            });

            return document.GeneratePdf();
        }

        private void DrawHeader(IContainer container, Invoices invoice, ManagerInvoiceViewModel company)
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
                                // Gestion d'erreur silencieuse
                            }
                        }
                    });

                    // Titre et infos à droite
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("ORDRE DE REPARATION").Bold().FontSize(16);
                        col.Item().Text($"Date : {invoice.DateAdded:dd-MM-yyyy}").FontSize(10);
                        col.Item().Text($"N° : {invoice.InvoiceNumber}").FontSize(10);
                    });
                });

                // Informations société
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"SIRET : {company.CompanySettings.Siret}").FontSize(10);
                        col.Item().Text($"APE : {company.CompanySettings.CodeNAF}").FontSize(10);
                        col.Item().Text($"TVA : {company.CompanySettings.TVA}").FontSize(10);
                    });
                });

                // Ligne de séparation
                column.Item().PaddingVertical(5).LineHorizontal(1);
            });
        }

        private void DrawClientAndVehicleInfo(IContainer container, ManagerInvoiceViewModel dataInvoice, int km, PhonesClients phones, EmailsClients emails)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); // Label colonne 1
                    columns.RelativeColumn();    // Valeur colonne 1
                    columns.ConstantColumn(70); // Label colonne 2
                    columns.RelativeColumn();    // Valeur colonne 2
                });

                // Client
                table.Cell().Border(1).Padding(3).Text("Nom :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Client.Lname.ToUpper()).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("Adresse :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Client.Address.ToUpper()).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Prénom :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Client.Fname.ToUpper()).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("Ville :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Client.City.ToUpper()).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Téléphone :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(phones.Phone).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("Code Postal :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Client.PostalCode).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Email :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(emails.Email ?? "-").FontSize(9);
                table.Cell().Border(1).Padding(3).Text("-").FontSize(9);
                table.Cell().Border(1).Padding(3).Text("-").FontSize(9);

                // Véhicule
                table.Cell().Border(1).Padding(3).Text("Marque :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.Mark).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("Modèle :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.Model).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Mise en circ. :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.CirculationDate.ToString("dd/MM/yyyy")).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("Type :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.Type).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Immatriculation :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.Immatriculation).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("N° série :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(dataInvoice.Vehicle.VIN).FontSize(9);

                table.Cell().Border(1).Padding(3).Text("Kilométrage :").FontSize(9);
                table.Cell().Border(1).Padding(3).Text(km.ToString()).FontSize(9);
                table.Cell().Border(1).Padding(3).Text("-").FontSize(9);
                table.Cell().Border(1).Padding(3).Text("-").FontSize(9);
            });
        }

        private void DrawServiceTable(IContainer container, ManagerInvoiceViewModel historyParts)
        {
            container.Border(1).Padding(0).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(75);  // Référence
                    columns.RelativeColumn(3);   // Description
                    columns.ConstantColumn(30);  // Qté
                    columns.ConstantColumn(60);  // Prix Unit
                    columns.ConstantColumn(50); // Remise
                    columns.ConstantColumn(70); // Montant HT
                });

                // En-tête
                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).Text("Référence").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).Text("Description").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).Text("Qté").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).Text("Prix Unit").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).Text("Remise").Bold().FontSize(10);
                    header.Cell().Border(1).Padding(5).Text("Montant HT").Bold().FontSize(10);
                });

                // Contenu
                foreach (var item in historyParts.HistoryParts)
                {
                    table.Cell().Padding(3).Text(item.PartNumber).FontSize(9);
                    table.Cell().Padding(3).Text($"- {item.Description}").FontSize(9);
                    table.Cell().Padding(3).Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().Padding(3).Text(item.Price.ToString("C")).FontSize(9);
                    table.Cell().Padding(3).Text($"{item.Discount} %").FontSize(9);
                    table.Cell().Padding(3).Text($"{item.Quantity * item.Price * (1 - item.Discount / 100):C}").FontSize(9);
                }
            });
        }

        private void DrawRequiredInformation(IContainer container, ManagerInvoiceViewModel invoiceInformation)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().AlignCenter().Text("INFORMATION COMPLEMENTAIRE").Bold().FontSize(10);
                column.Item().PaddingTop(5).Text(invoiceInformation.InvoiceData.GeneralConditionOrder).FontSize(9);
            });
        }

        private void DrawTotal(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("SOUS-TOTAL HT :").Bold().FontSize(10);
                    row.RelativeItem().Text(invoiceData.InvoiceData.HT.ToString("C")).Bold().FontSize(10);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"TAUX DE T.V.A :").FontSize(9);
                    row.RelativeItem().Text($"{invoiceData.Edition.TVA}%").FontSize(9);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("T.V.A :").FontSize(9);
                    row.RelativeItem().Text(invoiceData.InvoiceData.TVA.ToString("C")).FontSize(9);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("TOTAL TTC :").Bold().FontSize(10);
                    row.RelativeItem().Text(invoiceData.InvoiceData.TTC.ToString("C")).Bold().FontSize(10);
                });
            });
        }

        private void DrawCheckboxes(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Row(r =>
                    {
                        r.ConstantItem(15).Height(15).Border(1).Background(invoiceData.InvoiceData.PartReturnedCustomer ? Colors.Black : Colors.White);
                        r.RelativeItem().AlignMiddle().Text("Pièces remplacées à remettre au client").FontSize(9);
                    });

                    column.Item().PaddingTop(5).Row(r =>
                    {
                        r.ConstantItem(15).Height(15).Border(1).Background(invoiceData.InvoiceData.CustomerSuppliedPart ? Colors.Black : Colors.White);
                        r.RelativeItem().AlignMiddle().Text("Pièces fournies par le client").FontSize(9);
                    });
                });
            });
        }

        private void DrawFooter(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.Column(column =>
            {
                // Texte d'information
                if (!string.IsNullOrEmpty(invoiceData.Edition.RepairOrderSentenceTop))
                {
                    foreach (var line in invoiceData.Edition.RepairOrderSentenceTop.Split('\n'))
                    {
                        column.Item().AlignCenter().Text(line).FontSize(9);
                    }
                }

                // Rectangle jaune avec texte
                if (!string.IsNullOrEmpty(invoiceData.Edition.RepairOrderSentenceBottom))
                {
                    column.Item().PaddingTop(5).Background(Colors.Yellow.Lighten3).Border(1)
                        .Padding(5).AlignCenter().Text(invoiceData.Edition.RepairOrderSentenceBottom).FontSize(9);
                }

                // Signature
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(200).Height(50).Border(1)
                        .AlignCenter().AlignMiddle().Text("Signature pour accord").FontSize(9);
                });

                // Mention "Valable 1 mois"
                column.Item().PaddingTop(5).AlignCenter().Text("Valable 1 mois").Bold().FontSize(10);
            });
        }
    }
}