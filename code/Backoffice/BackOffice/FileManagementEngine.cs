using System;
using System.IO;

namespace BackOffice
{
    /// <summary>
    /// Used for IO tasks
    /// </summary>
    class FileManagementEngine
    {
        /// <summary>
        /// Adds the given directory to a zip files called files.zip
        /// Could be used with any directory, it just adds all .DBF files
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void CompressArchiveDirectory(string sSaveLoc)
        {
            try
            {
                if (!sSaveLoc.EndsWith("\\"))
                    sSaveLoc = sSaveLoc + "\\";
                if (!File.Exists(sSaveLoc + "files.zip")) // Check that it hasn't already been done
                {
                    // Otherwise, continue
                    // Add to a zip file
                    Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile(sSaveLoc + "\\files.zip");
                    foreach (string file in Directory.GetFiles(sSaveLoc))
                    {
                        zf.AddFile(file, "");
                    }
                    zf.Save();
                    zf.Dispose();
                }
                if (!File.Exists(sSaveLoc + "TILL1\\INGNG\\files.zip"))
                {
                    Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(sSaveLoc + "TILL1\\INGNG\\files.zip");
                    if (Directory.Exists(sSaveLoc + "TILL1\\INGNG\\"))
                    {
                        foreach (string file in Directory.GetFiles(sSaveLoc + "TILL1\\INGNG\\"))
                        {
                            zip.AddFile(file, "");
                        }
                        zip.Save();
                    }
                    zip.Dispose();
                }

                // Delete remaining dBase files
                foreach (string file in Directory.GetFiles(sSaveLoc))
                {
                    if (file.EndsWith(".DBF"))
                    {
                        File.Delete(file);
                    }
                }
                if (Directory.Exists(sSaveLoc + "TILL1\\INGNG\\"))
                {
                    foreach (string file in Directory.GetFiles(sSaveLoc + "TILL1\\INGNG\\"))
                    {
                        if (file.EndsWith(".DBF"))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Could not compress archive folder: " + ex.Message);
            }
        }

        /// <summary>
        /// Uncompress a directory so that it can be used in the archive
        /// For multiple tills this will need fixing!
        /// </summary>
        /// <param name="sDir">The Archive Directory to extract</param>
        public static void UncompressDirectory(string sDir)
        {
            try
            {
                if (!sDir.EndsWith("\\"))
                    sDir = sDir + "\\";
                if (File.Exists(sDir + "files.zip"))
                {
                    Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile(sDir + "files.zip");
                    zf.ExtractAll(sDir, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    zf.Dispose();
                }
                File.Delete(sDir + "files.zip");

                if (File.Exists(sDir + "TILL1\\INGNG\\files.zip"))
                {
                    Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile(sDir + "TILL1\\INGNG\\files.zip");
                    zf.ExtractAll(sDir + "TILL1\\INGNG\\", Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    zf.Dispose();
                }
                File.Delete(sDir + "TILL1\\INGNG\\files.zip");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Could not uncompress directory: " + ex.Message);
            }
        }

        /// <summary>
        /// Compresses the whole archive
        /// </summary>
        public static void CompressWholeArchive()
        {
            frmProgressBar fp = new frmProgressBar("Compressing Archive");
            string[] periods = { "Daily", "Weekly", "Monthly", "Yearly" };
            fp.Show();
            foreach (string period in periods)
            {
                string[] dirs = Directory.GetDirectories("Archive\\" + period);
                fp.pb.Value = 0;
                fp.pb.Maximum = dirs.Length;
                foreach (string dir in dirs)
                {
                    fp.pb.Value++;
                    fp.Text = "Compressing " + dir;
                    CompressArchiveDirectory(dir + "\\");
                }
            }
            fp.Close();
        }

        /// <summary>
        /// Backs up the current databases to a USB (or otherwise connected) drive
        /// </summary>
        public static void BackupToUSBPen(string CompanyName)
        {
            bool bFound = false;
            for (int i = 67; i < 84; i++)
            {
                if (Directory.Exists(((char)i).ToString() + ":\\BACKUP"))
                {
                    frmProgressBar fp = new frmProgressBar("Copying Tables");
                    fp.pb.Maximum = 24;
                    fp.pb.Value = 0;
                    fp.Show();
                    if (!Directory.Exists(((char)i).ToString() + ":\\BACKUP\\" + CompanyName))
                        Directory.CreateDirectory(((char)i).ToString() + ":\\BACKUP\\" + CompanyName);
                    string sSaveLoc = ((char)i).ToString() + ":\\BACKUP\\" + CompanyName + "\\";
                    File.Copy("ACCSTAT.DBF", sSaveLoc + "ACCSTAT.DBF", true);
                    fp.pb.Value = 1;
                    File.Copy("CATEGORY.DBF", sSaveLoc + "CATEGORY.DBF", true);
                    fp.pb.Value = 2;
                    File.Copy("CATGRPDA.DBF", sSaveLoc + "CATGRPDA.DBF", true);
                    fp.pb.Value = 3;
                    File.Copy("CATGPHDR.DBF", sSaveLoc + "CATGPHDR.DBF", true);
                    fp.pb.Value = 4;
                    File.Copy("COMMPPL.DBF", sSaveLoc + "COMMPPL.DBF", true);
                    fp.pb.Value = 6;
                    File.Copy("COMMITEM.DBF", sSaveLoc + "COMMITEM.DBF", true);
                    fp.pb.Value = 7;
                    //tCustomer.SaveToFile(sSaveLoc + "CUSTOMER.DBF");
                    File.Copy("EMAILS.DBF", sSaveLoc + "EMAILS.DBF", true);
                    fp.pb.Value = 8;
                    File.Copy("MULTIDAT.DBF", sSaveLoc + "MULTIDAT.DBF", true);
                    fp.pb.Value = 9;
                    File.Copy("MULTIHDR.DBF", sSaveLoc + "MULTIHDR.DBF", true);
                    fp.pb.Value = 10;
                    File.Copy("ORDER.DBF", sSaveLoc + "ORDER.DBF", true);
                    fp.pb.Value = 11;
                    File.Copy("ORDERLIN.DBF", sSaveLoc + "ORDERLIN.DBF", true);
                    fp.pb.Value = 12;
                    File.Copy("ORDERSUG.DBF", sSaveLoc + "ORDERSUG.DBF", true);
                    fp.pb.Value = 13;
                    File.Copy("SETTINGS.DBF", sSaveLoc + "SETTINGS.DBF", true);
                    fp.pb.Value = 14;
                    File.Copy("SHOP.DBF", sSaveLoc + "SHOP.DBF", true);
                    fp.pb.Value = 15;
                    File.Copy("STAFF.DBF", sSaveLoc + "STAFF.DBF", true);
                    fp.pb.Value = 16;
                    File.Copy("MAINSTOC.DBF", sSaveLoc + "MAINSTOC.DBF", true);
                    fp.pb.Value = 17;
                    File.Copy("STOCKSTA.DBF", sSaveLoc + "STOCKSTA.DBF", true);
                    fp.pb.Value = 18;
                    File.Copy("SUPPLIER.DBF", sSaveLoc + "SUPPLIER.DBF", true);
                    fp.pb.Value = 19;
                    File.Copy("SUPINDEX.DBF", sSaveLoc + "SUPINDEX.DBF", true);
                    fp.pb.Value = 20;
                    File.Copy("TILL.DBF", sSaveLoc + "TILL.DBF", true);
                    fp.pb.Value = 21;
                    File.Copy("TOTSALES.DBF", sSaveLoc + "TOTSALES.DBF", true);
                    fp.pb.Value = 22;
                    File.Copy("VAT.DBF", sSaveLoc + "VAT.DBF", true);
                    fp.pb.Value = 23;
                    File.Copy("STOCKLEN.DBF", sSaveLoc + "STOCKLEN.DBF", true);
                    fp.pb.Value = 24;
                    bFound = true;
                    fp.Close();
                    break;

                }
            }
            if (!bFound)
                System.Windows.Forms.MessageBox.Show("Could not find the USB pen drive! Please make sure it is inserted and has a folder called BACKUP");
            else
                System.Windows.Forms.MessageBox.Show("Backed up OK!");
        }


        public static void RestoreToArchiveDir(string archiveFolder, string newArchive)
        {
            // First, archive the existing files
            Directory.CreateDirectory("Archive\\Restorations\\" + newArchive);
            string[] toCopy = Directory.GetFiles(Environment.CurrentDirectory, "*.DBF");
            foreach (string file in toCopy)
            {
                File.Copy(file, "Archive\\Restorations\\" + newArchive + "\\" + file.Split('\\')[file.Split('\\').Length - 1], true);
            }
            CompressArchiveDirectory("Archive\\Restorations\\" + newArchive);

            // Now, decompress the desired restore point
            UncompressDirectory(archiveFolder);

            // And copy the files across
            toCopy = Directory.GetFiles(archiveFolder, "*.DBF");
            foreach (string file in toCopy)
            {
                File.Copy(file, Environment.CurrentDirectory + "\\" + file.Split('\\')[file.Split('\\').Length - 1], true);
            }

            System.Windows.Forms.MessageBox.Show("Restoration Complete. Exiting");

            Environment.Exit(1);
        }
    }

}
