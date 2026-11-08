using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using CultureDocs.Data;
using CultureDocs.Models;

namespace CultureDocs.Views
{
    public partial class EventsView : Window
    {
        public EventsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEvents();
        }

        private void LoadEvents()
        {
            List<Event> list = new List<Event>();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT * FROM Events ORDER BY EventDate DESC", conn))
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Event
                            {
                                EventID = (int)r["EventID"],
                                EventName = r["EventName"].ToString(),
                                EventDate = (DateTime)r["EventDate"],
                                Place = r["Place"] == DBNull.Value ? "" : r["Place"].ToString(),
                                EventType = r["EventType"] == DBNull.Value
                                    ? "" : r["EventType"].ToString()
                            });
                }
                GridEvents.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Название мероприятия:", "Новое мероприятие", "");
            if (name == "") return;

            string dateStr = Microsoft.VisualBasic.Interaction.InputBox(
                "Дата (дд.мм.гггг):", "Новое мероприятие", "");
            DateTime date;
            if (!DateTime.TryParse(dateStr, out date))
            {
                MessageBox.Show("Некорректная дата.");
                return;
            }

            string place = Microsoft.VisualBasic.Interaction.InputBox(
                "Место проведения:", "Новое мероприятие", "");
            string type = Microsoft.VisualBasic.Interaction.InputBox(
                "Тип мероприятия:", "Новое мероприятие", "");

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Events (EventName, EventDate, Place, EventType)
                                   VALUES (@name, @date, @place, @type)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@place", place);
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Event ev = GridEvents.SelectedItem as Event;
            if (ev == null) { MessageBox.Show("Выберите мероприятие."); return; }

            if (MessageBox.Show("Удалить мероприятие «" + ev.EventName + "»?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();

                        // Проверка ссылок
                        using (SqlCommand check = new SqlCommand(
                            "SELECT COUNT(*) FROM Documents WHERE EventID=@id", conn))
                        {
                            check.Parameters.AddWithValue("@id", ev.EventID);
                            if ((int)check.ExecuteScalar() > 0)
                            {
                                MessageBox.Show("Невозможно удалить: есть связанные документы.");
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Events WHERE EventID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", ev.EventID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadEvents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEvents();
        }
    }
}