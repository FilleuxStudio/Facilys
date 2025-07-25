﻿//using Facilys.Components.Models;
//using Facilys.Components.Models.ViewModels;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;

//namespace Facilys.Components.Services
//{
//    public class PdfInvoiceType2Service
//    {
//        // Constantes pour la mise en page
//        // Constantes pour la mise en page avec des marges étroites
//        private const double PageWidth = 595; // Largeur A4 en points (inchangée)
//        private const double PageHeight = 842; // Hauteur A4 en points (inchangée)
//        private const double Margin = 20; // Marge réduite à 20 points (environ 0,7 cm)
//        private const double LineHeight = 11; // Hauteur de ligne légèrement réduite

//        // Vous pouvez également ajouter ces constantes pour plus de précision :
//        private const double TopMargin = 20; // Marge supérieure
//        private const double BottomMargin = 20; // Marge inférieure
//        private const double LeftMargin = 20; // Marge gauche
//        private const double RightMargin = 20; // Marge droite

//        public byte[] GenerateInvoicePdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
//        {
//            var document = new PdfDocument();
//            var page = document.AddPage();
//            page.Size = PdfSharp.PageSize.A4;

//            var gfx = XGraphics.FromPdfPage(page);
//            var yPosition = Margin; // Position verticale initiale

//            // Configuration des polices
//            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
//            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
//            var strongFont = new XFont("Arial", 10, XFontStyleEx.Bold);
//            var normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);

//            // Dessiner l'en-tête de la facture
//            yPosition = DrawHeader(gfx, invoice, managerInvoiceView, km, titleFont, normalFont, yPosition);

//            // Informations client
//            yPosition = DrawCustomerInfo(gfx, managerInvoiceView, phones, normalFont, yPosition);

//            // Informations véhicule
//            yPosition = DrawVehicleInfo(gfx, managerInvoiceView, normalFont, yPosition);

//            // Tableau des services
//            yPosition = DrawServiceTable(gfx, managerInvoiceView, headerFont, normalFont, yPosition);

//            // Informations a prévoir
//            yPosition = DrawRequiredInformation(gfx, managerInvoiceView, normalFont, yPosition);

//            // Total
//            yPosition = DrawTotal(gfx, managerInvoiceView, strongFont, normalFont, yPosition);

//            // mentions légales
//            yPosition = DrawLegalMentions(gfx, managerInvoiceView, headerFont, normalFont, yPosition);
//            // Sauvegarde en mémoire
//            using var stream = new MemoryStream();
//            document.Save(stream);
//            return stream.ToArray();
//        }


//        private static byte[] PictureToStream(string logo)
//        {
//            // Convertir la chaîne base64 en tableau de bytes
//            string base64Image = logo.Split(',')[1];

//            byte[] imageBytes = Convert.FromBase64String(base64Image);

//            return imageBytes;

//        }

//        private static double DrawHeader(XGraphics gfx, Invoices invoice, ManagerInvoiceViewModel company, int km, XFont titleFont, XFont normalFont, double yPosition)
//        {
//            double newHeight = 0;

//            try
//            {
//                byte[] imageBytes = PictureToStream(company.CompanySettings.Logo);
//                using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length, true, true))
//                {
//                    XImage logo = XImage.FromStream(ms);

//                    // Définir la largeur maximale souhaitée
//                    double maxWidth = 168;

//                    // Calculer le ratio d'aspect
//                    double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;

//                    // Calculer la nouvelle hauteur en conservant le ratio d'aspect
//                    newHeight = maxWidth / aspectRatio;

//                    // Dessiner l'image avec les nouvelles dimensions
//                    gfx.DrawImage(logo, Margin, yPosition, maxWidth, newHeight);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Erreur lors du chargement de l'image : {ex.Message}");
//                // Gérer l'erreur ou la logger
//            }

//            yPosition += newHeight + LineHeight;

//            // Title
//            XRect rect = new(LeftMargin, yPosition, PageWidth - LeftMargin - RightMargin, LineHeight);
//            gfx.DrawString("FACTURE", titleFont, XBrushes.Black, rect, XStringFormats.TopRight);

