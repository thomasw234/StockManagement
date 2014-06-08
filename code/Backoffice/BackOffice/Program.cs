using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Windows.Forms.WormaldForms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Ionic.Zip;

namespace BackOffice
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            /*Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            if (File.Exists("buildNum.txt"))
            {
                TextReader tReader = new StreamReader("buildNum.txt");
                Console.WriteLine("BackOffice for Windows, by Thomas Wormald");
                Console.WriteLine();
                Console.WriteLine("Build Information :");
                Console.WriteLine(tReader.ReadToEnd());
                tReader.Close();
            }
            Console.WriteLine("Program started.");*/
            if (args.Length == 3)
            {
                System.Drawing.Color cBackColour = System.Drawing.Color.FromArgb(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                frmMenu.BackGroundColour = cBackColour;
            }
            string shortcutString = null;
            if (args.Length == 1)
                shortcutString = args[0];
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(CurrentDomain_UnhandledException);

            if (CheckIfApplicationIsOpenElsewhere())
            {
                if (System.Windows.Forms.MessageBox.Show("Application is open on " + ReadOpenLog() + ". Should I exit and you can try again later?", "Error", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    File.Delete("Open.log");
                }
            }

            WriteApplicationOpenFlag();

            //Application.EnableVisualStyles();

            // Check if it's April Fools, of course!
            // 30.3.14 : Maybe not

            /*if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4 && DateTime.Now.Hour < 12)
            {
                // hehe
                Form frmJoke = new Form();
                frmJoke.FormBorderStyle = FormBorderStyle.None;
                frmJoke.WindowState = FormWindowState.Maximized;
                PictureBox pb = new PictureBox();
                frmJoke.Controls.Add(pb);
                pb.Location = new Point(0, 0);
                pb.Size = Screen.PrimaryScreen.Bounds.Size;
                pb.Image = Properties.Resources.aprilfirst;
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                frmJoke.KeyDown += new KeyEventHandler(frmJoke_KeyDown);
                frmJoke.BackColor = Color.Black;
                frmJoke.ShowDialog();
            }*/

            Application.Run(new frmMenu(shortcutString));

            File.Delete("Open.log");
            //StockEngine sEngine = new StockEngine();
            //Application.Run(new frmGraphSettings(ref sEngine));
            //StockEngine sEngine = new StockEngine();
            //Application.Run(new frmBatchEditItems(ref sEngine));
        }

        static void frmJoke_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                ((Form)sender).Close();
            }
            else if (e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1)
            {
                MessageBox.Show("Not enough conventional memory");
            }
            else if (e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2)
            {
                MessageBox.Show("Error DBFNTX/1001 Open Error: details.DBF (DOS Error 2)", "Error", MessageBoxButtons.AbortRetryIgnore);
            }
            else if (e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3)
            {
                MessageBox.Show("Proc INIT line 42, open error DETAILS.DBF (2), Retry?", "Error", MessageBoxButtons.YesNo);
            }
            else if (e.KeyCode == Keys.NumPad4 || e.KeyCode == Keys.D4)
            {
                MessageBox.Show("Till Not Found");
            }
            else if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.D5)
            {
                ;
            }
            else if (e.KeyCode == Keys.NumPad6 || e.KeyCode == Keys.D6)
            {
                MessageBox.Show("Floppy Drive A: not found");
            }
            else if (e.KeyCode == Keys.C)
            {
                MessageBox.Show("1000GB Hard Disk Found. Check will take ~300 hours. Proceed?", "Error", MessageBoxButtons.YesNo);
            }
            else if (e.KeyCode == Keys.X)
            {
                System.Diagnostics.Process.Start("explorer.exe");
            }
            else if (e.KeyCode == Keys.P)
            {
                MessageBox.Show("This hard disk does not support parking");
            }
        }

        static void CurrentDomain_UnhandledException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Dump(e.Exception);
            if (System.Windows.Forms.MessageBox.Show("Would you like to quit the backoffice? Not doing so may give unexpected results and potentially cause more problems.", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.ExitThread();
            }
        }

        static void Dump(Exception e)
        {
            // Take a print screen

            if (!Directory.Exists("CrashLog"))
            {
                Directory.CreateDirectory("CrashLog");
            }
            string sDir = DateTime.Now.ToString().Replace('/', '.').Replace(':', '.');
            Directory.CreateDirectory("CrashLog\\" + sDir);

            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            printscreen.Save("CrashLog\\" + sDir + "\\screenshot.jpg", ImageFormat.Jpeg);
            string[] sDBaseFiles = Directory.GetFiles(Application.StartupPath, "*.dbf");
            for (int i = 0; i < sDBaseFiles.Length; i++)
            {
                File.Copy(sDBaseFiles[i], "CrashLog\\" + sDir + "\\" + sDBaseFiles[i].Split('\\')[sDBaseFiles[i].Split('\\').Length - 1]);
            }
            Exception ex = e;
            TextWriter tw = new StreamWriter("CrashLog\\" + sDir + "\\log.txt", true);
            tw.WriteLine(DateTime.Now.ToString());
            tw.WriteLine(ex.ToString());

            frmErrorCatcher fec = new frmErrorCatcher(e.ToString());
            fec.ShowDialog();
            tw.WriteLine("User said: " + fec.UserText);
            tw.WriteLine("Additional Info: \n" + frmErrorCatcher.AdditionalErrorInformation);
            tw.Close();

                // Time to compress those badboys
                string[] sFilesInDir = Directory.GetFiles("CrashLog\\" + sDir);
                using (ZipFile zip = new ZipFile())
                {
                    foreach (string file in sFilesInDir)
                    {
                        if (fec.storeMethod == frmErrorCatcher.StoreMethod.StoreServer || (fec.storeMethod == frmErrorCatcher.StoreMethod.StoreLocally && file.ToUpper().Contains(".TXT")))
                        {
                            ZipEntry ze = zip.AddFile(file);
                        }
                    }
                    zip.Password = "Lords12345";
                    zip.Save("CrashLog\\" + sDir + "\\files.zip");
                }
                if (File.Exists("errorupload.ftp"))
                {
                    File.Delete("errorupload.ftp");
                }
                tw = new StreamWriter("errorupload.ftp", true);
                tw.WriteLine("backoffice@thomaswormald.co.uk");
                tw.WriteLine("Lords12345");
                tw.WriteLine("binary");
                tw.WriteLine("cd backoffice\\crashlogs");
                tw.WriteLine("mkdir \"" + sDir + "\"");
                tw.WriteLine("cd \"" + sDir + "\"");
                /*foreach (string s in sFilesInDir)
                {
                    tw.WriteLine("send \"" + "CrashLog\\" + sDir + "\\" + s.Split('\\')[s.Split('\\').Length - 1] + "\"");
                }*/
                tw.WriteLine("send \"" + "CrashLog\\" + sDir + "\\files.zip");
                tw.WriteLine("bye");
                tw.Close();

                System.Diagnostics.Process.Start("FTP", "-s:errorupload.ftp 94.136.40.75");
                bool bRunning = true;
                do
                {
                    bRunning = false;
                    System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
                    for (int i = 0; i < processList.Length; i++)
                    {
                        if (processList[i].ProcessName.ToUpper() == "FTP")
                        {
                            bRunning = true;
                        }
                    }
                } while (bRunning);
                File.Delete("errorupload.ftp");
        }


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Have a massive
            Dump((Exception)e.ExceptionObject);
        }

        static void WriteApplicationOpenFlag()
        {
            TextWriter tWriter = new StreamWriter("Open.log");
            tWriter.WriteLine(System.Environment.MachineName + " at " + DateTime.Now.ToString());
            tWriter.Close();
        }

        static bool CheckIfApplicationIsOpenElsewhere()
        {
            if (File.Exists("Open.log"))
            {
                return true;
            }
            return false;
        }

        static string ReadOpenLog()
        {
            TextReader tReader = new StreamReader("Open.log");
            string details = tReader.ReadLine();
            tReader.Close();

            return details;
        }
    }
}
