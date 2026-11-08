using System;
using System.Collections.Generic;
using System.Windows;
using CultureDocs.Models;
using CultureDocs.Services;
using CultureDocs.Views;

namespace CultureDocs
{
    public partial class MainWindow : Window
    {
        private List<Document> _currentDocs = new List<Document>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Информация о пользователе
            if (AuthService.CurrentUser != null)
            {
                StatusUser.Text = "Пользователь: " +
                    (AuthService.CurrentUser.AuthorName ?? AuthService.CurrentUser.Login);
                StatusRole.Text = "Роль: " + AuthService.CurrentUser.RoleName;
            }

            // Права доступа
            BtnAdd.IsEnabled = AuthService.CanEdit();
            BtnEdit.IsEnabled = AuthService.CanEdit();
            BtnDelete.IsEnabled = AuthService.CanDelete();
            BtnVersions.IsEnabled = AuthService.CanEdit();

            // Типы
            CmbDocType.Items.Add("");
            CmbDocType.Items.Add("Сценарий");
            CmbDocType.Items.Add("Методическая разработка");
            CmbDocType.Items.Add("Положение");
            CmbDocType.Items.Add("План");
            CmbDocType.Items.Add("Отчёт");
            CmbDocType.SelectedIndex = 0;

            // Статусы
            CmbStatus.Items.Add("");
            CmbStatus.Items.Add("черновик");
            CmbStatus.Items.Add("на согласовании");
            CmbStatus.Items.Add("утверждён");
            CmbStatus.Items.Add("архив");
            CmbStatus.SelectedIndex = 0;

            // Авторы
            List<Author> authors = LoadAuthors();
            CmbAuthor.Items.Add(new Author { AuthorID = 0, LastName = "" });
            foreach (Author a in authors) CmbAuthor.Items.Add(a);
            CmbAuthor.SelectedIndex = 0;

            LoadDocuments();
        }

        private List<Author> LoadAuthors()
        {
            List<Author> list = new List<Author>();
            using (var conn = Data.DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new System.Data.SqlClient.SqlCommand(
                    "SELECT AuthorID, LastName, FirstName, MiddleName FROM Authors", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Author
                        {
                            AuthorID = (int)r["AuthorID"],
                            LastName = r["LastName"].ToString(),
                            FirstName = r["FirstName"].ToString(),
                            MiddleName = r["MiddleName"] == DBNull.Value
                                ? "" : r["MiddleName"].ToString()
                        });
            }
            return list;
        }

        private void LoadDocuments()
        {
            try
            {
                string title = TxtSearchTitle.Text.Trim();
                string type = CmbDocType.SelectedItem == null ? "" : CmbDocType.SelectedItem.ToString();
                string status = CmbStatus.SelectedItem == null ? "" : CmbStatus.SelectedItem.ToString();
                Author a = CmbAuthor.SelectedItem as Author;
                int? authorId = (a != null && a.AuthorID > 0) ? (int?)a.AuthorID : null;

                _currentDocs = DocumentService.GetDocuments(
                    string.IsNullOrEmpty(title) ? null : title,
                    string.IsNullOrEmpty(type) ? null : type,
                    authorId,
                    string.IsNullOrEmpty(status) ? null : status,
                    null, null);

                GridDocuments.ItemsSource = _currentDocs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e) { LoadDocuments(); }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchTitle.Text = "";
            CmbDocType.SelectedIndex = 0;
            CmbStatus.SelectedIndex = 0;
            CmbAuthor.SelectedIndex = 0;
            LoadDocuments();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditWindow w = new DocumentEditWindow(null);
            if (w.ShowDialog() == true) LoadDocuments();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Document d = GridDocuments.SelectedItem as Document;
            if (d == null) { MessageBox.Show("Выберите документ"); return; }
            DocumentEditWindow w = new DocumentEditWindow(d);
            if (w.ShowDialog() == true) LoadDocuments();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Document d = GridDocuments.SelectedItem as Document;
            if (d == null) { MessageBox.Show("Выберите документ"); return; }

            if (MessageBox.Show("Удалить документ «" + d.Title + "»?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try { DocumentService.DeleteDocument(d.DocumentID); LoadDocuments(); }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void BtnVersions_Click(object sender, RoutedEventArgs e)
        {
            Document d = GridDocuments.SelectedItem as Document;
            if (d == null) { MessageBox.Show("Выберите документ"); return; }
            VersionsWindow w = new VersionsWindow(d);
            w.ShowDialog();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "CSV (*.csv)|*.csv";
            dlg.FileName = "Документы_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExcelExportService.ExportDocumentsToCsv(_currentDocs, dlg.FileName);
                    MessageBox.Show("Экспорт выполнен.");
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void MenuAuthors_Click(object sender, RoutedEventArgs e)
        {
            AuthorsView w = new AuthorsView();
            w.ShowDialog();
        }

        private void MenuEvents_Click(object sender, RoutedEventArgs e)
        {
            EventsView w = new EventsView();
            w.ShowDialog();
        }

        private void MenuCategories_Click(object sender, RoutedEventArgs e)
        {
            CategoriesView w = new CategoriesView();
            w.ShowDialog();
        }

        private void MenuReports_Click(object sender, RoutedEventArgs e)
        {
            ReportsView w = new ReportsView();
            w.ShowDialog();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Система учёта методической и сценарной документации\n" +
                "творческого отдела дворца культуры\n\n" +
                "Курсовой проект по МДК 03.02\n" +
                "Разработка кода информационных систем\n\n" +
                "© 2026",
                "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            LoginWindow lw = new LoginWindow();
            lw.Show();
            this.Close();
        }
    }
}