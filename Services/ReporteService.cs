using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace slotsi_citas.Services
{
    public class ReporteService
    {
        public byte[] GenerarRankingNegociosPdf(List<NegocioRanckingItem> ranking, string subtitulo)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            writer.SetCloseStream(false);

            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var azul = new DeviceRgb(37, 99, 235);
                var gris = new DeviceRgb(107, 114, 128);

                doc.Add(new Paragraph("Ranking de Mejores Negocios")
                    .SetFont(fontBold).SetFontSize(20).SetFontColor(azul).SetMarginBottom(2));
                doc.Add(new Paragraph(subtitulo)
                    .SetFont(fontNormal).SetFontSize(11).SetFontColor(gris).SetMarginBottom(2));
                doc.Add(new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMarginBottom(14));

                var tabla = new Table(UnitValue.CreatePercentArray(new float[] { 6, 38, 24, 16, 16 }))
                    .UseAllAvailableWidth();

                foreach (var titulo in new[] { "#", "Negocio", "Categoría", "Promedio", "Reseñas" })
                {
                    tabla.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new Paragraph(titulo).SetFont(fontBold).SetFontSize(10).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(azul)
                        .SetPadding(6));
                }

                int pos = 1;
                foreach (var n in ranking)
                {
                    var fondo = pos % 2 == 0 ? new DeviceRgb(249, 250, 251) : new DeviceRgb(255, 255, 255);

                    tabla.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(pos.ToString()).SetFont(fontBold).SetFontSize(10))
                        .SetBackgroundColor(fondo).SetPadding(5));

                    var celdaNegocio = new iText.Layout.Element.Cell().SetBackgroundColor(fondo).SetPadding(5);
                    celdaNegocio.Add(new Paragraph(n.Titulo ?? "").SetFont(fontBold).SetFontSize(10).SetMargin(0));
                    celdaNegocio.Add(new Paragraph(n.NombreNegocio ?? "").SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMargin(0));
                    tabla.AddCell(celdaNegocio);

                    tabla.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(n.Categoria ?? "").SetFont(fontNormal).SetFontSize(10))
                        .SetBackgroundColor(fondo).SetPadding(5));
                    tabla.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(n.PromedioTexto ?? "").SetFont(fontNormal).SetFontSize(10))
                        .SetBackgroundColor(fondo).SetPadding(5));
                    tabla.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(n.ResenasTexto ?? "").SetFont(fontNormal).SetFontSize(10))
                        .SetBackgroundColor(fondo).SetPadding(5));

                    pos++;
                }

                doc.Add(tabla);
            }

            return ms.ToArray();
        }

        public byte[] GenerarRankingNegociosExcel(List<NegocioRanckingItem> ranking, string subtitulo)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Ranking");

            ws.Cell(1, 1).Value = "Ranking de Mejores Negocios";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(2, 1).Value = subtitulo;
            ws.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            string[] headers = { "#", "Negocio", "Descripción", "Categoría", "Promedio", "Reseñas" };
            for (int i = 0; i < headers.Length; i++)
            {
                var c = ws.Cell(5, i + 1);
                c.Value = headers[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontColor = XLColor.White;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            }

            int row = 6, pos = 1;
            foreach (var n in ranking)
            {
                ws.Cell(row, 1).Value = pos;
                ws.Cell(row, 2).Value = n.Titulo;
                ws.Cell(row, 3).Value = n.NombreNegocio;
                ws.Cell(row, 4).Value = n.Categoria;
                ws.Cell(row, 5).Value = n.PromedioTexto;
                ws.Cell(row, 6).Value = n.ResenasTexto;
                row++; pos++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

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
        public class ResumenReporteData
        {
            public string NombreNegocio { get; set; } = string.Empty;
            public string DireccionNegocio { get; set; } = string.Empty;
            public DateTime Desde { get; set; }
            public DateTime Hasta { get; set; }
            public int CitasConfirmadas { get; set; }
            public int CitasPendientes { get; set; }
            public int OcupacionPorcentaje { get; set; }
            public int ServiciosActivosHoy { get; set; }
            public List<ResumenCitaFila> Citas { get; set; } = new();
        }

        public class ResumenCitaFila
        {
            public string Hora { get; set; } = string.Empty;
            public string Cliente { get; set; } = string.Empty;
            public string Servicio { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
        }

        public byte[] GenerarResumenPdf(ResumenReporteData d)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            writer.SetCloseStream(false);

            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var azul = new DeviceRgb(37, 99, 235);
                var gris = new DeviceRgb(107, 114, 128);

                doc.Add(new Paragraph(d.NombreNegocio)
                    .SetFont(fontBold).SetFontSize(20).SetFontColor(azul).SetMarginBottom(2));
                doc.Add(new Paragraph(d.DireccionNegocio)
                    .SetFont(fontNormal).SetFontSize(11).SetFontColor(gris).SetMarginBottom(10));

                doc.Add(new Paragraph("Resumen de citas")
                    .SetFont(fontBold).SetFontSize(15).SetMarginBottom(2));
                doc.Add(new Paragraph($"Periodo: {d.Desde:dd/MM/yyyy} - {d.Hasta:dd/MM/yyyy}")
                    .SetFont(fontNormal).SetFontSize(10).SetFontColor(gris).SetMarginBottom(2));
                doc.Add(new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMarginBottom(12));

                var kpis = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 })).UseAllAvailableWidth();
                void Kpi(string titulo, string valor) =>
                    kpis.AddCell(new iText.Layout.Element.Cell()
                        .Add(new Paragraph(titulo).SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMargin(0))
                        .Add(new Paragraph(valor).SetFont(fontBold).SetFontSize(16).SetMargin(0))
                        .SetPadding(8).SetBackgroundColor(new DeviceRgb(239, 246, 255)));
                Kpi("Confirmadas", d.CitasConfirmadas.ToString());
                Kpi("Pendientes", d.CitasPendientes.ToString());
                Kpi("Ocupación", $"{d.OcupacionPorcentaje:0}%");
                Kpi("Servicios activos hoy", d.ServiciosActivosHoy.ToString());
                doc.Add(kpis.SetMarginBottom(14));

                var tabla = new Table(UnitValue.CreatePercentArray(new float[] { 16, 30, 32, 22 })).UseAllAvailableWidth();
                foreach (var t in new[] { "Hora", "Cliente", "Servicio", "Estado" })
                    tabla.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new Paragraph(t).SetFont(fontBold).SetFontSize(10).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(azul).SetPadding(6));

                int i = 0;
                foreach (var c in d.Citas)
                {
                    var fondo = i++ % 2 == 1 ? new DeviceRgb(249, 250, 251) : new DeviceRgb(255, 255, 255);
                    foreach (var texto in new[] { c.Hora, c.Cliente, c.Servicio, c.Estado })
                        tabla.AddCell(new iText.Layout.Element.Cell()
                            .Add(new Paragraph(texto ?? "").SetFont(fontNormal).SetFontSize(10))
                            .SetBackgroundColor(fondo).SetPadding(5));
                }
                doc.Add(tabla);
            }
            return ms.ToArray();
        }

        public byte[] GenerarResumenExcel(ResumenReporteData d)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Resumen");

            ws.Cell(1, 1).Value = d.NombreNegocio;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(2, 1).Value = d.DireccionNegocio;
            ws.Cell(3, 1).Value = $"Periodo: {d.Desde:dd/MM/yyyy} - {d.Hasta:dd/MM/yyyy}";
            ws.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            ws.Cell(6, 1).Value = "Confirmadas"; ws.Cell(6, 2).Value = d.CitasConfirmadas;
            ws.Cell(7, 1).Value = "Pendientes"; ws.Cell(7, 2).Value = d.CitasPendientes;
            ws.Cell(8, 1).Value = "Ocupación (%)"; ws.Cell(8, 2).Value = d.OcupacionPorcentaje;
            ws.Cell(9, 1).Value = "Servicios activos hoy"; ws.Cell(9, 2).Value = d.ServiciosActivosHoy;
            ws.Range(6, 1, 9, 1).Style.Font.Bold = true;

            string[] headers = { "Hora", "Cliente", "Servicio", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                var c = ws.Cell(11, i + 1);
                c.Value = headers[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontColor = XLColor.White;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            }

            int row = 12;
            foreach (var c in d.Citas)
            {
                ws.Cell(row, 1).Value = c.Hora;
                ws.Cell(row, 2).Value = c.Cliente;
                ws.Cell(row, 3).Value = c.Servicio;
                ws.Cell(row, 4).Value = c.Estado;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] GenerarCancelacionesPdf(CancelacionesReporteData d)
        {
            static string P(double v) => v.ToString("0.#", CultureInfo.InvariantCulture) + "%";

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            writer.SetCloseStream(false);

            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var azul = new DeviceRgb(37, 99, 235);
                var gris = new DeviceRgb(107, 114, 128);

                doc.Add(new Paragraph(d.NombreNegocio)
                    .SetFont(fontBold).SetFontSize(20).SetFontColor(azul).SetMarginBottom(2));
                doc.Add(new Paragraph(d.DireccionNegocio)
                    .SetFont(fontNormal).SetFontSize(11).SetFontColor(gris).SetMarginBottom(10));

                doc.Add(new Paragraph("Reporte de cancelaciones")
                    .SetFont(fontBold).SetFontSize(15).SetMarginBottom(2));
                doc.Add(new Paragraph($"{d.Periodo}  ({d.Desde:dd/MM/yyyy HH:mm} - {d.Hasta:dd/MM/yyyy HH:mm})")
                    .SetFont(fontNormal).SetFontSize(10).SetFontColor(gris).SetMarginBottom(2));
                doc.Add(new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMarginBottom(12));

                var kpis = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 })).UseAllAvailableWidth();
                void Kpi(string titulo, string valor) =>
                    kpis.AddCell(new iText.Layout.Element.Cell()
                        .Add(new Paragraph(titulo).SetFont(fontNormal).SetFontSize(9).SetFontColor(gris).SetMargin(0))
                        .Add(new Paragraph(valor).SetFont(fontBold).SetFontSize(16).SetMargin(0))
                        .SetPadding(8).SetBackgroundColor(new DeviceRgb(254, 242, 242)));
                Kpi("Pérdida", P(d.PorcentajePerdida));
                Kpi("Total de citas", d.Total.ToString());
                Kpi("Canceladas", $"{d.Canceladas} ({P(d.PorcentajeCancelacion)})");
                Kpi("No asistieron", $"{d.NoAsistio} ({P(d.PorcentajeNoAsistio)})");
                doc.Add(kpis.SetMarginBottom(14));

                // Detalle
                var tabla = new Table(UnitValue.CreatePercentArray(new float[] { 18, 12, 24, 30, 16 })).UseAllAvailableWidth();
                foreach (var t in new[] { "Fecha", "Hora", "Cliente", "Servicio", "Estado" })
                    tabla.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new Paragraph(t).SetFont(fontBold).SetFontSize(10).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(azul).SetPadding(6));

                int i = 0;
                foreach (var c in d.Citas)
                {
                    var fondo = i++ % 2 == 1 ? new DeviceRgb(249, 250, 251) : new DeviceRgb(255, 255, 255);
                    foreach (var texto in new[] { c.Fecha, c.Hora, c.Cliente, c.Servicio, c.Estado })
                        tabla.AddCell(new iText.Layout.Element.Cell()
                            .Add(new Paragraph(texto ?? "").SetFont(fontNormal).SetFontSize(10))
                            .SetBackgroundColor(fondo).SetPadding(5));
                }
                doc.Add(tabla);
            }
            return ms.ToArray();
        }

        public byte[] GenerarCancelacionesExcel(CancelacionesReporteData d)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Cancelaciones");

            ws.Cell(1, 1).Value = d.NombreNegocio;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(2, 1).Value = d.DireccionNegocio;
            ws.Cell(3, 1).Value = $"{d.Periodo}  ({d.Desde:dd/MM/yyyy HH:mm} - {d.Hasta:dd/MM/yyyy HH:mm})";
            ws.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            ws.Cell(6, 1).Value = "Pérdida (%)"; ws.Cell(6, 2).Value = d.PorcentajePerdida;
            ws.Cell(7, 1).Value = "Total de citas"; ws.Cell(7, 2).Value = d.Total;
            ws.Cell(8, 1).Value = "Canceladas"; ws.Cell(8, 2).Value = d.Canceladas;
            ws.Cell(9, 1).Value = "Cancelación (%)"; ws.Cell(9, 2).Value = d.PorcentajeCancelacion;
            ws.Cell(10, 1).Value = "No asistieron"; ws.Cell(10, 2).Value = d.NoAsistio;
            ws.Cell(11, 1).Value = "No asistió (%)"; ws.Cell(11, 2).Value = d.PorcentajeNoAsistio;
            ws.Range(6, 1, 11, 1).Style.Font.Bold = true;

            string[] headers = { "Fecha", "Hora", "Cliente", "Servicio", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                var c = ws.Cell(13, i + 1);
                c.Value = headers[i];
                c.Style.Font.Bold = true;
                c.Style.Font.FontColor = XLColor.White;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            }

            int row = 14;
            foreach (var c in d.Citas)
            {
                ws.Cell(row, 1).Value = c.Fecha;
                ws.Cell(row, 2).Value = c.Hora;
                ws.Cell(row, 3).Value = c.Cliente;
                ws.Cell(row, 4).Value = c.Servicio;
                ws.Cell(row, 5).Value = c.Estado;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}