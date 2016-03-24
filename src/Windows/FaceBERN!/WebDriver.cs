using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    [TestFixture]
    public class WebDriver
    {
        private IWebDriver _driverFirefox;
        private FirefoxProfile _profileFirefox;
        private ISelenium selenium;

        [TestFixtureSetUp]
        public void FixtureSetup(int browser)
        {
            switch (browser)
            {
                case Globals.FIREFOX:
                    _driverFirefox = new FirefoxDriver();
                    _driverFirefox.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, Globals.__TIMEOUT__));
                    break;
            }
        }

        [SetUp]
        public void TestSetUp(int browser, string URL)
        {
            switch (browser)
            {
                case Globals.FIREFOX:
                    _driverFirefox.Navigate().GoToUrl(URL);
                    break;
                case Globals.SIMPLE:
                    // TODO
                    break;
            }
        }
    }
}
