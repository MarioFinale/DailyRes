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
        
        public Tuple<string,string,string[]> GetResImg(int offset)
        {
            Page tpage = Workerbot.Getpage("Plantilla:RDD/º");
            int tpageindex = int.Parse(tpage.Extract) + offset;

            Page respage = Workerbot.Getpage("Plantilla:RDD/" + tpageindex.ToString());
            string pageimage = Utils.Utils.TextInBetween(respage.Content,"|imagen=", "|")[0].Trim();
            List<string> links = Utils.Utils.TextInBetween(respage.Content, "[[", "]]").ToList();

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains("|"))
                {
                    links[i] = links[i].Split('|')[0];
                }
                links[i] = "• " + Utils.Utils.UppercaseFirstCharacter(links[i]) + ": https://es.wikipedia.org/wiki/" + Utils.Utils.UppercaseFirstCharacter(links[i]).Replace(" ", "_");
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
                Utils.Utils.EventLogger.EX_Log(ex.Message, "DailyRes");
                img.Dispose();
                return null;
            }
        }


    }


  
}
