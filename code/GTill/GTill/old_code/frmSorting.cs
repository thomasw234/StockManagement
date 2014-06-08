using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmSorting : Form
    {
        Label lblProgramName;
        Label lblDevName;
        Label lblCurrentlyDoing;

        public frmSorting()
        {
            this.GotFocus +=new EventHandler(frmSorting_VisibleChanged);
        }

        void frmSorting_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                SetupForm();
                FadeInControls();
                this.Refresh();
                if (!BackupEngine.FullBackup("Till_Software_Start"))
                {
                    lblCurrentlyDoing.Text = "Backup failed, see the log for more information";
                    lblCurrentlyDoing.ForeColor = Color.Red;
                    this.Refresh();
                    System.Threading.Thread.Sleep(10000);
                }
                else
                {
                    lblCurrentlyDoing.Text = "Deleting old backup data";
                    this.Refresh();
                    BackupEngine.RemoveOldFiles();
                }
                FadeOutControls();
                this.Close();
            }
        }

        void SetupForm()
        {
            this.BackColor = Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = Properties.Settings.Default.cFrmForeColour;
            lblProgramName = new Label();
            lblProgramName.Text = "GTILL For Windows";
            lblProgramName.Font = new System.Drawing.Font(Properties.Settings.Default.sFontName, 70.0f);
            lblProgramName.AutoSize = true;
            this.Controls.Add(lblProgramName);
            lblProgramName.Left = (this.Width / 2) - (lblProgramName.Width / 2);
            lblProgramName.Top = (this.Height / 5);
            lblProgramName.ForeColor = this.BackColor;

            lblDevName = new Label();
            lblDevName.Text = "By Thomas Wormald";
            lblDevName.Font = new System.Drawing.Font(Properties.Settings.Default.sFontName, 45.0f);
            lblDevName.AutoSize = true;
            this.Controls.Add(lblDevName);
            lblDevName.Left = (this.Width / 2) - (lblDevName.Width / 2);
            lblDevName.Top = lblProgramName.Top + lblProgramName.Height + 30;
            lblDevName.ForeColor = this.BackColor;

            lblCurrentlyDoing = new Label();
            lblCurrentlyDoing.Text = "Currently backing up databases. Please wait.";
            lblCurrentlyDoing.Font = new System.Drawing.Font(Properties.Settings.Default.sFontName, 25.0f);
            lblCurrentlyDoing.Top = (this.Height / 5) * 3;
            lblCurrentlyDoing.AutoSize = true;
            this.Controls.Add(lblCurrentlyDoing);
            lblCurrentlyDoing.Left = (this.Width / 2) - (lblCurrentlyDoing.Width / 2);
            lblCurrentlyDoing.ForeColor = this.BackColor;
        }

        void FadeInControls()
        {
            Color cFrmBackColour = this.BackColor; Color cFrmForeColour = this.ForeColor;   
            int nR = cFrmBackColour.R, nG = cFrmBackColour.G, nB = cFrmBackColour.B;
            int nDiffR = (cFrmForeColour.R - cFrmBackColour.R) / 10;
            int nDiffG = (cFrmForeColour.G - cFrmBackColour.G) / 10;
            int nDiffB = (cFrmForeColour.B - cFrmBackColour.B) / 10;

            for (int i = 0; i < 10; i++)
            {
                nR += nDiffR;
                nG += nDiffG;
                nB += nDiffB;
                Color cToChangeTo = Color.FromArgb(nR, nG, nB);
                lblCurrentlyDoing.ForeColor = cToChangeTo;
                lblDevName.ForeColor = cToChangeTo;
                lblProgramName.ForeColor = cToChangeTo;
                this.Refresh();
                System.Threading.Thread.Sleep(1);
            }
        }

        void FadeOutControls()
        {
            Color cFrmForeColour = this.BackColor; Color cFrmBackColour = this.ForeColor;
            int nR = cFrmBackColour.R, nG = cFrmBackColour.G, nB = cFrmBackColour.B;
            int nDiffR = (cFrmForeColour.R - cFrmBackColour.R) / 10;
            int nDiffG = (cFrmForeColour.G - cFrmBackColour.G) / 10;
            int nDiffB = (cFrmForeColour.B - cFrmBackColour.B) / 10;

            for (int i = 0; i < 10; i++)
            {
                nR += nDiffR;
                nG += nDiffG;
                nB += nDiffB;
                Color cToChangeTo = Color.FromArgb(nR, nG, nB);
                lblCurrentlyDoing.ForeColor = cToChangeTo;
                lblDevName.ForeColor = cToChangeTo;
                lblProgramName.ForeColor = cToChangeTo;
                this.Refresh();
                System.Threading.Thread.Sleep(1);
            }
        }
    

    }
}
