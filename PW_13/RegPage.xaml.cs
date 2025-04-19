using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PW_13
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack(); // Возврат на страницу авторизации
        private void RegisterButton_Click(object sender, RoutedEventArgs e) => Reg(TextBoxSurname.Text.Trim(), TextBoxName.Text.Trim(), TextBoxPatronymic.Text.Trim(), TextBoxLogin.Text.Trim(), PasswordBox.Password, PasswordBoxConfirm.Password);

        public bool Reg(string surname, string name, string patronymic, string login, string password, string passwordConfirm)
        {

            // Проверка заполненности полей
            if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(patronymic) ||
                string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
            {
                MessageTextBlock.Text = "Пожалуйста, заполните все поля.";
                return false;
            }

            // Проверка наличия логина в базе данных
            using (PW_13Entities db = new PW_13Entities())
            {
                if (db.User.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageTextBlock.Text = "Логин уже существует. Пожалуйста, выберите другой.";
                    return false;
                }
            }

            // Проверка формата пароля
            if (!IsPasswordValid(password))
            {
                MessageTextBlock.Text = "Пароль должен содержать минимум 6 символов, включать английские буквы и хотя бы одну цифру.";
                return false;
            }

            // Проверка совпадения паролей
            if (password != passwordConfirm)
            {
                MessageTextBlock.Text = "Пароли не совпадают.";
                return false;
            }

            // Если все проверки пройдены
            using (PW_13Entities db = new PW_13Entities())
            {
                User userObject = new User
                {
                    FIO = $"{surname} {name} {patronymic}",
                    Login = login,
                    Password = password
                };

                db.User.Add(userObject);
                db.SaveChanges();
            }

            //MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.");
            TextBoxSurname.Clear();
            TextBoxName.Clear();
            TextBoxPatronymic.Clear();
            TextBoxLogin.Clear();
            PasswordBox.Clear();
            PasswordBoxConfirm.Clear();
            //NavigationService.Navigate(new AuthPage()); // Возврат на страницу авторизации
            return true;
        }              
             
        private bool IsPasswordValid(string password)
        {
            return password.Length >= 6 &&
                   Regex.IsMatch(password, @"^[a-zA-Z0-9]*$") &&
                   Regex.IsMatch(password, @"\d");
        }
    }
}
