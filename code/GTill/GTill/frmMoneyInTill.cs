using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TillEngine;

namespace GTill
{
    class frmMoneyInTill : Form
    {
        /// <summary>
        /// The background colour of the form
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The foreground (text) colour of the form
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// An array of labels (Cash/Credit Card Slips/Cheques etc)
        /// </summary>
        Label[] lblDescription;
        /// <summary>
        /// Matches up with the description array, but shows the amount of each type
        /// </summary>
        Label[] lblAmounts;
        /// <summary>
        /// The label showing the total amount in the till
        /// </summary>
        Label lblTotalAmount;
        /// <summary>
        /// The label that says "Total Amount"
        /// </summary>
        Label lblTotalDesc;
        /// <summary>
        /// A rectangle that boxes in the controls
        /// </summary>
        Rectangle rOutside;
        /// <summary>
        /// The title label
        /// </summary>
        Label lblTitle;
        /// <summary>
        /// The instruction label, telling the user to press any key to exit
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// A reference to the TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;
        /// <summary>
        /// The font name to use on this form
        /// </summary>
        string sFontName;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="te">A reference to the TillEngine</param>
        /// <param name="pLoc">The location on screen to place the form</param>
        /// <param name="s">The size of the form</param>
        public frmMoneyInTill(ref TillEngine.TillEngine te, Point pLoc, Size s)
        {
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;

            this.BackColor = cFrmBackColour;
            this.ForeColor = cFrmForeColour;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = pLoc;
            this.Size = s;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Paint += new PaintEventHandler(frmMoneyInTill_Paint);
            this.KeyDown += new KeyEventHandler(frmMoneyInTill_KeyDown);
            sFontName = Properties.Settings.Default.sFontName;
            tEngine = te;
            SetupFormLayout();
            DisplayMoneyInTill();
        }

