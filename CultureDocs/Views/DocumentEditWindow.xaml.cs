using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using CultureDocs.Data;
using CultureDocs.Models;
using CultureDocs.Services;

namespace CultureDocs.Views
{
    public partial class DocumentEditWindow : Window
    {
        private Document _doc;
        private bool _isNew;

        public DocumentEditWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            _isNew = (doc == null);
            Title = _isNew ? "Новый документ" : "Редактирование документа";

            // Заполняем типы
            CmbDocType.Items.Add("Сценарий");
            CmbDocType.Items.Add("Методическая разработка");
            CmbDocType.Items.Add("Положение");
            CmbDocType.Items.Add("План");
            CmbDocType.Items.Add("Отчёт");
            CmbDocType.SelectedIndex = 0;

            // Статусы
            CmbStatus.Items.Add("черновик");
            CmbStatus.Items.Add("на согласовании");
            CmbStatus.Items.Add("утверждён");
            CmbStatus.Items.Add("архив");
            CmbStatus.SelectedIndex = 0;

            // Авторы
            foreach (Author a in LoadAuthors()) CmbAuthor.Items.Add(a);
            if (CmbAuthor.Items.Count > 0) CmbAuthor.SelectedIndex = 0;

            // Категории
            CmbCategory.Items.Add(new Category { CategoryID = 0, CategoryName = "" });
            foreach (Category c in LoadCategories()) CmbCategory.Items.Add(c);
            CmbCategory.SelectedIndex = 0;

            // Мероприятия
            CmbEvent.Items.Add(new Event { EventID = 0, EventName = "" });
            foreach (Event ev in LoadEvents()) CmbEvent.Items.Add(ev);
            CmbEvent.SelectedIndex = 0;

            if (_isNew)
            {
                TxtCreatedDate.Text = "(будет установлена автоматически)";
            }
            else
            {
                TxtTitle.Text = _doc.Title;
                CmbDocType.SelectedItem = _doc.DocType;
                TxtGenre.Text = _doc.Genre;
                CmbStatus.SelectedItem = _doc.Status;
                TxtDescription.Text = _doc.Description;
                TxtCreatedDate.Text = _doc.CreatedDate.ToString("dd.MM.yyyy HH:mm");

                foreach (Author a in CmbAuthor.Items)
                    if (a.AuthorID == _doc.AuthorID) { CmbAuthor.SelectedItem = a; break; }

                foreach (Category c in CmbCategory.Items)
                    if (c.CategoryID == (_doc.CategoryID ?? 0)) { CmbCategory.SelectedItem = c; break; }

                foreach (Event ev in CmbEvent.Items)
                    if (ev.EventID == (_doc.EventID ?? 0)) { CmbEvent.SelectedItem = ev; break; }
            }
        }

        private List<Author> LoadAuthors()
        {
            List<Author> list = new List<Author>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT AuthorID, LastName, FirstName, MiddleName FROM Authors", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
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

        private List<Category> LoadCategories()
        {
            List<Category> list = new List<Category>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT CategoryID, CategoryName FROM Categories", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Category
                        {
                            CategoryID = (int)r["CategoryID"],
                            CategoryName = r["CategoryName"].ToString()
                        });
            }
            return list;
        }

        private List<Event> LoadEvents()
        {
            List<Event> list = new List<Event>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT EventID, EventName FROM Events", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Event
                        {
                            EventID = (int)r["EventID"],
                            EventName = r["EventName"].ToString()
                        });
            }
            return list;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (TxtTitle.Text.Trim() == "")
            {
                MessageBox.Show("Введите название документа.");
                TxtTitle.Focus();
                return;
            }
            if (CmbDocType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип документа.");
                return;
            }
            if (CmbAuthor.SelectedItem == null || ((Author)CmbAuthor.SelectedItem).AuthorID == 0)
            {
                MessageBox.Show("Выберите автора.");
                return;
            }
            if (CmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус.");
                return;
            }

            try
            {
                int? id = _isNew ? (int?)null : _doc.DocumentID;
                if (DocumentService.IsTitleDuplicate(TxtTitle.Text.Trim(), id))
                {
                    MessageBox.Show("Документ с таким названием уже существует.");
                    return;
                }

                Author a = CmbAuthor.SelectedItem as Author;
                Category c = CmbCategory.SelectedItem as Category;
                Event ev = CmbEvent.SelectedItem as Event;

                if (_isNew)
                {
                    Document d = new Document();
                    d.Title = TxtTitle.Text.Trim();
                    d.DocType = CmbDocType.SelectedItem.ToString();
                    d.Genre = TxtGenre.Text.Trim();
                    d.Description = TxtDescription.Text.Trim();
                    d.Status = CmbStatus.SelectedItem.ToString();
                    d.AuthorID = a.AuthorID;
                    d.CategoryID = (c != null && c.CategoryID > 0) ? (int?)c.CategoryID : null;
                    d.EventID = (ev != null && ev.EventID > 0) ? (int?)ev.EventID : null;

                    DocumentService.AddDocument(d, TxtComment.Text.Trim());
                    MessageBox.Show("Документ добавлен.");
                }
                else
                {
                    _doc.Title = TxtTitle.Text.Trim();
                    _doc.DocType = CmbDocType.SelectedItem.ToString();
                    _doc.Genre = TxtGenre.Text.Trim();
                    _doc.Description = TxtDescription.Text.Trim();
                    _doc.Status = CmbStatus.SelectedItem.ToString();
                    _doc.CategoryID = (c != null && c.CategoryID > 0) ? (int?)c.CategoryID : null;
                    _doc.EventID = (ev != null && ev.EventID > 0) ? (int?)ev.EventID : null;

                    DocumentService.UpdateDocument(_doc);

                    // Если указан комментарий — создаём новую версию
                    if (TxtComment.Text.Trim() != "")
                    {
                        int authorId = AuthService.CurrentUser.AuthorID ?? _doc.AuthorID;
                        VersionService.CreateVersion(_doc.DocumentID, authorId,
                            TxtComment.Text.Trim(), null);
                    }
                    MessageBox.Show("Изменения сохранены.");
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}