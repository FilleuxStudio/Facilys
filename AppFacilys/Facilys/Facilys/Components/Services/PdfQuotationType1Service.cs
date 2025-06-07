//using Facilys.Components.Models;
//using Facilys.Components.Models.ViewModels;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;

//namespace Facilys.Components.Services
//{
//    public class PdfQuotationType1Service // Nom du service modifié pour refléter la fonctionnalité
//    {
//        // Constantes pour la mise en page
//        private const double PageWidth = 595;
//        private const double PageHeight = 842;
//        private const double Margin = 20;
//        private const double LineHeight = 11;
//        private const double TopMargin = 20;
//        private const double BottomMargin = 20;
//        private const double LeftMargin = 20;
//        private const double RightMargin = 20;

//        public byte[] GenerateQuotationPdf(ManagerQuotationViewModel managerQuotationView, Quotes quotes, int km, PhonesClients phones, EmailsClients emails)
//        {
//            var document = new PdfDocument();
//            var page = document.AddPage();
//            page.Size = PdfSharp.PageSize.A4;

//            var gfx = XGraphics.FromPdfPage(page);
//            var yPosition = Margin;

//            // Configuration des polices
//            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
//            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
//            var strongFont = new XFont("Arial", 10, XFontStyleEx.Bold);
//            var normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);

//            // Dessiner l'en-tête du devis
//            yPosition = DrawHeader(gfx, quotes, managerQuotationView, km, titleFont, normalFont, yPosition);

//            // Informations client
//            yPosition = DrawCustomerInfo(gfx, managerQuotationView, phones, normalFont, yPosition);

//            // Informations véhicule
//            yPosition = DrawVehicleInfo(gfx, managerQuotationView, normalFont, yPosition);

//            // Tableau des prestations
//            yPosition = DrawServiceTable(gfx, managerQuotationView, headerFont, normalFont, yPosition);

//            // Informations complémentaires
//            yPosition = DrawAdditionalInformation(gfx, managerQuotationView, normalFont, yPosition);

//            // Total
//            yPosition = DrawTotal(gfx, managerQuotationView, strongFont, normalFont, yPosition);

//            // Mentions légales spécifiques au devis
//            yPosition = DrawLegalMentions(gfx, managerQuotationView, headerFont, normalFont, yPosition);

//            using var stream = new MemoryStream();
//            document.Save(stream);
//            return stream.ToArray();
//        }

//        private static byte[] PictureToStream(string logo)
//        {
//            string base64Image = logo.Split(',')[1];
//            return Convert.FromBase64String(base64Image);
//        }

//        private double DrawHeader(XGraphics gfx, Quotes quotes, ManagerQuotationViewModel company, int km, XFont titleFont, XFont normalFont, double yPosition)
//        {
//            double newHeight = 0;
//            try
//            {
//                byte[] imageBytes = PictureToStream(company.CompanySettings.Logo);
//                using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length, true, true))
//                {
//                    XImage logo = XImage.FromStream(ms);
//                    double maxWidth = 168;
//                    double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;
//                    newHeight = maxWidth / aspectRatio;
//                    gfx.DrawImage(logo, Margin, yPosition, maxWidth, newHeight);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Erreur lors du chargement de l'image : {ex.Message}");
//            }

//            yPosition += LineHeight;

//            // Titre modifié pour "DEVIS"
//            XRect rect = new(LeftMargin, yPosition, PageWidth - LeftMargin - RightMargin, LineHeight);
//            gfx.DrawString("DEVIS", titleFont, XBrushes.Black, rect, XStringFormats.TopRight);

//            yPosition += newHeight + LineHeight;
//            double initialYPosition = yPosition;

//            // Coordonnées du garage
//            gfx.DrawString(company.CompanySettings.HeadOfficeAddress.ToUpper(), normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("Téléphone : " + company.CompanySettings.Phone, normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("Email : " + company.CompanySettings.Email, normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString(company.CompanySettings.WebSite ?? "", normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight * 2;

//            yPosition = initialYPosition;


//            string dateString = $"Date : {quotes.DateAdded:dd-MM-yyyy}";
//            XSize dateSize = gfx.MeasureString(dateString, normalFont);
//            gfx.DrawString(dateString, normalFont, XBrushes.Black, PageWidth - RightMargin - dateSize.Width, yPosition);
//            yPosition += LineHeight;

//            string invoiceNumberString = $"N° Devis : {quotes.QuoteNumber}";
//            XSize invoiceNumberSize = gfx.MeasureString(invoiceNumberString, normalFont);
//            gfx.DrawString(invoiceNumberString, normalFont, XBrushes.Black, PageWidth - RightMargin - invoiceNumberSize.Width, yPosition);
//            yPosition += LineHeight;