//            yPosition += LineHeight;
//            double initialYPosition = yPosition;

//            // Bloc de gauche : Date et numéro de facture
//            string dateString = $"Date : {invoice.DateAdded:dd-MM-yyyy}";
//            gfx.DrawString(dateString, normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;

//            string invoiceNumberString = $"N° Facture : {invoice.InvoiceNumber}";
//            gfx.DrawString(invoiceNumberString, normalFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;

//            if (invoice.Vehicle != null)
//            {
//                string kmString = $"Km : {km}";
//                gfx.DrawString(kmString, normalFont, XBrushes.Black, Margin, yPosition);
//            }

//            // Réinitialiser yPosition pour le bloc de droite
//            yPosition = initialYPosition;

//            // Bloc de droite : Adresse du garage
//            gfx.DrawString(company.CompanySettings.HeadOfficeAddress.ToUpper(), normalFont, XBrushes.Black,
//                PageWidth - RightMargin - gfx.MeasureString(company.CompanySettings.HeadOfficeAddress.ToUpper(), normalFont).Width, yPosition);
//            yPosition += LineHeight;

//            gfx.DrawString("Téléphone : " + company.CompanySettings.Phone, normalFont, XBrushes.Black,
//                PageWidth - RightMargin - gfx.MeasureString("Téléphone : " + company.CompanySettings.Phone, normalFont).Width, yPosition);
//            yPosition += LineHeight;

//            gfx.DrawString("Email : " + company.CompanySettings.Email, normalFont, XBrushes.Black,
//                PageWidth - RightMargin - gfx.MeasureString("Email : " + company.CompanySettings.Email, normalFont).Width, yPosition);
//            yPosition += LineHeight;

//            if (!string.IsNullOrEmpty(company.CompanySettings.WebSite))
//            {
//                gfx.DrawString(company.CompanySettings.WebSite, normalFont, XBrushes.Black,
//                    PageWidth - RightMargin - gfx.MeasureString(company.CompanySettings.WebSite, normalFont).Width, yPosition);
//                yPosition += LineHeight;
//            }

//            yPosition += LineHeight;

//            return yPosition;
//        }


//        private static double DrawCustomerInfo(XGraphics gfx, ManagerInvoiceViewModel client, PhonesClients phones, XFont normalFont, double yPosition)
//        {
//            // Sauvegardez la position verticale initiale
//            double initialYPosition = yPosition;

//            // Calculez la largeur du rectangle (moitié de la largeur disponible)
//            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
//            double rectHeight = LineHeight * 7; // Ajustez selon le nombre de lignes

//            // Créez le rectangle
//            XRect rect = new(LeftMargin, yPosition, rectWidth, rectHeight);

//            // Dessinez le rectangle avec une bordure noire de 2px
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            // Centrez le titre "CLIENT"
//            XRect titleRect = new(rect.Left, yPosition, rectWidth, LineHeight);
//            gfx.DrawString("CLIENT", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
//            yPosition += LineHeight * 1.5; // Espace après le titre

//            // Dessinez les informations du client
//            string[][] clientInfo =
//[
//    ["Nom:", client.Client.Lname],
//    ["Prénom:", client.Client.Fname],
//    ["Rue:", client.Client.Address],
//    ["Code postal:", client.Client.PostalCode],
//    ["Ville:", client.Client.City],
//    ["Téléphone:", phones.Phone]
//];

//            // Trouvez la largeur maximale des labels
//            double maxLabelWidth = 0;
//            foreach (var info in clientInfo)
//            {
//                XSize size = gfx.MeasureString(info[0], normalFont);
//                if (size.Width > maxLabelWidth)
//                    maxLabelWidth = size.Width;
//            }

//            // Ajoutez un petit espace après le label le plus long
//            maxLabelWidth += 5;

