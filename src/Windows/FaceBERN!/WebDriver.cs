﻿using Awesomium;
using Awesomium.Core;
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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceBERN_
{
    [TestFixture]
    public class WebDriver
    {
        private IWebDriver _driverFirefox;
        private FirefoxProfile _profileFirefox;
        private ISelenium selenium;
        private WebView awesomium;
        private bool documentReady;

        public int error = 0;
        public const int ERROR_NOCREDENTIALS = 1;
        public const int ERROR_BADCREDENTIALS = 2;
        public const int ERROR_UNEXPECTED = 3;

        SynchronizationContext awesomiumContext;

        [TestFixtureSetUp]
        public void FixtureSetup(int browser)
        {
            switch (browser)
            {
                case Globals.FIREFOX:
                    _driverFirefox = new FirefoxDriver();
                    _driverFirefox.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, Globals.__TIMEOUT__));
                    Maximize(browser);
                    break;
                case Globals.AWESOMIUM:
                    Thread awesomiumThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                    {
                        WebCore.Started += (s, e) =>
                        {
                            awesomiumContext = SynchronizationContext.Current;
                        };

                        WebCore.Run();
                    }));

                    awesomiumThread.Start();
                    WebCore.Initialize(new WebConfig() { });
                    //WebCore.Initialize(new WebConfig(), true);
                    awesomium = WebCore.CreateWebView(1024, 768, WebViewType.Window);
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
                case Globals.AWESOMIUM:
                    awesomiumContext.Post(state =>
                    {
                        awesomium.Source = new Uri(URL);
                    }, null);
                    //awesomium.Source = new Uri(URL);
                    documentReady = false;
                    /*Task t = new Task(() =>
                    {
                        awesomium.DocumentReady += OnDocumentReadyHandler;
                        awesomium.Source = new Uri(URL);
                        WebCore.Run();
                    });
                    t.Start();*/
                    break;
            }
        }

        /* Alias for TestSetUp.  --Kris */
        public void GoToUrl(int browser, string URL)
        {
            TestSetUp(browser, URL);
        }

        // Used by Awesomium only.  --Kris
        private void OnDocumentReadyHandler(Object sender, DocumentReadyEventArgs e)
        {
            if (e != null && e.ReadyState == DocumentReadyState.Loaded)
            {
                documentReady = true;
            }
            else
            {
                documentReady = false;
            }
        }

        [Test]
        public string GetPageSource(int browser, int retry = 5, int aRetry = 30)
        {
            dynamic driver;

            driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    if (driver.PageSource != null)
                    {
                        return driver.PageSource;
                    }
                    else
                    {
                        retry--;
                        if (retry == 0)
                        {
                            return null;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__);
                            return GetPageSource(browser, retry);
                        }
                    }
                case Globals.AWESOMIUM:
                    if (documentReady)
                    {
                        return awesomium.HTML;
                    }
                    else
                    {
                        aRetry--;
                        if (aRetry == 0)
                        {
                            return null;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__);
                            return GetPageSource(browser, retry, aRetry);
                        }
                    }
            }
        }

        [Test]
        public dynamic GetDriver(int browser)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    return _driverFirefox;
                case Globals.AWESOMIUM:
                    return awesomium;
            }
        }

        [Test]
        public void Maximize(int browser)
        {
            IWebDriver driver = GetDriver(browser);

            string script;
            string name;

            /* Just in case the black magic below doesn't work....  --Kris */
            script = "window.moveTo( 0, 1 ); ";
            script += "window.resizeTo( screen.width, screen.height );";

            ((IJavaScriptExecutor)driver).ExecuteScript(script);

            name = "ed47cd2a4fcb5534a49f6eeb3bfcc564";

            script = "document.title='" + name + "';";

            ((IJavaScriptExecutor)driver).ExecuteScript(script);

            /* This is some real voodoo magic here!  Muaa ha ha ha!!  --Kris */
            IntPtr hWnd = FindWindow(null, name + " - " + Globals.BrowserPIDName(browser));
            if (!hWnd.Equals(IntPtr.Zero))
            {
                ShowWindowAsync(hWnd, 3);  // 3 = Maximize!  --Kris
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string sClassName, string sAppName);

        [Test]
        public dynamic GetElementById(int browser, string elementid, bool iefix = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX:
                        IWebDriver driver = GetDriver(browser);
                        
                        /*
                         * This is necessary to fix a bug in the IE WebDriver.
                         * Basically, what this does is "click" on a parent element in 
                         * order to force IE to give focus to the element we're really 
                         * clicking on.  Otherwise, the field will sometimes fail to gain 
                         * focus without actually throwing an exception, which in turn will 
                         * likely cause another part of the test to fail.
                         * 
                         * --Kris
                         */
                        if (iefix == true && browser == Globals.IE)
                        {
                            driver.FindElement(By.Id(elementid)).FindElement(By.XPath("..")).Click();
                        }

                        return driver.FindElement(By.Id(elementid));
                    case Globals.AWESOMIUM:
                        dynamic document = (Awesomium.Core.JSObject)awesomium.ExecuteJavascriptWithResult("document");
                        return document.getElementById(elementid);
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
        }

        [Test]
        public dynamic GetElementByName(int browser, string elementname, bool iefix = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX:
                        IWebDriver driver = GetDriver(browser);

                        /*
                         * This is necessary to fix a bug in the IE WebDriver.
                         * Basically, what this does is "click" on a parent element in 
                         * order to force IE to give focus to the element we're really 
                         * clicking on.  Otherwise, the field will sometimes fail to gain 
                         * focus without actually throwing an exception, which in turn will 
                         * likely cause another part of the test to fail.
                         * 
                         * --Kris
                         */
                        if (iefix == true && browser == Globals.IE)
                        {
                            driver.FindElement(By.Name(elementname)).FindElement(By.XPath("..")).Click();
                        }

                        return driver.FindElement(By.Name(elementname));
                    case Globals.AWESOMIUM:
                        dynamic document = (Awesomium.Core.JSObject)awesomium.ExecuteJavascriptWithResult("document");
                        return document.getElementByName(elementname);
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
        }

        [Test]
        public dynamic GetElementByLinkText(int browser, string linktext, bool partial = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX:
                        IWebDriver driver = GetDriver(browser);

                        if (partial == true)
                        {
                            return driver.FindElement(By.PartialLinkText(linktext));
                        }
                        else
                        {
                            return driver.FindElement(By.LinkText(linktext));
                        }
                    case Globals.AWESOMIUM:
                        // TODO
                        return null;
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
        }

        [Test]
        public dynamic GetElementByXPath(int browser, string xpath, int timeout = -1)
        {
            dynamic res;
            IWebDriver driver;

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    driver = GetDriver(browser);
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    return null;
            }

            if (timeout >= 0)
            {
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(timeout));
            }

            try
            {
                res = driver.FindElement(By.XPath(xpath));
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
            finally
            {
                if (timeout >= 0)
                {
                    driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
                }
            }

            return res;
        }

        [Test]
        public List<IWebElement> GetElementsByTagName(int browser, string tagName)
        {
            try
            {
                IWebDriver driver = GetDriver(browser);

                return new List<IWebElement>(driver.FindElements(By.TagName(tagName)));
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
        }

        [Test]
        public IWebElement GetInputElementByPlaceholder(int browser, string placeholder)
        {
            return GetElementByTagNameAndAttribute(browser, "input", "placeholder", placeholder);
        }

        [Test]
        public IWebElement GetElementByTagNameAndAttribute(int browser, string tagName, string attributeName, string attributeValue)
        {
            List<IWebElement> eles = GetElementsByTagName(browser, tagName);
            if (eles == null)
            {
                return null;
            }

            foreach (IWebElement ele in eles)
            {
                if (ele.GetAttribute(attributeName).Equals(attributeValue))
                {
                    return ele;
                }
            }

            return null;
        }

        [Test]
        public bool ClickElement(int browser, dynamic element, bool viewportfix = false, bool autoscroll = false)
        {
            dynamic driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        //WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, Globals.__TIMEOUT__));
                        //wait.Until(d => d.FindElement(By.Id(element.GetAttribute("id"))));

                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                        Assert.IsTrue(element.Displayed);
                    }
                    catch
                    {
                        return false;
                    }

                    if (autoscroll == true)
                    {
                        string script;

                        script = "arguments[0].scrollIntoView(true);";

                        ((IJavaScriptExecutor)driver).ExecuteScript(script, element);
                    }

                    if (viewportfix == true)
                    {
                        element.SendKeys(" ");
                    }
                    else
                    {
                        element.Click();
                    }
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }

            return true;
        }

        [Test]
        public void ScrollToBottom(ref IWebDriver driver, int scrollLimit = 100)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));
            
            IJavaScriptExecutor jse = (IJavaScriptExecutor) driver;
            //const string script = "var i=100;var timeId=setInterval(function(){i--;window.scrollY<document.body.scrollHeight-window.screen.availHeight&&i>0?window.scrollTo(0,document.body.scrollHeight):(clearInterval(timeId),window.scrollTo(0,0));return!(window.scrollY<document.body.scrollHeight-window.screen.availHeight&&i>0);},3000);";
            const string scrollScript = "window.scrollTo(0,document.body.scrollHeight);";
            const string checkScript = "return!(window.scrollY<document.body.scrollHeight-window.screen.availHeight);";

            if (scrollLimit > 0)
            {
                bool done = false;
                int i = scrollLimit;
                do
                {
                    jse.ExecuteScript(scrollScript);
                    System.Threading.Thread.Sleep(3000);
                    done = (bool)jse.ExecuteScript(checkScript);
                    i--;
                } while (done == false && i > 0);
            }
            else
            {
                jse.ExecuteScript(scrollScript);
            }

            System.Threading.Thread.Sleep(3000);
        }

        [Test]
        // Modified from:  http://stackoverflow.com/questions/13244225/selenium-how-to-make-the-web-driver-to-wait-for-page-to-refresh-before-executin
        public void WaitForPageLoad(int browser, int maxWaitTimeInSeconds = 60)
        {
            string state = string.Empty;
            dynamic _driver = GetDriver(browser);
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(maxWaitTimeInSeconds));

                //Checks every 500 ms whether predicate returns true if returns exit otherwise keep trying till it returns ture
                wait.Until(d =>
                {
                    try
                    {
                        state = ((IJavaScriptExecutor)_driver).ExecuteScript(@"return document.readyState").ToString();
                    }
                    catch (InvalidOperationException)
                    {
                        //Ignore
                    }
                    catch (NoSuchWindowException)
                    {
                        //when popup is closed, switch to last windows
                        _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                    }
                    //In IE7 there are chances we may get state as loaded instead of complete
                    return (state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) || state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase));

                });
            }
            catch (TimeoutException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (NullReferenceException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (WebDriverException)
            {
                if (_driver.WindowHandles.Count == 1)
                {
                    _driver.SwitchTo().Window(_driver.WindowHandles[0]);
                }
                state = ((IJavaScriptExecutor)_driver).ExecuteScript(@"return document.readyState").ToString();
                if (!(state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) || state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase)))
                    throw;
            }
        }

        [Test]
        /* When all else fails, have JavaScript do it.  --Kris */
        public void JavaScriptClickElementId(int browser, string elementid, bool checkbox = false)
        {
            dynamic driver = GetDriver(browser);
            string script;

            script = (checkbox ? "document.getElementById( '" + elementid + "' ).checked = true;" : "document.getElementById( '" + elementid + "' ).Click();");

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    ((IJavaScriptExecutor)driver).ExecuteScript(script);
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public bool ClickOnLink(int browser, string linktext, bool partial = false, bool viewportfix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByLinkText(browser, linktext, partial);
                        return ClickElement(browser, element, viewportfix);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return ClickOnLink(browser, linktext, partial, viewportfix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool ClickOnXPath(int browser, string xpath, bool viewportfix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByXPath(browser, xpath);
                        return ClickElement(browser, element, viewportfix);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return ClickOnXPath(browser, xpath, viewportfix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool TypeText(int browser, dynamic element, string text)
        {
            dynamic driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        //WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, Globals.__TIMEOUT__));
                        //wait.Until(d => d.FindElement(By.Id(element.GetAttribute("id"))));

                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                        Assert.IsTrue(element.Displayed);
                    }
                    catch
                    {
                        return false;
                    }

                    element.SendKeys(text);

                    return true;
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool TypeInXPath(int browser, string xpath, string text, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByXPath(browser, xpath);
                        return TypeText(browser, element, text);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return TypeInXPath(browser, xpath, text, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool TypeInId(int browser, string elementid, string text, bool iefix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementById(browser, elementid, iefix);
                        return TypeText(browser, element, text);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return TypeInId(browser, elementid, text, iefix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool TypeInName(int browser, string elementname, string text, bool iefix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByName(browser, elementname, iefix);
                        return TypeText(browser, element, text);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return TypeInName(browser, elementname, text, iefix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool ClearText(int browser, IWebElement element)
        {
            dynamic driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                        Assert.IsTrue(element.Displayed);
                    }
                    catch
                    {
                        return false;
                    }

                    element.Clear();

                    return true;
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool ClearXPath(int browser, string xpath, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByXPath(browser, xpath);
                        return ClearText(browser, element);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return ClearXPath(browser, xpath, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool ClearId(int browser, string id, bool iefix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementById(browser, id, iefix);
                        return ClearText(browser, element);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return ClearId(browser, id, iefix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public bool ClearName(int browser, string name, bool iefix = false, int retry = 2)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        IWebElement element = GetElementByName(browser, name, iefix);
                        return ClearText(browser, element);
                    }
                    catch
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            return ClearName(browser, name, iefix, retry);
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [Test]
        public void SwitchToFrame(int browser, string frame, int retry = 2)
        {
            dynamic driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        driver.SwitchTo().DefaultContent();
                        driver.SwitchTo().Frame(Int32.Parse(frame));
                    }
                    catch (Exception e)
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            SwitchToFrame(browser, frame, retry);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public void SwitchToTop(int browser, int retry = 2)
        {
            dynamic driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    try
                    {
                        driver.SwitchTo().DefaultContent();
                    }
                    catch (Exception e)
                    {
                        retry--;
                        if (retry > 0)
                        {
                            System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                            SwitchToTop(browser, retry);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckTextOnPage(int browser, string text, bool wait = false, long endticks = 0, int retrywait = 5)
        {
            dynamic driver;

            driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    /* Recursively wait and retry if specified.  --Kris */
                    if (wait == true)
                    {
                        System.DateTime now = System.DateTime.Now;

                        if (driver.PageSource.Contains(text) == true
                            || now.Ticks >= endticks)
                        {
                            CheckTextOnPage(browser, text);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep((retrywait * 1000));

                            CheckTextOnPage(browser, text, wait, endticks, retrywait);
                        }
                    }
                    else
                    {
                        Assert.IsTrue(driver.PageSource.Contains(text));
                    }
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckTextOnPageRegex(int browser, string regex)
        {
            dynamic driver;

            driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    Assert.IsTrue(Regex.IsMatch(regex, driver.PageSource));
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckPageTitle(int browser, string title)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    Assert.AreEqual(title, _driverFirefox.Title);
                    break;
                case Globals.AWESOMIUM:
                    // TODO
                    break;
            }
        }

        [Test]
        public bool CheckElementExists(int browser, string text, string by)
        {
            dynamic driver;

            driver = GetDriver(browser);

            switch (browser)
            {
                default:
                case Globals.FIREFOX:
                    IWebElement element;
                    switch (by)
                    {
                        case "linktext":
                            element = GetElementByLinkText(browser, text);
                            break;
                        case "id":
                            element = GetElementById(browser, text);
                            break;
                        case "name":
                            element = GetElementById(browser, text);
                            break;
                        default:
                            element = null;
                            break;
                    }
                    return (!(element.Size.IsEmpty));
                case Globals.AWESOMIUM:
                    // TODO
                    return false;
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown(int browser)
        {
            switch (browser)
            {
                case Globals.FIREFOX:
                    if (_driverFirefox != null)
                    {
                        _driverFirefox.Close();
                    }
                    break;
                case Globals.AWESOMIUM:
                    awesomium.DocumentReady -= OnDocumentReadyHandler;
                    WebCore.Shutdown();
                    break;
            }
        }
    }
}
