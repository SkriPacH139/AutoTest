using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int _failedAttempts = 0;
        private string _currentCaptcha;

        public AuthPage()
        {
            InitializeComponent();
            InitializePlaceholders();
        }

        private void InitializePlaceholders()
        {
            // Инициализация видимости заполнителей
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e) => LoginPlaceholder.Visibility = Visibility.Collapsed;
        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e) => LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e) => PasswordPlaceholder.Visibility = Visibility.Collapsed;
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e) => PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        private void RegisterButton_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new RegPage());

        private void ShowCaptcha()
        {
            // Генерируем случайную строку из 5 символов [A–Z0–9]
            var rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            _currentCaptcha = new string(Enumerable.Range(0, 5)
                                         .Select(_ => chars[rnd.Next(chars.Length)]).ToArray());

            // Создаём маленькое изображение и «шумим»
            var bmp = new RenderTargetBitmap(200, 60, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // фон
                dc.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, 200, 60));
                // шум: случайные линии
                for (int i = 0; i < 20; i++)
                {
                    var p1 = new Point(rnd.Next(0, 200), rnd.Next(0, 60));
                    var p2 = new Point(rnd.Next(0, 200), rnd.Next(0, 60));
                    dc.DrawLine(new Pen(Brushes.DarkGray, 1), p1, p2);
                }
                // текст капчи
                var ft = new FormattedText(
                    _currentCaptcha,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"), 32, Brushes.Black, 1.0);
                dc.DrawText(ft, new Point(20, 10));
            }
            bmp.Render(dv);

            CaptchaImage.Source = bmp;
            CaptchaImage.Visibility = Visibility.Visible;
            CaptchaInput.Clear();
            CaptchaInput.Visibility = Visibility.Visible;
        }

        public bool Auth(string login, string password, string captchaResponse = null)
        {
            if (_failedAttempts >= 3)
            {
                if (string.IsNullOrEmpty(captchaResponse) || captchaResponse.Trim().ToUpper() != _currentCaptcha)
                {
                    MessageTextBlock.Text = "Неверная капча.";
                    _failedAttempts++;
                    return false;
                }
            }

            // Проверка заполненности полей
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageTextBlock.Text = "Пожалуйста, заполните все поля.";

                _failedAttempts++;
                if (_failedAttempts == 3) ShowCaptcha();

                return false;
            }

            // Проверка существования учетной записи
            using (var db = new PW_13Entities()) // Создаем экземпляр контекста
            {
                var user = db.User // Получаем доступ к таблице пользователей
                             .FirstOrDefault(u => u.Login == login && u.Password == password); // Поиск пользователя

                if (user != null)
                {
                    // Успешная авторизация
                    MessageTextBlock.Text = $"Здравствуйте, {user.FIO}!";

                    _failedAttempts = 0;
                    CaptchaImage.Visibility = Visibility.Collapsed;
                    CaptchaInput.Visibility = Visibility.Collapsed;

                    LoginTextBox.Clear();
                    PasswordBox.Clear();
                    return true;

                }
                // Учетная запись не найдена
                MessageTextBlock.Text = "Неверный логин или пароль.";

                _failedAttempts++;
                if (_failedAttempts == 3) ShowCaptcha();

                return false;
                
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            var capResponse = CaptchaInput.Visibility == Visibility.Visible ? CaptchaInput.Text : null;

            Auth(LoginTextBox.Text.Trim(), PasswordBox.Password, capResponse);
        }
       
        
    }
}
