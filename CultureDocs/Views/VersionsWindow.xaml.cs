using System;
using System.Windows;
using CultureDocs.Models;
using CultureDocs.Services;

namespace CultureDocs.Views
{
    public partial class VersionsWindow : Window
    {
        private Document _doc;

        public VersionsWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            TxtHeader.Text = "История версий: " + doc.Title;
            LoadVersions();
        }

        private void LoadVersions()
        {
            try
            {
                GridVersions.ItemsSource = VersionService.GetVersions(_doc.DocumentID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки версий: " + ex.Message);
            }
        }

        private void BtnCreateVersion_Click(object sender, RoutedEventArgs e)
        {
            string comment = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите комментарий к версии:", "Новая версия", "");
            if (comment == "") return;

            try
            {
                int authorId = AuthService.CurrentUser.AuthorID ?? _doc.AuthorID;
                VersionService.CreateVersion(_doc.DocumentID, authorId, comment, null);
                LoadVersions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            // Полное имя типа — снимает неоднозначность
            CultureDocs.Models.Version v =
                GridVersions.SelectedItem as CultureDocs.Models.Version;

            if (v == null)
            {
                MessageBox.Show("Выберите версию.");
                return;
            }

            if (MessageBox.Show("Восстановить версию №" + v.VersionNumber +
                "?\nБудет создана новая версия на её основе.",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int authorId = AuthService.CurrentUser.AuthorID ?? _doc.AuthorID;
                    VersionService.RestoreVersion(v.VersionID, authorId);
                    LoadVersions();
                    MessageBox.Show("Версия восстановлена.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}