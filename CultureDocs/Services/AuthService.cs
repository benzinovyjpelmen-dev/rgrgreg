using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Services
{
    public static class AuthService
    {
        public static User CurrentUser = null;

        // Хеширование пароля
        public static string ComputeSha256Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                string result = "";
                foreach (byte b in bytes)
                    result += b.ToString("x2");
                return result;
            }
        }

        // Вход в систему
        public static bool Authenticate(string login, string password)
        {
            string hash = ComputeSha256Hash(password);
            string sql = @"SELECT u.UserID, u.Login, u.RoleID, u.AuthorID, r.RoleName,
                                  a.LastName + ' ' + a.FirstName AS AuthorName
                           FROM Users u
                           INNER JOIN Roles r ON u.RoleID = r.RoleID
                           LEFT JOIN Authors a ON u.AuthorID = a.AuthorID
                           WHERE u.Login = @login AND u.PasswordHash = @hash";

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@hash", hash);

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            CurrentUser = new User();
                            CurrentUser.UserID = (int)r["UserID"];
                            CurrentUser.Login = r["Login"].ToString();
                            CurrentUser.RoleID = (int)r["RoleID"];
                            CurrentUser.AuthorID = r["AuthorID"] as int?;
                            CurrentUser.RoleName = r["RoleName"].ToString();
                            CurrentUser.AuthorName = r["AuthorName"] == System.DBNull.Value
                                ? null : r["AuthorName"].ToString();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        // Проверки прав
        public static bool IsAdmin()
        {
            return CurrentUser != null && CurrentUser.RoleName == "Администратор";
        }

        public static bool CanEdit()
        {
            if (CurrentUser == null) return false;
            return CurrentUser.RoleName == "Администратор"
                || CurrentUser.RoleName == "Методист"
                || CurrentUser.RoleName == "Режиссёр";
        }

        public static bool CanDelete()
        {
            if (CurrentUser == null) return false;
            return CurrentUser.RoleName == "Администратор"
                || CurrentUser.RoleName == "Методист";
        }
    }
}