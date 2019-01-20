using System;
using MWBot.net.WikiBot;
using MWBot.net;
using System.Drawing;

namespace DailyRes
{
    class Program
    {
        static void Main(string[] args)
        {

            GlobalVars.Log_Filepath = GlobalVars.Exepath + "dreslog.psv";
            Bot ESWikiBOT = new Bot(new ConfigFile(GlobalVars.ConfigFilePath));
            DailyRes tres = new DailyRes(ESWikiBOT);
            string folderpath = GlobalVars.Exepath + GlobalVars.DirSeparator + "dres" + GlobalVars.DirSeparator;

            for (;;)
            {
                try
                {
                    if (System.IO.File.Exists(GlobalVars.Exepath + "DailyRes.exe.runme"))
                    {
                        System.IO.File.Delete(GlobalVars.Exepath + "DailyRes.exe.runme");

                        if (!System.IO.Directory.Exists(folderpath))
                        {
                            System.IO.Directory.CreateDirectory(folderpath);
                        }

                        for (int i = 0; i < 31; i++)
                        {
                            DateTime tday = DateTime.UtcNow.AddDays(i);
                            string datename = tday.ToString("dd-MM-yyyy");

                            Tuple<string, string, string[]> tx = tres.GetResImg(i);
                            Tuple<Image, string[]> imgdat = tres.GetCommonsFile(tx.Item2);

                            string tdesc = tx.Item1 + Environment.NewLine + Environment.NewLine;
                            tdesc = tdesc + tx.Item3[0] + Environment.NewLine + "• Enlace a la imagen completa: https://commons.wikimedia.org/wiki/File:" + Utils.UrlWebEncode(tx.Item2.Replace(" ", "_")) + Environment.NewLine + Environment.NewLine + "• Imagen por " + imgdat.Item2[2];
                            tdesc = tdesc + Environment.NewLine + "• Licencia: " + imgdat.Item2[0] + " (" + imgdat.Item2[1] + ")";
                            System.IO.File.WriteAllText(folderpath + datename + ".txt", tdesc);
                            System.IO.File.WriteAllText(folderpath + datename + ".commons.txt", "https://commons.wikimedia.org/wiki/File:" + Utils.UrlWebEncode(tx.Item2.Replace(" ", "_")));
                            imgdat.Item1.Save(folderpath + datename + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                        }
                        //delete old data
                        for (int i = -1; i > -4; i = i - 1)
                        {
                            DateTime tday = DateTime.UtcNow.AddDays(i);
                            string datename = tday.ToString("dd-MM-yyyy");

                            if (System.IO.File.Exists(folderpath + datename + ".txt"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".txt");
                            }
                            if (System.IO.File.Exists(folderpath + datename + ".commons.txt"))
                            {
                                System.IO.File.Delete(folderpath + datename + ".commons.txt");
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
                    Utils.EventLogger.EX_Log(ex.Message, "DailyRes");
                }
                System.Threading.Thread.Sleep(250);
            }

        }
    }






}
