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
        static void Main(string[] args)
        {
            string qpath = Utils.Utils.Exepath + "req" + Utils.Utils.DirSeparator;

            Utils.Utils.Log_Filepath = Utils.Utils.Exepath + "dreslog.psv";
            Bot ESWikiBOT = new Bot(new ConfigFile(Utils.Utils.ConfigFilePath), Utils.Utils.Log_Filepath);
            DailyRes tres = new DailyRes(ESWikiBOT);
            string folderpath = Utils.Utils.Exepath + Utils.Utils.DirSeparator + "dres" + Utils.Utils.DirSeparator;

            for (;;)
            {
                try
                {
                    if (System.IO.File.Exists(qpath + "DailyRes.runme"))
                    {
                        System.IO.File.Delete(qpath + "DailyRes.runme");

                        if (!System.IO.Directory.Exists(folderpath))
                        {
                            System.IO.Directory.CreateDirectory(folderpath);
                        }

                        for (int i = 0; i < 31; i++)
                        {
                            DateTime tday = DateTime.UtcNow.AddDays(i);
                            string datename = tday.ToString("dd-MM-yyyy");

                            Tuple<string, string, string[]> tx = tres.GetResImg(i);
                            Tuple<System.Drawing.Image, string[]> imgdat = tres.GetCommonsFile(tx.Item2);

                            string tdesc = tx.Item1 + Environment.NewLine + Environment.NewLine;
                            tdesc = tdesc + tx.Item3[0] + Environment.NewLine + "• Enlace a la imagen completa: https://commons.wikimedia.org/wiki/File:" + Utils.Utils.UrlWebEncode(tx.Item2.Replace(" ", "_")) + Environment.NewLine + Environment.NewLine + "• Imagen por " + imgdat.Item2[2];
                            tdesc = tdesc + Environment.NewLine + "• Licencia: " + imgdat.Item2[0] + " (" + imgdat.Item2[1] + ")";
                            tdesc = Resources.header + tdesc + Resources.bottom;
                            System.IO.File.WriteAllText(folderpath + datename + ".htm", tdesc);
                            System.IO.File.WriteAllText(folderpath + datename + ".commons.htm", "https://commons.wikimedia.org/wiki/File:" + Utils.Utils.UrlWebEncode(tx.Item2.Replace(" ", "_")));
                            imgdat.Item1.Save(folderpath + datename + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

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
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    Utils.Utils.EventLogger.EX_Log(ex.Message, "DailyRes");
                }
                System.Threading.Thread.Sleep(250);
            }

        }
    }






}
