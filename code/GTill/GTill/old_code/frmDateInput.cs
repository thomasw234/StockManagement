using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GTill
{
    class frmDateInput : Form
    {
        /// <summary>
        /// The background and font colours to use
        /// </summary>
        Color cFrmForeColour, cFrmBackColour;
        /// <summary>
        /// The name of the font to use
        /// </summary>
        string sFontName;
        /// <summary>
        /// Shows the instructions for inputting the date
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// The textbox the user inputs the date into
        /// </summary>
        TextBox tbDateInput;

        Timer tmrUpdater;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="s">The size of the form</param>
        public frmDateInput(Size s)
        {
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            sFontName = Properties.Settings.Default.sFontName;

            this.BackColor = cFrmBackColour;
            this.ForeColor = cFrmForeColour;
            this.Size = s;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Show();
            SetupForm();
        }

        /// <summary>
        /// Initialises controls and places them in the correct place on the form
        /// </summary>
        public void SetupForm()
        {
            string sCurrentDate = DateTime.Now.Day.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();

            lblInstruction = new Label();
            lblInstruction.Left = 0;
            lblInstruction.AutoSize = false;
            lblInstruction.Width = this.Width;
            lblInstruction.Top = (this.Height / 7);
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            lblInstruction.Height = (this.Height / 7);
            lblInstruction.ForeColor = cFrmBackColour;
            lblInstruction.BackColor = cFrmBackColour;
            lblInstruction.Font = new Font(sFontName, 16.0f);
            lblInstruction.Text = "Today's date is " + sCurrentDate + ", press enter to confirm that this is correct, or enter the correct date (DDMMYYYY).";
            this.Controls.Add(lblInstruction);

            tbDateInput = new TextBox();
            tbDateInput.Font = new Font(sFontName, 24.0f);
            tbDateInput.Width = 170;
            tbDateInput.Left = (this.Width / 2) - (tbDateInput.Width / 2);
            tbDateInput.Top = (this.Height / 7) * 4;
            tbDateInput.BackColor = cFrmBackColour;
            tbDateInput.BorderStyle = BorderStyle.None;
            tbDateInput.ForeColor = cFrmBackColour;
            this.Controls.Add(tbDateInput);
            tbDateInput.Text = WorkOutDateString();
            tbDateInput.KeyUp += new KeyEventHandler(tbDateInput_KeyUp);
            tbDateInput.KeyDown += new KeyEventHandler(tbDateInput_KeyDown);
            System.Threading.Thread.Sleep(1000);
            FadeIn();
            System.Threading.Thread.Sleep(100);
            tbDateInput.Focus();
            tbDateInput.SelectAll();

            tmrUpdater = new Timer();
            tmrUpdater.Interval = 1000;
            tmrUpdater.Enabled = true;
            tmrUpdater.Tick += new EventHandler(tmrUpdater_Tick);
        }

        void tmrUpdater_Tick(object sender, EventArgs e)
        {
            if (!tbDateInput.Focused)
                tbDateInput.Focus();
        }

        /// <summary>
        /// Is called when a key is pressed in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void tbDateInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string sBefore = tbDateInput.Text;
                tbDateInput.Text = AddSeperators(tbDateInput.Text);
                if (sBefore == tbDateInput.Text) //If the format was incorrect
                    tbDateInput.Text = WorkOutDateString();
                else
                {
                    tbDateInput.BorderStyle = BorderStyle.None;
                    tbDateInput.Select(0, 0);
                    tbDateInput.Width += 150;
                    this.Refresh();                    
                    Fadeout();
                    System.Threading.Thread.Sleep(500);
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Occurs when a key goes up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has gone up</param>
        void tbDateInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbDateInput.Text == "" && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
            {
                tbDateInput.Text = WorkOutDateString(); // If the textbox is empty, then fill it with the current date
                tbDateInput.SelectAll();
            }
        }

        /// <summary>
        /// Adds the '/' between the date
        /// </summary>
        /// <param name="sDateTimeString">The string without the seperators</param>
        /// <returns>A string with the seperators</returns>
        private string AddSeperators(string sDateTimeString)
        {
            if (sDateTimeString.Length != 8)
                return sDateTimeString;

            string sDay = sDateTimeString[0].ToString() + sDateTimeString[1].ToString();
            string sMonth = sDateTimeString[2].ToString() + sDateTimeString[3].ToString();
            string sYear = sDateTimeString[4].ToString() + sDateTimeString[5].ToString() + sDateTimeString[6].ToString() + sDateTimeString[7].ToString();
            return sDay + "/" + sMonth + "/" + sYear;
        }

        /// <summary>
        /// Works out today's date in the format DDMMYYYY
        /// </summary>
        /// <returns>Today's date DDMMYYYY</returns>
        private string WorkOutDateString()
        {
            string sDay = DateTime.Now.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            string sMonth = DateTime.Now.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sYear = DateTime.Now.Year.ToString();
            return sDay + sMonth + sYear;
        }

        /// <summary>
        /// Fades the control font colours in from the background colour to the font colour
        /// </summary>
        private void FadeIn()
        {
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
                lblInstruction.ForeColor = cToChangeTo;
                tbDateInput.ForeColor = cToChangeTo;
                tbDateInput.ForeColor = cToChangeTo;
                this.Refresh();
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Fades the control font colours out from the font colour to the background colour
        /// </summary>
        private void Fadeout()
        {
            int nR = cFrmForeColour.R, nG = cFrmForeColour.G, nB = cFrmForeColour.B;
            int nDiffR = (cFrmBackColour.R - cFrmForeColour.R) / 10;
            int nDiffG = (cFrmBackColour.G - cFrmForeColour.G) / 10;
            int nDiffB = (cFrmBackColour.B - cFrmForeColour.B) / 10;

            for (int i = 0; i < 10; i++)
            {
                nR += nDiffR;
                nG += nDiffG;
                nB += nDiffB;
                Color cToChangeTo = Color.FromArgb(nR, nG, nB);
                lblInstruction.ForeColor = cToChangeTo;
                tbDateInput.ForeColor = cToChangeTo;
                tbDateInput.ForeColor = cToChangeTo;
                this.Refresh();
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets the date that was input by the user
        /// </summary>
        public string DateTimeInput
        {
            get
            {
                return tbDateInput.Text;
            }
        }
    }
}