//            // Dessinez les informations alignées
//            double textMargin = Margin + 5; // Marge intérieure pour le texte
//            foreach (var info in clientInfo)
//            {
//                gfx.DrawString(info[0], normalFont, XBrushes.Black, textMargin, yPosition);
//                gfx.DrawString(info[1], normalFont, XBrushes.Black, textMargin + maxLabelWidth, yPosition);
//                yPosition += LineHeight;
//            }

//            // Mettez à jour yPosition pour le prochain élément après le rectangle
//            yPosition = initialYPosition;

//            return yPosition;
//        }

//        private static double DrawVehicleInfo(XGraphics gfx, ManagerInvoiceViewModel vehicle, XFont normalFont, double yPosition)
//        {
//            // Sauvegardez la position verticale initiale
//            double initialYPosition = yPosition;

//            // Calculez la largeur du rectangle (moitié de la largeur disponible)
//            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
//            double rectHeight = LineHeight * 7; // Ajustez selon le nombre de lignes

//            // Créez le rectangle pour le véhicule (à droite)
//            XRect rect = new(PageWidth - RightMargin - rectWidth, yPosition, rectWidth, rectHeight);

//            // Dessinez le rectangle avec une bordure noire de 1px
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            // Centrez le titre "VEHICULE"
//            XRect titleRect = new(rect.Left, yPosition, rectWidth, LineHeight);
//            gfx.DrawString("VEHICULE", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
//            yPosition += LineHeight * 1.5; // Espace après le titre

//            // Préparez les informations du véhicule
//            string[][] vehicleInfo;

//            if (vehicle.Vehicle != null)
//            {
//                vehicleInfo = [
//                ["Marque:", vehicle.Vehicle.Mark],
//        ["Modèle:", vehicle.Vehicle.Model],
//        ["Immatriculation:", vehicle.Vehicle.Immatriculation],
//        ["VIN:", vehicle.Vehicle.VIN],
//        ["Type:", vehicle.Vehicle.Type],
//        ["Mise en circulation:", vehicle.Vehicle.CirculationDate.ToString("dd/MM/yyyy")]
//            ];
//            }
//            else
//            {
//                vehicleInfo = [
//                ["Marque:", vehicle.OtherVehicle.Mark],
//        ["Modèle:", vehicle.OtherVehicle.Model],
//        ["Numéro :", vehicle.OtherVehicle.SerialNumber],
//        ["Type:", vehicle.OtherVehicle.Type],
//            ];
//            }

//            // Trouvez la largeur maximale des labels
//            double maxLabelWidth = 0;
//            foreach (var info in vehicleInfo)
//            {
//                XSize size = gfx.MeasureString(info[0], normalFont);
//                if (size.Width > maxLabelWidth)
//                    maxLabelWidth = size.Width;
//            }

//            // Ajoutez un petit espace après le label le plus long
//            maxLabelWidth += 5;

//            // Dessinez les informations alignées
//            double textMargin = rect.Left + 5; // Marge intérieure pour le texte
//            foreach (var info in vehicleInfo)
//            {
//                gfx.DrawString(info[0], normalFont, XBrushes.Black, textMargin, yPosition);
//                gfx.DrawString(info[1], normalFont, XBrushes.Black, textMargin + maxLabelWidth, yPosition);
//                yPosition += LineHeight;
//            }

//            // Mettez à jour yPosition pour le prochain élément après le rectangle
//            return initialYPosition + rectHeight;
//        }

//        private static double DrawServiceTable(XGraphics gfx, ManagerInvoiceViewModel historyParts, XFont strongFont, XFont normalFont, double yPosition)
//        {
//            // Définir les largeurs de colonnes (ajustées pour tenir dans la page)
//            double[] columnWidths = [295, 55, 75, 55, 75];
//            double tableWidth = columnWidths.Sum();
//            double tableStartX = Margin;
//            double tableStartY = yPosition;
//            double cellPadding = 5; // Espace à l'intérieur des cellules

//            // Créer un stylo pour les lignes
//            XPen tablePen = new(XColors.Black, 1);

