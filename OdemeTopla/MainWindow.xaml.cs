using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace OdemeTopla
{
    public partial class MainWindow : Window
    {
        WebDriver driver;
        double dolar = 0;
        List<string> excludeds;

        IReadOnlyCollection<IWebElement> rgList;
        public MainWindow()
        {
            InitializeComponent();
            excludeds = new List<string>();
            #region driverSettings
            new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var options = new ChromeOptions();
            options.AddArgument("--window-position=-32000,-32000");
            options.AddArguments("--lang=tr");
            driver = new ChromeDriver(driverService, options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            #endregion
        }

        private void btnHesapla_Click(object sender, RoutedEventArgs e)
        {
            double usdKur = kurCek("USD");
            lblUsdKur.Content = $"USD : {Math.Round(usdKur, 2)}";
            double euroKur = kurCek("EUR");
            lblEuroKur.Content = $"EUR : {Math.Round(euroKur, 2)}";

            var cevap = MessageBox.Show("excludedList.json dosyasına SmartCat hesaplamasında hariç olanların(Ödeme bekleniyor olanlardan) belge kodunu uygun formatta girdiğinizden emin olun. Program bu süreçte işlem yaptırmayacaktır, işlemler bitene kadar kapatmayınız. Hata ile karşılaşırsanız mail ve şifrelerinizi veya internet bağlantınızı kontrol ediniz.", "UYARI", MessageBoxButton.OKCancel, MessageBoxImage.Warning);


            if (cevap == MessageBoxResult.Cancel)
                return;

            btnHesapla.IsEnabled = false;

            string jsonData;
            string dir = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location);

            string file = dir + @"\excludedList.json";
            try
            {
                using (System.IO.StreamReader _StreamReader = new System.IO.StreamReader(file))
                {
                    jsonData = _StreamReader.ReadToEnd();
                }
                excludeds = JsonSerializer.Deserialize<List<string>>(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("excludedList.json dosyası bulunamadı veya bozuk.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            driver.Navigate().GoToUrl($"https://smartcat.com/billing/executive-payment-statuses");
            IWebElement userName = driver.FindElement(By.CssSelector(".md-input__textfield"));
            userName.SendKeys(txtMail.Text);
            IWebElement dvmBtn = driver.FindElement(By.CssSelector(".g-btn__text"));
            dvmBtn.Click();
            IWebElement passwrd = driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[2]/div/div[3]/div[2]/div/form/div[3]/div[1]/div[1]/input"));
            passwrd.SendKeys(txtPassword.Password);
            IWebElement loginBtn = driver.FindElement(By.CssSelector(".g-btn.g-btn_big.g-btn_purple.g-btn_weight_medium"));
            loginBtn.Click();
            Thread.Sleep(4000);

            double totalPrice = 0;
            if (chBoxAvrupa.IsChecked == true)
                SmartCatTopla("AVRUPA", "EUROPE", "");
            if (chBoxAbd.IsChecked == true)
                SmartCatTopla("ABD", "USA", "us.");
            if (chBoxAsya.IsChecked == true)
                SmartCatTopla("ASYA", "ASIA", "ea.");


            double euro = 0;
            if (chBoxBeluga.IsChecked == true)
            {
                driver.Navigate().GoToUrl("https://belugalinguistics.s.xtrf.eu/vendors/#/sign-in");
                IWebElement userNameBeluga = driver.FindElement(By.Id("email"));
                userNameBeluga.SendKeys(txtMail.Text);
                IWebElement passwrdBeluga = driver.FindElement(By.Id("password"));
                passwrdBeluga.SendKeys(txtPasswordBeluga.Password);
                IWebElement loginBtnBeluga = driver.FindElement(By.CssSelector(".btn.btn-success.pull-left.ng-isolate-scope"));
                loginBtnBeluga.Click();
                Thread.Sleep(4000);


                try
                {
                    string nextBtnStatus;
                    do
                    {
                        string jsCommand = "" +
                    "sayfa = document.querySelector('body > div > div.content.portal.ng-scope > ui-view > div > div > page-section:nth-child(5) > div > div > div > section > div.loader-container.ng-scope > div > div > div > ul > li:nth-child(5) > div > a');" +
                    "return sayfa;";
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        IWebElement nextBtn = (IWebElement)js.ExecuteScript(jsCommand);
                        IReadOnlyCollection<IWebElement> paymentRowsBeluga = driver.FindElements(By.CssSelector("body > div > div.content.portal.ng-scope > ui-view > div > div > page-section:nth-child(5) > div > div > div > section > div.loader-container.ng-scope > table > tbody > tr"));
                        foreach (var item in paymentRowsBeluga)
                        {
                            IWebElement tutar = item.FindElement(By.XPath("td[3]"));
                            string tutarText = tutar.Text.Substring(0, tutar.Text.Length - 4);
                            euro += Convert.ToDouble(tutarText);
                            lblEuro.Content = Math.Round(euro, 2) + " €";
                        }
                        nextBtnStatus = nextBtn.GetAttribute("ng-show");
                        nextBtn.Click();
                        Thread.Sleep(1000);
                    } while (nextBtnStatus == "page.active");

                }
                catch { }
            }

            totalPrice = (dolar * usdKur) + (euro * euroKur);

            if (totalPrice == 0)
            {
                MessageBox.Show("", "");
            }
            else
            {
                lblTotalPrice.Content = Math.Round(totalPrice, 2) + " TL";
                lblDolar.Content = Math.Round(dolar, 2) + " $";
                lblEuro.Content = Math.Round(euro, 2) + " €";
            }

            btnHesapla.IsEnabled = true;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (driver != null)
                driver.Close();
        }

        #region Methods
        void SmartCatTopla(string region, string regionEng, string regionURL)
        {
            IWebElement profile = driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div/div/div[1]/button"));
            profile.Click();
            IWebElement regBtn = driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div[2]/div[3]/div/div[2]/div/div/div[2]/span/span[1]"));
            regBtn.Click();
            IReadOnlyCollection<IWebElement> rgList = driver.FindElements(By.XPath("/html/body/div[1]/div[1]/div[2]/div[2]/div/div[2]/div[2]/div[1]/div/a"));


            foreach (var item in rgList)
            {
                if (item.Text == region || item.Text == regionEng)
                {
                    item.Click();
                    Thread.Sleep(4000);
                    driver.Navigate().GoToUrl($"https://{regionURL}smartcat.com/billing/executive-payment-statuses");
                    Thread.Sleep(4000);
                    break;
                }

            }

            string jsCommand = "" +
                "sayfa = document.querySelector('html');" +
                "sayfa.scrollTo(0,sayfa.scrollHeight);" +
                "var sayfaSonu = sayfa.scrollHeight;" +
                "return sayfaSonu;";
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var sayfaSonu = Convert.ToInt32(js.ExecuteScript(jsCommand));

            while (true)
            {
                var son = sayfaSonu;
                Thread.Sleep(1500);
                sayfaSonu = Convert.ToInt32(js.ExecuteScript(jsCommand));
                if (son == sayfaSonu)
                    break;
            }


            IReadOnlyCollection<IWebElement> paymentRows = driver.FindElements(By.CssSelector(".freelancer-payments-column__row"));

            foreach (var item in paymentRows)
            {
                IWebElement status = item.FindElement(By.CssSelector(".g-text_weight_semibold"));

                if (status.Text == "Ödeme bekleniyor" || status.Text == "Devam ediyor" || status.Text == "Ödeme işleniyor" || status.Text == "In progress" || status.Text == "Processing payment" || status.Text == "Awaiting payment")
                {
                    string jobID = item.FindElement(By.CssSelector(".freelancer-payments__block-document-name")).Text.Trim();

                    if (excludeds != null)
                        if (excludeds.IndexOf(jobID) > -1)
                            continue;

                    IWebElement tutar = item.FindElement(By.XPath("div[5]/div/span/span"));
                    dolar += Convert.ToDouble(tutar.Text);
                }
            }
        }

        double kurCek(string doviz)
        {
            string kurUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
            var xmldoc = new XmlDocument();
            xmldoc.Load(kurUrl);
            double kur = Convert.ToDouble(xmldoc.SelectSingleNode($"Tarih_Date/Currency [@Kod='{doviz}']/ForexBuying").InnerXml);

            return kur;
        }
        #endregion
    }
}
