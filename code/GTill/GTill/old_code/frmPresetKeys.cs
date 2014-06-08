using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmPresetKeys : Form
    {
        /// <summary>
        /// The background colour of the form
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The foreground (text) colour
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// The form's title
        /// </summary>
        Label lblTitle;
        /// <summary>
        /// A label giving instructions on 
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// A list of the names of keys
        /// </summary>
        ListBox lbKeyName;
        /// <summary>
        /// A list of barcodes assigned to keys
        /// </summary>
        ListBox lbKeyCode;
        /// <summary>
        /// A list of the desciptions associated with barcodes
        /// </summary>
        ListBox lbDescription;
        /// <summary>
        /// The titles of each column
        /// </summary>
        Label[] lblColumnTitles;
        /// <summary>
        /// The name of the font to use on this form
        /// </summary>
        string sFontName;
        /// <summary>
        /// The current key that is being re-assigned a barcode
        /// </summary>
        string sCurrentKeyBeingEdited;
        /// <summary>
        /// A reference to the TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;
        /// <summary>
        /// An input form to get the new barcode that will be assigned to a key
        /// </summary>
        frmInput fiGetNewCode;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="p">The location of the form</param>
        /// <param name="s">The size of the form</param>
        /// <param name="te">A reference to the TillEngine</param>
        public frmPresetKeys(Point p, Size s, ref TillEngine.TillEngine te)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = s;
            this.Location = p;
            this.BackColor = Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = Properties.Settings.Default.cFrmForeColour;
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            sFontName = Properties.Settings.Default.sFontName;
            tEngine = te;
            SetupForm();
            this.Paint += new PaintEventHandler(frmPresetKeys_Paint);
        }

        /// <summary>
        /// Draws a box around the form's controls when the form is repainted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmPresetKeys_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), 10, this.Height - 70, this.Width - 20, 50);
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lblColumnTitles[0].Left - 4, lblColumnTitles[0].Top - 4, ((this.Width / 3) * 2) + 8, this.Height - lbKeyCode.Top - 76 + lblColumnTitles[0].Top));
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lblColumnTitles[0].Left - 8, lblColumnTitles[0].Top - 8, ((this.Width / 3) * 2) + 16, this.Height - lbKeyCode.Top - 68 + lblColumnTitles[0].Top));
            e.Graphics.DrawLine(new Pen(cFrmForeColour, 2.0f), new Point(lblColumnTitles[0].Left - 4, lbKeyCode.Top - 4), new Point(lblColumnTitles[0].Left + ((this.Width / 3) * 2) + 4, lbKeyCode.Top - 4));
            e.Graphics.DrawLine(new Pen(cFrmForeColour, 2.0f), new Point(lblColumnTitles[0].Left - 4, lbKeyCode.Top - 8), new Point(lblColumnTitles[0].Left + ((this.Width / 3) * 2) + 4, lbKeyCode.Top - 8));
            e.Graphics.DrawLine(new Pen(cFrmBackColour, 2.0f), new Point(lblColumnTitles[0].Left - 6, lbKeyCode.Top - 6), new Point(lblColumnTitles[0].Left + ((this.Width / 3) * 2) + 6, lbKeyCode.Top - 6));
        }

        /// <summary>
        /// Sets up the form's controls
        /// </summary>
        public void SetupForm()
        {
            sCurrentKeyBeingEdited = "";
            lblTitle = new Label();
            lblTitle.BackColor = cFrmBackColour;
            lblTitle.ForeColor = cFrmForeColour;
            lblTitle.Font = new Font(sFontName, 14.0f);
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.Width, 25);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Text = "Define Preset Keys";
            this.Controls.Add(lblTitle);

            int nTotalWidth = (this.Width / 3) * 2;

            lblColumnTitles = new Label[3];
            for (int i = 0; i < lblColumnTitles.Length; i++)
            {
                lblColumnTitles[i] = new Label();
                lblColumnTitles[i].BackColor = cFrmBackColour;
                lblColumnTitles[i].ForeColor = cFrmForeColour;
                lblColumnTitles[i].Font = new Font(sFontName, 16.0f);
                lblColumnTitles[i].Top = lblTitle.Top + (lblTitle.Height * 2);
                lblColumnTitles[i].AutoSize = true;
                this.Controls.Add(lblColumnTitles[i]);
            }

            lblColumnTitles[0].Text = "Key";
            lblColumnTitles[1].Text = "Barcode";
            lblColumnTitles[2].Text = "Description";
            lblColumnTitles[0].Left = (this.Width/ 2) - (nTotalWidth / 2);
            lblColumnTitles[1].Left = lblColumnTitles[0].Left + 75;
            lblColumnTitles[2].Left = lblColumnTitles[1].Left + 200;

            lbKeyName = new ListBox();
            lbKeyName.BackColor = cFrmBackColour;
            lbKeyName.ForeColor = cFrmForeColour;
            lbKeyName.Font = new Font(sFontName, 16.0f);
            lbKeyName.Top = lblColumnTitles[0].Top + (lblColumnTitles[0].Height * 2);
            lbKeyName.Left = lblColumnTitles[0].Left;
            lbKeyName.Width = 100;
            lbKeyName.BorderStyle = BorderStyle.None;
            lbKeyName.Height = this.Height - lbKeyName.Top - 100;
            lbKeyName.SelectedIndexChanged += new EventHandler(lbKeyName_SelectedIndexChanged);
            lbKeyName.KeyDown += new KeyEventHandler(lbKeyName_KeyDown);
            this.Controls.Add(lbKeyName);

            lbKeyCode = new ListBox();
            lbKeyCode.BackColor = cFrmBackColour;
            lbKeyCode.ForeColor = cFrmForeColour;
            lbKeyCode.Font = new Font(sFontName, 16.0f);
            lbKeyCode.Top = lbKeyName.Top;
            lbKeyCode.Left = lblColumnTitles[1].Left;
            lbKeyCode.Width = 225;
            lbKeyCode.Height = this.Height - lbKeyCode.Top - 100;
            lbKeyCode.BorderStyle = BorderStyle.None;
            lbKeyCode.SelectedIndexChanged += new EventHandler(lbKeyName_SelectedIndexChanged);
            lbKeyCode.KeyDown += new KeyEventHandler(lbKeyName_KeyDown);
            this.Controls.Add(lbKeyCode);

            lbDescription = new ListBox();
            lbDescription.BackColor = cFrmBackColour;
            lbDescription.ForeColor = cFrmForeColour;
            lbDescription.Font = new Font(sFontName, 16.0f);
            lbDescription.Top = lbKeyName.Top;
            lbDescription.Left = lblColumnTitles[2].Left;
            lbDescription.Width = nTotalWidth - 200 - 75;
            lbDescription.Height = this.Height - lbDescription.Top - 100;
            lbDescription.BorderStyle = BorderStyle.None;
            lbDescription.SelectedIndexChanged += new EventHandler(lbKeyName_SelectedIndexChanged);
            lbDescription.KeyDown += new KeyEventHandler(lbKeyName_KeyDown);
            this.Controls.Add(lbDescription);

            lbKeyCode.BringToFront();
            lbDescription.BringToFront();

            lblInstruction = new Label();
            lblInstruction.Text = "Press Enter to change the currently selected key, or ESC to return to the till";
            lblInstruction.BackColor = cFrmBackColour;
            lblInstruction.ForeColor = cFrmForeColour;
            lblInstruction.Font = new Font(sFontName, 14.0f);
            lblInstruction.AutoSize = false;
            lblInstruction.Width = this.Width - 40;
            lblInstruction.Height = 30;
            lblInstruction.Top = this.Height - 60;
            lblInstruction.Left = 20;
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblInstruction);

            for (int i = 0; i < 12; i++)
            {
                lbKeyName.Items.Add("F" + (i + 1).ToString());
                string sItemBarcode = tEngine.sBarcodeFromFunctionKey("F" + (i + 1).ToString());
                if (sItemBarcode.TrimEnd('\0').TrimEnd(' ') != "")
                {
                    string[] sItemDetails = tEngine.GetItemDetailsForLookup(sItemBarcode.TrimEnd('\0').TrimEnd(' '));
                    lbDescription.Items.Add(sItemDetails[0]);
                }
                else
                    lbDescription.Items.Add("Undefined Key");
                lbKeyCode.Items.Add(sItemBarcode);
            }
            for (int i = 0; i < 12; i++)
            {
                lbKeyName.Items.Add("SF" + (i + 1).ToString());
                string sItemBarcode = tEngine.sBarcodeFromFunctionKey("SF" + (i + 1).ToString());
                if (sItemBarcode.TrimEnd('\0').TrimEnd(' ') != "")
                {
                    string[] sItemDetails = tEngine.GetItemDetailsForLookup(sItemBarcode.TrimEnd('\0').TrimEnd(' '));
                    lbDescription.Items.Add(sItemDetails[0]);
                }
                else
                    lbDescription.Items.Add("Undefined Key");
                lbKeyCode.Items.Add(sItemBarcode);
            }

            lbKeyCode.SelectedIndex = 0;
            lbDescription.SelectedIndex = 0;
            lbKeyName.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles a keydown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbKeyName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                string[] sExtraInfo = { lbKeyName.Items[lbKeyName.SelectedIndex].ToString() };
                sCurrentKeyBeingEdited = sExtraInfo[0];
                fiGetNewCode = new frmInput(frmInput.FormType.PresetKeyEntry, new Point(this.Left, this.Top + this.Height - 70 - 40), new Size(this.Width, 40), sExtraInfo);
                fiGetNewCode.Show();
                fiGetNewCode.FormClosing += new FormClosingEventHandler(fiGetNewCode_FormClosing);
            }
        }

        /// <summary>
        /// Changes the barcode of the selected key once the input form has closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fiGetNewCode_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sNewBarcode = fiGetNewCode.sGetDataToReturn();
            if (sNewBarcode != "CANCELLED")
            {
                string[] sProductData = tEngine.GetItemDetailsForLookup(sNewBarcode);
                if (sProductData != null)
                {
                    tEngine.EditPresetKeys(sCurrentKeyBeingEdited, sNewBarcode);
                    lbDescription.Items[lbDescription.SelectedIndex] = sProductData[0];
                    lbKeyCode.Items[lbKeyCode.SelectedIndex] = sNewBarcode;
                }
            }
        }

        /// <summary>
        /// Moves all listboxes in the same direction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbKeyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lbSender = (ListBox)sender;
            int nSelectedIndex = lbSender.SelectedIndex;
            lbDescription.SelectedIndex = nSelectedIndex;
            lbKeyCode.SelectedIndex = nSelectedIndex;
            lbKeyName.SelectedIndex = nSelectedIndex;
        }
    }
}
