using Facilys.Components.Models;
using Facilys.Components.Models.PdfModels;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Facilys.Components.Services
{
    public class PdfInvoiceType1Service
    {
        // Constantes pour la mise en page
        private const double PageWidth = 595; // Largeur A4 en points
        private const double PageHeight = 842; // Hauteur A4 en points
        private const double Margin = 40; // Marge de 40 points (environ 1,4 cm)
        private const double LineHeight = 20; // Hauteur de ligne en points

        public byte[] GenerateInvoicePdf(InvoicePDF invoice)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            var gfx = XGraphics.FromPdfPage(page);
            var yPosition = Margin; // Position verticale initiale

            // Configuration des polices
            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 10);

            // Dessiner l'en-tête de la facture
            yPosition = DrawHeader(gfx, invoice, titleFont, normalFont, yPosition);

            // Informations client
            yPosition = DrawCustomerInfo(gfx, invoice, normalFont, yPosition);

            // Informations véhicule
            yPosition = DrawVehicleInfo(gfx, invoice, normalFont, yPosition);

            // Tableau des services
            yPosition = DrawServiceTable(gfx, invoice, headerFont, normalFont, yPosition);

            // Total et mentions légales
            yPosition = DrawTotalAndLegalMentions(gfx, invoice, headerFont, normalFont, yPosition);

            // Sauvegarde en mémoire
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private double DrawHeader(XGraphics gfx, InvoicePDF invoice, XFont titleFont, XFont normalFont, double yPosition)
        {
            // Nom du garage
            gfx.DrawString("GP AUTO", titleFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;

            // Adresse du garage
            gfx.DrawString("8 RUE DU MOULIN FONDU, 60840, CATENOY", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;

            // Téléphone et email
            gfx.DrawString("Téléphone : 0682017910", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("Email : gpauto60@orange.fr", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString("https://www.gpauto60.fr", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            // Date et numéro de facture
            gfx.DrawString($"Date : {invoice.Date:dd-MM-yyyy}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"N° Facture : {invoice.InvoiceNumber}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Km : {invoice.Kilometers}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            return yPosition;
        }

        private double DrawCustomerInfo(XGraphics gfx, InvoicePDF invoice, XFont normalFont, double yPosition)
        {
            gfx.DrawString("CLIENT", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;

            gfx.DrawString($"Nom: {invoice.Customer.LastName}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Prénom: {invoice.Customer.FirstName}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Rue: {invoice.Customer.Address}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Code postal: {invoice.Customer.PostalCode}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Ville: {invoice.Customer.City}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Téléphone: {invoice.Customer.Phone}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            return yPosition;
        }

        private double DrawVehicleInfo(XGraphics gfx, InvoicePDF invoice, XFont normalFont, double yPosition)
        {
            gfx.DrawString("VEHICULE", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;

            gfx.DrawString($"Marque : {invoice.Vehicle.Brand}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Modèle : {invoice.Vehicle.Model}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Immatriculation : {invoice.Vehicle.Registration}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"VIN : {invoice.Vehicle.VIN}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Type : {invoice.Vehicle.Type}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"Mise en circulation : {invoice.Vehicle.ReleaseDate:dd/MM/yyyy}", normalFont, XBrushes.Black, Margin, yPosition);
            yPosition += LineHeight * 2;

            return yPosition;
        }

        private double DrawServiceTable(XGraphics gfx, InvoicePDF invoice, XFont headerFont, XFont normalFont, double yPosition)
        {
            // En-tête du tableau
            gfx.DrawString("Description", headerFont, XBrushes.Black, Margin, yPosition);
            gfx.DrawString("Quantité", headerFont, XBrushes.Black, Margin + 200, yPosition);
            gfx.DrawString("Prix Unitaire", headerFont, XBrushes.Black, Margin + 300, yPosition);
            gfx.DrawString("Montant HT", headerFont, XBrushes.Black, Margin + 400, yPosition);
            yPosition += LineHeight;

            // Lignes du tableau
            foreach (var item in invoice.Services)
            {
                gfx.DrawString(item.Description, normalFont, XBrushes.Black, Margin, yPosition);
                gfx.DrawString(item.Quantity.ToString(), normalFont, XBrushes.Black, Margin + 200, yPosition);
                gfx.DrawString(item.UnitPrice.ToString("C"), normalFont, XBrushes.Black, Margin + 300, yPosition);
                gfx.DrawString(item.Total.ToString("C"), normalFont, XBrushes.Black, Margin + 400, yPosition);
                yPosition += LineHeight;
            }

            yPosition += LineHeight;

            return yPosition;
        }

        private double DrawTotalAndLegalMentions(XGraphics gfx, InvoicePDF invoice, XFont headerFont, XFont normalFont, double yPosition)
        {
            // Sous-total HT
            gfx.DrawString($"SOUS-TOTAL HT : {invoice.SubTotal:C}", headerFont, XBrushes.Black, Margin + 300, yPosition);
            yPosition += LineHeight;

            // TVA
            gfx.DrawString($"TAUX DE T.V.A : {invoice.TaxRate}%", normalFont, XBrushes.Black, Margin + 300, yPosition);
            yPosition += LineHeight;
            gfx.DrawString($"T.V.A : {invoice.TaxAmount:C}", normalFont, XBrushes.Black, Margin + 300, yPosition);
            yPosition += LineHeight;

            // Total TTC
            gfx.DrawString($"TOTAL TTC : {invoice.TotalAmount:C}", headerFont, XBrushes.Black, Margin + 300, yPosition);
            yPosition += LineHeight * 2;

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
