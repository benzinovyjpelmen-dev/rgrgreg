using System;
using System.Windows;
using CultureDocs.Services;

namespace CultureDocs
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            try
            {
                if (AuthService.Authenticate(login, password))
                {
                    MainWindow dw = new MainWindow();
                    dw.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД:\n" + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}