//            // Dessiner l'en-tête du tableau
//            string[] headers = ["Description", "Quantité", "Prix Unitaire", "Remise", "Montant HT"];
//            for (int i = 0; i < headers.Length; i++)
//            {
//                double columnX = tableStartX + columnWidths.Take(i).Sum();
//                XRect headerRect = new(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);

//                // Dessiner la cellule de l'en-tête (avec toutes les bordures)
//                gfx.DrawRectangle(tablePen, headerRect);

//                // Centrer le texte dans la cellule
//                gfx.DrawString(headers[i], strongFont, XBrushes.Black, headerRect, XStringFormats.Center);
//            }
//            yPosition += LineHeight + cellPadding * 2;

//            // Dessiner les lignes verticales du tableau
//            for (int i = 0; i <= headers.Length; i++)
//            {
//                double x = tableStartX + columnWidths.Take(i).Sum();
//                gfx.DrawLine(tablePen, x, tableStartY, x, yPosition);
//            }

//            // Dessiner les lignes du tableau
//            foreach (var item in historyParts.HistoryParts)
//            {
//                string[] rowData = ["- " + item.Description, item.Quantity.ToString(), item.Price.ToString("C"), item.Discount.ToString() + " %", ((item.Quantity * item.Price) * (1 - item.Discount / 100)).ToString("C")];

//                double rowStartY = yPosition;

//                for (int i = 0; i < rowData.Length; i++)
//                {
//                    double columnX = tableStartX + columnWidths.Take(i).Sum();
//                    XRect cellRect = new(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);

//                    // Aligner le texte à gauche dans la cellule (sauf pour les nombres)
//                    XStringFormat format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.TopRight;
//                    gfx.DrawString(rowData[i], normalFont, XBrushes.Black,
//                        new XRect(cellRect.X + cellPadding, cellRect.Y + cellPadding,
//                                  cellRect.Width - cellPadding * 2, cellRect.Height - cellPadding * 2),
//                        format);
//                }
//                yPosition += LineHeight + cellPadding * 2;

//                // Redessiner les lignes verticales pour cette rangée
//                for (int i = 0; i <= headers.Length; i++)
//                {
//                    double x = tableStartX + columnWidths.Take(i).Sum();
//                    gfx.DrawLine(tablePen, x, rowStartY, x, yPosition);
//                }
//            }

//            // Dessiner la ligne du bas du tableau
//            gfx.DrawLine(tablePen, tableStartX, yPosition, tableStartX + tableWidth, yPosition);

//            //yPosition += LineHeight;

//            return yPosition;

//        }

//        private static double DrawRequiredInformation(XGraphics gfx, ManagerInvoiceViewModel invoiceInformation, XFont normalFont, double yPosition)
//        {
//            // Sauvegardez la position verticale initiale
//            double initialYPosition = yPosition;

//            // Calculez la largeur du rectangle (toute la largeur disponible)
//            double rectWidth = PageWidth - LeftMargin - RightMargin;
//            double rectHeight = LineHeight * 4; // Ajustez selon le nombre de lignes

//            // Créez le rectangle pour l'information
//            XRect rect = new(LeftMargin, yPosition, rectWidth, rectHeight);

//            // Dessinez le rectangle avec une bordure noire de 1px
//            XPen pen = new(XColors.Black, 1);
//            gfx.DrawRectangle(pen, rect);

//            // Centrez le titre "INTERVENTION A PREVOIR"
//            XRect titleRect = new(rect.X, rect.Y, rect.Width, LineHeight);
//            gfx.DrawString("INTERVENTION A PREVOIR", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);

//            // Dessinez le texte à l'intérieur du rectangle
//            double textY = rect.Y + LineHeight + 5; // Espace après le titre
//            string text = invoiceInformation.InvoiceData.GeneralConditionInvoice;
//            gfx.DrawString(text, normalFont, XBrushes.Black, new XRect(rect.X + 5, textY, rect.Width - 10, rect.Height - LineHeight - 10), XStringFormats.TopLeft);

//            // Mettez à jour yPosition pour le prochain élément
//            return yPosition = initialYPosition + rectHeight;

//        }

