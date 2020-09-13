using System;
using MWBot.net.WikiBot;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing;
using Image = System.Drawing.Image;
using DailyRes_dotnet.Properties;

namespace DailyRes
{
    class DailyRes
    {
        public static Bot Workerbot { get; private set; }

        public DailyRes(ref Bot tbot)
        {
            Workerbot = tbot;
        }


        string GetPageUrlByName(string pagename, string wiki)
        {
            return "https://" + wiki + ".wikipedia.org/wiki/" + Utils.Utils.UppercaseFirstCharacter(pagename.Replace(" ", "_").Trim());
        }

        public bool MakeResourceFile(DateTime tdate, Resource tresource, string folderpath)
        {
            string filename = tdate.Day.ToString("00") + "-" + tdate.Month.ToString("00") + "-" + tdate.Year.ToString("0000") + ".png";
            string filepath = folderpath + filename;


            if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
            try
            {
                tresource.Image.Save(filepath);
            }
            catch (Exception e)
            {
                Program.EventLogger.EX_Log(e.Message, "MakeResourceFile");
                return false;
            }
            return true;
        }
        

       public bool MakeResourceDescriptionFile(DateTime tdate, Resource tresource, string folderpath)
        {
            string body = Resources.header;
            body += tresource.Extract;

            if (tresource.FBCompat)
            {
                body += Environment.NewLine + Environment.NewLine + "• " + tresource.PagesLinks[0] + ": " + GetPageUrlByName(tresource.PagesLinks[0], "es");
                body += Resources.bottom1;
                body += Resources.bottom5_licensecompat_;
                if (tresource.CheckMediaLicense)
                {
                    body += "La licencia del recurso permite que se suba directamente a Facebook, pero puede ser necesario dar atribución al autor." + 
                                  Environment.NewLine + "<br />Es necesario verificar el recurso en commons.";
                }
                else
                {
                    body += "La licencia del recurso permite que se suba directamente a Facebook.";
                }
                if (tresource.Type != ResourceType.Image)
                {
                    body += Environment.NewLine + "La imagen en la derecha es solo una previsualización del " + nameof(tresource.Type) +
                                                        " para subir el recurso a Facebook es necesario descargarlo desde Wikimedia Commons.";
                }
                body += Resources.bottom6_licensecompat2_;
            } else
            {
                body += Resources.bottom1;
                body += Resources.bottom2_pboturl_ + MakeDresURL(tresource, tdate) + Resources.bottom3_pboturlclose_;
                body += Resources.bottom4_pboturlcopybutton_;
                body += Resources.bottomlast;
            }

            string filename = tdate.Day.ToString("00") + "-" + tdate.Month.ToString("00") + "-" + tdate.Year.ToString("0000") + ".htm";
            string filepath = folderpath + filename;

            if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
            try
            {
                System.IO.File.WriteAllText(filepath, body);
            }
            catch (Exception e)
            {
                Program.EventLogger.EX_Log(e.Message, "MakeResourceDescriptionFile");
                return false;
            }
            return true;
        }


       public bool MakeCommonsFile(DateTime tdate, Resource tresource, string folderpath)
        {
            string commonsUrl = tresource.CommonsUrl;
            string filename = tdate.Day.ToString("00") + "-" + tdate.Month.ToString("00") + "-" + tdate.Year.ToString("0000") + ".commons.htm";
            string filepath = folderpath + filename;

            if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
            try
            {
                System.IO.File.WriteAllText(filepath, commonsUrl);
            }
            catch (Exception e)
            {
                Program.EventLogger.EX_Log(e.Message, "MakeCommonsFile");
                return false;
            }
            return true;
        }

        

        string MakeDresURL(Resource tresource, DateTime tdate)
        {

            string tURL;
            string tlink = tresource.PagesLinks[0];
            if (tlink.Contains("|")) tlink = tlink.Split('|')[0];

            tURL =  "https://tools.wmflabs.org/periodibot/drespage.php?"
                    + "wikiurl=" + Utils.Utils.UrlWebEncode("https://es.wikipedia.org/wiki/" + Utils.Utils.UppercaseFirstCharacter(tlink).Replace(" ", "_"))
                    + "&commonsfilename=" + Utils.Utils.UrlWebEncode(tresource.Name)
                    + "&imgdesc=" + Utils.Utils.UrlWebEncode(tresource.Extract)
                    + "&authorurl="
                    + "&timage=" + Utils.Utils.UrlWebEncode("https://tools.wmflabs.org/periodibot/dres/" + tdate.Day.ToString("00") + "-" + tdate.Month.ToString("00") + "-" + tdate.Year.ToString() + ".png")
                    + "&title=" + Utils.Utils.UrlWebEncode("Recurso del día en Wikipedia, la enciclopedia libre.");
            return tURL;
        }



        public Tuple<string, string, string[]> GetResImg(DateTime tdate)
        {
            int tpageindex = GetNumber(tdate.Year, tdate.Month, tdate.Day);
            Page respage = Workerbot.Getpage("Plantilla:RDD/" + tpageindex.ToString());
            string pageimage = Utils.Utils.TextInBetween(respage.Content, "|imagen=", "|")[0].Trim();
            List<string> links = Utils.Utils.TextInBetween(respage.Content, "[[", "]]").ToList();

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains("|"))
                {
                    links[i] = links[i].Split('|')[0];
                }
                links[i] = "• " + Utils.Utils.UppercaseFirstCharacter(links[i]) + ": https://tools.wmflabs.org/periodibot/drespage.php?"
                    + "wikiurl=" + Utils.Utils.UrlWebEncode("https://es.wikipedia.org/wiki/" + Utils.Utils.UppercaseFirstCharacter(links[i]).Replace(" ", "_"))
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
            return new Tuple<Image, string[]>(img, new string[] { licence, licenceurl, author });
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
                if (temp.Name.Trim().Contains("#switch"))
                {

                    foreach (Tuple<string, string> param in temp.Parameters)
                    {
                        if (param.Item1.Trim() == day.ToString())
                        {
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
