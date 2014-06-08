using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace GTill
{
    class frmProcessingINGNG : Form
    {
        Timer tmrCountDown;
        Label lblInfo;
        Label lblCountdown;
        int nTimeLeft = 5;
        public bool bOkToProcess = true;

        public frmProcessingINGNG()
        {
            this.Size = new Size(700, 300);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblInfo = new Label();
            lblInfo.Font = new Font(GTill.Properties.Settings.Default.sFontName, 20.0f);
            lblInfo.Text = "An update is ready to process";
            lblInfo.ForeColor = GTill.Properties.Settings.Default.cFrmBackColour;
            lblInfo.BackColor = GTill.Properties.Settings.Default.cFrmForeColour;
            lblInfo.AutoSize = true;
            this.Controls.Add(lblInfo);
            lblInfo.Location = new Point((this.Width / 2) - (lblInfo.Width / 2), this.Height / 3);

            lblCountdown = new Label();
            lblCountdown.Font = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
            lblCountdown.Text = "Press any key within 5 seconds to cancel processing it.\n\r(F1 to process now)";
            lblCountdown.ForeColor = GTill.Properties.Settings.Default.cFrmBackColour;
            lblCountdown.BackColor = GTill.Properties.Settings.Default.cFrmForeColour;
            lblCountdown.AutoSize = true;
            this.Controls.Add(lblCountdown);
            lblCountdown.Location = new Point((this.Width / 2) - (lblCountdown.Width / 2), this.Height / 2);

            tmrCountDown = new Timer();
            tmrCountDown.Interval = 1000;
            tmrCountDown.Tick += new EventHandler(tmrCountDown_Tick);
            tmrCountDown.Enabled = true;

            this.KeyDown += new KeyEventHandler(frmProcessingINGNG_KeyDown);

        }

        void frmProcessingINGNG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                bOkToProcess = true;
                this.Close();
            }
            else
            {
                bOkToProcess = false;
                this.Close();
            }
        }

        void tmrCountDown_Tick(object sender, EventArgs e)
        {
            nTimeLeft -= 1;
            lblCountdown.Text = "Press any key within " + nTimeLeft.ToString() + " seconds to cancel processing it.\n\r(F1 to process now)";
            if (nTimeLeft == 0)
            {
                this.Close();
            }
        }
    }
}
