using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facilys.Components.Services
{
    public class PdfInvoiceType3Service
    {
        public byte[] GenerateInvoicePdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // En-tête avec bandeau coloré
                    page.Header().Element(comp => DrawHeader(comp, invoice, managerInvoiceView, km));

                    // Contenu principal avec mise en page moderne
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // Section client/véhicule en colonnes
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(comp => DrawModernCustomerInfo(comp, managerInvoiceView, phones));
                            row.RelativeItem().Element(comp => DrawModernVehicleInfo(comp, managerInvoiceView));
                        });

                        // Tableau des services avec style minimaliste
                        column.Item().Element(comp => DrawModernServiceTable(comp, managerInvoiceView));

                        // Section totale avec accent visuel
                        column.Item().Element(comp => DrawModernTotalSection(comp, managerInvoiceView));

                        // Mentions légales compactes
                        column.Item().Element(comp => DrawModernLegalMentions(comp, managerInvoiceView));
                    });

                    // Pied de page discret
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void DrawHeader(IContainer container, Invoices invoice, ManagerInvoiceViewModel company, int km)
        {
            container.Background(Colors.Blue.Darken3)
                   .Padding(15)
                   .Row(row =>
                   {
                       // Logo à gauche
                       row.AutoItem().AlignLeft().Width(80).Column(col =>
                       {
                           if (!string.IsNullOrEmpty(company.CompanySettings.Logo))
                           {
                               try
                               {
                                   var base64Image = company.CompanySettings.Logo.Split(',')[1];
                                   var imageBytes = Convert.FromBase64String(base64Image);
                                   col.Item().Image(imageBytes);
                               }
                               catch { /* Ignorer les erreurs */ }
                           }
                       });

                       // Titre et infos au centre
                       row.RelativeItem().AlignCenter().Column(col =>
                       {
                           col.Item().Text("FACTURE").Bold().FontColor(Colors.White).FontSize(20);
                           col.Item().Text($"N° {invoice.InvoiceNumber}").FontColor(Colors.White).FontSize(12);
                       });

                       // Date à droite
                       row.AutoItem().AlignRight().Column(col =>
                       {
                           col.Item().Text($"Date: {invoice.DateAdded:dd/MM/yyyy}")
                              .FontColor(Colors.White).FontSize(10);

                           if (invoice.Vehicle != null)
                           {
                               col.Item().Text($"Km: {km}")
                                  .FontColor(Colors.White).FontSize(10);
                           }
                       });
                   });
        }

        private void DrawModernCustomerInfo(IContainer container, ManagerInvoiceViewModel client, PhonesClients phones)
        {
            container.Background(Colors.Grey.Lighten4)
                   .Border(1)
                   .BorderColor(Colors.Grey.Lighten1)
                   .Padding(10)
                   .Column(column =>
                   {
                       column.Item().Text("CLIENT").Bold().FontSize(12);
                       column.Item().PaddingTop(5);

                       column.Item().Text($"{client.Client.Fname} {client.Client.Lname}").FontSize(10);
                       column.Item().Text(client.Client.Address).FontSize(10);
                       column.Item().Text($"{client.Client.PostalCode} {client.Client.City}").FontSize(10);
                       column.Item().Text($"Tél: {phones.Phone}").FontSize(10);
                   });
        }

        private void DrawModernVehicleInfo(IContainer container, ManagerInvoiceViewModel vehicle)
        {
            container.Background(Colors.Grey.Lighten4)
                   .Border(1)
                   .BorderColor(Colors.Grey.Lighten1)
                   .Padding(10)
                   .Column(column =>
                   {
                       column.Item().Text("VÉHICULE").Bold().FontSize(12);
                       column.Item().PaddingTop(5);

                       if (vehicle.Vehicle != null)
                       {
                           column.Item().Text($"{vehicle.Vehicle.Mark} {vehicle.Vehicle.Model}").FontSize(10);
                           column.Item().Text($"Immat: {vehicle.Vehicle.Immatriculation}").FontSize(10);
                           column.Item().Text($"VIN: {vehicle.Vehicle.VIN}").FontSize(10);
                           column.Item().Text($"Mise en circ.: {vehicle.Vehicle.CirculationDate:dd/MM/yyyy}").FontSize(10);
                       }
                       else if (vehicle.OtherVehicle != null)
                       {
                           column.Item().Text($"{vehicle.OtherVehicle.Mark} {vehicle.OtherVehicle.Model}").FontSize(10);
                           column.Item().Text($"N° série: {vehicle.OtherVehicle.SerialNumber}").FontSize(10);
                       }
                   });
        }

        private void DrawModernServiceTable(IContainer container, ManagerInvoiceViewModel historyParts)
        {
            container.Table(table =>
            {
                // Style minimaliste avec bordures seulement entre les lignes
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Description
                    columns.ConstantColumn(60); // Qté
                    columns.ConstantColumn(80); // Prix
                    columns.ConstantColumn(80); // Total
                });

                // En-tête avec fond coloré
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Lighten3)
                          .PaddingVertical(5).PaddingHorizontal(2)
                          .Text("Description").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Lighten3)
                          .PaddingVertical(5).PaddingHorizontal(2)
                          .AlignRight()
                          .Text("Qté").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Lighten3)
                          .PaddingVertical(5).PaddingHorizontal(2)
                          .AlignRight()
                          .Text("Prix").FontColor(Colors.White).Bold().FontSize(10);

                    header.Cell().Background(Colors.Blue.Lighten3)
                          .PaddingVertical(5).PaddingHorizontal(2)
                          .AlignRight()
                          .Text("Total").FontColor(Colors.White).Bold().FontSize(10);
                });

                // Contenu du tableau
                foreach (var item in historyParts.HistoryParts)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                         .PaddingVertical(5).PaddingHorizontal(2)
                         .Text($"- {item.Description}").FontSize(9);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                         .PaddingVertical(5).PaddingHorizontal(2)
                         .AlignRight()
                         .Text(item.Quantity.ToString()).FontSize(9);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                         .PaddingVertical(5).PaddingHorizontal(2)
                         .AlignRight()
                         .Text(item.Price.ToString("C")).FontSize(9);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                         .PaddingVertical(5).PaddingHorizontal(2)
                         .AlignRight()
                         .Text($"{item.Quantity * item.Price * (1 - item.Discount / 100):C}").FontSize(9);
                }
            });
        }

        private void DrawModernTotalSection(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.AlignRight().Width(200).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Sous-total HT:").FontSize(10);
                    row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.HT.ToString("C")).FontSize(10);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"TVA ({invoiceData.Edition.TVA}%):").FontSize(10);
                    row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.TVA.ToString("C")).FontSize(10);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Total TTC:").Bold().FontSize(11);
                    row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.TTC.ToString("C")).Bold().FontSize(11);
                });
            });
        }

        private void DrawModernLegalMentions(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.PaddingTop(20).Column(column =>
            {
                // Informations société
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                     .PaddingTop(10).Row(row =>
                     {
                         row.RelativeItem().Text($"SIRET: {invoiceData.CompanySettings.Siret}").FontSize(8);
                         row.RelativeItem().AlignRight().Text($"TVA: {invoiceData.CompanySettings.TVA}").FontSize(8);
                     });

                // Conditions générales
                if (!string.IsNullOrEmpty(invoiceData.Edition.SentenceInformationBottom))
                {
                    column.Item().PaddingTop(5).Text(invoiceData.Edition.SentenceInformationBottom).FontSize(8);
                }

                // Message important
                if (!string.IsNullOrEmpty(invoiceData.Edition.SentenceBottom))
                {
                    column.Item().PaddingTop(5).Background(Colors.Red.Lighten4)
                         .Padding(5).Border(1).BorderColor(Colors.Red.Lighten2)
                         .Text(invoiceData.Edition.SentenceBottom).FontSize(8);
                }
            });
        }
    }
}