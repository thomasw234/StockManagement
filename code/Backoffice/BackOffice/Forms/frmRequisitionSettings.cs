using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmRequisitionSettings : ScalableForm
    { 
        StockEngine sEngine;
        public bool bOK = false;
        public decimal dAveSalesMin = 0;
        public decimal dNumberOfDays = 0;
        public string sCategory = "";

        public frmRequisitionSettings(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(739, 180);
            this.AllowScaling = false;
            AddMessage("INST", "Requisition Order Settings", new Point(10, 15));
            AddInputControl("DAYS", "Select items with less than how many days' stock?", new Point(10, BelowLastControl), 450);
            AddInputControl("AVESALES", "Ignore items with an average daily sales figure less than :", new Point(10, BelowLastControl), 450);
            AddInputControl("CAT", "Item Category : ", new Point(10, BelowLastControl), 450, "Items from other suppliers will be shown too.");
            InputTextBox("DAYS").KeyDown += new KeyEventHandler(DaysKeyDown);
            InputTextBox("AVESALES").KeyDown += new KeyEventHandler(AveSalesKeyDown);
            InputTextBox("CAT").KeyDown += new KeyEventHandler(CatKeyDown);
            InputTextBox("DAYS").Text = "7";
            InputTextBox("AVESALES").Text = "0.006";
            InputTextBox("AVESALES").GotFocus += new EventHandler(AveSalesGotFocus);
            InputTextBox("DAYS").SelectAll();
            InputTextBox("DAYS").GotFocus += new EventHandler(frmRequisitionSettings_GotFocus);
            this.VisibleChanged += new EventHandler(frmRequisitionSettings_VisibleChanged);
            AlignInputTextBoxes();
            this.Text = "Requisition Settings";
        }

        void DaysKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                bOK = false;
                this.Close();
            }
        }

        void AveSalesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("DAYS").Focus();
            }
        }

        void CatKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
                fcs.ShowDialog();
                if (fcs.SelectedItemCategory != "$NULL")
                {
                    InputTextBox("CAT").Text = fcs.SelectedItemCategory;
                }
                InputTextBox("CAT").SelectionStart = InputTextBox("CAT").Text.Length;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    dAveSalesMin = Convert.ToDecimal(InputTextBox("AVESALES").Text);
                    dNumberOfDays = Convert.ToDecimal(InputTextBox("DAYS").Text);
                    sCategory = InputTextBox("CAT").Text;
                    bOK = true;
                }
                catch
                {
                    bOK = false;
                }
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("AVESALES").Focus();
            }
        }

        void frmRequisitionSettings_GotFocus(object sender, EventArgs e)
        {
            InputTextBox("DAYS").SelectAll();
        }

        void frmRequisitionSettings_VisibleChanged(object sender, EventArgs e)
        {
            InputTextBox("DAYS").SelectAll();
        }

        void AveSalesGotFocus(object sender, EventArgs e)
        {
            InputTextBox("AVESALES").SelectAll();
        }
    }
}
