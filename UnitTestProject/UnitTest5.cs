using Microsoft.VisualStudio.TestTools.UnitTesting;
using PW_13;
using System;
using System.Reflection;
using System.Windows;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest5
    {
        private AuthPage _page;
        private FieldInfo _captchaField;

        [TestInitialize]
        public void Init()
        {
            _page = new AuthPage();
            // рефлексия для доступа к сгенерированному коду капчи
            _captchaField = typeof(AuthPage).GetField("_currentCaptcha", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private string GetCaptcha() => (string)_captchaField.GetValue(_page);

        [TestMethod]
        public void CaptchaTestIsTrue()
        {
            // 1) Успешный вход с первого раза — капча не показывается
            bool ok1 = _page.Auth("Admin", "123456");
            Assert.IsTrue(ok1, "Первый вход должен быть успешным.");
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility, "Капча не должна отображаться.");
            StringAssert.StartsWith(_page.MessageTextBlock.Text, "Здравствуйте", "Должно быть приветствие.");

            // 2) Две неудачные попытки, а затем успешный вход — капча не должна появиться, счётчик сбросится
            _page = new AuthPage();
            _page.Auth("x", "x"); // 1
            _page.Auth("x", "x"); // 2
            bool ok2 = _page.Auth("Admin", "123456");
            Assert.IsTrue(ok2, "Вход после двух неудач должен быть успешным.");
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility, "Капча не должна появиться после двух неудач.");

            // 3) Три неудачи → капча показана → правильная капча + данные → успешный вход
            _page = new AuthPage();
            _page.Auth("a", "a"); // 1
            _page.Auth("a", "a"); // 2
            _page.Auth("a", "a"); // 3 → ShowCaptcha()
            Assert.AreEqual(Visibility.Visible, _page.CaptchaImage.Visibility, "Капча должна появиться после трёх неудач.");
            string code = GetCaptcha();
            bool ok3 = _page.Auth("Admin", "123456", code);
            Assert.IsTrue(ok3, "Вход с правильной капчей после трёх неудач должен пройти.");
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility, "Капча должна скрыться после успешного входа.");

            // 4) Проверка нечувствительности к регистру капчи
            _page = new AuthPage();
            _page.Auth("a", "a"); // 1
            _page.Auth("a", "a"); // 2
            _page.Auth("a", "a"); // 3 → Капча
            string code2 = GetCaptcha();
            bool ok4 = _page.Auth("Admin", "123456", code2.ToLower());
            Assert.IsTrue(ok4, "Капча должна приниматься без учёта регистра.");
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility, "Капча скрывается после успешного прохождения.");

            // 5) Более трёх неудач: капча сохраняется до успеха
            _page = new AuthPage();
            _page.Auth("b", "b"); // 1
            _page.Auth("b", "b"); // 2
            _page.Auth("b", "b"); // 3 → Капча
            string code3 = GetCaptcha();
            _page.Auth("Admin", "123456", code3); // 4 → вход
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility, "Капча скрыта после успеха.");
        }


        [TestMethod]
        public void CaptchaTestsIsFalse()
        {
            // 1) Первая неудача: пустые поля → стандартная ошибка, капча НЕ показывается
            bool r1 = _page.Auth("", "");
            Assert.IsFalse(r1);
            Assert.AreEqual("Пожалуйста, заполните все поля.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility);

            // 2) Вторая неудача: неверные данные → сообщение о неверном логине/пароле, капча всё ещё скрыта
            bool r2 = _page.Auth("wrong", "wrong");
            Assert.IsFalse(r2);
            Assert.AreEqual("Неверный логин или пароль.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Collapsed, _page.CaptchaImage.Visibility);

            // 3) Третья неудача: неверные данные → капча появляется и возвращается false с сообщением об учётке
            bool r3 = _page.Auth("wrong", "wrong");
            Assert.IsFalse(r3);
            Assert.AreEqual("Неверный логин или пароль.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Visible, _page.CaptchaImage.Visibility);
            string initialCaptcha = GetCaptcha();
            Assert.IsFalse(string.IsNullOrEmpty(initialCaptcha), "Код капчи должен быть сгенерирован.");

            // 4) Четвёртая попытка без ввода капчи → сообщение «Неверная капча.», капча остаётся
            bool r4 = _page.Auth("test", "test", captchaResponse: null);
            Assert.IsFalse(r4);
            Assert.AreEqual("Неверная капча.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Visible, _page.CaptchaImage.Visibility);
            Assert.AreEqual(initialCaptcha, GetCaptcha(), "Код капчи не должен меняться после неудачи.");

            // 5) Пятая попытка с некорректной капчей → та же самая ошибка
            bool r5 = _page.Auth("test", "test", captchaResponse: "BADCODE");
            Assert.IsFalse(r5);
            Assert.AreEqual("Неверная капча.", _page.MessageTextBlock.Text);
            Assert.AreEqual(initialCaptcha, GetCaptcha());

            // 6) Шестая попытка: вводим правильную капчу, но неверные данные → сообщение по паролю, капча остаётся
            bool r6 = _page.Auth("wrong", "wrong", captchaResponse: initialCaptcha);
            Assert.IsFalse(r6);
            Assert.AreEqual("Неверный логин или пароль.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Visible, _page.CaptchaImage.Visibility);
            Assert.AreEqual(initialCaptcha, GetCaptcha());

            // 7) Седьмая попытка: вводим капчу с лишними пробелами вокруг (тримминг) → если код верен, должна пройти проверка капчи,
            //     но т.к. данные снова неверны — сообщение по паролю, капча остаётся
            string padded = "  " + initialCaptcha + "  ";
            bool r7 = _page.Auth("wrong", "wrong", captchaResponse: padded);
            Assert.IsFalse(r7);
            Assert.AreEqual("Неверный логин или пароль.", _page.MessageTextBlock.Text);
            Assert.AreEqual(Visibility.Visible, _page.CaptchaImage.Visibility);

            // 8) Восьмая попытка: вводим слишком длинную строку → всегда «Неверная капча.»
            string longInput = initialCaptcha + initialCaptcha;
            bool r8 = _page.Auth("test", "test", captchaResponse: longInput);
            Assert.IsFalse(r8);
            Assert.AreEqual("Неверная капча.", _page.MessageTextBlock.Text);

            // 9) Девятая: после неудачи с правильной капчей счётчик неудач растёт, капча не регенерируется
            Assert.AreEqual(initialCaptcha, GetCaptcha(), "Капча должна оставаться неизменной до успешного прохождения.");
        }

    }
}
