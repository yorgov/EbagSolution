using MailKit.Net.Smtp;
using MimeKit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;

namespace Runner
{
    class Program
    {
        const string _sender = "ivan.yorgov";
        const string _password = "rtmW5!3IY";

        static int[] _minutesToSleepInterval = new[] { 5, 10 };

        const string _logFilePath = @"\EbagLog.txt";

        static void Main(string[] args)
        {
            RunCheck();
        }

        private static void RunCheck()
        {
            IWebDriver driver = null;
            IWebElement timeSlotsElement;
            var text = "НЯМА СВОБОДНИ СЛОТОВЕ.";

            while (true)
            {
                try
                {
                    driver = new ChromeDriver();
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    driver.Navigate().GoToUrl("https://www.ebag.bg/");
                    int countIndex = 1;

                    LogMessage($"Number of refreshes: {countIndex++}");
                    wait.Until(dr => dr.FindElement(By.XPath("//span[@class='text']//b")));
                    timeSlotsElement = driver.FindElement(By.XPath("//span[@class='text']//b"));
                    if (timeSlotsElement.Text == text)
                    {
                        var timeToSleep = new Random().Next(_minutesToSleepInterval[0], _minutesToSleepInterval[1]);
                        LogMessage($"No Slots Available sleeping for {timeToSleep} minutes");
                        Thread.Sleep(TimeSpan.FromMinutes(timeToSleep));
                    }
                    else
                    {
                        LogMessage("Slot Found Sending email");
                        SendMail();
                        // var remainingTime = GetRemainingTime();
                        // LogMessage($"Waiting for {remainingTime.Hours:D2} hours, {remainingTime.Minutes:D2} minutes and {remainingTime.Seconds:D2} seconds");
                        // Thread.Sleep((int)remainingTime.TotalMilliseconds);
                    }

                }
                catch (Exception e)
                {
                    LogMessage(e.ToString());
                }
                finally
                {
                    if (driver != null)
                    {
                        driver.Close();
                        driver.Quit();
                    }
                }
            }
        }

        private static TimeSpan GetRemainingTime()
        {
            var endOfDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            return endOfDay.Subtract(DateTime.Now);
        }

        private static void Login(IWebDriver driver, WebDriverWait wait)
        {
            driver.FindElement(By.XPath("//button[@class='login btn-text']")).Click();
            wait.Until(dr => dr.FindElement(By.XPath("//form[@class='login-form']")));
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@placeholder='Email']")).SendKeys("ivan.yorgov@gmail.com");
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@placeholder='Парола']")).SendKeys("zs6qGPXF3jgU");
            driver.FindElement(By.XPath("//form[@class='login-form']//input[@class='btn']")).Click();
            Thread.Sleep(1000);
            driver.Navigate().GoToUrl("https://www.ebag.bg/cart/");
        }
        public static void SendMail()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ivan.yorgov@gmail.com"));
            message.To.Add(new MailboxAddress("ivan.yorgov@gmail.com"));
            message.Subject = "Slotove";
            message.Body = new TextPart("plain")
            {
                Text = "Ima slotove!"
            };
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(_sender, _password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
        private static void LogMessage(string message)
        {
            //throw new Exception();
            var messageToWrite = $"{DateTime.Now} - {message}";
            Console.WriteLine(messageToWrite);
            File.AppendAllLines(_logFilePath, new[] { messageToWrite });
        }
    }
}
