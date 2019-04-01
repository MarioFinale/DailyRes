using System;
using MWBot.net;
using MWBot.net.WikiBot;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Net;
using Image = System.Drawing.Image;

namespace DailyRes
{
    class DailyRes
    {
        public static Bot Workerbot { get; private set; }

        public DailyRes(Bot tbot)
        {
            Workerbot = tbot;
        }
        
        public Tuple<string,string,string[]> GetResImg(DateTime tdate)
        {
            int tpageindex = GetNumber(tdate.Year, tdate.Month, tdate.Day);
            Page respage = Workerbot.Getpage("Plantilla:RDD/" + tpageindex.ToString());
            string pageimage = Utils.Utils.TextInBetween(respage.Content,"|imagen=", "|")[0].Trim();
            List<string> links = Utils.Utils.TextInBetween(respage.Content, "[[", "]]").ToList();

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains("|"))
                {
                    links[i] = links[i].Split('|')[0];
                }
                links[i] = "• " + Utils.Utils.UppercaseFirstCharacter(links[i]) + ": https://tools.wmflabs.org/periodibot/drespage.php?" 
                    + "wikiurl=" + Utils.Utils.UrlWebEncode("https://es.wikipedia.org/" + Utils.Utils.UppercaseFirstCharacter(links[i]).Replace(" ", "_")) 
                    + "&commonsfilename=" + Utils.Utils.UrlWebEncode(pageimage) 
                    + "&imgdesc=" + Utils.Utils.UrlWebEncode(respage.Extract)
                    + "&authorurl="
                    + "&timage=" + Utils.Utils.UrlWebEncode("https://tools.wmflabs.org/periodibot/dres/" + tdate.Day.ToString("00") + "-" + tdate.Month.ToString("00") + "-" + tdate.Year.ToString() + ".png")
                    + "&title=" + Utils.Utils.UrlWebEncode("Recurso del día en Wikipedia, la enciclopedia libre.");
            }
            return new Tuple<string, string, string[]>(respage.Extract, pageimage, links.ToArray());
        }

        public Tuple<Image, string[]> GetCommonsFile(string CommonsFilename)
        {
            string responsestring = Utils.Utils.NormalizeUnicodetext(Workerbot.GETQUERY("action=query&format=json&titles=File:" + Utils.Utils.UrlWebEncode(CommonsFilename) + "&prop=imageinfo&iiprop=extmetadata|url&iiurlwidth=1000"));
            string[] thumburlmatches = Utils.Utils.TextInBetween(responsestring, "\"thumburl\":\"", "\",");
            string[] licencematches = Utils.Utils.TextInBetween(responsestring, "\"LicenseShortName\":{\"value\":\"", "\",");
            string[] licenceurlmatches = Utils.Utils.TextInBetween(responsestring, "\"LicenseUrl\":{\"value\":\"", "\",");
            string[] authormatches = Utils.Utils.TextInBetween(responsestring, "\"Artist\":{\"value\":\"", "\",");
            string matchstring = @"<[\S\s]+?>";
            string matchstring2 = @"\([\S\s]+?\)";

            string licence = string.Empty;
            string licenceurl = string.Empty;
            string author = string.Empty;

            if (licencematches.Count() > 0)
                licence = Regex.Replace(licencematches[0], matchstring, "");
            if (licenceurlmatches.Count() > 0)
                licenceurl = Regex.Replace(licenceurlmatches[0], matchstring, "");
            if (authormatches.Count() > 0)
            {
                author = Regex.Replace(authormatches[0], matchstring, "");
                author = Regex.Replace(author, matchstring2, "").Trim();
                string[] authors = author.Split(Environment.NewLine.ToCharArray());
                if (authors.Count() > 1)
                {
                    for (int i = 0; i <= authors.Count() - 1; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(authors[i]))
                            author = authors[i];
                    }
                }
                if (author.Contains(":"))
                    author = author.Split(':')[1].Trim();
            }
            Image img = new Bitmap(1, 1);
            if (thumburlmatches.Count() > 0)
                img = PicFromUrl(thumburlmatches[0]);
            if (string.IsNullOrWhiteSpace(author) | (author.ToLower().Contains("unknown")))
                author = "Desconocido";
            return new Tuple<Image, string[]>(img, new string[]{licence,licenceurl,author});
        }

        public Image PicFromUrl(string url)
        {
            Image img = new Bitmap(1, 1);
            try
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        img = (Image)Image.FromStream(stream).Clone();
                    }
                }
                return img;
            }
            catch (Exception ex)
            {
                Program.EventLogger.EX_Log(ex.Message, "DailyRes");
                img.Dispose();
                return null;
            }
        }

        public int GetNumber(int year, int month, int day)
        {
            string datestring = year.ToString() + "-" + month.ToString("00");

            Page tpage = Workerbot.Getpage("Plantilla:RDD/º/" + datestring);
            List<Template> tlist = Template.GetTemplates(tpage);

            foreach (Template temp in tlist)
            {
                if (temp.Name.Trim().Contains("#switch")){

                    foreach (Tuple<string,string> param in temp.Parameters)
                    {
                        if (param.Item1.Trim() == day.ToString()){
                            if (param.Item2.Contains("<!--"))
                            {
                                string ptext = param.Item2.Split('<')[0];
                                return int.Parse(Utils.Utils.RemoveAllAlphas(ptext));
                            }
                            else
                            {
                                return int.Parse(Utils.Utils.RemoveAllAlphas(param.Item2));
                            }                            
                        }
                    }
                }
            }
            return 0;
        }        
    }


  
}
