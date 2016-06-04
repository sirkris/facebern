using NHtmlUnit;
using NHtmlUnit.Html;
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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    [TestFixture]
    public class WebDriver
    {
        private IWebDriver _driverFirefox;
        private FirefoxProfile _profileFirefox;
        private ISelenium selenium;
        private NHtmlUnit.WebClient webClient;
        private HtmlPage page;
        private bool documentReady;

        public int error = 0;
        public const int ERROR_NOCREDENTIALS = 1;
        public const int ERROR_BADCREDENTIALS = 2;
        public const int ERROR_UNEXPECTED = 3;

        private string logName = "WebDriver";
        public Log WebDriverLog;
        private Form1 Main;

        private int browser;

        private int staleRetry = 0;

        public WebDriver(Form1 Main, int browser, Log MainLog = null)
        {
            this.Main = Main;
            if (MainLog == null)
            {
                InitLog();
            }
            else
            {
                WebDriverLog = MainLog;
                WebDriverLog.Init("WebDriver");
            }

            this.browser = browser;
        }

        private void InitLog()
        {
            WebDriverLog = new Log();
            WebDriverLog.Init(logName);
        }

        private void Log(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Log(text, show, appendW, newline, timestamp); }));
            }
            else
            {
                Main.LogW(text, show, appendW, newline, timestamp, logName, WebDriverLog);

                Main.Refresh();
            }
        }

        private void SetExecState(int state)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { SetExecState(state); }));
            }
            else
            {
                Main.SetExecState(state, logName, WebDriverLog);
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            switch (browser)
            {
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    Log("Opening new Firefox window....");

                    _driverFirefox = new FirefoxDriver();
                    _driverFirefox.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, Globals.__TIMEOUT__));

                    ModWindow();
                    Maximize();

                    break;
                case Globals.FIREFOX_HEADLESS:
                    Log("Opening new headless browser process....");

                    webClient = new NHtmlUnit.WebClient(BrowserVersion.FIREFOX_38);
                    webClient.Options.JavaScriptEnabled = true;
                    webClient.Options.RedirectEnabled = true;
                    webClient.Options.ThrowExceptionOnFailingStatusCode = false;
                    webClient.Options.ThrowExceptionOnScriptError = false;
                    webClient.WaitForBackgroundJavaScript(10000);
                    webClient.AjaxController = new NicelyResynchronizingAjaxController();

                    break;
            }
        }

        [SetUp]
        public void TestSetUp(string URL)
        {
            switch (browser)
            {
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    Log("Navigating Firefox to:  " + URL);

                    _driverFirefox.Navigate().GoToUrl(URL);

                    WaitForPageLoad();
                    break;
                case Globals.FIREFOX_HEADLESS:
                    Log("Navigating headless browser to:  " + URL);
                    Log("This may take a few minutes....");

                    System.Threading.Thread.Sleep(500);

                    page = webClient.GetHtmlPage(URL);
                    
                    break;
            }

        }

        private void ModWindow(bool unhide = false)
        {
            switch (browser)
            {
                case Globals.FIREFOX_HIDDEN:
                    Log("Hiding Firefox window....", false);

                    _driverFirefox.Manage().Window.Position = new Point(-9999, -9999);
                    //_driverFirefox.Manage().Window.Size = new Size(1, 1);

                    IntPtr hWnd = IntPtr.Zero;

                    int i = 300;
                    do
                    {
                        hWnd = GetWindowHandle();
                        System.Threading.Thread.Sleep(100);
                        i--;
                    } while (!hWnd.Equals(IntPtr.Zero)
                        && AutoIt.AutoItX.WinExists(hWnd) == 0
                        && i > 0);

                    if (AutoIt.AutoItX.WinExists(hWnd) == 0)
                    {
                        browser = Globals.FIREFOX_WINDOWED;
                        Log("WARNING:  Unable to hide browser window!  Switched to windowed mode.");
                    }
                    else
                    {
                        AutoIt.AutoItX.WinSetState(hWnd, (unhide ? AutoIt.AutoItX.SW_SHOW : AutoIt.AutoItX.SW_HIDE));
                    }

                    break;
            }
        }

        /* Alias for TestSetUp.  --Kris */
        public void GoToUrl(string URL)
        {
            TestSetUp(URL);
        }

        [Test]
        public string GetPageSource(int retry = 5)
        {
            dynamic driver;

            driver = GetDriver();

            string res = null;
            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    res = driver.PageSource;
                    break;
                case Globals.FIREFOX_HEADLESS:
                    res = page.AsXml();
                    break;
            }

            if (res != null)
            {
                return res;
            }
            else
            {
                retry--;
                if (retry == 0)
                {
                    return null;
                }

                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__);

                return GetPageSource(retry);
            }
        }

        [Test]
        public dynamic GetDriver()
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    return _driverFirefox;
                case Globals.FIREFOX_HEADLESS:
                    return page;  // Doesn't go through the WebDriver interface, unfortunately.  --Kris
            }
        }

        [Test]
        public void Maximize()
        {
            if (browser == Globals.FIREFOX_HEADLESS 
                || browser == Globals.FIREFOX_HIDDEN)
            {
                return;
            }

            IntPtr hWnd = GetWindowHandle();

            if (!hWnd.Equals(IntPtr.Zero))
            {
                ShowWindowAsync(hWnd, 3);  // 3 = Maximize!  --Kris
            }
        }

        private IntPtr GetWindowHandle()
        {
            IWebDriver driver = GetDriver();

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
            return FindWindow(null, name + " - " + Globals.BrowserPIDName(browser));
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string sClassName, string sAppName);

        [Test]
        public string GetURL()
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    IWebDriver driver = GetDriver();
                    return driver.Url;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    return null;
            }
        }

        /* This exception is so common with Facebook, it makes sense to create a separate handler for retries, instead of doing it in every single function that touches elements.  --Kris */
        public dynamic StaleElementReferenceException_Handler(string callerName, object[] parameters)
        {
            staleRetry++;
            if (staleRetry >= 5)
            {
                /* Let's make the error message as helpful as possible for triage purposes.  There are no security considerations since we're just dealing with identifying HTML elements.  --Kris */
                string args = "";
                foreach (object param in parameters)
                {
                    args += (args != "" ? @", " : "");

                    try
                    {
                        dynamic j;
                        switch (Type.GetTypeCode(param.GetType()))
                        {
                            default:
                                args += @"<" + param.GetType().ToString() + @">";
                                break;
                            case TypeCode.Boolean:
                                args += ((bool) param == true ? "true" : "false");
                                break;
                            case TypeCode.Char:
                                args += ((char) param).ToString();
                                break;
                            case TypeCode.String:
                                args += param;
                                break;
                            case TypeCode.Decimal:
                                args += ((double) param).ToString();
                                break;
                            case TypeCode.Double:
                                args += ((decimal) param).ToString();
                                break;
                            case TypeCode.Int16:
                                args += ((Int16) param).ToString();
                                break;
                            case TypeCode.Int32:
                                args += ((Int32) param).ToString();
                                break;
                            case TypeCode.Int64:
                                args += ((Int64) param).ToString();
                                break;
                            case TypeCode.UInt16:
                                args += ((UInt16) param).ToString();
                                break;
                            case TypeCode.UInt32:
                                args += ((UInt32) param).ToString();
                                break;
                            case TypeCode.UInt64:
                                args += ((UInt64) param).ToString();
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log("Warning:  Error parsing parameter for message arguments list : " + e.Message);

                        args += @"????";
                    }
                }

                Log("WebDriver ERROR:  Unable to handle stale element in : " + callerName + "( " + args + " ).");

                return null;
            }
            
            System.Threading.Thread.Sleep(100);

            Type thisType = this.GetType();
            MethodInfo staleMeth = thisType.GetMethod(callerName);

            return staleMeth.Invoke(this, parameters);
        }

        public dynamic StaleReturn(object[] parameters, [CallerMemberName] string callerName = "")
        {
            dynamic staleReturn = StaleElementReferenceException_Handler(callerName, parameters);
            if (staleReturn != null)
            {
                staleRetry = 0;
            }

            return staleReturn;
        }

        private object[] GetParams(params object[] parameters)
        {
            return parameters;
        }

        [Test]
        public dynamic GetElementById(string elementid, bool iefix = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        IWebDriver driver = GetDriver();

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
                    case Globals.FIREFOX_HEADLESS:
                        return page.GetElementById(elementid);
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(elementid, iefix));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(elementid, iefix));
            }
        }

        [Test]
        public dynamic GetElementByName(string elementname, bool iefix = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        IWebDriver driver = GetDriver();

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
                    case Globals.FIREFOX_HEADLESS:
                        return page.GetElementByName(elementname);
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(elementname, iefix));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(elementname, iefix));
            }
        }

        [Test]
        public dynamic GetElementByLinkText(string linktext, bool partial = false)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        IWebDriver driver = GetDriver();

                        if (partial == true)
                        {
                            return driver.FindElement(By.PartialLinkText(linktext));
                        }
                        else
                        {
                            return driver.FindElement(By.LinkText(linktext));
                        }
                    case Globals.FIREFOX_HEADLESS:
                        return page.GetAnchorByText(linktext);
                }
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(linktext, partial));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(linktext, partial));
            }
        }

        [Test]
        public dynamic GetElementByXPath(string xpath, int timeout = -1)
        {
            try
            {
                dynamic res;
                IWebDriver driver;

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        driver = GetDriver();

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
                    case Globals.FIREFOX_HEADLESS:
                        return page.GetByXPath(xpath);
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(xpath, timeout));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(xpath, timeout));
            }
        }

        [Test]
        public dynamic GetElementByCSSSelector(string cssSelector, int timeout = -1)
        {
            try
            {
                dynamic res;
                IWebDriver driver;

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        driver = GetDriver();

                        if (timeout >= 0)
                        {
                            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(timeout));
                        }

                        try
                        {
                            res = driver.FindElement(By.CssSelector(cssSelector));
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
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return null;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(cssSelector, timeout));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(cssSelector, timeout));
            }
        }

        [Test]
        public List<IWebElement> GetElementsByTagName(string tagName)
        {
            try
            {
                IWebDriver driver = GetDriver();

                return new List<IWebElement>(driver.FindElements(By.TagName(tagName)));
            }
            catch (NoSuchElementException e)
            {
                return null;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(tagName));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(tagName));
            }
        }

        [Test]
        public IWebElement GetInputElementByPlaceholder(string placeholder)
        {
            return GetElementByTagNameAndAttribute("input", "placeholder", placeholder);
        }

        [Test]
        public IWebElement GetElementByTagNameAndAttribute(string tagName, string attributeName, string attributeValue, int offset = 0)
        {
            try
            {
                List<IWebElement> eles = GetElementsByTagName(tagName);
                if (eles == null)
                {
                    return null;
                }

                int i = 0;
                foreach (IWebElement ele in eles)
                {
                    if (ele.GetAttribute(attributeName) != null && ele.GetAttribute(attributeName).Equals(attributeValue))
                    {
                        if (i == offset)
                        {
                            return ele;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                return null;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(tagName, attributeName, attributeValue, offset));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(tagName, attributeName, attributeValue, offset));
            }
        }

        [Test]
        public List<IWebElement> GetElementsByTagNameAndAttribute(string tagName, string attributeName, string attributeValue, int limit = 0)
        {
            try
            {
                List<IWebElement> eles = GetElementsByTagName(tagName);
                if (eles == null)
                {
                    return null;
                }

                int i = 0;
                List<IWebElement> res = new List<IWebElement>();
                foreach (IWebElement ele in eles)
                {
                    if (ele.GetAttribute(attributeName) != null && ele.GetAttribute(attributeName).Equals(attributeValue))
                    {
                        res.Add(ele);

                        i++;
                        if (i == limit)
                        {
                            break;
                        }
                    }
                }

                return res;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(tagName, attributeName, attributeValue, limit));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(tagName, attributeName, attributeValue, limit));
            }
        }

        [Test]
        public dynamic ClickElement(dynamic element, bool viewportfix = false, bool autoscroll = false)
        {
            try
            {
                dynamic driver = GetDriver();

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
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

                        WaitForPageLoad();
                        break;
                    case Globals.FIREFOX_HEADLESS:
                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                        element.RemoveAttribute("disabled");
                        return element.Click();
                }

                return true;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(element, viewportfix, autoscroll));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(element, viewportfix, autoscroll));
            }
        }

        public string GetAttribute(IWebElement element, string attr)
        {
            try
            {
                return element.GetAttribute(attr);
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(element, attr));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(element, attr));
            }
        }

        public string GetText(IWebElement element)
        {
            try
            {
                return element.Text;
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(element));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(element));
            }
        }

        [Test]
        public void ScrollToBottom(string logFirstMsg = null, string logMsg = null, string logLastMsg = null, int scrollLimit = 2000)
        {
            IWebDriver driver = GetDriver();

            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            //const string script = "var i=100;var timeId=setInterval(function(){i--;window.scrollY<document.body.scrollHeight-window.screen.availHeight&&i>0?window.scrollTo(0,document.body.scrollHeight):(clearInterval(timeId),window.scrollTo(0,0));return!(window.scrollY<document.body.scrollHeight-window.screen.availHeight&&i>0);},3000);";
            const string scrollScript = "window.scrollTo(0,document.body.scrollHeight);";
            const string checkScript = "return!(window.scrollY<document.body.scrollHeight-window.screen.availHeight);";

            if (logFirstMsg != null)
            {
                Log(logFirstMsg);
            }

            if (scrollLimit > 0)
            {
                bool done = false;
                int i = scrollLimit;
                do
                {
                    // Logging.  --Kris
                    if (logMsg != null)
                    {
                        Log(logMsg.Replace(@"$i", (scrollLimit - (i - 1)).ToString()).Replace(@"$L", scrollLimit.ToString()));  // $i increments 1 to scrollLimit.  --Kris
                    }
                    
                    // Do the scroll.  --Kris
                    jse.ExecuteScript(scrollScript);

                    // Check to see if the page expands.  If it doesn't after 6 seconds, assume we're done.  --Kris
                    int ii = 12;
                    do
                    {
                        System.Threading.Thread.Sleep(500);
                        
                        done = (bool) jse.ExecuteScript(checkScript);

                        ii--;
                    } while (done == true && ii > 0);
                    i--;
                    // Uncomment below for DEBUG.  --Kris
                    /*
                    if (i == (scrollLimit - 5))
                    {
                        i = i;  // In case you need a convenient breakpoint.  --Kris
                        break;
                    }
                    */
                } while (done == false && i > 0);
            }
            else
            {
                jse.ExecuteScript(scrollScript);
            }

            if (logLastMsg != null)
            {
                Log(logLastMsg);
            }

            System.Threading.Thread.Sleep(3000);
        }

        public void ScrollToBottom(ref IWebDriver driver, int scrollLimit)
        {
            ScrollToBottom(null, null, null, scrollLimit);
        }

        [Test]
        public void ScrollToBottom(ref HtmlPage page, int scrollLimit = 100)
        {
            const string scrollScript = "window.scrollTo(0,document.body.scrollHeight);";
            const string checkScript = "return!(window.scrollY<document.body.scrollHeight-window.screen.availHeight);";

            if (scrollLimit > 0)
            {
                bool done = false;
                int i = scrollLimit;
                do
                {
                    Log("Scrolling down....");

                    page.ExecuteJavaScript(scrollScript);
                    System.Threading.Thread.Sleep(3000);
                    ScriptResult sr = page.ExecuteJavaScript(checkScript);
                    done = (bool) sr.JavaScriptResult;
                    i--;
                } while (done == false && i > 0);
            }
            else
            {
                page.ExecuteJavaScript(scrollScript);
            }

            System.Threading.Thread.Sleep(3000);

            Log("Scrolling complete.");
        }

        [Test]
        // Modified from:  http://stackoverflow.com/questions/13244225/selenium-how-to-make-the-web-driver-to-wait-for-page-to-refresh-before-executin
        public void WaitForPageLoad(int maxWaitTimeInSeconds = 60)
        {
            System.Threading.Thread.Sleep(3000);

            string state = string.Empty;
            dynamic _driver = GetDriver();
            switch (browser)
            {
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
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
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // I think this is already built-in to NHtmlUnit, somehow.  I could be wrong, but given how slow the fucker is, it's kinda moot, anyway.  --Kris
                    break;
            }

            ModWindow();
        }

        [Test]
        /* When all else fails, have JavaScript do it.  --Kris */
        public void JavaScriptClickElementId(string elementid, bool checkbox = false)
        {
            dynamic driver = GetDriver();
            string script;

            script = (checkbox ? "document.getElementById( '" + elementid + "' ).checked = true;" : "document.getElementById( '" + elementid + "' ).Click();");

            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    ((IJavaScriptExecutor)driver).ExecuteScript(script);
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public bool ClickOnLink(string linktext, bool partial = false, bool viewportfix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByLinkText(linktext, partial);
                            return ClickElement(element, viewportfix);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClickOnLink(linktext, partial, viewportfix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        try
                        {
                            dynamic element = GetElementByLinkText(linktext, partial);
                            return ClickElement(element);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClickOnLink(linktext, partial, viewportfix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(linktext, partial, viewportfix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(linktext, partial, viewportfix, retry));
            }
        }

        [Test]
        public bool ClickOnXPath(string xpath, bool viewportfix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByXPath(xpath);
                            return ClickElement(element, viewportfix);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClickOnXPath(xpath, viewportfix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        try
                        {
                            dynamic element = GetElementByLinkText(xpath);
                            return ClickElement(element);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClickOnXPath(xpath, viewportfix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(xpath, viewportfix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(xpath, viewportfix, retry));
            }
        }

        [Test]
        public bool TypeText(dynamic element, string text)
        {
            try
            {
                dynamic driver = GetDriver();

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
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
                    case Globals.FIREFOX_HEADLESS:
                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);

                        element.Type(text);

                        return true;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(element, text));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(element, text));
            }
        }

        [Test]
        public bool TypeInXPath(string xpath, string text, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByXPath(xpath);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInXPath(xpath, text, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        try
                        {
                            dynamic element = GetElementByXPath(xpath);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInXPath(xpath, text, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(xpath, text, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(xpath, text, retry));
            }
        }

        [Test]
        public bool TypeInId(string elementid, string text, bool iefix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementById(elementid, iefix);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInId(elementid, text, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        try
                        {
                            dynamic element = GetElementById(elementid);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInId(elementid, text, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(elementid, text, iefix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(elementid, text, iefix, retry));
            }
        }

        [Test]
        public bool TypeInName(string elementname, string text, bool iefix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByName(elementname, iefix);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInName(elementname, text, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        try
                        {
                            dynamic element = GetElementByName(elementname);
                            return TypeText(element, text);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return TypeInName(elementname, text, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(elementname, text, iefix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(elementname, text, iefix, retry));
            }
        }

        [Test]
        public bool ClearText(IWebElement element)
        {
            try
            {
                dynamic driver = GetDriver();

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
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
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return false;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(element));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(element));
            }
        }

        [Test]
        public bool ClearXPath(string xpath, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByXPath(xpath);
                            return ClearText(element);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClearXPath(xpath, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return false;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(xpath, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(xpath, retry));
            }
        }

        [Test]
        public bool ClearId(string id, bool iefix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementById(id, iefix);
                            return ClearText(element);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClearId(id, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return false;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(id, iefix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(id, iefix, retry));
            }
        }

        [Test]
        public bool ClearName(string name, bool iefix = false, int retry = 2)
        {
            try
            {
                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        try
                        {
                            IWebElement element = GetElementByName(name, iefix);
                            return ClearText(element);
                        }
                        catch
                        {
                            retry--;
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);
                                return ClearName(name, iefix, retry);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return false;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(name, iefix, retry));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(name, iefix, retry));
            }
        }

        [Test]
        public void SwitchToFrame(string frame, int retry = 2)
        {
            dynamic driver = GetDriver();

            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
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
                            SwitchToFrame(frame, retry);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public void SwitchToTop(int retry = 2)
        {
            dynamic driver = GetDriver();

            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
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
                            SwitchToTop(retry);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckTextOnPage(string text, bool wait = false, long endticks = 0, int retrywait = 5)
        {
            dynamic driver;

            driver = GetDriver();

            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    /* Recursively wait and retry if specified.  --Kris */
                    if (wait == true)
                    {
                        System.DateTime now = System.DateTime.Now;

                        if (driver.PageSource.Contains(text) == true
                            || now.Ticks >= endticks)
                        {
                            CheckTextOnPage(text);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep((retrywait * 1000));

                            CheckTextOnPage(text, wait, endticks, retrywait);
                        }
                    }
                    else
                    {
                        Assert.IsTrue(driver.PageSource.Contains(text));
                    }
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckTextOnPageRegex(string regex)
        {
            dynamic driver;

            driver = GetDriver();

            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    Assert.IsTrue(Regex.IsMatch(regex, driver.PageSource));
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public void CheckPageTitle(string title)
        {
            switch (browser)
            {
                default:
                case Globals.FIREFOX_WINDOWED:
                case Globals.FIREFOX_HIDDEN:
                    Assert.AreEqual(title, _driverFirefox.Title);
                    break;
                case Globals.FIREFOX_HEADLESS:
                    // TODO
                    break;
            }
        }

        [Test]
        public bool CheckElementExists(string text, string by)
        {
            try
            {
                dynamic driver;

                driver = GetDriver();

                switch (browser)
                {
                    default:
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        IWebElement element;
                        switch (by)
                        {
                            case "linktext":
                                element = GetElementByLinkText(text);
                                break;
                            case "id":
                                element = GetElementById(text);
                                break;
                            case "name":
                                element = GetElementById(text);
                                break;
                            default:
                                element = null;
                                break;
                        }
                        return (!(element.Size.IsEmpty));
                    case Globals.FIREFOX_HEADLESS:
                        // TODO
                        return false;
                }
            }
            catch (StaleElementReferenceException e)
            {
                return StaleReturn(GetParams(text, by));
            }
            catch (Exception e)
            {
                return StaleReturn(GetParams(text, by));
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            try
            {
                switch (browser)
                {
                    case Globals.FIREFOX_WINDOWED:
                    case Globals.FIREFOX_HIDDEN:
                        if (_driverFirefox != null)
                        {
                            ModWindow(true);
                            System.Threading.Thread.Sleep(500);
                            _driverFirefox.Dispose();
                        }
                        break;
                    case Globals.FIREFOX_HEADLESS:
                        page.CleanUp();
                        webClient.Close();
                        break;
                }
            }
            catch (Exception e)
            {
                Log("FATAL ERROR:  Unable to close browser!  Please restart FaceBERN! and try again.");

                SetExecState(Globals.STATE_BROKEN);
            }
        }
    }
}
