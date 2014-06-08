using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace GTill
{
    class BackupEngine
   { 

        /// <summary>
        /// Backs up all files that are .DBF
        /// </summary>
        /// <param name="sDirToBackup">The directory to backup</param>
        /// <returns>True if successful, otherwise false</returns>
        public static bool FullBackup(string sSubDir)
        {
            if (GTill.Properties.Settings.Default.bDoBackups)
            {
                if (Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation))
                {
                    try
                    {
                        // sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir + "\\" + sDirToBackup[z] + "\\" - How confusing!
                        string[] sDirToBackup = { GTill.Properties.Settings.Default.sINGNGDir, GTill.Properties.Settings.Default.sOUTGNGDir, "..\\TILL" };
                        string[] sDestinationDirs = { "INGNG", "OUTGNG", "TILL" };
                        string sBackupDir = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
                        Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir);
                        Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir);
                        for (int z = 0; z < sDirToBackup.Length; z++)
                        {
                            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                            {
                                //Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir + "\\" + sDestinationDirs[z]);
                                string[] sFiles = Directory.GetFiles(sDirToBackup[z], "*.DBF");
                                for (int i = 0; i < sFiles.Length; i++)
                                {
                                    //File.Copy(sFiles[i], GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir + "\\" + sDestinationDirs[z] + "\\" + sFiles[i].Split('\\')[sFiles[i].Split('\\').Length - 1], true);
                                    Ionic.Zip.ZipEntry ze = zip.AddFile(sFiles[i]);
                                }
                                zip.Save(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir + "\\" + sDestinationDirs[z] + ".zip");
                            }
                        }
                        TextWriter tw = new StreamWriter(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\" + sSubDir + "\\backuplog.txt", true);
                        tw.WriteLine("Full backup performed - " + DateTime.Now.ToString());
                        tw.WriteLine("");
                        tw.Close();
                    }
                    catch (Exception e)
                    {
                        GTill.ErrorHandler.LogError("Full backup failed. Error: " + e.Message);
                        return false;
                    }
                }
                else
                {
                    GTill.ErrorHandler.LogError("Full backup failed. sBackupDir (Directory " + GTill.Properties.Settings.Default.sBackupLocation + ") doesn't exist. To disable backups please read the manual about changing bDoBackups to False in the configuration file.");
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Performs a partial backup, by copying REPDATA, TDATA and THDR to the backup location
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool PartBackup()
        {
            if (GTill.Properties.Settings.Default.bDoBackups)
            {
                if (Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation))
                {
                    try
                    {
                        string sBackupDir = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
                        string sBackupSubDir = DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;
                        if (!Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir))
                        {
                            Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir);
                        }
                        if (!Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction"))
                        {
                            Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction");
                        }
                        Directory.CreateDirectory(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir);
                        using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                        {
                            zip.AddFile(GTill.Properties.Settings.Default.sRepDataLoc);
                            zip.AddFile(GTill.Properties.Settings.Default.sTDataLoc);
                            zip.AddFile(GTill.Properties.Settings.Default.sTHdrLoc);
                            zip.Save(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir + "\\Files.zip");
                            /* Removed as files are now compressed
                            File.Copy(GTill.Properties.Settings.Default.sRepDataLoc, GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir + "\\REPDATA.DBF", true);
                            File.Copy(GTill.Properties.Settings.Default.sTDataLoc, GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir + "\\TDATA.DBF", true);
                            File.Copy(GTill.Properties.Settings.Default.sTHdrLoc, GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir + "\\THDR.DBF", true);*/
                        }
                        TextWriter tw = new StreamWriter(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir + "\\After_Transaction\\" + sBackupSubDir + "\\backuplog.txt", true);
                        tw.WriteLine("Part backup performed - " + DateTime.Now.ToString());
                        tw.WriteLine("");
                        tw.Close();
                    }
                    catch (Exception e)
                    {
                        GTill.ErrorHandler.LogError("Part backup failed. Error : " + e.Message);
                    }
                }
                else
                {
                    GTill.ErrorHandler.LogError("Part backup failed. sBackupDir (Directory " + GTill.Properties.Settings.Default.sBackupLocation + ") doesn't exist. To disable backups please read the manual about changing bDoBackups to False in the configuration file.");
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Removes backup files from more than 2 weeks ago
        /// </summary>
        public static void RemoveOldFiles()
        {
            DateTime d = DateTime.Today;
            d = d.Subtract(TimeSpan.FromDays(14.0));
            string sBackupDir = d.Day.ToString() + "." + d.Month.ToString() + "." + d.Year.ToString();
            if (Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir))
            {
                Directory.Delete(GTill.Properties.Settings.Default.sBackupLocation + "\\" + sBackupDir, true);
            }
        }
    }
}