//        private static double DrawTotal(XGraphics gfx, ManagerInvoiceViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
//        {
//            // Définir les dimensions et la position du rectangle
//            double rectWidth = 190; // Ajustez selon vos besoins
//            double rectHeight = LineHeight * 8; // 4 lignes
//            double rectX = PageWidth - RightMargin - rectWidth;
//            double rectY = yPosition;

//            // Créer le rectangle principal
//            XRect mainRect = new(rectX, rectY, rectWidth, rectHeight);
//            gfx.DrawRectangle(new XPen(XColors.Black, 1), mainRect);

//            // Dessiner les lignes et le texte
//            string[] labels = ["SOUS-TOTAL HT :", "TAUX DE T.V.A :", "T.V.A :", "TOTAL TTC :"];
//            string[] values = [
//    invoiceData.InvoiceData.HT.ToString("C"),
//    $"{invoiceData.Edition.TVA}%",
//    invoiceData.InvoiceData.TVA.ToString("C"),
//    invoiceData.InvoiceData.TTC.ToString("C")
//];

//            for (int i = 0; i < 4; i++)
//            {
//                double lineY = rectY + i * LineHeight;

//                // Dessiner la ligne de séparation (sauf pour la dernière ligne)
//                if (i < 3)
//                {
//                    gfx.DrawLine(new XPen(XColors.Black, 1), rectX, lineY + LineHeight * (2 + i), rectX + rectWidth, lineY + LineHeight * (2 + i));
//                }

//                // Dessiner le texte
//                XRect textRect = new(rectX + 5, lineY, rectWidth - 10, LineHeight * ((2 + i) + i));
//                gfx.DrawString(labels[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterLeft);
//                gfx.DrawString(values[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterRight);
//            }

//            // Mettre à jour yPosition pour le prochain élément
//            yPosition = rectY + rectHeight + LineHeight;

//            // Mentions légales
//            gfx.DrawString("SIRET : " + invoiceData.CompanySettings.Siret, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("TVA : " + invoiceData.CompanySettings.TVA, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight;
//            gfx.DrawString("APE : " + invoiceData.CompanySettings.CodeNAF, headerFont, XBrushes.Black, Margin, yPosition);
//            yPosition += LineHeight * 2;

//            return yPosition;
//        }

//        private static double DrawLegalMentions(XGraphics gfx, ManagerInvoiceViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
//        {
//            // Calcul de la position de départ depuis le bas de page
//            double startY = PageHeight - BottomMargin - (LineHeight * 6);

//            // 1. Première phrase centrée
//            XStringFormat centerFormat = new()
//            {
//                Alignment = XStringAlignment.Center,
//                LineAlignment = XLineAlignment.Near
//            };

//            XRect firstTextRect = new(LeftMargin, startY, PageWidth - 2 * LeftMargin, LineHeight);
//            gfx.DrawString(invoiceData.Edition.SentenceInformationBottom, normalFont, XBrushes.Black, firstTextRect, centerFormat);
//            startY += LineHeight;

//            // 2. Rectangle rouge avec texte
//            const double padding = 5;
//            double redBoxHeight = 3 * LineHeight + padding * 2;

//            // Positionnement du rectangle
//            XRect redRect = new(
//                LeftMargin,
//                startY,
//                PageWidth - 2 * LeftMargin,
//                redBoxHeight
//            );

//            gfx.DrawRectangle(XBrushes.IndianRed, redRect);

//            // Texte dans le rectangle
//            string[] redBoxLines = invoiceData.Edition.SentenceBottom.Split('.');

//            XStringFormat redBoxFormat = new()
//            {
//                Alignment = XStringAlignment.Center,
//                LineAlignment = XLineAlignment.Center
//            };

//            double textY = startY + padding;
//            foreach (string line in redBoxLines)
//            {
//                XRect lineRect = new(LeftMargin, textY, PageWidth - 2 * LeftMargin, LineHeight);
//                gfx.DrawString(line, normalFont, XBrushes.Black, lineRect, redBoxFormat);
//                textY += LineHeight;
//            }

//            startY += redBoxHeight + LineHeight;

