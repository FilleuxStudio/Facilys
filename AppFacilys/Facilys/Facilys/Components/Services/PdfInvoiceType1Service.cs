using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Pages;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.UniversalAccessibility.Drawing;
using System.Drawing;

namespace Facilys.Components.Services
{
    public class PdfInvoiceType1Service
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

        public byte[] GenerateInvoicePdf(ManagerInvoiceViewModel managerInvoiceView, Invoices invoice, int km, PhonesClients phones, EmailsClients emails)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            var gfx = XGraphics.FromPdfPage(page);
            var yPosition = Margin; // Position verticale initiale

            // Configuration des polices
            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);

            // Dessiner l'en-tête de la facture
            yPosition = DrawHeader(gfx, invoice, managerInvoiceView, km, titleFont, normalFont, yPosition);

            // Informations client
            yPosition = DrawCustomerInfo(gfx, managerInvoiceView, phones, normalFont, yPosition);

            // Informations véhicule
            yPosition = DrawVehicleInfo(gfx, managerInvoiceView, normalFont, yPosition);

            // Tableau des services
            yPosition = DrawServiceTable(gfx, managerInvoiceView, headerFont, normalFont, yPosition);

            // Informations a prévoir
            yPosition = DrawRequiredInformation(gfx, managerInvoiceView, normalFont, yPosition);

            // Total et mentions légales
            yPosition = DrawTotalAndLegalMentions(gfx, managerInvoiceView, headerFont, normalFont, yPosition);

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

        private double DrawHeader(XGraphics gfx, Invoices invoice, ManagerInvoiceViewModel company, int km, XFont titleFont, XFont normalFont, double yPosition)
        {
            double newHeight = 0;
            double availableWidth = 0;
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
            gfx.DrawString("FACTURE", titleFont, XBrushes.Black, rect, XStringFormats.TopRight);

            yPosition += newHeight + LineHeight;
            double initialYPosition = yPosition;
            // Adresse du garage
            gfx.DrawString(company.CompanySettings.HeadOfficeAddress.ToUpper(), normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            // Téléphone et email
            gfx.DrawString("Téléphone : " + company.CompanySettings.Phone, normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("Email : " + company.CompanySettings.Email, normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString(company.CompanySettings.WebSite ?? "", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            // Réinitialiser yPosition pour les informations de droite
            yPosition = initialYPosition;

            // Calculer la largeur disponible
            availableWidth = PageWidth - Margin - RightMargin;

            // Date et numéro de facture (à droite)
            string dateString = $"Date : {invoice.DateAdded:dd-MM-yyyy}";
            XSize dateSize = gfx.MeasureString(dateString, normalFont);
            gfx.DrawString(dateString, normalFont, XBrushes.Black, PageWidth - RightMargin - dateSize.Width, yPosition);
            yPosition += LineHeight;

            string invoiceNumberString = $"N° Facture : {invoice.InvoiceNumber}";
            XSize invoiceNumberSize = gfx.MeasureString(invoiceNumberString, normalFont);
            gfx.DrawString(invoiceNumberString, normalFont, XBrushes.Black, PageWidth - RightMargin - invoiceNumberSize.Width, yPosition);
            yPosition += LineHeight;

            if (invoice.Vehicle != null)
            {
                string kmString = $"Km : {km}";
                XSize kmSize = gfx.MeasureString(kmString, normalFont);
                gfx.DrawString(kmString, normalFont, XBrushes.Black, PageWidth - RightMargin - kmSize.Width, yPosition);
            }

            yPosition += LineHeight * 2;

            return yPosition;
        }

        private double DrawCustomerInfo(XGraphics gfx, ManagerInvoiceViewModel client, PhonesClients phones, XFont normalFont, double yPosition)
        {
            // Sauvegardez la position verticale initiale
            double initialYPosition = yPosition;

            // Calculez la largeur du rectangle (moitié de la largeur disponible)
            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
            double rectHeight = LineHeight * 7; // Ajustez selon le nombre de lignes

            // Créez le rectangle
            XRect rect = new XRect(LeftMargin, yPosition, rectWidth, rectHeight);

            // Dessinez le rectangle avec une bordure noire de 2px
            XPen pen = new XPen(XColors.Black, 1);
            gfx.DrawRectangle(pen, rect);

            // Centrez le titre "CLIENT"
            XRect titleRect = new XRect(rect.Left, yPosition, rectWidth, LineHeight);
            gfx.DrawString("CLIENT", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
            yPosition += LineHeight * 1.5; // Espace après le titre

            // Dessinez les informations du client
            string[][] clientInfo = new string[][]
{
    new[] {"Nom:", client.Client.Lname},
    new[] {"Prénom:", client.Client.Fname},
    new[] {"Rue:", client.Client.Address},
    new[] {"Code postal:", client.Client.PostalCode},
    new[] {"Ville:", client.Client.City},
    new[] {"Téléphone:", phones.Phone}
};

            // Trouvez la largeur maximale des labels
            double maxLabelWidth = 0;
            foreach (var info in clientInfo)
            {
                XSize size = gfx.MeasureString(info[0], normalFont);
                if (size.Width > maxLabelWidth)
                    maxLabelWidth = size.Width;
            }

            // Ajoutez un petit espace après le label le plus long
            maxLabelWidth += 5;

            // Dessinez les informations alignées
            double textMargin = Margin + 5; // Marge intérieure pour le texte
            foreach (var info in clientInfo)
            {
                gfx.DrawString(info[0], normalFont, XBrushes.Black, textMargin, yPosition);
                gfx.DrawString(info[1], normalFont, XBrushes.Black, textMargin + maxLabelWidth, yPosition);
                yPosition += LineHeight;
            }

            // Mettez à jour yPosition pour le prochain élément après le rectangle
            yPosition = initialYPosition;

            return yPosition;
        }

        private double DrawVehicleInfo(XGraphics gfx, ManagerInvoiceViewModel vehicle, XFont normalFont, double yPosition)
        {
            // Sauvegardez la position verticale initiale (utilisez la même que pour le rectangle client)
            double initialYPosition = yPosition;

            // Calculez la largeur du rectangle (moitié de la largeur disponible)
            double rectWidth = (PageWidth - LeftMargin - RightMargin) / 2;
            double rectHeight = LineHeight * 7; // Ajustez selon le nombre de lignes

            // Créez le rectangle pour le véhicule (à droite du rectangle client)
            XRect rect = new XRect(LeftMargin + rectWidth, yPosition, rectWidth, rectHeight);

            // Dessinez le rectangle avec une bordure noire de 2px
            XPen pen = new XPen(XColors.Black, 1);
            gfx.DrawRectangle(pen, rect);

            // Centrez le titre "VEHICULE"
            XRect titleRect = new XRect(rect.Left, yPosition, rectWidth, LineHeight);
            gfx.DrawString("VEHICULE", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);
            yPosition += LineHeight * 1.5; // Espace après le titre

            // Dessinez les informations du véhicule
            double textMargin = rect.Left + 5; // Marge intérieure pour le texte
            gfx.DrawString($"Marque : {vehicle.Vehicle.Mark}", normalFont, XBrushes.Black, textMargin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Modèle : {vehicle.Vehicle.Model}", normalFont, XBrushes.Black, textMargin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Immatriculation : {vehicle.Vehicle.Immatriculation}", normalFont, XBrushes.Black, textMargin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"VIN : {vehicle.Vehicle.VIN}", normalFont, XBrushes.Black, textMargin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Type : {vehicle.Vehicle.Type}", normalFont, XBrushes.Black, textMargin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Mise en circulation : {vehicle.Vehicle.CirculationDate:dd/MM/yyyy}", normalFont, XBrushes.Black, textMargin, yPosition);

            // Mettez à jour yPosition pour le prochain élément après les deux rectangles
            yPosition = initialYPosition + rectHeight;

            return yPosition;

        }

        private double DrawServiceTable(XGraphics gfx, ManagerInvoiceViewModel historyParts, XFont headerFont, XFont normalFont, double yPosition)
        {
            // Définir les largeurs de colonnes (ajustées pour tenir dans la page)
            double[] columnWidths = { 295, 55, 75, 55, 75 };
            double tableWidth = columnWidths.Sum();
            double tableStartX = Margin;
            double tableStartY = yPosition;
            double cellPadding = 5; // Espace à l'intérieur des cellules

            // Créer un stylo pour les lignes
            XPen tablePen = new XPen(XColors.Black, 1);

            // Dessiner l'en-tête du tableau
            string[] headers = { "Description", "Quantité", "Prix Unitaire", "Remise", "Montant HT" };
            for (int i = 0; i < headers.Length; i++)
            {
                double columnX = tableStartX + columnWidths.Take(i).Sum();
                XRect headerRect = new XRect(columnX, yPosition, columnWidths[i], LineHeight + cellPadding * 2);

                // Dessiner la cellule de l'en-tête (avec toutes les bordures)
                gfx.DrawRectangle(tablePen, headerRect);

                // Centrer le texte dans la cellule
                gfx.DrawString(headers[i], headerFont, XBrushes.Black, headerRect, XStringFormats.Center);
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
                    XStringFormat format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.TopRight;
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
            // Sauvegardez la position verticale initiale
            double initialYPosition = yPosition;

            // Calculez la largeur du rectangle (toute la largeur disponible)
            double rectWidth = PageWidth - LeftMargin - RightMargin;
            double rectHeight = LineHeight * 4; // Ajustez selon le nombre de lignes

            // Créez le rectangle pour l'information
            XRect rect = new XRect(LeftMargin, yPosition, rectWidth, rectHeight);

            // Dessinez le rectangle avec une bordure noire de 1px
            XPen pen = new XPen(XColors.Black, 1);
            gfx.DrawRectangle(pen, rect);

            // Centrez le titre "INTERVENTION A PREVOIR"
            XRect titleRect = new XRect(rect.X, rect.Y, rect.Width, LineHeight);
            gfx.DrawString("INTERVENTION A PREVOIR", normalFont, XBrushes.Black, titleRect, XStringFormats.Center);

            // Dessinez le texte à l'intérieur du rectangle
            double textY = rect.Y + LineHeight + 5; // Espace après le titre
            string text = invoiceInformation.InvoiceData.GeneralConditionInvoice;
            gfx.DrawString(text, normalFont, XBrushes.Black, new XRect(rect.X + 5, textY, rect.Width - 10, rect.Height - LineHeight - 10), XStringFormats.TopLeft);

            // Mettez à jour yPosition pour le prochain élément
            return yPosition = initialYPosition + rectHeight;

        }

        private double DrawTotalAndLegalMentions(XGraphics gfx, ManagerInvoiceViewModel invoiceData, XFont headerFont, XFont normalFont, double yPosition)
        {
            // Définir les dimensions et la position du rectangle
            double rectWidth = 200; // Ajustez selon vos besoins
            double rectHeight = LineHeight * 6; // 4 lignes
            double rectX = PageWidth - RightMargin - rectWidth;
            double rectY = yPosition;

            // Créer le rectangle principal
            XRect mainRect = new XRect(rectX, rectY, rectWidth, rectHeight);
            gfx.DrawRectangle(new XPen(XColors.Black, 1), mainRect);

            // Dessiner les lignes et le texte
            string[] labels = { "SOUS-TOTAL HT :", "TAUX DE T.V.A :", "T.V.A :", "TOTAL TTC :" };
            string[] values = {
    invoiceData.InvoiceData.HT.ToString("C"),
    $"{invoiceData.Edition.TVA}%",
    invoiceData.InvoiceData.TVA.ToString("C"),
    invoiceData.InvoiceData.TTC.ToString("C")
};

            for (int i = 0; i < 4; i++)
            {
                double lineY = rectY + i * LineHeight;

                // Dessiner la ligne de séparation (sauf pour la dernière ligne)
                if (i < 3)
                {
                    gfx.DrawLine(new XPen(XColors.Black, 1), rectX, lineY + LineHeight * (2 + i), rectX + rectWidth, lineY + LineHeight * (2 + i));
                }

                // Dessiner le texte
                XRect textRect = new XRect(rectX + 5, lineY, rectWidth - 10, LineHeight * (2 + i));
                gfx.DrawString(labels[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterLeft);
                gfx.DrawString(values[i], i == 0 || i == 3 ? headerFont : normalFont, XBrushes.Black, textRect, XStringFormats.CenterRight);
            }

            // Mettre à jour yPosition pour le prochain élément
            yPosition = rectY + rectHeight + LineHeight;

            // Mentions légales
            gfx.DrawString("SIRET : 53085129400021", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("TVA : FR37530851294", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("APE : 4520A", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            // Conditions de règlement
            gfx.DrawString("Acceptant les règlements des sommes dues par chèque libellé à mon nom.", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("Règlement nos réparations sont payables exclusivement au comptant.", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("Toutes pièces fournies par le client ne sont pas garanties, ainsi que les dommages causés.", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("Déplacement non inclus dans la prise sous garantie.", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            // Remerciements
            gfx.DrawString("MERCI DE VOTRE CONFIANCE !", headerFont, XBrushes.Black, Margin, yPosition);

            return yPosition;
        }
    }
}
