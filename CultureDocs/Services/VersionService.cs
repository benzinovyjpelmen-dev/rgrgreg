using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CultureDocs.Data;

namespace CultureDocs.Services
{
    public static class VersionService
    {
        // Получить все версии документа
        public static List<CultureDocs.Models.Version> GetVersions(int documentId)
        {
            List<CultureDocs.Models.Version> list = new List<CultureDocs.Models.Version>();
            string sql = @"SELECT v.*, a.LastName + ' ' + a.FirstName AS AuthorName
                           FROM Versions v
                           INNER JOIN Authors a ON v.AuthorID = a.AuthorID
                           WHERE v.DocumentID = @docId
                           ORDER BY v.VersionNumber DESC";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@docId", documentId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new CultureDocs.Models.Version
                            {
                                VersionID = (int)r["VersionID"],
                                DocumentID = (int)r["DocumentID"],
                                VersionNumber = (int)r["VersionNumber"],
                                CreatedDate = (DateTime)r["CreatedDate"],
                                AuthorID = (int)r["AuthorID"],
                                Comment = r["Comment"] == DBNull.Value ? "" : r["Comment"].ToString(),
                                FilePath = r["FilePath"] == DBNull.Value ? "" : r["FilePath"].ToString(),
                                AuthorName = r["AuthorName"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Создать новую версию (номер = MAX + 1)
        public static void CreateVersion(int documentId, int authorId, string comment, string filePath)
        {
            string sql = @"INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment, FilePath)
                           VALUES (@docId,
                                   ISNULL((SELECT MAX(VersionNumber) FROM Versions WHERE DocumentID=@docId), 0) + 1,
                                   @author, @comment, @file)";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@docId", documentId);
                    cmd.Parameters.AddWithValue("@author", authorId);
                    cmd.Parameters.AddWithValue("@comment", (object)comment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@file", (object)filePath ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Восстановить версию (создаёт новую на её основе)
        public static void RestoreVersion(int versionId, int authorId)
        {
            string select = "SELECT DocumentID, VersionNumber, Comment FROM Versions WHERE VersionID=@id";
            int docId = 0;
            int num = 0;
            string comment = "";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(select, conn))
                {
                    cmd.Parameters.AddWithValue("@id", versionId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            docId = (int)r["DocumentID"];
                            num = (int)r["VersionNumber"];
                            comment = r["Comment"] == DBNull.Value ? "" : r["Comment"].ToString();
                        }
                    }
                }
            }

            CreateVersion(docId, authorId, "Восстановлено из версии " + num + ". " + comment, null);
        }
    }
}