//            // 3. Message de remerciement en bas de page
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

//            gfx.DrawString("MERCI DE VOTRE CONFIANCE !",
//                          headerFont,
//                          XBrushes.Black,
//                          footerRect,
//                          footerFormat);

//            // Retourne la nouvelle position Y maximale
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
    public class PdfInvoiceType2Service
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

                    // En-tête avec positionnement spécifique
                    page.Header().Element(comp => DrawHeader(comp, invoice, managerInvoiceView, km));

                    // Contenu principal
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // Section client/véhicule côte à côte
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(comp => DrawCustomerInfo(comp, managerInvoiceView, phones));
                            row.RelativeItem().Element(comp => DrawVehicleInfo(comp, managerInvoiceView));
                        });

                        // Tableau des services avec bordures
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
                    row.RelativeItem().AlignLeft().Width(168).Column(col =>
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

                    // Bloc de droite aligné à droite
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

                // Deuxième ligne avec coordonnées société à droite
                column.Item().Row(row =>
                {
                    row.RelativeItem(); // Espace vide à gauche

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text(company.CompanySettings.HeadOfficeAddress.ToUpper()).FontSize(10);
                        col.Item().Text($"Téléphone : {company.CompanySettings.Phone}").FontSize(10);
                        col.Item().Text($"Email : {company.CompanySettings.Email}").FontSize(10);

                        if (!string.IsNullOrEmpty(company.CompanySettings.WebSite))
                        {
                            col.Item().Text(company.CompanySettings.WebSite).FontSize(10);
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
            container.Border(1).PaddingVertical(3).PaddingHorizontal(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Description
                    columns.ConstantColumn(55); // Quantité
                    columns.ConstantColumn(75); // Prix Unitaire
                    columns.ConstantColumn(55); // Remise
                    columns.ConstantColumn(75); // Montant HT
                });

                // En-tête avec style différent
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3)
                          .Text("Description").Bold().FontSize(10);
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
                foreach (var item in historyParts.HistoryParts)
                {
                    table.Cell().Text($"- {item.Description}").FontSize(9);
                    table.Cell().AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().AlignCenter().Text(item.Price.ToString("C")).FontSize(9);
                    table.Cell().AlignCenter().Text($"{item.Discount} %").FontSize(9);
                    table.Cell().AlignCenter().Text($"{item.Quantity * item.Price * (1 - item.Discount / 100):C}").FontSize(9);
                }
            });
        }

        private void DrawRequiredInformation(IContainer container, ManagerInvoiceViewModel invoiceInformation)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                column.Item().AlignCenter().Text("INTERVENTION A PREVOIR").Bold();
                column.Item().PaddingTop(5).Text(invoiceInformation.InvoiceData.GeneralConditionInvoice).FontSize(9);
            });
        }

        private void DrawTotalAndLegalMentions(IContainer container, ManagerInvoiceViewModel invoiceData)
        {
            container.Column(column =>
            {
                // Section Total avec bordures
                column.Item().AlignRight().Width(190).Border(1).Padding(5).Column(totalCol =>
                {
                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("SOUS-TOTAL HT :").Bold().FontSize(10);
                        row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.HT.ToString("C")).Bold().FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"TAUX DE T.V.A :").FontSize(10);
                        row.RelativeItem().AlignRight().Text($"{invoiceData.Edition.TVA}%").FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("T.V.A :").FontSize(10);
                        row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.TVA.ToString("C")).FontSize(10);
                    });

                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL TTC :").Bold().FontSize(10);
                        row.RelativeItem().AlignRight().Text(invoiceData.InvoiceData.TTC.ToString("C")).Bold().FontSize(10);
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

                // Rectangle rouge avec texte important
                if (!string.IsNullOrEmpty(invoiceData.Edition.SentenceBottom))
                {
                    column.Item().PaddingTop(5).Background(Colors.Red.Lighten1).Padding(5)
                        .AlignCenter().Text(invoiceData.Edition.SentenceBottom).FontSize(10);
                }
            });
        }
    }
}