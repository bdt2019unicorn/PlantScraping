using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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
        public string ScrapeAction(string[] links)
        {
            Dictionary<string, object>[] dictArray = new Dictionary<string, object>[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                Dictionary<string, object> plant_information = Support.TableRowValues(links[i]);
                dictArray[i] = plant_information;
            }

            string json = JsonConvert.SerializeObject(dictArray);
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            myFunction(dictArray);
            String cnames = "";
            foreach (String c in columnNames)
            {
                cnames += c + "\n";
            }

            return cnames;
            //return directory;
        }

        
        
        
public static List<String> columnNames = new List<string>();
public static void columnNamesGenerator(Dictionary<string, object> dict)
{

    foreach (KeyValuePair<string, object> entry in dict)
    {
        String key = entry.Key;
        if (!columnNames.Contains(key))
        {
            columnNames.Add(key);
        }

    }



}
public static void myFunction(Dictionary<string, object>[] dictArray)
{
    foreach (Dictionary<string, object> dict in dictArray)
    {
        columnNamesGenerator(dict);
    }
}

public static class Support
{
    public static Dictionary<string, object> TableRowValues(string url)
    {
        HtmlNode main_html = MainHtml(url);

        Dictionary<string, object> key_values = new Dictionary<string, object>();
        key_values["name"] = main_html.SelectSingleNode("h2").InnerText;
        HtmlNodeCollection paragraph_div = main_html.SelectNodes("p|div");

        Dictionary<string, object> categories = Categories(paragraph_div[0]);
        categories["Description"] = DescriptionText(paragraph_div, (string)categories["Description"]);
        categories["Image Links"] = ImageLinks(main_html);

        key_values = key_values.Concat(categories).ToDictionary(x => x.Key, x => x.Value);

        return key_values;
    }

    private static string[] ImageLinks(HtmlNode main_html)
    {
        List<string> all_links = new List<string>();
        string bad_img_src = "/images/template/donate.png";
        Uri base_uri = new Uri("http://www.terrain.net.nz/");
        HtmlNodeCollection images = main_html.SelectNodes("//img");
        foreach (HtmlNode img in images)
        {
            string src = img.GetAttributeValue("src", bad_img_src);
            if (!(src.IndexOf(bad_img_src) >= 0))
            {
                if (MainHtml(src, true) == null)
                {
                    Uri uri = new Uri(base_uri, src);
                    src = uri.AbsoluteUri;
                }
                all_links.Add(src);
            }

        }

        return all_links.ToArray();
    }


    private static HtmlNode MainHtml(string url, bool test_only = false)
    {
        HtmlDocument doc = null;
        try
        {
            var web = new HtmlWeb();
            doc = web.Load(url);
        }
        catch
        {
            return null;
        }
        if (test_only)
        {
            return CategoriesDiv("<h1>Good</h1>");
        }
        HtmlNode main_html = doc.GetElementbyId("main");
        HtmlNodeCollection bad_nodes = main_html.SelectNodes("div[contains(@class, 'hr')]|div[contains(@class, 'right49')]|div[contains(@class, 'left49')]|hr[contains(@class, 'accessibility')]");

        foreach (HtmlNode node in bad_nodes)
        {
            node.ParentNode.RemoveChild(node, false);
        }



        return main_html;
    }

    private static Dictionary<string, object> Categories(HtmlNode category_paragraph)
    {
        Dictionary<string, object> categories = new Dictionary<string, object>();
        categories["Description"] = "";
        string html = category_paragraph.InnerHtml;
        string[] paragraphs = html.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string paragraph in paragraphs)
        {
            HtmlNode div = CategoriesDiv(paragraph);
            int first_strong = paragraph.IndexOf("<strong>");
            if (first_strong >= 0)
            {
                string name = StrongInnerText(first_strong, paragraph);
                try
                {
                    name = name.Substring(0, name.IndexOf(":"));
                }
                catch { }
                string inner_text = div.InnerText;
                inner_text = inner_text.Replace(name + ":", String.Empty);
                categories[name] = inner_text;
            }
            else
            {
                categories["Description"] += div.InnerText.Trim() + "\n";
            }



            //try
            //{
            //    HtmlNodeCollection strong = div.SelectNodes("//strong");
            //    string category = strong[0].InnerText;
            //    category = category.Substring(0, category.IndexOf(":"));
            //    strong[0].ParentNode.RemoveChild(strong[0]);
            //    categories[category] = div.InnerText; 
            //}
            //catch 
            //{
            //    categories["Description"] +=div.InnerText.Trim()+"\n"; 
            //}
        }
        categories["Description"] = ((string)categories["Description"]).Trim();
        return categories;
    }

    private static string StrongInnerText(int first, string html)
    {
        int strong_length = "<strong>".Length;
        int last = html.IndexOf("</strong>");
        first += strong_length;
        return html.Substring(first, last - first);
    }

    private static HtmlNode CategoriesDiv(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        HtmlNode div = doc.CreateElement("div");
        div.InnerHtml = html;
        return div;
    }


    private static string DescriptionText(HtmlNodeCollection paragraph_div, string description)
    {
        string bad_line = "Thanks to Wikipedia for text and information";
        for (int i = 1; i < paragraph_div.Count; i++)
        {
            description += paragraph_div[i].InnerText.Trim() + "\n";
        }
        try
        {
            description = description.Substring(0, description.IndexOf(bad_line));
        }
        catch { }
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