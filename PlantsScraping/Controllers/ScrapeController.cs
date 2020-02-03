using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Data;
using ClosedXML.Excel; 

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
            DataTable excel_data_table = new DataTable(); 
            for (int i = 0; i < links.Length; i++)
            {
                Dictionary<string, object> plant_information = Support.TableRowValues(links[i]);
                if(plant_information.Keys.Contains(""))
                {
                    plant_information.Remove(""); 
                }
                DataRow excel_data_row = excel_data_table.NewRow(); 
                foreach (string category in plant_information.Keys)
                {
                    if(!excel_data_table.Columns.Contains(category))
                    {
                        excel_data_table.Columns.Add(category); 
                    }
                    excel_data_row[category] = plant_information[category].GetType()==typeof(string)?plant_information[category]:JsonConvert.SerializeObject(plant_information[category], Formatting.Indented);
                }
                excel_data_table.Rows.Add(excel_data_row); 
                dictArray[i] = plant_information;
            }

            return ExportToExcel(excel_data_table); 
        }

        [HttpGet]
        public ActionResult DownloadExcel(string file_name)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file_name);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(path);
            System.IO.File.Delete(path);
            return File(fileByteArray, "application/vnd.ms-excel", file_name);
        }
        public static string ExportToExcel(DataTable table)
        {
            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(table, "plants");
            string file_name = "plants.xlsx"; 
            wb.SaveAs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file_name));
            return file_name; 
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
                    return DivFromHtml("<h1>Good</h1>");
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
                    HtmlNode div = DivFromHtml(paragraph);
                    int first_strong = paragraph.IndexOf("<strong");
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
                }
                categories["Description"] = ((string)categories["Description"]).Trim();
                return categories;
            }

            private static string StrongInnerText(int first, string html)
            {
                try
                {
                    int last = html.IndexOf("</strong>");
                    string div_inner_html = html.Substring(first, last - first) + "</strong>";
                    HtmlNode strong_div = DivFromHtml(div_inner_html); 
                    return strong_div.InnerText.Trim();
                }
                catch
                {
                    string inner_text = "";
                    first += "<strong".Length; 
                    while(first<html.Length)
                    {
                        char character = html[first]; 
                        if(character=='>')
                        {
                            int i = first+1;
                            while (i < html.Length)
                            {
                                character = html[i];
                                if (character != ':')
                                {
                                    inner_text += character;
                                }
                                else
                                {
                                    break;
                                }
                                i++;
                            }
                            break; 
                        }
                        first++; 
                    }
                    return inner_text; 
                }

            }

            private static HtmlNode DivFromHtml(string html)
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

        }
    }
}