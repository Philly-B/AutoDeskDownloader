using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace AutoDeskDownloader;

public class DownloadLinkExtractor
{
    private readonly LoginDataProvider _loginDataProvider = new LoginDataProvider();
    private readonly ChromeDriver _driver;
    private readonly ChromeDriver _driverHeadless;
    private readonly WebDriverWait _waiter;
    private readonly WebDriverWait _waiterHeadless;

    public DownloadLinkExtractor()
    {
        var opts = new ChromeOptions();
        opts.AddArgument("--headless"); // we can only print to pdf in headless mode
        opts.AddArgument("--window-size=1920,1080");
        opts.AddArgument("--log-level=3");
        var userAgent =
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.50 Safari/537.36";
        opts.AddArgument($"user-agent={userAgent}");
        _driverHeadless = new ChromeDriver(opts);
        _waiterHeadless = new WebDriverWait(_driverHeadless, TimeSpan.FromSeconds(20));

        _driver = new ChromeDriver();
        _waiter = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    public void DownloadScreenCasts(int startPage)
    {
        try
        {
            _driver.Navigate().GoToUrl(Constants.AutoDeskLoginUrl);
            _driverHeadless.Navigate().GoToUrl(Constants.AutoDeskLoginUrl);
            Login(_driver, _waiter);
            Login(_driverHeadless, _waiterHeadless);

            NavigateToDashboard(_driver, _waiter);
            CloseCookiesOverlay();
            SelectScreenCasts();
            
            if (startPage != 1)
            {
                NavigateToPage(startPage);
            }
            
            DownloadAllScreenCasts();
            Console.WriteLine("Done");
        }
        catch (Exception)
        {
            var screenshot = _driver.TakeScreenshot();
            screenshot.SaveAsFile(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Error.jpg"));
            
            var screenshot2 = _driverHeadless.TakeScreenshot();
            screenshot2.SaveAsFile(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Error_Headless.jpg"));
            throw;
        }
    }

    private void CloseCookiesOverlay()
    {
        // wait until this page is fully loaded
        Thread.Sleep(2000);
        _driver.FindElement(By.Id("adsk-eprivacy-privacy-decline-all-button"))?.Click();
        _driverHeadless.FindElement(By.Id("adsk-eprivacy-privacy-decline-all-button"))?.Click();
    }

    private void DownloadAllScreenCasts()
    {
        Console.WriteLine("Start downloading all screen casts");
        do
        {
            Console.WriteLine($"Extracting screen casts of page {GetCurrentPage()}");
            var elements = _driver.FindElements(By.ClassName("teaser__list")).ToArray();

            for (int i = 0; i < elements.Length; i++)
            {
                var (name, detailPageLink) = HandleOneElement(elements[i]);
                GoToPageAndStoreAsPdf(name, detailPageLink);
            }

            // Ensure we are not downloading too much at once
            WaitUntilOnlyNConcurrentDownloads();
        } while (NavigateToNextPage());
    }

    /// <summary>
    /// Navigates to next page if not already last page
    /// </summary>
    /// <returns>false if it was the last page</returns>
    private bool NavigateToNextPage()
    {
        _driver.ExecuteJavaScript("window.scrollBy(0,document.body.scrollHeight)");

        IWebElement GetFirst() =>
            _driver.FindElements(By.ClassName("teaser__list")).First().FindElement(By.TagName("h3"));

        string GetTextOfFirst() => GetFirst().Text;

        var lastPageFirst = GetTextOfFirst();

        var pagination = _driver.FindElement(By.ClassName("react-pagination__menu"));
        if (pagination == null)
        {
            Console.WriteLine("No pagination, therefore just one page to handle");
            // probably just one site
            return false;
        }

        var pages = pagination.FindElements(By.TagName("li")).ToArray();
        var notLastPage = false;

        for (int i = 0; i < pages.Length - 1; i++)
        {
            if (pages[i].GetAttribute("class").Contains("--active"))
            {
                pages[i + 1].Click();
                _waiter.Until(d => WaitForElement(d, By.ClassName("teaser__list")));
                notLastPage = true;
                break;
            }
        }

        do
        {
            Thread.Sleep(200);
        } while (lastPageFirst == GetTextOfFirst());

        _driver.ExecuteJavaScript("window.scroll(0, 0);");
        Thread.Sleep(200);

        return notLastPage;
    }
    
    private void NavigateToPage(int pageNumber)
    {
        _driver.ExecuteJavaScript("window.scrollBy(0,document.body.scrollHeight)");
        
        while (int.Parse(GetCurrentPage()) < pageNumber)
        {
            NavigateToNextPage();
        }
 
        _driver.ExecuteJavaScript("window.scroll(0, 0);");
        Thread.Sleep(200);
    }

    private string GetCurrentPage()
    {
        var pagination = _driver.FindElement(By.ClassName("react-pagination__menu"));
        if (pagination == null)
        {
            return "1";
        }

        var pages = pagination.FindElements(By.TagName("li")).ToArray();

        for (int i = 0; i < pages.Length - 1; i++)
        {
            if (pages[i].GetAttribute("class").Contains("--active"))
            {
                return pages[i].Text;
            }
        }

        return "Page number not found";
    }

    private void SelectScreenCasts()
    {
        _driver.FindElement(By.ClassName("akn-form-field__control")).Click();
        var dropdown = _driver.FindElement(By.ClassName("akn-form-select__menu"));

        var options = dropdown.FindElements(By.ClassName("akn-form-select__option"));
        foreach (var option in options)
        {
            if (!option.Text.Contains("Screencasts"))
                continue;

            option.Click();
            break;
        }
    }

    private void GoToPageAndStoreAsPdf(string name, string url)
    {
        _driverHeadless.Navigate().GoToUrl(url);
        Thread.Sleep(1000); // just to be sure
        var printOptions = new PrintOptions
        {
            Orientation = PrintOrientation.Landscape
        };
        var doc = _driverHeadless.Print(printOptions);
        var fileName = Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), $"{name}.pdf");
        if (File.Exists(fileName))
        {
            fileName = $"{name}_{Guid.NewGuid().ToString().Substring(0, 5)}.pdf";
        }

        Console.WriteLine($"Storing detail page of {name} as {fileName}");
        doc.SaveAsFile(fileName);
    }

    private (string name, string url) HandleOneElement(IWebElement element)
    {
        var title = element.FindElement(By.TagName("h3"));
        var link = title.FindElement(By.TagName("a"));
        var detailPageLink = link.GetAttribute("href");
        Console.WriteLine($"Found screencast {title.Text} with detail page {detailPageLink}");
        try
        {
            DownloadMp4(element, title);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not download screencast {title.Text} because of {e.Message}");
            throw;
        }

        _driver.FindElements(By.ClassName("akn_react_modal__close")).FirstOrDefault()?.Click();
        _driver.ExecuteJavaScript($"window.scrollBy(0,{element.Size.Height})");

        return (link.Text, detailPageLink);
    }

    private void DownloadMp4(IWebElement element, IWebElement titleElement)
    {
        element.FindElement(By.ClassName("menu-item")).Click();

        Thread.Sleep(1000);

        var optionsMenu = _driver.FindElement(By.ClassName("tippy-content"));
        var menuItems = optionsMenu.FindElements(By.TagName("p")).ToArray();

        var foundDownload = false;
        foreach (var menuItem in menuItems)
        {
            if (!menuItem.Text.ToLowerInvariant().Trim().Contains("screencast"))
                continue;

            menuItem.Click();

            _waiter.Until(d => WaitForElement(d, By.ClassName("akn_react_modal__body-inner")));
            _waiter.Until(d => WaitForElementWithText(d, By.TagName("span"), "MP4 herunterladen"));

            Thread.Sleep(1000);

            var overlay = _driver.FindElement(By.ClassName("akn_react_modal__body"));
            var downloadButtons = overlay.FindElements(By.TagName("button")).ToArray();
            var mp4Download = downloadButtons.SingleOrDefault(w => w.Text.Contains("MP4"));
            if (mp4Download != null)
            {
                foundDownload = true;
                mp4Download.Click();
                Thread.Sleep(1000);
            }
            else
            {
                throw new ArgumentException("Unable to download, did not find mp4 download button");
            }

            _driver.FindElements(By.ClassName("akn_react_modal__close")).FirstOrDefault()?.Click();
            try
            {
                _driver.FindElement(By.XPath("//html"))?.Click();
            }
            catch (Exception)
            {
                // ignored
            }

            Thread.Sleep(500);

            break;
        }

        if (!foundDownload)
            Console.WriteLine($"Did not find MP4 download for element '{titleElement.Text}'");
    }

    private void NavigateToDashboard(IWebDriver driver, WebDriverWait waiter)
    {
        driver.Navigate().GoToUrl(Constants.AutoDeskKnowledgeUrl);

        waiter.Until(d => WaitForElement(d, By.Id("uh-me-menu-avatar")));

        driver.Navigate().GoToUrl(Constants.AutoDeskDashboardUrl);

        try
        {
            waiter.Until(d => WaitForElement(d, By.ClassName("teaser__list")));
        }
        catch (WebDriverTimeoutException)
        {
            // I don't know what autodesk website is doing here
            if (driver.Url.Contains("signin"))
            {
                driver.Navigate().GoToUrl(Constants.AutoDeskDashboardUrl);
                waiter.Until(d => WaitForElement(d, By.ClassName("teaser__list")));
            }
        }
    }

    private void Login(IWebDriver driver, WebDriverWait waiter)
    {
        var userNameElement = driver.FindElement(By.Id("userName"));
        userNameElement.SendKeys(_loginDataProvider.GetUserName());
        driver.FindElement(By.Id("verify_user_btn")).Click();

        waiter.Until(d => WaitForElement(d, By.Id("password")));

        var passwordElement = driver.FindElement(By.Id("password"));
        passwordElement.SendKeys(_loginDataProvider.GetPassword());
        driver.FindElement(By.Id("btnSubmit")).Click();

        waiter.Until(d => WaitForElement(d, By.Id("uh-title")));
    }


    private bool WaitForElement(IWebDriver driver, By by)
    {
        return driver.FindElement(by).Displayed;
    }

    private bool WaitForElementWithText(IWebDriver driver, By by, string text)
    {
        return driver.FindElements(by).Where(e => e.Text.Contains(text)).Count(e => e.Displayed) > 0;
    }

    private void WaitUntilOnlyNConcurrentDownloads()
    {
        var downloadFolder = KnownFolders.GetPath(KnownFolder.Downloads);
        int CurrentlyDownloadingFiles() => Directory.GetFiles(downloadFolder).Count(f => f.EndsWith("crdownload"));

        var currentlyDownloading = CurrentlyDownloadingFiles();
        while (currentlyDownloading > Constants.ConcurrentDownloadsAllowed)
        {
            Console.WriteLine(
                $"Currently are {currentlyDownloading} files downloading, waiting until only {Constants.ConcurrentDownloadsAllowed} downloading to continue.");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            currentlyDownloading = CurrentlyDownloadingFiles();
        }
    }
}