//            if (quotes.Vehicle != null)
//            {
//                string kmString = $"Km : {km}";
//                XSize kmSize = gfx.MeasureString(kmString, normalFont);
//                gfx.DrawString(kmString, normalFont, XBrushes.Black, PageWidth - RightMargin - kmSize.Width, yPosition);
//            }

//            yPosition += LineHeight * 2;

//            return yPosition;
//        }

//        private static double DrawCustomerInfo(XGraphics gfx, ManagerQuotationViewModel client, PhonesClients phones, XFont normalFont, double yPosition)
//        {
//            double initialYPosition = yPosition;
//            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
//            double rectHeight = LineHeight * 7;

//            XRect rect = new(LeftMargin, yPosition, rectWidth, rectHeight);
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            XRect titleRect = new(rect.Left, yPosition, rectWidth, LineHeight);
//            gfx.DrawString("CLIENT", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
//            yPosition += LineHeight * 1.5;

//            string[][] clientInfo =
//            [
//                ["Nom:", client.Client.Lname],
//                ["Prénom:", client.Client.Fname],
//                ["Rue:", client.Client.Address],
//                ["Code postal:", client.Client.PostalCode],
//                ["Ville:", client.Client.City],
//                ["Téléphone:", phones.Phone]
//            ];

//            double maxLabelWidth = 0;
//            foreach (var info in clientInfo)
//            {
//                XSize size = gfx.MeasureString(info[0], normalFont);
//                if (size.Width > maxLabelWidth)
//                    maxLabelWidth = size.Width;
//            }

//            maxLabelWidth += 5;
//            double textMargin = Margin + 5;
//            foreach (var info in clientInfo)
//            {
//                gfx.DrawString(info[0], normalFont, XBrushes.Black, textMargin, yPosition);
//                gfx.DrawString(info[1], normalFont, XBrushes.Black, textMargin + maxLabelWidth, yPosition);
//                yPosition += LineHeight;
//            }

//            return initialYPosition + rectHeight;
//        }

//        private static double DrawVehicleInfo(XGraphics gfx, ManagerQuotationViewModel vehicle, XFont normalFont, double yPosition)
//        {
//            double initialYPosition = yPosition;
//            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
//            double rectHeight = LineHeight * 7;

//            XRect rect = new(PageWidth - RightMargin - rectWidth, yPosition, rectWidth, rectHeight);
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            XRect titleRect = new(rect.Left, yPosition, rectWidth, LineHeight);
//            gfx.DrawString("VEHICULE", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
//            yPosition += LineHeight * 1.5;

//            string[][] vehicleInfo;

//            if (vehicle.Vehicle != null)
//            {
//                vehicleInfo = [
//                    ["Marque:", vehicle.Vehicle.Mark],
//                    ["Modèle:", vehicle.Vehicle.Model],
//                    ["Immatriculation:", vehicle.Vehicle.Immatriculation],
//                    ["VIN:", vehicle.Vehicle.VIN],
//                    ["Type:", vehicle.Vehicle.Type],
//                    ["Mise en circulation:", vehicle.Vehicle.CirculationDate.ToString("dd/MM/yyyy")]
//                ];
//            }
//            else
//            {
//                vehicleInfo = [
//                    ["Marque:", vehicle.OtherVehicle.Mark],
//                    ["Modèle:", vehicle.OtherVehicle.Model],
//                    ["Numéro :", vehicle.OtherVehicle.SerialNumber],
//                    ["Type:", vehicle.OtherVehicle.Type],
//                ];
//            }

//            double maxLabelWidth = 0;
//            foreach (var info in vehicleInfo)
//            {
//                XSize size = gfx.MeasureString(info[0], normalFont);
//                if (size.Width > maxLabelWidth)
//                    maxLabelWidth = size.Width;
//            }

//            maxLabelWidth += 5;
//            double textMargin = rect.Left + 5;
//            foreach (var info in vehicleInfo)
//            {
//                gfx.DrawString(info[0], normalFont, XBrushes.Black, textMargin, yPosition);
//                gfx.DrawString(info[1], normalFont, XBrushes.Black, textMargin + maxLabelWidth, yPosition);
//                yPosition += LineHeight;
//            }

//            return initialYPosition + rectHeight;
//        }

