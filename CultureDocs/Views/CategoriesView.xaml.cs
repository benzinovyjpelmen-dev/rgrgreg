using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Views
{
    public partial class CategoriesView : Window
    {
        public CategoriesView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            List<Category> list = new List<Category>();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT * FROM Categories ORDER BY CategoryName", conn))
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Category
                            {
                                CategoryID = (int)r["CategoryID"],
                                CategoryName = r["CategoryName"].ToString(),
                                Description = r["Description"] == DBNull.Value
                                    ? "" : r["Description"].ToString()
                            });
                }
                GridCategories.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Название категории:", "Новая категория", "");
            if (name == "") return;
            string desc = Microsoft.VisualBasic.Interaction.InputBox(
                "Описание:", "Новая категория", "");

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "INSERT INTO Categories (CategoryName, Description) VALUES (@n, @d)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@d", desc);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Category c = GridCategories.SelectedItem as Category;
            if (c == null) { MessageBox.Show("Выберите категорию."); return; }

            if (MessageBox.Show("Удалить категорию «" + c.CategoryName + "»?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();

                        using (SqlCommand check = new SqlCommand(
                            "SELECT COUNT(*) FROM Documents WHERE CategoryID=@id", conn))
                        {
                            check.Parameters.AddWithValue("@id", c.CategoryID);
                            if ((int)check.ExecuteScalar() > 0)
                            {
                                MessageBox.Show("Невозможно удалить: есть связанные документы.");
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Categories WHERE CategoryID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", c.CategoryID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }
    }
}