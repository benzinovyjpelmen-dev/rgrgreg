using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Views
{
    public partial class UsersView : Window
    {
        public UsersView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            List<User> list = new List<User>();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT u.UserID, u.Login, r.RoleName,
                                          a.LastName + ' ' + a.FirstName AS AuthorName
                                   FROM Users u
                                   INNER JOIN Roles r ON u.RoleID = r.RoleID
                                   LEFT JOIN Authors a ON u.AuthorID = a.AuthorID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new User
                            {
                                UserID = (int)r["UserID"],
                                Login = r["Login"].ToString(),
                                RoleName = r["RoleName"].ToString(),
                                AuthorName = r["AuthorName"] == DBNull.Value
                                    ? "" : r["AuthorName"].ToString()
                            });
                }
                GridUsers.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}