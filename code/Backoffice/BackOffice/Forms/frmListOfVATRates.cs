using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmListOfVATRates : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbCode;
        CListBox lbName;
        public string sSelectedVATCode = "NULL";
        string[] sListOfVATCodes;

        public frmListOfVATRates(ref StockEngine se)
        {
            sEngine = se;
            AddMessage("CODE", "VAT Name", new Point(10, 10));
            AddMessage("NAME", "VAT Rate", new Point(160, 10));
            this.AllowScaling = false;
            this.Size = new Size(260, 100);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Select A VAT Rate";
            this.SurroundListBoxes = true;

            lbCode = new CListBox();
            lbCode.Location = new Point(10, BelowLastControl);
            lbCode.Size = new Size(150, lbCode.ItemHeight);
            lbCode.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbCode);

            lbName = new CListBox();
            lbName.Location = new Point(160, lbCode.Top);
            lbName.BorderStyle = BorderStyle.None;
            lbName.Size = new Size(this.ClientSize.Width - 10 - 150 - 10, lbName.ItemHeight);
            lbName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Controls.Add(lbName);

            string[,] sVATCodes = sEngine.VATRates;
            sListOfVATCodes = new string[sEngine.NumberOfVATRates];
            for (int i = 0; i < sEngine.NumberOfVATRates; i++)
            {
                lbCode.Items.Add(sVATCodes[i,1]);
                lbName.Items.Add(FormatMoneyForDisplay(Convert.ToDecimal(sVATCodes[i,2])) + "%");
                lbCode.Height += lbCode.ItemHeight;
                lbName.Height += lbName.ItemHeight;
                this.Height += lbName.ItemHeight;
                sListOfVATCodes[i] = sVATCodes[i, 0];
            }

            lbName.Focus();
            lbName.SelectedIndexChanged += new EventHandler(lbName_SelectedIndexChanged);
            lbName.KeyDown += new KeyEventHandler(lbName_KeyDown);
            lbCode.KeyDown +=new KeyEventHandler(lbName_KeyDown);
            lbCode.SelectedIndexChanged += new EventHandler(lbCode_SelectedIndexChanged);

            if (lbName.Items.Count >= 1)
                lbName.SelectedIndex = 0;
        }

        void lbCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbName.SelectedIndex = lbCode.SelectedIndex;
        }

        void lbName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sSelectedVATCode = sListOfVATCodes[lbCode.SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void lbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbCode.SelectedIndex = lbName.SelectedIndex;
        }
    }
}
