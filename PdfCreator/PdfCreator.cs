using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Contracts;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;

namespace PdfCreator
{
    public class PdfCreator
    {
        public void CreatePdf(string path, int terminalId, List<Telemetry> data)
        {
            var properties = data.First().GetType().GetProperties();

            var doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();
            PdfPTable table = new PdfPTable(properties.Length);

            PdfPCell cell =
                new PdfPCell(new Phrase($"Report on the terminal #{terminalId}",
                    new Font(Font.FontFamily.TIMES_ROMAN, 16, Font.BOLD)))
                {
                    Padding = 5,
                    PaddingBottom = 15,
                    Colspan = properties.Length,
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
            table.AddCell(cell);

            foreach (var property in properties)
            {
                table.AddCell(property.Name);
            }

            foreach (var d in data)
            {
                table.AddCell($"{d.Coordinates.Latitude:F4},{d.Coordinates.Longitude:F4}");
                table.AddCell(d.Engine.ToString());
                table.AddCell(d.SpeedKmh.ToString("F4"));
                table.AddCell(d.Time.ToString(CultureInfo.CurrentCulture));
                table.AddCell(d.TotalMileageKm.ToString("F4"));
            }

            table.AddCell("");
            table.AddCell("");
            table.AddCell($"Max speed: {data.Max(s => s.SpeedKmh):F2} Km/h\r\nAverage speed: {data.Average(s => s.SpeedKmh):F2} Km/h");
            var temp = data.Where(e => e.Engine).ToList();
            table.AddCell($"Travel time: {temp.Zip(temp.Skip(1), (x, y) => y.Time.Subtract(x.Time).TotalHours).Sum(t => t)} Hr");
            table.AddCell($"Mileage: {data.Sum(m => m.TotalMileageKm):F2} Km");

            doc.Add(table);
            doc.Close();
        }
    }
}
