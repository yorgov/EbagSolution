using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ebag
{
    public class Tests
    {

        const string _sender = "ivan.yorgov@outlook.com";
        const string _password = "Bp9TH8Y3vftt";

        [Test]
        public void Test1()
        {           
            IWebElement timeSlotsElement;
            var text = "НЯМА СВОБОДНИ СЛОТОВЕ.";

            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.ebag.bg/");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            int countIndex = 1;
            Login(driver,wait);
            Thread.Sleep(1000);
            driver.Navigate().GoToUrl("https://www.ebag.bg/cart/");

            while (true)
            {
                Console.WriteLine($"Number of refreshes: {countIndex++}");
                wait.Until(dr => dr.FindElement(By.XPath("//b[contains(text(),'.')]")));
                timeSlotsElement = driver.FindElement(By.XPath("//b[contains(text(),'.')]"));
                if (timeSlotsElement.Text == text)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(15));
                    driver.Navigate().Refresh();
                }
                else
                {
                    break;
                }
            }


            var valueElementText = driver.FindElement(By.XPath("//div[@class='text']//p")).Text;
            valueElementText = valueElementText.Replace("лв.", string.Empty);
            if (double.TryParse(valueElementText,out var value))
            {
                if (value > 0)
                {
                    driver.FindElement(By.XPath("//input[@class='btn order-btn']")).Click();
                    driver.FindElement(By.XPath("//div[@class='order-finalize-btn-wrapper']//button")).Click();
                }                
            }
            SendMail();
            driver.Close();
            driver.Quit();
        }

        private static void Login(ChromeDriver driver, WebDriverWait wait)
        {
            driver.FindElement(By.XPath("//button[@class='login btn-text']")).Click();
            wait.Until(dr => dr.FindElement(By.XPath("//form[@class='login-form']")));
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@placeholder='Email']")).SendKeys("ivan.yorgov@gmail.com");
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@placeholder='Парола']")).SendKeys("zs6qGPXF3jgU");
            
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@class='btn']")).Click();
        }

        public void SendMail()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ivan.yorgov@outlook.com"));
            message.To.Add(new MailboxAddress("ivan.yorgov@gmail.com"));
            message.Subject = "Slotove";
            message.Body = new TextPart("plain")
            {
                Text = "Ima slotove!"
            };
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.office365.com", 587, false);
                client.Authenticate(_sender, _password);
                client.Send(message);
                client.Disconnect(true);
            }
        }

    }
}