using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    /// <summary>
    /// Typing error! Should be frmBackup
    /// </summary>
    class frnBackup : Form
    {
        /// <summary>
        /// A label showing the name of the program
        /// </summary>
        Label lblProgramName;
        /// <summary>
        /// A label showing the name of the developer
        /// </summary>
        Label lblDevName;
        /// <summary>
        /// A label telling the user what the program is currently doing
        /// </summary>
        Label lblCurrentlyDoing;

        /// <summary>
        /// Initialises the form
        /// </summary>
        public frnBackup()
        {
            this.GotFocus +=new EventHandler(frmSorting_VisibleChanged);
        }

        /// <summary>
        /// Handles the form being shown or hidden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmSorting_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                SetupForm();
                FadeInControls();
                this.Refresh();
                // Try a full backup
                if (!BackupEngine.FullBackup("Till_Software_Start"))
                {
                    // If the backup failed, tell the user
                    lblCurrentlyDoing.Text = "Backup failed, see the log for more information";
                    lblCurrentlyDoing.ForeColor = Color.Red;
                    this.Refresh();
                    // Wait 10 seconds so that the user has time to see the message
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

        /// <summary>
        /// Sets up the controls of the form
        /// </summary>
        void SetupForm()
        {
            this.BackColor = Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = Properties.Settings.Default.cFrmForeColour;
            lblProgramName = new Label();
            lblProgramName.Text = "GTill For Windows";
            lblProgramName.Font = new System.Drawing.Font(Properties.Settings.Default.sFontName, 70.0f);
            lblProgramName.AutoSize = true;
            this.Controls.Add(lblProgramName);
            lblProgramName.Left = (this.Width / 2) - (lblProgramName.Width / 2);
            lblProgramName.Top = (this.Height / 4);
            lblProgramName.ForeColor = this.BackColor;

            lblDevName = new Label();
            lblDevName.Text = "";
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
            lblCurrentlyDoing.AutoSize = false;
            lblCurrentlyDoing.Size = new Size(this.Width, 50);
            lblCurrentlyDoing.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(lblCurrentlyDoing);
            lblCurrentlyDoing.Left = (this.Width / 2) - (lblCurrentlyDoing.Width / 2);
            lblCurrentlyDoing.ForeColor = this.BackColor;
        }

        /// <summary>
        /// Fade in the form's controls
        /// </summary>
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

        /// <summary>
        /// Fade out the form's controls
        /// </summary>
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
