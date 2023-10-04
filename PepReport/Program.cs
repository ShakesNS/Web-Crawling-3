using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PepReport.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace PepReport
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChromeDriver driver = null;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ChromeOptions options = new ChromeOptions();
            driver = new ChromeDriver();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(180);
            driver.Navigate().GoToUrl("https://www.senado.gob.mx/65/senadores/por_orden_alfabetico");

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(driver.PageSource);

            List<Pep> peps = new List<Pep>();

            Thread.Sleep(5000);


            var ulElement=driver.FindElement(By.XPath("//*[@id=\"imPage\"]/div[6]/div[2]/div[1]/nav/ul"));

            var liElement = ulElement.FindElements(By.TagName("li"));
            foreach (var li in liElement)
            {
                li.Click();
                Thread.Sleep(2000);
                //*[@id="contCards"]/div[1]/div[1]/div[1]/div/a/img
                htmlDocument.LoadHtml(driver.PageSource);
                var pepDivs = htmlDocument.DocumentNode.SelectNodes("//div[@id='contCards']//div[@id_senador]");
                if (pepDivs != null)
                {
                    if (pepDivs.Count > 0)
                    {
                        foreach (var pep in pepDivs)
                        {
                            Pep pep1 = new Pep();
                            var imgLink = pep.SelectSingleNode(".//img[@class='rounded ']")?.Attributes["src"]?.Value.Trim();
                            if (!imgLink.StartsWith("http"))
                                imgLink = "https://www.senado.gob.mx/65" + imgLink;

                            var fullName = pep.SelectSingleNode(".//div[@class='cardNombre']//a").InnerText.Trim();

                            var contactDetails = pep.SelectSingleNode(".//div[@class='card-body']").InnerText.Trim();
                            var contactDetails2 = pep.SelectNodes(".//div[@class='card-body']//div")[0].InnerText.Trim();

                            var eMail = pep.SelectNodes(".//div[@class='card-body']//div")[1].InnerText.Trim();

                            pep1.ImageLink = imgLink;
                            pep1.FullName = fullName;
                            pep1.ContactDetails = contactDetails + " " + contactDetails2;
                            var position = pep1.ContactDetails.IndexOf("Correo");
                            var newText = "";
                            if (position >= 0)
                            {
                                // Belirli karakterden sonrasını silerek yeni bir string oluşturun
                                newText = pep1.ContactDetails.Substring(0, position + 1);


                            }
                            pep1.ContactDetails = newText;

                            pep1.Email = eMail;

                            string[] parcalar = pep1.Email.Split(":", StringSplitOptions.RemoveEmptyEntries);

                            pep1.Email = parcalar[1];

                            peps.Add(pep1);

                        }
                    }
                }
                else
                    continue;
            }

            var filepath2 = @"C:\Users\SS\source\repos\PepReport\PepReport\Files\abc.json";
            string json = JsonSerializer.Serialize(peps);
            File.WriteAllText(filepath2, json);
        }
    }
}
