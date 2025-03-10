using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Facilys.Components.Services
{
    public class PdfRepairOrderService
    {
        // Constantes pour la mise en page
        // Constantes pour la mise en page avec des marges étroites
        private const double PageWidth = 595; // Largeur A4 en points (inchangée)
        private const double PageHeight = 842; // Hauteur A4 en points (inchangée)
        private const double Margin = 20; // Marge réduite à 20 points (environ 0,7 cm)
        private const double LineHeight = 11; // Hauteur de ligne légèrement réduite

        // Vous pouvez également ajouter ces constantes pour plus de précision :
        private const double TopMargin = 20; // Marge supérieure
        private const double BottomMargin = 20; // Marge inférieure
        private const double LeftMargin = 20; // Marge gauche
        private const double RightMargin = 20; // Marge droite

        public byte[] GenerateRepairOrderPdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            var gfx = XGraphics.FromPdfPage(page);
            var yPosition = Margin; // Position verticale initiale

            // Configuration des polices
            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            var strongFont = new XFont("Arial", 10, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 9, XFontStyleEx.Regular);

            // Dessiner l'en-tête de la facture
            yPosition = DrawHeader(gfx, invoice, managerInvoiceView, titleFont, headerFont, yPosition);

            // Informations
            yPosition = DrawClientAndVehicleInfo(gfx, managerInvoiceView, km, phones, emails, normalFont, yPosition);

            // Tableau des services
            yPosition = DrawServiceTable(gfx, managerInvoiceView, headerFont, normalFont, yPosition);

            // Informations a prévoir
            yPosition = DrawRequiredInformation(gfx, managerInvoiceView, normalFont, yPosition);

            // Total
            yPosition = DrawTotal(gfx, managerInvoiceView, strongFont, normalFont, yPosition);

            // mentions légales
            yPosition = DrawLegalMentions(gfx, managerInvoiceView, headerFont, normalFont, yPosition);
            // Sauvegarde en mémoire
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private byte[] PictureToStream(string logo)
        {
            // Convertir la chaîne base64 en tableau de bytes
            string base64Image = logo.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            return imageBytes;

        }

        private double DrawHeader(XGraphics gfx, Invoices invoice, ManagerInvoiceViewModel company, XFont titleFont, XFont headerFont, double yPosition)
        {
            double newHeight = 0;
            try
            {
                byte[] imageBytes = PictureToStream(company.CompanySettings.Logo);
                using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length, true, true))
                {
                    XImage logo = XImage.FromStream(ms);

                    // Définir la largeur maximale souhaitée
                    double maxWidth = 168;

                    // Calculer le ratio d'aspect
                    double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;

                    // Calculer la nouvelle hauteur en conservant le ratio d'aspect
                    newHeight = maxWidth / aspectRatio;

                    // Dessiner l'image avec les nouvelles dimensions
                    gfx.DrawImage(logo, Margin, yPosition, maxWidth, newHeight);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'image : {ex.Message}");
                // Gérer l'erreur ou la logger
            }
            yPosition += LineHeight;
            //Title
            XRect rect = new XRect(LeftMargin, yPosition, PageWidth - LeftMargin - RightMargin, LineHeight);
            gfx.DrawString("ORDRE DE REPARATION", titleFont, XBrushes.Black, rect, XStringFormats.TopRight);

            yPosition += newHeight + LineHeight;
            double initialYPosition = yPosition;
            // Téléphone et email
            gfx.DrawString("SIRET : " + company.CompanySettings.Siret, headerFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("APE : " + company.CompanySettings.CodeNAF, headerFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("TVA : " + company.CompanySettings.TVA, headerFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            // Réinitialiser yPosition pour les informations de droite
            yPosition = initialYPosition;

            // Date et numéro de facture (à droite)
            string dateString = $"Date : {invoice.DateAdded:dd-MM-yyyy}";
            XSize dateSize = gfx.MeasureString(dateString, headerFont);
            gfx.DrawString(dateString, headerFont, XBrushes.Black, PageWidth - RightMargin - dateSize.Width, yPosition);
            yPosition += LineHeight;

            string invoiceNumberString = $"N° : {invoice.InvoiceNumber}";
            XSize invoiceNumberSize = gfx.MeasureString(invoiceNumberString, headerFont);
            gfx.DrawString(invoiceNumberString, headerFont, XBrushes.Black, PageWidth - RightMargin - invoiceNumberSize.Width, yPosition);

            yPosition += LineHeight * 2;

            return yPosition;
        }

        private double DrawClientAndVehicleInfo(XGraphics gfx, ManagerInvoiceViewModel dataInvoice, int km, PhonesClients phones, EmailsClients emails, XFont normalFont, double yPosition)
        {
            // Définir les dimensions du tableau
            int rows = 8;
            int cols = 4;
            double tableWidth = PageWidth - LeftMargin - RightMargin;
            double[] columnWidths = { tableWidth * 0.18, tableWidth * 0.32, tableWidth * 0.12, tableWidth * 0.38 };
            double cellHeight = LineHeight * 1.5;
            double tableHeight = cellHeight * rows;

            // Créer un stylo pour les bordures
            XPen pen = new XPen(XColors.Black, 1);

            // Dessiner les lignes horizontales
            for (int i = 0; i <= rows; i++)
            {
                gfx.DrawLine(pen, LeftMargin, yPosition + i * cellHeight, LeftMargin + tableWidth, yPosition + i * cellHeight);
            }

            // Dessiner les lignes verticales
            double xPos = LeftMargin;
            for (int j = 0; j <= cols; j++)
            {
                gfx.DrawLine(pen, xPos, yPosition, xPos, yPosition + tableHeight);
                if (j < cols) xPos += columnWidths[j];
            }

            // Préparer les données
            string[][] data = new string[rows][];
            data[0] = new[] { " Nom : ", " " + dataInvoice.Client.Lname.ToUpper(), " Adresse : ", " " + dataInvoice.Client.Address.ToUpper() };
            data[1] = new[] { " Prénom : ", " " + dataInvoice.Client.Fname.ToUpper(), " Ville : ", " " + dataInvoice.Client.City.ToUpper() };
            data[2] = new[] { " Téléphone : ", " " + phones.Phone, " Code Postal : ", " " + dataInvoice.Client.PostalCode };
            data[3] = new[] { " Email : ", " " + emails.Email ?? "", " -", " -" };
            data[4] = new[] { " Marque : ", " " + dataInvoice.Vehicle.Mark, " Modèle : ", " " + dataInvoice.Vehicle.Model };
            data[5] = new[] { " Mise en circulation : ", " " + dataInvoice.Vehicle.CirculationDate.ToString("dd/MM/yyyy"), " Type : ", " " + dataInvoice.Vehicle.Type };
            data[6] = new[] { " Immatriculation : ", " " + dataInvoice.Vehicle.Immatriculation, " N° série : ", " " + dataInvoice.Vehicle.VIN };
            data[7] = new[] { " Kilométrage : ", " " + km.ToString(), " -", " -" };

            // Remplir le tableau avec les données
            double cellX = LeftMargin;
            for (int i = 0; i < rows; i++)
            {
                cellX = LeftMargin;
                for (int j = 0; j < cols; j++)
                {
                    XRect cellRect = new XRect(cellX, yPosition + i * cellHeight, columnWidths[j], cellHeight);
                    gfx.DrawString(data[i][j], normalFont, XBrushes.Black, cellRect, XStringFormats.CenterLeft);
                    cellX += columnWidths[j];
                }
            }

            return yPosition + tableHeight;
        }

        private double DrawServiceTable(XGraphics gfx, ManagerInvoiceViewModel historyParts, XFont strongFont, XFont normalFont, double yPosition)
        {
            // Définir les largeurs de colonnes (ajustées pour tenir dans la page)
            double[] columnWidths = { 75, 270, 30, 60, 50, 70 };
            double tableWidth = columnWidths.Sum();
            double tableStartX = Margin;
            double tableStartY = yPosition;
            double cellPadding = 5; // Espace à l'intérieur des cellules

            // Créer un stylo pour les lignes
            XPen tablePen = new XPen(XColors.Black, 1);

            // Dessiner l'en-tête du tableau
            string[] headers = { "Référence", "Description", "Qté", "Prix Unit", "Remise", "Montant HT" };
            for (int i = 0; i < headers.Length; i++)
            {
                double columnX = tableStartX + columnWidths.Take(i).Sum();
                XRect headerRect = new XRect(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);

                // Dessiner la cellule de l'en-tête (avec toutes les bordures)
                gfx.DrawRectangle(tablePen, headerRect);

                // Centrer le texte dans la cellule
                gfx.DrawString(headers[i], strongFont, XBrushes.Black, headerRect, XStringFormats.Center);
            }
            yPosition += LineHeight + cellPadding * 2;

            // Dessiner les lignes verticales du tableau
            for (int i = 0; i <= headers.Length; i++)
            {
                double x = tableStartX + columnWidths.Take(i).Sum();
                gfx.DrawLine(tablePen, x, tableStartY, x, yPosition);
            }

            // Dessiner les lignes du tableau
            foreach (var item in historyParts.HistoryParts)
            {
                string[] rowData = {
    item.PartNumber, // Colonne Référence ajoutée
    "- " + item.Description,
    item.Quantity.ToString(),
    item.Price.ToString("C"),
    item.Discount.ToString() + " %",
    ((item.Quantity * item.Price) * (1 - item.Discount / 100)).ToString("C")
};

                double rowStartY = yPosition;

                for (int i = 0; i < rowData.Length; i++)
                {
                    double columnX = tableStartX + columnWidths.Take(i).Sum();
                    XRect cellRect = new XRect(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);

                    // Aligner le texte à gauche dans la cellule (sauf pour les nombres)
                    XStringFormat format = i <= 1 ? XStringFormats.CenterLeft : XStringFormats.TopRight;
                    gfx.DrawString(rowData[i], normalFont, XBrushes.Black,
                        new XRect(cellRect.X + cellPadding, cellRect.Y + cellPadding,
                                  cellRect.Width - cellPadding * 2, cellRect.Height - cellPadding * 2),
                        format);
                }
                yPosition += LineHeight + cellPadding * 2;

                // Redessiner les lignes verticales pour cette rangée
                for (int i = 0; i <= headers.Length; i++)
                {
                    double x = tableStartX + columnWidths.Take(i).Sum();
                    gfx.DrawLine(tablePen, x, rowStartY, x, yPosition);
                }
            }

            // Dessiner la ligne du bas du tableau
            gfx.DrawLine(tablePen, tableStartX, yPosition, tableStartX + tableWidth, yPosition);

            //yPosition += LineHeight;

            return yPosition;

        }

        private double DrawRequiredInformation(XGraphics gfx, ManagerInvoiceViewModel invoiceInformation, XFont normalFont, double yPosition)
        {
            // Largeur combinée des colonnes Référence (50) + Description (260)
            const double combinedWidth = 50 + 260;
            double rectHeight = LineHeight * 4;

            // Positionnement aligné à gauche avec le tableau
            XRect rect = new XRect(LeftMargin, yPosition, combinedWidth, rectHeight);

            // Style de la bordure
            XPen pen = new XPen(XColors.Black, 1);
            gfx.DrawRectangle(pen, rect);

            // Titre centré
            XRect titleRect = new XRect(rect.X, rect.Y, rect.Width, LineHeight);
            gfx.DrawString("INFORMATION COMPLEMENTAIRE", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);

            // Texte avec gestion du wrapping
            string text = invoiceInformation.InvoiceData.GeneralConditionOrder;
            XRect textRect = new XRect(
                rect.X + 5,
                rect.Y + LineHeight + 5,
                rect.Width - 10,
                rect.Height - LineHeight - 10
            );

            PdfHelper.DrawWrappedText(gfx, text, normalFont, textRect, XStringFormats.TopLeft);

            //return yPosition + rectHeight;

            return yPosition;
        }

        private double DrawTotal(XGraphics gfx, ManagerInvoiceViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
        {
            double rectWidth = 190;
            double rectHeight = LineHeight * 6; // Réduit à 6 lignes
            double rectX = PageWidth - RightMargin - rectWidth;
            double rectY = yPosition;

            XRect mainRect = new XRect(rectX, rectY, rectWidth, rectHeight);
            gfx.DrawRectangle(new XPen(XColors.Black, 1), mainRect);

            string[] labels = { "SOUS-TOTAL HT :", "TAUX DE T.V.A :", "TOTAL TTC :" };
            string[] values = {
        invoiceData.InvoiceData.HT.ToString("C"),
        $"{invoiceData.Edition.TVA}%",
        invoiceData.InvoiceData.TTC.ToString("C")
    };

            for (int i = 0; i < 3; i++)
            {
                double lineY = rectY + i * LineHeight * 2;

                if (i < 2)
                {
                    gfx.DrawLine(new XPen(XColors.Black, 1), rectX, lineY + LineHeight * 2, rectX + rectWidth, lineY + LineHeight * 2);
                }

                XRect textRect = new XRect(rectX + 5, lineY, rectWidth - 10, LineHeight * 2);

                if (i == 1) // Cellule spéciale pour TVA
                {
                    // Dessiner le trait vertical
                    double middleX = rectX + rectWidth / 2;
                    gfx.DrawLine(new XPen(XColors.Black, 1), middleX, lineY, middleX, lineY + LineHeight * 2);

                    // TAUX DE T.V.A
                    gfx.DrawString("TAUX DE T.V.A :", normalFont, XBrushes.Black,
                        new XRect(rectX + 5, lineY, rectWidth / 2 - 10, LineHeight * 2), XStringFormats.CenterLeft);
                    gfx.DrawString(values[i], normalFont, XBrushes.Black,
                        new XRect(rectX + 5, lineY, rectWidth / 2 - 10, LineHeight * 2), XStringFormats.CenterRight);

                    // T.V.A
                    gfx.DrawString("T.V.A :", normalFont, XBrushes.Black,
                        new XRect(middleX + 5, lineY, rectWidth / 2 - 10, LineHeight * 2), XStringFormats.CenterLeft);
                    gfx.DrawString(invoiceData.InvoiceData.TVA.ToString("C"), normalFont, XBrushes.Black,
                        new XRect(middleX + 5, lineY, rectWidth / 2 - 10, LineHeight * 2), XStringFormats.CenterRight);
                }
                else
                {
                    gfx.DrawString(labels[i], i == 0 || i == 2 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterLeft);
                    gfx.DrawString(values[i], i == 0 || i == 2 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterRight);
                }
            }

            yPosition = rectY + rectHeight;

            //// Ajout des cases à cocher
            //double checkboxSize = 10;
            //double textMargin = 5;

            //// Première case à cocher
            //XRect checkbox1 = new XRect(Margin, yPosition, checkboxSize, checkboxSize);
            //gfx.DrawRectangle(new XPen(XColors.Black, 1), checkbox1);
            //gfx.DrawString("Pièces remplacées à remettre au client", normalFont, XBrushes.Black,
            //    new XPoint(Margin + checkboxSize + textMargin, yPosition + checkboxSize));

            //yPosition += LineHeight * 1.5;

            //// Deuxième case à cocher
            //XRect checkbox2 = new XRect(Margin, yPosition, checkboxSize, checkboxSize);
            //gfx.DrawRectangle(new XPen(XColors.Black, 1), checkbox2);
            //gfx.DrawString("Pièces fournies par le client", normalFont, XBrushes.Black,
            //    new XPoint(Margin + checkboxSize + textMargin, yPosition + checkboxSize));

            ////yPosition += LineHeight * 2;
            ///
            yPosition = DrawCheckbox(gfx, (yPosition - 15), invoiceData.InvoiceData.PartReturnedCustomer, invoiceData.InvoiceData.CustomerSuppliedPart, normalFont);

            return yPosition;
        }

        private double DrawCheckbox(XGraphics gfx, double yPosition, bool isChecked1, bool isChecked2, XFont normalFont)
        {
            double checkboxSize = 10; // Taille de la case à cocher
            double textMargin = 5;    // Marge entre la case et le texte

            // Première case à cocher
            XRect checkbox1 = new XRect(Margin, yPosition, checkboxSize, checkboxSize);
            gfx.DrawRectangle(new XPen(XColors.Black, 1), checkbox1);

            if (isChecked1) // Si le booléen est vrai, dessiner une croix
            {
                gfx.DrawLine(new XPen(XColors.Black, 1),
                    checkbox1.X + 2, checkbox1.Y + 2, checkbox1.X + checkboxSize - 2, checkbox1.Y + checkboxSize - 2);
                gfx.DrawLine(new XPen(XColors.Black, 1),
                    checkbox1.X + 2, checkbox1.Y + checkboxSize - 2, checkbox1.X + checkboxSize - 2, checkbox1.Y + 2);
            }

            gfx.DrawString("Pièces remplacées à remettre au client", normalFont, XBrushes.Black,
                new XPoint(Margin + checkboxSize + textMargin, yPosition + checkboxSize));

            yPosition += LineHeight * 1.5;

            // Deuxième case à cocher
            XRect checkbox2 = new XRect(Margin, yPosition, checkboxSize, checkboxSize);
            gfx.DrawRectangle(new XPen(XColors.Black, 1), checkbox2);

            if (isChecked2) // Si le booléen est vrai, dessiner une croix
            {
                gfx.DrawLine(new XPen(XColors.Black, 1),
                    checkbox2.X + 2, checkbox2.Y + 2, checkbox2.X + checkboxSize - 2, checkbox2.Y + checkboxSize - 2);
                gfx.DrawLine(new XPen(XColors.Black, 1),
                    checkbox2.X + 2, checkbox2.Y + checkboxSize - 2, checkbox2.X + checkboxSize - 2, checkbox2.Y + 2);
            }

            gfx.DrawString("Pièces fournies par le client", normalFont, XBrushes.Black,
                new XPoint(Margin + checkboxSize + textMargin, yPosition + checkboxSize));

            yPosition += LineHeight * 1.5;

            return yPosition;
        }


        private double DrawLegalMentions(XGraphics gfx, ManagerInvoiceViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
        {
            // Calcul de la position de départ depuis le bas de page
            double startY = PageHeight - BottomMargin - (LineHeight * 11); // Ajusté pour plus d'espace

            // 1. Première phrase centrée
            XStringFormat centerFormat = new XStringFormat
            {
                Alignment = XStringAlignment.Center,
                LineAlignment = XLineAlignment.Near
            };

            // Texte dans le rectangle
            string[] texte = invoiceData.Edition.RepairOrderSentenceTop.Split('\n');

            startY += LineHeight * 1.5;
            foreach (string line in texte)
            {
                XRect firstTextRect = new XRect(LeftMargin, startY, PageWidth - 2 * LeftMargin, LineHeight);
                gfx.DrawString(line, normalFont, XBrushes.Black, firstTextRect, centerFormat);
                startY += LineHeight;
            }

            // 2. Rectangle jaune avec texte (une seule ligne)
            const double padding = 5;
            double yellowBoxHeight = LineHeight + padding;

            XRect yellowRect = new XRect(
                LeftMargin,
                startY,
                PageWidth - 2 * LeftMargin,
                yellowBoxHeight
            );

            // Remplir en jaune
            gfx.DrawRectangle(XBrushes.LightYellow, yellowRect);
            // Dessiner la bordure noire
            gfx.DrawRectangle(new XPen(XColors.Black, 1), yellowRect);

            // Texte dans le rectangle jaune
            XStringFormat yellowBoxFormat = new XStringFormat
            {
                Alignment = XStringAlignment.Center,
                LineAlignment = XLineAlignment.Center
            };

            gfx.DrawString(invoiceData.Edition.RepairOrderSentenceBottom, normalFont, XBrushes.Black, yellowRect, yellowBoxFormat);

            startY += yellowBoxHeight + LineHeight * 1.5;

            // 3. Rectangle pour signature
            double signatureBoxWidth = 200; // Ajustez selon vos besoins
            double signatureBoxHeight = 50; // Ajustez selon vos besoins

            XRect signatureRect = new XRect(
                PageWidth - RightMargin - signatureBoxWidth - 10,
                startY,
                signatureBoxWidth,
                signatureBoxHeight
            );

            gfx.DrawRectangle(new XPen(XColors.Black, 1), signatureRect);
            gfx.DrawString("Signature pour accord", normalFont, XBrushes.Black,
                new XRect(signatureRect.X, signatureRect.Y - LineHeight, signatureRect.Width, LineHeight),
                new XStringFormat { Alignment = XStringAlignment.Center });

            startY += signatureBoxHeight + LineHeight * 2;

            // 4. "Valable 1 mois" en bas de page
            XRect footerRect = new XRect(
                0,
                PageHeight - (BottomMargin + 15),
                PageWidth,
                LineHeight
            );

            XStringFormat footerFormat = new XStringFormat
            {
                Alignment = XStringAlignment.Center,
                LineAlignment = XLineAlignment.Far
            };

            gfx.DrawString("Valable 1 mois",
                          headerFont,
                          XBrushes.Black,
                          footerRect,
                          footerFormat);

            // Retourne la nouvelle position Y maximale
            return PageHeight - BottomMargin;
        }
    }

    public static class PdfHelper
    {
        public static void DrawWrappedText(XGraphics gfx, string text, XFont font, XRect rect, XStringFormat format)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                var width = gfx.MeasureString(testLine, font).Width;

                if (width < rect.Width)
                    currentLine = testLine;
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }
            lines.Add(currentLine);

            var y = rect.Top;
            foreach (var line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black,
                    new XRect(rect.Left, y, rect.Width, rect.Height),
                    format);
                y += font.GetHeight();
            }
        }
    }

}
