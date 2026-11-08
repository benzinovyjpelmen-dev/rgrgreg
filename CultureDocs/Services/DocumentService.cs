using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Services
{
    public static class DocumentService
    {
        // Получить документы с фильтрами
        public static List<Document> GetDocuments(string title, string docType,
            int? authorId, string status, int? categoryId, int? eventId)
        {
            List<Document> list = new List<Document>();

            string sql = @"SELECT d.*, 
                                  a.LastName + ' ' + a.FirstName AS AuthorName,
                                  e.EventName, c.CategoryName
                           FROM Documents d
                           INNER JOIN Authors a ON d.AuthorID = a.AuthorID
                           LEFT JOIN Events e ON d.EventID = e.EventID
                           LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                           WHERE 1=1";

            if (!string.IsNullOrEmpty(title)) sql += " AND d.Title LIKE @title";
            if (!string.IsNullOrEmpty(docType)) sql += " AND d.DocType = @docType";
            if (authorId.HasValue) sql += " AND d.AuthorID = @authorId";
            if (!string.IsNullOrEmpty(status)) sql += " AND d.Status = @status";
            if (categoryId.HasValue) sql += " AND d.CategoryID = @categoryId";
            if (eventId.HasValue) sql += " AND d.EventID = @eventId";

            sql += " ORDER BY d.CreatedDate DESC";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(title))
                        cmd.Parameters.AddWithValue("@title", "%" + title + "%");
                    if (!string.IsNullOrEmpty(docType))
                        cmd.Parameters.AddWithValue("@docType", docType);
                    if (authorId.HasValue)
                        cmd.Parameters.AddWithValue("@authorId", authorId.Value);
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@status", status);
                    if (categoryId.HasValue)
                        cmd.Parameters.AddWithValue("@categoryId", categoryId.Value);
                    if (eventId.HasValue)
                        cmd.Parameters.AddWithValue("@eventId", eventId.Value);

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Document
                            {
                                DocumentID = (int)r["DocumentID"],
                                Title = r["Title"].ToString(),
                                DocType = r["DocType"].ToString(),
                                Genre = r["Genre"] == DBNull.Value ? "" : r["Genre"].ToString(),
                                Description = r["Description"] == DBNull.Value ? "" : r["Description"].ToString(),
                                CreatedDate = (DateTime)r["CreatedDate"],
                                Status = r["Status"].ToString(),
                                AuthorID = (int)r["AuthorID"],
                                EventID = r["EventID"] as int?,
                                CategoryID = r["CategoryID"] as int?,
                                AuthorName = r["AuthorName"].ToString(),
                                EventName = r["EventName"] == DBNull.Value ? "" : r["EventName"].ToString(),
                                CategoryName = r["CategoryName"] == DBNull.Value ? "" : r["CategoryName"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Добавить документ + первая версия
        public static void AddDocument(Document d, string comment)
        {
            string sql = @"INSERT INTO Documents (Title, DocType, Genre, Description, Status, AuthorID, EventID, CategoryID)
                           VALUES (@title, @type, @genre, @desc, @status, @author, @event, @cat);
                           SELECT SCOPE_IDENTITY();";

            int newId;
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", d.Title);
                    cmd.Parameters.AddWithValue("@type", d.DocType);
                    cmd.Parameters.AddWithValue("@genre", (object)d.Genre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@desc", (object)d.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", d.Status);
                    cmd.Parameters.AddWithValue("@author", d.AuthorID);
                    cmd.Parameters.AddWithValue("@event", (object)d.EventID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cat", (object)d.CategoryID ?? DBNull.Value);
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            string firstComment = string.IsNullOrEmpty(comment) ? "Первая версия" : comment;
            VersionService.CreateVersion(newId, d.AuthorID, firstComment, null);
        }

        // Обновить документ
        public static void UpdateDocument(Document d)
        {
            string sql = @"UPDATE Documents SET Title=@title, DocType=@type, Genre=@genre,
                           Description=@desc, Status=@status, EventID=@event, CategoryID=@cat
                           WHERE DocumentID=@id";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", d.Title);
                    cmd.Parameters.AddWithValue("@type", d.DocType);
                    cmd.Parameters.AddWithValue("@genre", (object)d.Genre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@desc", (object)d.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", d.Status);
                    cmd.Parameters.AddWithValue("@event", (object)d.EventID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cat", (object)d.CategoryID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", d.DocumentID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удалить документ и все версии
        public static void DeleteDocument(int documentId)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Versions WHERE DocumentID=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", documentId);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Documents WHERE DocumentID=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", documentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Проверка дубликата названия
        public static bool IsTitleDuplicate(string title, int? excludeId)
        {
            string sql = "SELECT COUNT(*) FROM Documents WHERE Title=@t";
            if (excludeId.HasValue) sql += " AND DocumentID<>@id";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@t", title);
                    if (excludeId.HasValue)
                        cmd.Parameters.AddWithValue("@id", excludeId.Value);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}