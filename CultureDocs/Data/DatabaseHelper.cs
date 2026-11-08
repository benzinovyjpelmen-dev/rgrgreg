using System.Data.SqlClient;

namespace CultureDocs.Data
{
    public static class DatabaseHelper
    {
        // Строка подключения — поменяйте на свой сервер
        public static string ConnectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=CultureDocs;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}