//        private static double DrawServiceTable(XGraphics gfx, ManagerQuotationViewModel quotesItems, XFont strongFont, XFont normalFont, double yPosition)
//        {
//            // En-tête modifié pour "PRESTATIONS"
//            string[] headers = ["Prestations", "Quantité", "Prix Unitaire", "Remise", "Montant HT"];
//            double[] columnWidths = [295, 55, 75, 55, 75];
//            double tableWidth = columnWidths.Sum();
//            double tableStartX = Margin;
//            double tableStartY = yPosition;
//            double cellPadding = 5;

//            XPen tablePen = new(XColors.Black, 1);

//            // Dessiner l'en-tête
//            for (int i = 0; i < headers.Length; i++)
//            {
//                double columnX = tableStartX + columnWidths.Take(i).Sum();
//                XRect headerRect = new(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);
//                gfx.DrawRectangle(tablePen, headerRect);
//                gfx.DrawString(headers[i], strongFont, XBrushes.Black, headerRect, XStringFormats.Center);
//            }
//            yPosition += LineHeight + cellPadding * 2;

//            for (int i = 0; i <= headers.Length; i++)
//            {
//                double x = tableStartX + columnWidths.Take(i).Sum();
//                gfx.DrawLine(tablePen, x, tableStartY, x, yPosition);
//            }

//            // Corps du tableau
//            foreach (var item in quotesItems.QuotesItems)
//            {
//                string[] rowData = [
//                    "- " + item.Description,
//                    item.Quantity.ToString(),
//                    item.Price.ToString("C"),
//                    ((item.Quantity * item.Price)).ToString("C")
//                ];

//                double rowStartY = yPosition;

//                for (int i = 0; i < rowData.Length; i++)
//                {
//                    double columnX = tableStartX + columnWidths.Take(i).Sum();
//                    XRect cellRect = new(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);
//                    XStringFormat format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.TopRight;
//                    gfx.DrawString(rowData[i], normalFont, XBrushes.Black,
//                        new XRect(cellRect.X + cellPadding, cellRect.Y + cellPadding,
//                                  cellRect.Width - cellPadding * 2, cellRect.Height - cellPadding * 2),
//                        format);
//                }
//                yPosition += LineHeight + cellPadding * 2;

//                for (int i = 0; i <= headers.Length; i++)
//                {
//                    double x = tableStartX + columnWidths.Take(i).Sum();
//                    gfx.DrawLine(tablePen, x, rowStartY, x, yPosition);
//                }
//            }

//            gfx.DrawLine(tablePen, tableStartX, yPosition, tableStartX + tableWidth, yPosition);

//            return yPosition;
//        }

//        private static double DrawAdditionalInformation(XGraphics gfx, ManagerQuotationViewModel quoationInformation, XFont normalFont, double yPosition)
//        {
//            double initialYPosition = yPosition;
//            double rectWidth = PageWidth - LeftMargin - RightMargin;
//            double rectHeight = LineHeight * 4;

//            XRect rect = new(LeftMargin, yPosition, rectWidth, rectHeight);
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            // Titre modifié pour "INFORMATIONS COMPLÉMENTAIRES"
//            XRect titleRect = new(rect.X, rect.Y, rect.Width, LineHeight);
//            gfx.DrawString("INFORMATIONS COMPLÉMENTAIRES", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);

//            double textY = rect.Y + LineHeight + 5;
//            string text = quoationInformation.Quote.Observations;
//            gfx.DrawString(text, normalFont, XBrushes.Black,
//                new XRect(rect.X + 5, textY, rect.Width - 10, rect.Height - LineHeight - 10),
//                XStringFormats.TopLeft);

//            return initialYPosition + rectHeight;
//        }

//        private static double DrawTotal(XGraphics gfx, ManagerQuotationViewModel quotationData, XFont headerFont, XFont normalFont, double yPosition)
//        {
//            double rectWidth = 190;
//            double rectHeight = LineHeight * 8;
//            double rectX = PageWidth - RightMargin - rectWidth;
//            double rectY = yPosition;

//            XRect mainRect = new(rectX, rectY, rectWidth, rectHeight);
//            gfx.DrawRectangle(new XPen(XColors.Black, 1), mainRect);

//            string[] labels = ["SOUS-TOTAL HT :", "TAUX DE T.V.A :", "T.V.A :", "TOTAL TTC :"];
//            string[] values = [
//                quotationData.QuotationData.HT.ToString("C"),
//                $"{quotationData.Edition.TVA}%",
//                quotationData.QuotationData.TVA.ToString("C"),
//                quotationData.QuotationData.TTC.ToString("C")
//            ];

//            for (int i = 0; i < 4; i++)
//            {
//                double lineY = rectY + i * LineHeight;

