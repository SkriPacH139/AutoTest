using PW_13;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest4
    {
        [TestMethod]
        public void RegTestAllIsTrue()
        {
            var page = new RegPage();
            // Все поля заполнены корректно, разные уникальные логины
            Assert.IsTrue(page.Reg("Ivanov", "Ivan", "Ivanovich", "userPos1", "Passw0rd", "Passw0rd"));
            Assert.IsTrue(page.Reg("Petrov", "Petr", "Petrovich", "userPos2", "AbC12345", "AbC12345"));
            Assert.IsTrue(page.Reg("Sidorov", "Sidr", "Sidorovich", "userPos3", "XyZ78901", "XyZ78901"));
            Assert.IsTrue(page.Reg("Smirnov", "Sergey", "Sergeevich", "userPos4", "Qwerty1", "Qwerty1"));
            Assert.IsTrue(page.Reg("Volkov", "Volk", "Volkovich", "userPos5", "Alpha99", "Alpha99"));
        }

        [TestMethod]
        public void RegTestAllIsFalse()
        {
            var page = new RegPage();

            // Подготовка дубликата: первый вызов - создаём пользователя
            page.Reg("Dupov", "Dup", "Dupovich", "dupLogin", "Valid1", "Valid1");

            // 1) Пустые поля
            Assert.IsFalse(page.Reg("", "Ivan", "Ivanovich", "userNeg1", "Passw0rd", "Passw0rd"));
            Assert.IsFalse(page.Reg("Ivanov", "", "Ivanovich", "userNeg2", "Passw0rd", "Passw0rd"));
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "", "userNeg3", "Passw0rd", "Passw0rd"));
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "", "Passw0rd", "Passw0rd"));
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg4", "", ""));

            // 2) Пароль не соответствует требованиям
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg5", "Ab1", "Ab1"));      // <6 символов
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg6", "ABCdef", "ABCdef"));   // нет цифр
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg7", "Abc!23", "Abc!23"));   // спецсимвол
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg8", "Пароль1", "Пароль1"));  // кириллица

            // 3) Несовпадение паролей
            Assert.IsFalse(page.Reg("Ivanov", "Ivan", "Ivanovich", "userNeg9", "Password1", "Password2"));

            // 4) Дубликат логина
            Assert.IsFalse(page.Reg("Dupov", "Dup", "Dupovich", "dupLogin", "Valid1", "Valid1"));
        }
    }
}
