using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack; 

namespace PlantsScraping.Controllers
{
    public class ScrapeController : Controller
    {
        // GET: Scrape
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string ScrapeAction(string[]links)
        {
            string url = links[0];
            Dictionary<string, object> plant_information = Support.TableRowValues(url); 

            return "receive"; 
        }


        public static class Support
        {
            public static Dictionary<string,object>TableRowValues(string url)
            {
                HtmlNode main_html = MainHtml(url);

                Dictionary<string, object> key_values = new Dictionary<string, object>();
                key_values["name"] = main_html.SelectSingleNode("h2").InnerText;
                HtmlNodeCollection paragraph_div = main_html.SelectNodes("p|div"); 

                Dictionary<string, object> categories = Categories(paragraph_div[0]); 

                return null; 
            }


            private static HtmlNode MainHtml(string url)
            {
                var web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                HtmlNode main_html = doc.GetElementbyId("main");
                HtmlNodeCollection bad_nodes = main_html.SelectNodes("div[contains(@class, 'hr')]|div[contains(@class, 'right49')]|div[contains(@class, 'left49')]|hr[contains(@class, 'accessibility')]");

                foreach (HtmlNode node in bad_nodes)
                {
                    node.ParentNode.RemoveChild(node,false); 
                }

                    

                return main_html; 
            }

            private static Dictionary<string,object>Categories(HtmlNode category_paragraph)
            {
                Dictionary<string, object> categories = new Dictionary<string, object>();
                categories["Description"] = "";
                string bad_line = "Thanks to Wikipedia for text and information";
                string html = category_paragraph.InnerHtml;
                string[] paragraphs = html.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries); 
                return categories; 
            }
        }
    }
}