using MWBot.net.WikiBot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Image = System.Drawing.Image;

namespace DailyRes
{
    class Resource
    {
        private string _Name;
        /// <summary>
        /// Nombre del recurso.
        /// </summary>
        public string Name { get { return _Name; } }
        private string _CommonsUrl;
        /// <summary>
        /// URL al recurso en Wikimedia commons.
        /// </summary>
        public string CommonsUrl { get { return _CommonsUrl; } }
        private string[] _PagesLinks;
        /// <summary>
        /// Lista con nombre de las páginas enlazadas dentro de la página de la cual se extrajo el recurso.
        /// </summary>
        public string[] PagesLinks { get { return _PagesLinks; } }
        private ResourceType _Type;
        /// <summary>
        /// Tipo de recurso según el enum ResourceType
        /// </summary>
        public ResourceType Type { get { return _Type; } }
        private string _License;
        /// <summary>
        /// Licencia que cubre al recurso.
        /// </summary>
        public string License { get { return _License; } }
        private string _LicenseUrl;
        /// <summary>
        /// URL a la página que describe la licencia que cubre al recurso.
        /// </summary>
        public string LicenseUrl { get { return _LicenseUrl; } }
        private string _Author;
        /// <summary>
        /// Autor del recurso según metadatos de Commons.
        /// </summary>
        public string Author { get { return _Author; } }
        private bool _FBCompat;
        /// <summary>
        /// Indica si el recurso contiene una licencia compatible con la subida a facebook.
        /// </summary>
        public bool FBCompat { get { return _FBCompat; } }
        private bool _CheckMediaLicense;
        /// <summary>
        /// Indica si es necesario verificar requisitos extra de uso indicados en la página en commons.
        /// </summary>
        public bool CheckMediaLicense { get { return _CheckMediaLicense; } }
        private Image _Image;
        /// <summary>
        /// En caso de ser una imagen, entrega el contenido (PNG).
        /// </summary>
        public Image Image { get { return _Image; } }
        private bool _Avaliable;
        /// <summary>
        /// Indica si el recurso existe y está disponible.
        /// </summary>
        public bool Avaliable { get { return _Avaliable; } }
        private string _Extract;
        /// <summary>
        /// Entrega el extracto de la página de la cual fue estraído el recurso.
        /// </summary>
        public string Extract { get { return _Extract; } }
        private string _MediaDescription;
        /// <summary>
        /// Entrega la descripción del recurso en commons si corresponde.
        /// </summary>
        public string MediaDescription { get { return _MediaDescription; } }

        public Resource(DateTime resDate, ref Bot workerBot)
        {
            int resNumber = GetNumber(resDate, ref workerBot);
            Page resPage = workerBot.Getpage("Plantilla:RDD/" + resNumber.ToString());
            string imageName = Utils.Utils.TextInBetween(resPage.Content, "|imagen=", "|")[0].Trim();     
            Tuple<Image, string[]> tImage = GetCommonsFile(imageName, ref workerBot);

            Image image = tImage.Item1;
            string commonsurl = "https://commons.wikimedia.org/wiki/File:" + WebUtility.UrlEncode(imageName.Trim().Replace(" ", "_"));
           // string mediadescription = ""; //Pending!
            string license = tImage.Item2[0];
            string licenseurl = tImage.Item2[1];
            string author = tImage.Item2[2];
            string extract = resPage.Extract;
            ResourceType type = GetResourceType(imageName);
            bool fbcompat = LicenseFBCompat(license);
            bool avaliable = (image == null);
            string[] links =  Utils.Utils.TextInBetween(resPage.Content, "[[", "]]");
            List<string> linkslist = new List<string>();
            foreach (string link in links)
            {
                if (link.Contains("|"))
                {
                    linkslist.Add(link.Split('|')[0].Trim());
                }
                else
                {
                    linkslist.Add(link.Trim());
                }
            }
            links = linkslist.ToArray();

            _Name = imageName;
            _CommonsUrl = commonsurl;
            _PagesLinks = links;
            _Type = type;
            _License = license;
            _LicenseUrl = licenseurl;
            _Author = author;
            _FBCompat = fbcompat;
            _CheckMediaLicense = CheckCopyRights(license);
            _Image = image;
            _Avaliable = avaliable;
            _Extract = extract;
            _MediaDescription = "";
        }

        bool CheckCopyRights(string licensename)
        {
            if (licensename.ToLowerInvariant().Trim().Contains("copyright free")) return true;
            if (licensename.ToLowerInvariant().Trim().Contains("attribution")) return true;
            return false;
        }

        bool LicenseFBCompat(string licensename)
        {
            if (licensename.ToLowerInvariant().Trim().Contains("public domain")) return true;
            if (licensename.ToLowerInvariant().Trim().Contains("zero")) return true;
            if (licensename.ToLowerInvariant().Trim().Contains("cc0")) return true;
            if (licensename.ToLowerInvariant().Trim().Contains("copyright free")) return true;
            if (licensename.ToLowerInvariant().Trim().Contains("attribution")) return true;
            return false;
        }
        
        ResourceType GetResourceType(string filename)
        {            
            MatchCollection tmatches = Regex.Matches(filename, "\\.[^\\s,\\.-]{3,4}");
            string extension = tmatches[tmatches.Count - 1].Value.Trim().ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                    {
                        return ResourceType.Image;
                    }
                case ".png":
                    {
                        return ResourceType.Image;
                    }
                case ".jpeg":
                    {
                        return ResourceType.Image;
                    }
                case ".tiff":
                    {
                        return ResourceType.Image;
                    }
                case ".xcf":
                    {
                        return ResourceType.Image;
                    }
                case ".svg":
                    {
                        return ResourceType.Image;
                    }
                case ".gif":
                    {
                        return ResourceType.Image;
                    }
                case ".mp3":
                    {
                        return ResourceType.Sound;
                    }
                case ".mid":
                    {
                        return ResourceType.Sound;
                    }
                case ".ogg":
                    {
                        return ResourceType.Sound;
                    }
                case ".spx":
                    {
                        return ResourceType.Sound;
                    }
                case ".flac":
                    {
                        return ResourceType.Sound;
                    }
                case ".opus":
                    {
                        return ResourceType.Sound;
                    }
                case ".wav":
                    {
                        return ResourceType.Sound;
                    }
                case ".ogv":
                    {
                        return ResourceType.Video;
                    }
                case ".webm":
                    {
                        return ResourceType.Video;
                    }
            }
            return ResourceType.Other;
        }


        Image PicFromUrl(string url)
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
                Program.EventLogger.EX_Log(ex.Message, "Resource");
                img.Dispose();
                return null;
            }
        }

        int GetNumber(DateTime resdate, ref Bot workerBot)
        {
            string datestring = resdate.Year.ToString("0000") + "-" + resdate.Month.ToString("00");
            Page tpage = workerBot.Getpage("Plantilla:RDD/º/" + datestring);
            List<Template> tlist = Template.GetTemplates(tpage);

            foreach (Template temp in tlist)
            {
                if (temp.Name.Trim().Contains("#switch"))
                {

                    foreach (Tuple<string, string> param in temp.Parameters)
                    {
                        if (param.Item1.Trim() == resdate.Day.ToString())
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

        Tuple<Image, string[]> GetCommonsFile(string CommonsFilename, ref Bot workerBot)
        {
            string responsestring = Utils.Utils.NormalizeUnicodetext(workerBot.GETQUERY("action=query&format=json&titles=File:" + Utils.Utils.UrlWebEncode(CommonsFilename) + "&prop=imageinfo&iiprop=extmetadata|url&iiurlwidth=1000"));
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
    }


}
