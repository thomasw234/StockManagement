using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.IO;

namespace BackOffice
{
    class frmUpdater : ScalableForm
    {
        TextBox tbChanges;
        Button btnInstall;
        StockEngine sEngine;
        bool updateAvailable = true;

        public frmUpdater(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new System.Drawing.Size(600, 697);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            tbChanges = new TextBox();
            tbChanges.Multiline = true;
            tbChanges.Location = new System.Drawing.Point(10, 10);
            tbChanges.Size = new System.Drawing.Size(this.ClientSize.Width - tbChanges.Left - 10, 600);
            tbChanges.Font = new System.Drawing.Font("Consolas", 11.0f);
            this.Controls.Add(tbChanges);

            btnInstall = new Button();
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.Location = new System.Drawing.Point(10, 615);
            btnInstall.Size = new System.Drawing.Size(this.ClientSize.Width - tbChanges.Left - 10, this.ClientSize.Height - btnInstall.Top - 10);
            this.Controls.Add(btnInstall);
            btnInstall.Text = "Install";
            btnInstall.Click += new EventHandler(btnInstall_Click);
            btnInstall.Focus();
            btnInstall.KeyDown += new KeyEventHandler(btnInstall_KeyDown);

            if (!File.Exists("Update\\changeLog.txt"))
            {
                sEngine.CheckForUpdate(true);
                if (!File.Exists("Update\\changeLog.txt"))
                {
                    MessageBox.Show("Sorry, no updates available");
                    this.updateAvailable = false;
                    return;
                }
            }
            TextReader tr = new StreamReader("Update\\Changelog.txt");
            string s = tr.ReadToEnd();
            tr.Close();
            tbChanges.Text = s;
            tbChanges.ReadOnly = true;
            tbChanges.ScrollBars = ScrollBars.Vertical;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tbChanges.DeselectAll();
            btnInstall.Focus();
            if (!updateAvailable)
            {
                this.Close();
            }
        }

        void btnInstall_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void btnInstall_Click(object sender, EventArgs e)
        {
            this.Close();
            sEngine.InstallUpdate();
        }


    }
}
