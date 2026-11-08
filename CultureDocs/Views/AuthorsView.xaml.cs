using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Views
{
    public partial class AuthorsView : Window
    {
        public AuthorsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAuthors();
        }

        private void LoadAuthors()
        {
            List<Author> list = new List<Author>();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT * FROM Authors ORDER BY LastName", conn))
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Author
                            {
                                AuthorID = (int)r["AuthorID"],
                                LastName = r["LastName"].ToString(),
                                FirstName = r["FirstName"].ToString(),
                                MiddleName = r["MiddleName"] == DBNull.Value
                                    ? "" : r["MiddleName"].ToString(),
                                Position = r["Position"].ToString(),
                                Phone = r["Phone"] == DBNull.Value ? "" : r["Phone"].ToString(),
                                Email = r["Email"] == DBNull.Value ? "" : r["Email"].ToString()
                            });
                }
                GridAuthors.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string ln = Microsoft.VisualBasic.Interaction.InputBox("Фамилия:", "Новый автор", "");
            if (ln == "") return;
            string fn = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Новый автор", "");
            if (fn == "") return;
            string pos = Microsoft.VisualBasic.Interaction.InputBox("Должность:", "Новый автор", "");
            if (pos == "") return;
            string ph = Microsoft.VisualBasic.Interaction.InputBox("Телефон:", "Новый автор", "");
            string em = Microsoft.VisualBasic.Interaction.InputBox("Email:", "Новый автор", "");

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Authors (LastName, FirstName, Position, Phone, Email)
                                   VALUES (@ln, @fn, @pos, @ph, @em)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ln", ln);
                        cmd.Parameters.AddWithValue("@fn", fn);
                        cmd.Parameters.AddWithValue("@pos", pos);
                        cmd.Parameters.AddWithValue("@ph", ph);
                        cmd.Parameters.AddWithValue("@em", em);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadAuthors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Author a = GridAuthors.SelectedItem as Author;
            if (a == null) { MessageBox.Show("Выберите автора."); return; }

            if (MessageBox.Show("Удалить автора «" + a.FullName + "»?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();

                        // Проверка ссылочной целостности
                        using (SqlCommand check = new SqlCommand(
                            "SELECT COUNT(*) FROM Documents WHERE AuthorID=@id", conn))
                        {
                            check.Parameters.AddWithValue("@id", a.AuthorID);
                            if ((int)check.ExecuteScalar() > 0)
                            {
                                MessageBox.Show("Невозможно удалить: у автора есть документы.");
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Authors WHERE AuthorID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", a.AuthorID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadAuthors();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAuthors();
        }
    }
}