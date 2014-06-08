using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.IO;

namespace BackOffice
{
    class frmRestoreChoice : ScalableForm
    {

        private string[] folders;           // These two match up
        private string[] formattedDates;    // to each other

        private StockEngine sEngine;

        public frmRestoreChoice(ref StockEngine sEngine)
        {
            this.sEngine = sEngine;
            this.AllowScaling = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Text = "Restore";

            this.Size = new System.Drawing.Size(160, 600);
            ListBox lbDates = new ListBox();
            lbDates.Items.AddRange(this.getDates());
            lbDates.KeyDown += lbDates_KeyDown;
            lbDates.Size = new System.Drawing.Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
            lbDates.Location = new System.Drawing.Point(10, 10);
            lbDates.BorderStyle = BorderStyle.FixedSingle;
            lbDates.SelectedIndex = lbDates.Items.Count - 1;
            this.Controls.Add(lbDates);
            this.StartPosition = FormStartPosition.CenterParent;

            MessageBox.Show("The following restore points are made just before collection occurs on the date shown.");
        }

        void lbDates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Warn user
                if (MessageBox.Show("Are you 100% certain that you want to restore to " + formattedDates[((ListBox)sender).SelectedIndex] + "? If so, I will first calculate the number of changes that will occur on items in the database.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    int numberOfChanges = 0;
                    int numberOfNewItems = 0;

                    sEngine.CompareWithArchivedDay("Archive\\Daily\\" + folders[((ListBox)sender).SelectedIndex], out numberOfNewItems, out numberOfChanges);

                    if (MessageBox.Show("Number of new items that will be lost: " + numberOfNewItems.ToString(), "Lost Items", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == System.Windows.Forms.DialogResult.OK)
                    {
                        if (MessageBox.Show("Number of irreversible changes to items: " + numberOfChanges.ToString(), "Items Changes",MessageBoxButtons.OKCancel, MessageBoxIcon.Stop) == System.Windows.Forms.DialogResult.OK)
                        {
                            if (MessageBox.Show("Proceed with restore to " + formattedDates[((ListBox)sender).SelectedIndex] + "? The Backoffice will exit after completing the restore.", "Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            {

                                // Perform Restore

                                FileManagementEngine.RestoreToArchiveDir("Archive\\Daily\\" + folders[((ListBox)sender).SelectedIndex],DateTime.Now.ToString().Replace('/', '.').Replace(':', '.'));

                            }
                        }
                    }
                }
            
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        public string[] getDates()
        {
            folders = Directory.GetDirectories("Archive\\Daily");
            for (int i = 0; i < folders.Length; i++)
            {
                folders[i] = folders[i].Split('\\')[folders[i].Split('\\').Length - 1]; // Get just the last folder name
            }

            formattedDates = new string[folders.Length];
            for (int i = 0; i < formattedDates.Length; i++)
            {
                string[] dateParts = folders[i].Split('.');
                formattedDates[i] = dateParts[2] + "/" + dateParts[1] + "/" + dateParts[0];
            }
            return formattedDates;
        }
    }
}
