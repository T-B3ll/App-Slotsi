using ClosedXML.Excel;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Slotsi.Shared.Dtos;
using System.Collections.Generic;
using System.IO;

namespace slotsi_citas.Services
{
    public class ReporteService
    {
        public byte[] GenerarTopServiciosPdf(List<TopServicioDto> servicios, string periodo)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var fontNormal = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            document.Add(new Paragraph($"Reporte de Top Servicios ({periodo})")
                .SetFont(fontBold).SetFontSize(18).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            var table = new Table(new float[] { 1, 4, 2, 2 }).UseAllAvailableWidth();

            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("#").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Servicio").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Citas").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Duración").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            int index = 1;
            foreach (var item in servicios)
            {
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(index.ToString()).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.NombreServicio).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.TotalCitas.ToString()).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph($"{item.DuracionMinutos} min").SetFont(fontNormal)));
                index++;
            }

            document.Add(table);
            document.Close();
            return stream.ToArray();
        }

        public byte[] GenerarTopServiciosExcel(List<TopServicioDto> servicios, string periodo)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Top Servicios");

            worksheet.Cell(1, 1).Value = $"Reporte de Top Servicios ({periodo})";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;

            worksheet.Cell(3, 1).Value = "#";
            worksheet.Cell(3, 2).Value = "Servicio";
            worksheet.Cell(3, 3).Value = "Citas";
            worksheet.Cell(3, 4).Value = "Duración (min)";

            var headerRange = worksheet.Range(3, 1, 3, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 4;
            int index = 1;
            foreach (var item in servicios)
            {
                worksheet.Cell(row, 1).Value = index;
                worksheet.Cell(row, 2).Value = item.NombreServicio;
                worksheet.Cell(row, 3).Value = item.TotalCitas;
                worksheet.Cell(row, 4).Value = item.DuracionMinutos;
                row++;
                index++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerarDemandaSemanalPdf(List<DemandaSemanalDto> demanda, string periodo)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var fontNormal = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            document.Add(new Paragraph($"Reporte de Demanda esta   ({periodo})")
                .SetFont(fontBold).SetFontSize(18).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

     
            var topDemanda = demanda
                .OrderByDescending(d => d.TotalCitas)
                .Take(10)
                .ToList();

            var table = new Table(new float[] { 1, 3, 2, 2 }).UseAllAvailableWidth();

            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("#").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Día").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Hora").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("Citas").SetFont(fontBold)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            int index = 1;
            foreach (var item in topDemanda)
            {
                string diaEspanol = item.DiaSemana switch
                {
                    "Monday" => "Lunes",
                    "Tuesday" => "Martes",
                    "Wednesday" => "Miércoles",
                    "Thursday" => "Jueves",
                    "Friday" => "Viernes",
                    "Saturday" => "Sábado",
                    "Sunday" => "Domingo",
                    _ => item.DiaSemana
                };

                string horaFormateada = DateTime.Today.AddHours(item.Hora).ToString("h:mm tt");

                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(index.ToString()).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(diaEspanol).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(horaFormateada).SetFont(fontNormal)));
                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.TotalCitas.ToString()).SetFont(fontNormal)));
                index++;
            }

            document.Add(table);
            document.Close();
            return stream.ToArray();
        }
        public byte[] GenerarDemandaSemanalExcel(List<DemandaSemanalDto> demanda, string periodo)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Demanda Semanal");

            worksheet.Cell(1, 1).Value = $"Reporte de Demanda  ({periodo})";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;

            worksheet.Cell(3, 1).Value = "Día";
            worksheet.Cell(3, 2).Value = "Hora";
            worksheet.Cell(3, 3).Value = "Total Citas";

            var headerRange = worksheet.Range(3, 1, 3, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 4;
            foreach (var item in demanda)
            {
                worksheet.Cell(row, 1).Value = item.DiaSemana;
                worksheet.Cell(row, 2).Value = $"{item.Hora}:00";
                worksheet.Cell(row, 3).Value = item.TotalCitas;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}