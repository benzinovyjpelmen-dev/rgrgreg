using System.Collections.Generic;
using System.IO;
using CultureDocs.Models;

namespace CultureDocs.Services
{
    public static class ExcelExportService
    {
        // Экспорт документов в CSV (открывается в Excel)
        public static void ExportDocumentsToCsv(List<Document> documents, string path)
        {
            using (StreamWriter w = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                w.WriteLine("ID;Название;Тип;Автор;Статус;Дата");
                foreach (Document d in documents)
                {
                    w.WriteLine(d.DocumentID + ";" + d.Title + ";" + d.DocType + ";" +
                                d.AuthorName + ";" + d.Status + ";" +
                                d.CreatedDate.ToString("dd.MM.yyyy"));
                }
            }
        }

        // Экспорт аналитического отчёта
        public static void ExportAnalyticsToCsv(List<AnalyticsRow> rows, string path)
        {
            using (StreamWriter w = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                w.WriteLine("Автор;Тип;Документов;Версий;Последняя версия");
                foreach (AnalyticsRow r in rows)
                {
                    w.WriteLine(r.AuthorName + ";" + r.DocType + ";" +
                                r.DocumentCount + ";" + r.VersionCount + ";" +
                                (r.LastVersionDate.HasValue
                                    ? r.LastVersionDate.Value.ToString("dd.MM.yyyy")
                                    : ""));
                }
            }
        }
    }
}