//                if (i < 3)
//                {
//                    gfx.DrawLine(new XPen(XColors.Black, 1), rectX, lineY + LineHeight * (2 + i), rectX + rectWidth, lineY + LineHeight * (2 + i));
//                }

//                XRect textRect = new(rectX + 5, lineY, rectWidth - 10, LineHeight * ((2 + i) + i));
//                gfx.DrawString(labels[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterLeft);
//                gfx.DrawString(values[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterRight);
//            }

//            yPosition = rectY + rectHeight + LineHeight;

//            // Mentions légales
//            gfx.DrawString("SIRET : " + quotationData.CompanySettings.Siret, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("TVA : " + quotationData.CompanySettings.TVA, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("APE : " + quotationData.CompanySettings.CodeNAF, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight * 2;

//            return yPosition;
//        }

//        private static double DrawLegalMentions(XGraphics gfx, ManagerQuotationViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
//        {
//            double startY = PageHeight - BottomMargin - (LineHeight * 6);

//            // Première phrase spécifique aux devis
//            XStringFormat centerFormat = new()
//            {
//                Alignment = XStringAlignment.Center,
//                LineAlignment = XLineAlignment.Near
//            };

//            string validityText = "Devis valable 30 jours à compter de la date d'édition";
//            XRect firstTextRect = new(LeftMargin, startY, PageWidth - 2 * LeftMargin, LineHeight);
//            gfx.DrawString(validityText, normalFont, XBrushes.Black, firstTextRect, centerFormat);
//            startY += LineHeight;

//            // Rectangle d'informations
//            const double padding = 5;
//            double infoBoxHeight = 3 * LineHeight + padding * 2;

//            XRect infoRect = new(
//                LeftMargin,
//                startY,
//                PageWidth - 2 * LeftMargin,
//                infoBoxHeight
//            );

//            gfx.DrawRectangle(XBrushes.LightGray, infoRect); // Couleur plus neutre

//            string[] infoLines = [
//                "Ce devis est établi sous réserve d'erreurs matérielles",
//                "Les prix sont indicatifs et susceptibles de variations",
//                "Garantie pièces et main d'œuvre selon conditions constructeur"
//            ];

//            double textY = startY + padding;
//            foreach (string line in infoLines)
//            {
//                XRect lineRect = new(LeftMargin, textY, PageWidth - 2 * LeftMargin, LineHeight);
//                gfx.DrawString(line, normalFont, XBrushes.Black, lineRect, XStringFormats.Center);
//                textY += LineHeight;
//            }

//            // Message de pied de page
//            XRect footerRect = new(
//                0,
//                PageHeight - BottomMargin,
//                PageWidth,
//                LineHeight
//            );

//            XStringFormat footerFormat = new()
//            {
//                Alignment = XStringAlignment.Center,
//                LineAlignment = XLineAlignment.Far
//            };

//            gfx.DrawString("Nous vous remercions de votre confiance",
//                          headerFont,
//                          XBrushes.Black,
//                          footerRect,
//                          footerFormat);

