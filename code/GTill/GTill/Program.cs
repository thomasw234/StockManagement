using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TillEngine;
using System.IO;

namespace GTill
{
    static class Program
    {
        /// <summary>
        /// The very first procedure that is called in the program
        /// </summary>
        /// <param name="args">Any arguments passed to the program</param>
        [STAThread]
        static void Main(string[] args)
        {
            // Ensure that the compression dll exists
            if (!File.Exists("Ionic.Zip.dll"))
            {
                FileStream fs = new FileStream("Ionic.Zip.dll", FileMode.Create);
                fs.Write(Properties.Resources.Ionic_Zip, 0, Properties.Resources.Ionic_Zip.Length);
                fs.Close();
            }
            
            bool bFatalErrors = false;
            if (args.Length > 0)
            {
                if (args[0] == "config")
                {
                    frmConfig c = new frmConfig();
                    c.ShowDialog();
                }
                else if (args[0] == "regreport")
                {
                    try
                    {
                        // Print a register report and exit
                        TillEngine.TillEngine tEngine = new TillEngine.TillEngine();
                        tEngine.LoadTablesForJustRegisterReport();
                        tEngine.PrintRegisterReport();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error. Unable to print the selected register report:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Application.Exit();
                    return;
                }
            }
            // Checks for fatal errors with the configuration file
            if (PreRunChecks(ref bFatalErrors))
            {
                // If no errors, then continue to start the program
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //try
                //{
                    Application.Run(new frmMain());
                //}
                /*catch (Exception ex)
                {
                    ErrorHandler.LogError(ex.ToString()); 
                    if (MessageBox.Show("Sorry, an error has occured with GTill. GTill will restart automatically when you select OK. Press Cancel to not restart", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Application.Restart();
                    }
                }*/
            }
            else
            {
                if (bFatalErrors)
                {
                    System.Windows.Forms.MessageBox.Show("One or more fatal errors occured whilst trying to load the till. Please see the log for more information.");
                }
                else
                {
                    // Not a serious error
                    System.Windows.Forms.MessageBox.Show("One or more errors occured whilst trying to load the till. Please see the log for more information");
                }
            }
             
            /*
            TillEngine.TillEngine tEngine = new TillEngine.TillEngine();
            tEngine.LoadTable("STOCK");
            tEngine.LoadTable("STKLEVEL");
            tEngine.LoadTable("TILLCAT");
            //Application.Run(new frmSearchForItemV2(ref tEngine));
            //Application.Run(new frmCategorySelect(ref tEngine));
             */
             
        }

        /// <summary>
        /// Various checks on the configuration 
        /// </summary>
        /// <param name="bFatal">Reference to whether or not the error occured (if any) is fatal</param>
        /// <returns>Whether or not the test was passed</returns>
        static bool PreRunChecks(ref bool bFatal)
        {
            return true;
            bool bPassed = true;
            bFatal = false;

            if (!File.Exists("GTill.exe.config"))
            {
                ErrorHandler.LogError("GTill.config.exe is missing");
                bPassed = false;
            }

            if (GTill.Properties.Settings.Default.bAutoSwapBootFiles)
            {
                if (!File.Exists("C:\\BOOT1.INI"))
                {
                    ErrorHandler.LogError("C:\\BOOT1.INI doesn't exist, but bAutoSwapBootFiles = true. BOOT1.INI should default to Windows");
                    bPassed = false;
                    bFatal = true;
                }
                if (!File.Exists("C:\\BOOT2.INI"))
                {
                    ErrorHandler.LogError("C:\\BOOT2.INI doesn't exist, but bAutoSwapBootFiles = true. BOOT2.INI should default to MS-DOS");
                    bPassed = false;
                    bFatal = true;
                }
            }

            if (GTill.Properties.Settings.Default.bDoBackups)
            {
                if (!Directory.Exists(GTill.Properties.Settings.Default.sBackupLocation))
                {
                    ErrorHandler.LogError("bDoBackups is true, but sBackupDir doesn't exist. Make sure sBackupDir doesn't have a backslash at the end. So F:\\till_backup, not F:\\till_backup\\");
                    bPassed = false;
                    bFatal = true;
                }
            }

            if (GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy && !GTill.Properties.Settings.Default.bUseFloppyCashup)
            {
                ErrorHandler.LogError("bUsingDosInsteadOfFloppy is true, but bUseFloppyCashup is false. Set bUseFloppyCashup to true");
                bFatal = true;
                bPassed = false;
            }

            if (!File.Exists(GTill.Properties.Settings.Default.sAccStatLoc))
            {
                ErrorHandler.LogError("Couldn't find the file for ACCSTAT.DBF, check sAccStatLoc");
                bFatal = true;
                bPassed = false;
            }

            if (!File.Exists(GTill.Properties.Settings.Default.sBranchLoc))
            {
                ErrorHandler.LogError("Couldn't find the file for BRANCH.DBF, check sBranchLoc");
                bFatal = true;
                bPassed = false;
            }

            if (!File.Exists(GTill.Properties.Settings.Default.sCustRecLoc))
            {
                ErrorHandler.LogError("Couldn't find the file for CUSTREC.DBF, check sCustRecLoc");
                bPassed = false;
            }

            if (!File.Exists(GTill.Properties.Settings.Default.sDetailsLoc))
            {
                ErrorHandler.LogError("Couldn't find the file for DETAILS.DBF, check sDetailsLoc");
                bFatal = true;
                bPassed = false;
            }

            if (!Directory.Exists(GTill.Properties.Settings.Default.sINGNGDir))
            {
                ErrorHandler.LogError("Couldn't find the directory for INGNG, check sINGNGDir");
                bFatal = true;
                bPassed = false;
            }

            if (!Directory.Exists(GTill.Properties.Settings.Default.sOUTGNGDir))
            {
                ErrorHandler.LogError("Couldn't find the directory for OUTGNG");
                bFatal = true;
                bPassed = false;
            }

            if (!Directory.Exists(GTill.Properties.Settings.Default.sFloppyLocation) && GTill.Properties.Settings.Default.bUseFloppyCashup)
            {
                ErrorHandler.LogError("bUseFloppyCashup is true, but the sFloppyLocation doesn't exist. Make sure that sFloppyLocation doesn't end with a backslash (just delete the backslash if it does)");
                bPassed = false;
            }

            if (!File.Exists("log.txt"))
            {
                File.Create("log.txt");
            }

            if (GTill.Properties.Settings.Default.fMainScreenFontSize < 1.0f)
            {
                GTill.Properties.Settings.Default.fMainScreenFontSize = 5.0f;
            }

            return bPassed;
        }

    }
}
