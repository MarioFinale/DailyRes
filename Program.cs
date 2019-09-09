using System;
using MWBot.net.WikiBot;
using MWBot.net;
using System.Drawing;
using DailyRes.Properties;
using Image = System.Drawing.Image;

namespace DailyRes
{
    /// <summary>
    /// 
    /// </summary>
    public static class Program
    {
        static readonly string Reqpath = Utils.Utils.Exepath + "req" + Utils.Utils.DirSeparator;
        static readonly string Log_Filepath = Utils.Utils.Exepath + "dreslog.psv";
        static readonly string Users_Filepath = Utils.Utils.Exepath + "Users.psv";
        static readonly string ConfigFile = Utils.Utils.Exepath + "Config.cfg";
        /// <summary>
        /// Event logger.
        /// </summary>
        public static LogEngine.LogEngine EventLogger = new LogEngine.LogEngine(Log_Filepath, Users_Filepath, "DailyRes");
        static void Main(string[] args)
        {
            Bot ESWikiBOT = new Bot(ConfigFile);
            string folderpath = Utils.Utils.Exepath + "dres" + Utils.Utils.DirSeparator;

            for (; ; )
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
                            DailyRes dailyRes = new DailyRes(ref ESWikiBOT);
                            Resource resource = new Resource(tday, ref ESWikiBOT);
                            dailyRes.MakeResourceDescriptionFile(tday, resource, folderpath);
                            dailyRes.MakeCommonsFile(tday, resource, folderpath);
                            dailyRes.MakeResourceFile(tday, resource, folderpath);
                 

                        }
                        //delete old data
                        for (int i = -1; i > -4; i -= 1)
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
