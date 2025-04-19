using PW_13;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void AuthTestAllIsTrue()
        {
            var page = new AuthPage();
            Assert.IsTrue(page.Auth("Admin", "123456"));
            Assert.IsTrue(page.Auth("Adminn", "123456"));
        }

        [TestMethod]
        public void AuthTestAllIsFalse()
        {
            var page = new AuthPage();
            Assert.IsFalse(page.Auth("Admin123", "123456"));
            Assert.IsFalse(page.Auth("Админ", "123456"));
            Assert.IsFalse(page.Auth("Admin", "12345"));
            Assert.IsFalse(page.Auth("Admin123", "А123456"));
            Assert.IsFalse(page.Auth("", "123456"));
            Assert.IsFalse(page.Auth("Admin123", ""));
            Assert.IsFalse(page.Auth(" ", "123456"));
            Assert.IsFalse(page.Auth("Admin123", " "));
        }
            
    }
}
