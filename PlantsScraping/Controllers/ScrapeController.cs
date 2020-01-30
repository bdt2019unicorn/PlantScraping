using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO; 

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
                categories["Description"] += DescriptionText(paragraph_div); 

                return categories; 
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
                string html = category_paragraph.InnerHtml;
                string[] paragraphs = html.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string paragraph in paragraphs)
                {
                    HtmlNode div = CategoriesDiv(paragraph);
                    try
                    {
                        HtmlNode strong = div.SelectSingleNode("strong");
                        string category = strong.InnerText;
                        category = category.Substring(0, category.IndexOf(":"));
                        strong.ParentNode.RemoveChild(strong);
                        categories[category] = div.InnerText; 
                    }
                    catch 
                    {
                        categories["Description"] +=div.InnerText.Trim()+"\n"; 
                    }
                }
                categories["Description"] = ((string)categories["Description"]).Trim(); 
                return categories; 
            }

            private static HtmlNode CategoriesDiv(string html)
            {
                HtmlDocument doc = new HtmlDocument();
                HtmlNode div = doc.CreateElement("div");
                div.InnerHtml = html;
                return div; 
            }


            private static string DescriptionText(HtmlNodeCollection paragraph_div)
            {
                string description = "";
                string bad_line = "Thanks to Wikipedia for text and information";
                for (int i = 1; i < paragraph_div.Count; i++)
                {
                    description += paragraph_div[i].InnerText.Trim() + "\n"; 
                }
                description = description.Substring(0, description.IndexOf(bad_line));
                description = description.Trim();
                return description; 
            }


            //public static Dictionary<string,object>[]DumpyDictionaries()
            //{
            //    int n = 3;
            //    string[] columns = { "Name", "Description", "Other Things" };
            //    string[] links =
            //    {
            //        "http://www.terrain.net.nz/friends-of-te-henui-group/plants-native-botanical-names-r-to-z/wahlenbergia-albomarginata-subsp-laxa-new-zealand-harebell.html",
            //        "http://www.terrain.net.nz/friends-of-te-henui-group/plants-native-botanical-names-r-to-z/wahlenbergia-matthewsii-rock-harebell.html",
            //        "http://www.terrain.net.nz/friends-of-te-henui-group/plants-native-botanical-names-r-to-z/wahlenbergia-pygmaea-subsp-pygmaea-north-island-harebell.html",
            //        "http://www.terrain.net.nz/friends-of-te-henui-group/plants-native-botanical-names-r-to-z/wahlenbergia-violacea-rimu-roa.html",
            //         "http://www.terrain.net.nz/friends-of-te-henui-group/plants-native-botanical-names-r-to-z/poor-knights-lily-xeronema-callistemon.html"
            //    };
            //    Dictionary<string, object>[] dumpy = new Dictionary<string, object>[n];
            //    for (int i = 0; i < n; i++)
            //    {
            //        Dictionary<string, object> key_values = new Dictionary<string, object>(); 
            //        foreach (string column in columns)
            //        {
            //            key_values[column] = Path.GetRandomFileName(); 
            //        }
            //        key_values["Image Links"] = links;
            //        dumpy[i] = key_values; 
            //    }
            //    return dumpy; 
            //}
        }
    }
}