//            return PageHeight - BottomMargin;
//        }
//    }
//}
using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facilys.Components.Services
{
    public class PdfQuotationType1Service
    {
        public byte[] GenerateQuotationPdf(ManagerQuotationViewModel managerQuotationView, Quotes quotes, int km, PhonesClients phones, EmailsClients emails)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // En-tête
                    page.Header().Element(comp => DrawHeader(comp, quotes, managerQuotationView, km));

                    // Contenu principal
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Informations client et véhicule
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(comp => DrawCustomerInfo(comp, managerQuotationView, phones));
                            row.RelativeItem().Element(comp => DrawVehicleInfo(comp, managerQuotationView));
                        });

                        // Tableau des prestations
                        column.Item().Element(comp => DrawServiceTable(comp, managerQuotationView));

                        // Informations complémentaires
                        column.Item().Element(comp => DrawAdditionalInformation(comp, managerQuotationView));

                        // Total et mentions légales
                        column.Item().Element(comp => DrawTotalAndLegalMentions(comp, managerQuotationView));
                    });

                    // Pied de page
                    page.Footer().AlignCenter().Text("Nous vous remercions de votre confiance").Bold();
                });
            });

            return document.GeneratePdf();
        }

        private void DrawHeader(IContainer container, Quotes quotes, ManagerQuotationViewModel company, int km)
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

                        // Informations société
                        col.Item().Text(company.CompanySettings.HeadOfficeAddress.ToUpper()).FontSize(10);
                        col.Item().Text($"Téléphone : {company.CompanySettings.Phone}").FontSize(10);
                        col.Item().Text($"Email : {company.CompanySettings.Email}").FontSize(10);
                        col.Item().Text(company.CompanySettings.WebSite ?? "").FontSize(10);
                    });

                    // Titre et infos devis à droite
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("DEVIS").Bold().FontSize(16);
                        col.Item().Text($"Date : {quotes.DateAdded:dd-MM-yyyy}").FontSize(10);
                        col.Item().Text($"N° Devis : {quotes.QuoteNumber}").FontSize(10);

                        if (quotes.Vehicle != null)
                        {
                            col.Item().Text($"Km : {km}").FontSize(10);
                        }
                    });
                });

                // Ligne de séparation
                column.Item().PaddingVertical(5).LineHorizontal(1);
            });
        }

        private void DrawCustomerInfo(IContainer container, ManagerQuotationViewModel client, PhonesClients phones)
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

        private void DrawVehicleInfo(IContainer container, ManagerQuotationViewModel vehicle)
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

        private void DrawServiceTable(IContainer container, ManagerQuotationViewModel quotesItems)
        {
            container.Border(1).Padding(0).Table(table =>
            {

                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Prestations
                    columns.ConstantColumn(60); // Quantité
                    columns.ConstantColumn(75); // Prix Unitaire
                    columns.ConstantColumn(55); // Remise
                    columns.ConstantColumn(75); // Montant HT
                });

                // En-tête avec style différent
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Prestations").Bold().FontSize(10);
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Quantité").Bold().FontSize(10);
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Prix Unitaire").Bold().FontSize(10);
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Remise").Bold().FontSize(10);
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Montant HT").Bold().FontSize(10);
                });

                // Contenu
                foreach (var item in quotesItems.QuotesItems)
                {
                    table.Cell().Text($"- {item.Description}").FontSize(9);
                    table.Cell().AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().AlignCenter().Text(item.Price.ToString("C")).FontSize(9);
                    //table.Cell().AlignCenter().Text($"{item.Discount} %").FontSize(9);
                    table.Cell().AlignCenter().Text($"{item.Quantity * item.Price}").FontSize(9);
                }
            });
        }

        private void DrawAdditionalInformation(IContainer container, ManagerQuotationViewModel quotationInformation)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().AlignCenter().Text("INFORMATIONS COMPLÉMENTAIRES").Bold();
                column.Item().PaddingTop(5).Text(quotationInformation.Quote.Observations).FontSize(9);
            });
        }

        private void DrawTotalAndLegalMentions(IContainer container, ManagerQuotationViewModel quotationData)
        {
            container.Column(column =>
            {
                // Section Total
                column.Item().Border(1).Padding(3).AlignRight().Width(200).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("SOUS-TOTAL HT :").Bold().FontSize(10);
                    table.Cell().AlignRight().Text(quotationData.QuotationData.HT.ToString("C")).Bold().FontSize(10);

                    table.Cell().Text($"TAUX DE T.V.A :").FontSize(10);
                    table.Cell().AlignRight().Text($"{quotationData.Edition.TVA}%").FontSize(10);

                    table.Cell().Text("T.V.A :").FontSize(10);
                    table.Cell().AlignRight().Text(quotationData.QuotationData.TVA.ToString("C")).FontSize(10);

                    table.Cell().Text("TOTAL TTC :").Bold().FontSize(10);
                    table.Cell().AlignRight().Text(quotationData.QuotationData.TTC.ToString("C")).Bold().FontSize(10);
                });

                // Mentions légales
                column.Item().PaddingTop(10).Column(legalCol =>
                {
                    legalCol.Item().Text($"SIRET : {quotationData.CompanySettings.Siret}").Bold().FontSize(9);
                    legalCol.Item().Text($"TVA : {quotationData.CompanySettings.TVA}").Bold().FontSize(9);
                    legalCol.Item().Text($"APE : {quotationData.CompanySettings.CodeNAF}").Bold().FontSize(9);
                });

                // Validité du devis
                column.Item().PaddingTop(10).AlignCenter().Text("Devis valable 30 jours à compter de la date d'édition").FontSize(9);

                // Informations complémentaires
                column.Item().PaddingTop(5).Background(Colors.Grey.Lighten3).Border(1)
                    .Padding(5).Column(infoCol =>
                    {
                        infoCol.Item().AlignCenter().Text("Ce devis est établi sous réserve d'erreurs matérielles").FontSize(9);
                        infoCol.Item().AlignCenter().Text("Les prix sont indicatifs et susceptibles de variations").FontSize(9);
                        infoCol.Item().AlignCenter().Text("Garantie pièces et main d'œuvre selon conditions constructeur").FontSize(9);
                    });
            });
        }
    }
}
