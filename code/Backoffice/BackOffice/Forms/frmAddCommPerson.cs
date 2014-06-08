using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddCommPerson : ScalableForm
    {
        StockEngine sEngine;

        public frmAddCommPerson(ref StockEngine se)
        {
            sEngine = se;

            AddInputControl("COMCODE", "Commissioner's Code :", new Point(10, 10), 300, "Enter a code that will be used to identify this commissioner");
            InputTextBox("COMCODE").MaxCharCount = 8;
            InputTextBox("COMCODE").KeyDown += new KeyEventHandler(ComCodeKeyDown);
            AddInputControl("COMNAME", "Commissioner's Name :", new Point(10, BelowLastControl), 300, "Enter the name of this commissioner");
            InputTextBox("COMNAME").MaxCharCount = 30;
            InputTextBox("COMNAME").KeyDown += new KeyEventHandler(ComNameKeyDown);
            AlignInputTextBoxes();
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(725, 105);
            this.Text = "Add / Edit Commissioner";
        }

        void ComCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                InputTextBox("COMNAME").Text = sEngine.GetCommissionerName(InputTextBox("COMCODE").Text.ToUpper());
            }
        }

        void ComNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sEngine.AddCommissioner(InputTextBox("COMCODE").Text, InputTextBox("COMNAME").Text.ToUpper());
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("COMCODE").Focus();
            }
        }

    }
}
