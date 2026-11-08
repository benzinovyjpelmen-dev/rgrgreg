using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CultureDocs.Data;

namespace CultureDocs.Services
{
    public class AnalyticsRow
    {
        public string AuthorName { get; set; }
        public string DocType { get; set; }
        public int DocumentCount { get; set; }
        public int VersionCount { get; set; }
        public DateTime? LastVersionDate { get; set; }
    }

    public static class AnalyticsService
    {
        // Аналитический запрос: 3 таблицы, агрегация, расчётное поле
        public static List<AnalyticsRow> GetAnalytics()
        {
            List<AnalyticsRow> list = new List<AnalyticsRow>();
            string sql = @"
                SELECT 
                    a.LastName + ' ' + a.FirstName AS AuthorName,
                    d.DocType,
                    COUNT(DISTINCT d.DocumentID) AS DocumentCount,
                    COUNT(v.VersionID) AS VersionCount,
                    MAX(v.CreatedDate) AS LastVersionDate
                FROM Authors a
                INNER JOIN Documents d ON a.AuthorID = d.AuthorID
                LEFT JOIN Versions v ON d.DocumentID = v.DocumentID
                GROUP BY a.LastName, a.FirstName, d.DocType
                ORDER BY DocumentCount DESC";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new AnalyticsRow
                        {
                            AuthorName = r["AuthorName"].ToString(),
                            DocType = r["DocType"].ToString(),
                            DocumentCount = (int)r["DocumentCount"],
                            VersionCount = (int)r["VersionCount"],
                            LastVersionDate = r["LastVersionDate"] == DBNull.Value
                                ? (DateTime?)null : (DateTime)r["LastVersionDate"]
                        });
                    }
                }
            }
            return list;
        }
    }
}