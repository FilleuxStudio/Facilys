using ClosedXML.Excel;
using Facilys.Components.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Facilys.Components.Services
{
    public class ExportService
    {

        public static async Task<byte[]> GenerateCsvAsync<T>(IQueryable<T> query) where T : class
        {
            // Récupère toutes les propriétés publiques de T
            var props = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // En-tête
            sb.AppendLine(string.Join(";", props.Select(p => p.Name)));

            // Lignes
            await foreach (var item in query.AsAsyncEnumerable())
            {
                var values = props.Select(p =>
                {
                    var v = p.GetValue(item)?.ToString() ?? "";
                    // Échapper point-virgule et guillemets
                    return $"\"{v.Replace("\"", "\"\"")}\"";
                });
                sb.AppendLine(string.Join(";", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static async Task<byte[]> GenerateExcelAsync<T>(IQueryable<T> query, string sheetName = "Données") where T : class
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            var props = typeof(T).GetProperties();

            // En-têtes
            for (int i = 0; i < props.Length; i++)
                ws.Cell(1, i + 1).Value = props[i].Name;

            // Contenu
            int row = 2;
            await foreach (var item in query.AsAsyncEnumerable())
            {
                for (int col = 0; col < props.Length; col++)
                {
                    ws.Cell(row, col + 1).Value = (XLCellValue)props[col].GetValue(item);
                }
                row++;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
