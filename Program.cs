using System;
using MWBot.net.WikiBot;
using MWBot.net;
using System.Drawing;
using DailyRes.Properties;
using Image = System.Drawing.Image;

namespace DailyRes
{
   class Program
    {
        static string Reqpath = Utils.Utils.Exepath + "req" + Utils.Utils.DirSeparator;
        static string Log_Filepath = Utils.Utils.Exepath + "dreslog.psv";
        static string Users_Filepath = Utils.Utils.Exepath + "Users.psv";
        public static LogEngine.LogEngine EventLogger = new LogEngine.LogEngine(Log_Filepath, Users_Filepath, "DailyRes");
        static void Main(string[] args)
        {
            Bot ESWikiBOT = new Bot(new ConfigFile(Utils.Utils.Exepath + "Config.cfg"));
            DailyRes tres = new DailyRes(ESWikiBOT);
            string folderpath = Utils.Utils.Exepath + Utils.Utils.DirSeparator + "dres" + Utils.Utils.DirSeparator;

            for (;;)
            {
                try
                {
                    if (!System.IO.Directory.Exists(Reqpath))
                    {
                        System.IO.Directory.CreateDirectory(Reqpath);
                    }
                    if (System.IO.File.Exists(Reqpath + "DailyRes.runme"))
                    {
                        System.IO.File.Delete(Reqpath + "DailyRes.runme");

                        if (!System.IO.Directory.Exists(folderpath))
                        {
                            System.IO.Directory.CreateDirectory(folderpath);
                        }

                        for (int i = 0; i < 31; i++)
                        {
                            DateTime tday = DateTime.UtcNow.AddDays(i);
                            string datename = tday.ToString("dd-MM-yyyy");

                            Tuple<string, string, string[]> tx = tres.GetResImg(tday);
                            
                            string tdesc = tx.Item1 + Environment.NewLine + Environment.NewLine;
                            tdesc = tdesc + tx.Item3[0];
                            tdesc = Resources.header + tdesc + Resources.bottom;
                            System.IO.File.WriteAllText(folderpath + datename + ".htm", tdesc);
                            System.IO.File.WriteAllText(folderpath + datename + ".commons.htm", "https://commons.wikimedia.org/wiki/File:" + Utils.Utils.UrlWebEncode(tx.Item2.Replace(" ", "_")));
                            tres.GetCommonsFile(tx.Item2).Item1.Save(folderpath + datename + ".png", System.Drawing.Imaging.ImageFormat.Png);

                        }
                        //delete old data
                        for (int i = -1; i > -4; i = i - 1)
                        {
                            DateTime tday = DateTime.UtcNow.AddDays(i);
                            string datename = tday.ToString("dd-MM-yyyy");

                            if (System.IO.File.Exists(folderpath + datename + ".htm"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".htm");
                            }
                            if (System.IO.File.Exists(folderpath + datename + ".commons.htm"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".commons.htm");
                            }
                            if (System.IO.File.Exists(folderpath + datename + ".jpg"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".jpg");
                            }
                            if (System.IO.File.Exists(folderpath + datename + ".png"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".png");
                            }
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.EX_Log(ex.Message, "DailyRes");
                }
                System.Threading.Thread.Sleep(250);
            }
        }
    }
}
