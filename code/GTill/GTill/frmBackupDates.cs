using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;

namespace GTill
{
    class frmBackupDates : Form
    {
        ListBox lbDates;

        public frmBackupDates()
        {
            lbDates = new ListBox();
            lbDates.Location = new System.Drawing.Point(10, 10);
            this.Size = new System.Drawing.Size(300, 500);
            lbDates.Size = new System.Drawing.Size(275, 450);
            lbDates.KeyDown += new KeyEventHandler(lbDates_KeyDown);
            

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Controls.Add(lbDates);
            this.StartPosition = FormStartPosition.CenterScreen;

            lbDates.Items.AddRange(getDates());
            if (lbDates.Items.Count > 1)
                lbDates.SelectedIndex = lbDates.Items.Count - 2;
            lbDates.Font = new System.Drawing.Font(Properties.Settings.Default.sFontName, 12.0f);
        }

        void lbDates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {

                    // Create the TILL Directory
                    Directory.CreateDirectory(Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL");

                    // Extract the files in the chosen location
                    Ionic.Zip.ZipFile zFile = new ZipFile(Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL.zip");
                    zFile.ExtractAll(Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL", ExtractExistingFileAction.OverwriteSilently);

                    // Copy GTill.exe to the chosen location
                    File.Copy(Application.ExecutablePath, Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL\\GTill.exe", true);

                    // Start the GTill with the regreport argument
                    System.Diagnostics.Process.Start(Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL\\GTill.exe", "regreport");

                    // Wait for the other instance of GTill to finish running before trying to delete it
                    System.Threading.Thread.Sleep(5000);

                    try
                    {
                        // Delete the newly created directory and all containing files
                        Directory.Delete(Properties.Settings.Default.sBackupLocation + "\\" + lbDates.Items[lbDates.SelectedIndex] + "\\Pre_Cash_Up\\TILL", true);
                    }
                    catch
                    {
                        // Other instance of GTill probably hadn't finished running, so we'll just leave it
                    }

                    this.Close();
                }
                catch
                {
                    MessageBox.Show("Can't reprint a register report for that date, sorry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private string[] getDates()
        {
            try
            {
                string[] sDirs = Directory.GetDirectories(Properties.Settings.Default.sBackupLocation);
                DateTime[] dtDates = new DateTime[sDirs.Length];
                for (int i = 0; i < sDirs.Length; i++)
                {
                    string sToDisplay = sDirs[i].Split('\\')[sDirs[i].Split('\\').Length - 1];
                    sDirs[i] = sToDisplay;

                    dtDates[i] = DateTime.Parse(sDirs[i]);
                }
                Array.Sort(dtDates);
                for (int i = 0; i < dtDates.Length; i++)
                {
                    sDirs[i] = dtDates[i].ToShortDateString();
                }

                return sDirs;
            }
            catch
            {
                return new string[] { "" };
            }
        }
    }
}