        /// <summary>
        /// Keydown handler (closes the form)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMoneyInTill_KeyDown(object sender, KeyEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Boxes in the form's controls when the form is painted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMoneyInTill_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), rOutside);
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(rOutside.Left - 6, rOutside.Top - 6, rOutside.Width + 12, rOutside.Height + 12));
            if (lblTotalAmount != null)
            {
                e.Graphics.DrawLine(new Pen(cFrmForeColour, 1.5f), lblAmounts[0].Left, lblTotalAmount.Top - 4, lblAmounts[0].Left + lblAmounts[0].Width, lblTotalAmount.Top - 4);
                e.Graphics.DrawLine(new Pen(cFrmForeColour, 1.5f), lblAmounts[0].Left, lblTotalAmount.Top - 7, lblAmounts[0].Left + lblAmounts[0].Width, lblTotalAmount.Top - 7);
                e.Graphics.DrawLine(new Pen(cFrmForeColour, 1.5f), lblAmounts[0].Left, lblTotalAmount.Top + lblTotalAmount.Height + 4, lblAmounts[0].Left + lblAmounts[0].Width, lblTotalAmount.Top + lblTotalAmount.Height + 4);
                e.Graphics.DrawLine(new Pen(cFrmForeColour, 1.5f), lblAmounts[0].Left, lblTotalAmount.Top + lblTotalAmount.Height + 7, lblAmounts[0].Left + lblAmounts[0].Width, lblTotalAmount.Top + lblTotalAmount.Height + 7);
            }
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), 10, this.Height - 70, this.Width - 20, 50);
        }

        /// <summary>
        /// Sets up all the controls on the form and places them
        /// </summary>
        void SetupFormLayout()
        {
            lblTitle = new Label();
            lblTitle.BackColor = cFrmBackColour;
            lblTitle.ForeColor = cFrmForeColour;
            lblTitle.Font = new Font(sFontName, 14.0f);
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.Width, 25);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Text = "Money In Till";
            this.Controls.Add(lblTitle);

            lblInstruction = new Label();
            lblInstruction.Text = "Press Any Key To Continue...";
            lblInstruction.BackColor = cFrmBackColour;
            lblInstruction.ForeColor = cFrmForeColour;
            lblInstruction.Font = new Font(sFontName, 14.0f);
            lblInstruction.AutoSize = false;
            lblInstruction.Width = this.Width - 40;
            lblInstruction.Height = 30;
            lblInstruction.Top = this.Height - 60;
            lblInstruction.Left = 20;
            this.Controls.Add(lblInstruction);
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;

            lblDescription = new Label[5];
            for (int i = 0; i < lblDescription.Length; i++)
            {
                lblDescription[i] = new Label();
                lblDescription[i].Font = new Font(sFontName, 25.0f);
                lblDescription[i].BackColor = cFrmBackColour;
                lblDescription[i].ForeColor = cFrmForeColour;
                lblDescription[i].AutoSize = true;
                this.Controls.Add(lblDescription[i]);
            }
            lblDescription[0].Text = "Cash";
            lblDescription[1].Text = "Credit Card Slips";
            lblDescription[2].Text = "Cheques";
            lblDescription[3].Text = "Vouchers";
            lblDescription[4].Text = "Customers";
            int nMaxDescWidth = 0;
            int nMaxAmountWidth = 0;
            int nTotalHeight = 0;
            int nSingleLabelHeight = 0;
            foreach (Label lbl in lblDescription)
            {
                if (lbl.Width > nMaxDescWidth)
                    nMaxDescWidth = lbl.Width;
                nTotalHeight += lbl.Height;
            }
            lblAmounts = new Label[lblDescription.Length];
            for (int i = 0; i < lblAmounts.Length; i++)
            {
                lblAmounts[i] = new Label();
                lblAmounts[i].Font = new Font(sFontName, 25.0f);
                lblAmounts[i].BackColor = cFrmBackColour;
                lblAmounts[i].ForeColor = cFrmForeColour;
                lblAmounts[i].AutoSize = true;
                lblAmounts[i].TextAlign = ContentAlignment.MiddleRight;
                this.Controls.Add(lblAmounts[i]);
                lblAmounts[i].Text = "00000.00";
                nMaxAmountWidth = lblAmounts[i].Width;
                nSingleLabelHeight = lblAmounts[i].Height;
            }
            for (int i = 0; i < lblAmounts.Length; i++)
            {
                lblAmounts[i].AutoSize = false;
                lblAmounts[i].Width = nMaxAmountWidth;
                lblAmounts[i].Height = nSingleLabelHeight;
            }
            lblTotalAmount = new Label();
            lblTotalAmount.AutoSize = false;
            lblTotalAmount.Width = lblAmounts[0].Width;
            lblTotalAmount.Height = nSingleLabelHeight;
            lblTotalAmount.BackColor = cFrmBackColour;
            lblTotalAmount.ForeColor = cFrmForeColour;
            lblTotalAmount.Font = new Font(sFontName, 25.0f);
            lblTotalAmount.Text = "00000.00";
            lblTotalAmount.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblTotalAmount);
            lblTotalDesc = new Label();
            lblTotalDesc.Font = new Font(sFontName, 25.0f);
            lblTotalDesc.BackColor = cFrmBackColour;
            lblTotalDesc.ForeColor = cFrmForeColour;
            lblTotalDesc.Width = nMaxDescWidth;
            lblTotalDesc.Height = nSingleLabelHeight;
            lblTotalDesc.Text = "Total";
            this.Controls.Add(lblTotalDesc);
            nTotalHeight += lblTotalAmount.Height;
            nTotalHeight += 50;
            int nTotalWidth = nMaxAmountWidth + nMaxDescWidth;
            for (int i = 0; i < lblDescription.Length; i++)
            {
                lblDescription[i].Left = (this.Width / 2) - (nTotalWidth / 2);
                lblDescription[i].Top = (this.Height / 2) - (nTotalHeight / 2) + (i * lblDescription[i].Height);
                lblAmounts[i].Left = (this.Width / 2) + (nTotalWidth / 2) - (lblAmounts[i].Width);
                lblAmounts[i].Top = lblDescription[i].Top;
            }
            lblTotalAmount.Top = (this.Height / 2) + ((nTotalHeight -25) / 2) - lblTotalAmount.Height;
            lblTotalAmount.Left = (this.Width / 2) + (nTotalWidth / 2) - lblTotalAmount.Width;
            rOutside = new Rectangle(lblDescription[0].Left - 5, lblDescription[0].Top - 5, nTotalWidth + 10, nTotalHeight + 10);
            lblTotalDesc.Left = lblDescription[0].Left;
            lblTotalDesc.Top = lblTotalAmount.Top;
        }

        /// <summary>
        /// Gets the amount of money in the till and shows it on the form
        /// </summary>
        void DisplayMoneyInTill()
        {
            float[] fMoneyInTill = tEngine.GetAmountOfMoneyInTill();
            float fTotal = 0.0f;
            for (int i = 0; i < fMoneyInTill.Length; i++)
            {
                lblAmounts[i].Text = TillEngine.TillEngine.FormatMoneyForDisplay(fMoneyInTill[i]);
                fTotal += fMoneyInTill[i];
            }
            fTotal = tEngine.fFixFloatError(fTotal);
            lblTotalAmount.Text = TillEngine.TillEngine.FormatMoneyForDisplay(fTotal);
            lblAmounts[4].Text = tEngine.GetListOfTransactionNumbers().Length.ToString();
        }
    }
}
