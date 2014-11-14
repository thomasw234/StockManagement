using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmOutstandingItemsSetup : ScalableForm
    {
        StockEngine sEngine;
        public string SkipCode = "N";

        public frmOutstandingItemsSetup(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Text = "Outstanding Items Report Setup";
            AddInputControl("SUPCODE", "Supplier Code : ", new Point(10, 10), 300, "F5 for a list of suppliers");
            InputTextBox("SUPCODE").KeyDown += new KeyEventHandler(frmOutstandingItemsSetup_KeyDown);
            InputTextBox("SUPCODE").AutoCompleteMode = AutoCompleteMode.Append;
            InputTextBox("SUPCODE").AutoCompleteSource = AutoCompleteSource.CustomSource;
            InputTextBox("SUPCODE").AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());
            AddInputControl("SORP", "<S>creen or <P>rinter", new Point(10, BelowLastControl), 300, "Choose the output");
            InputTextBox("SORP").Text = "S";
            InputTextBox("SORP").KeyDown += new KeyEventHandler(SorpKeyDown);
            InputTextBox("SORP").GotFocus += frmOutstandingItemsSetup_GotFocus;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(480, 110);
            this.VisibleChanged += frmOutstandingItemsSetup_VisibleChanged;
        }

        void frmOutstandingItemsSetup_VisibleChanged(object sender, EventArgs e)
        {
            if (this.SkipCode != null && this.SkipCode.Length >= 2 && !this.SkipCode.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                string printerOptions = SkipCode[SkipCode.Length - 1].ToString();
                string supcode = SkipCode.Substring(0, SkipCode.Length - 1).ToUpper();
                if (printerOptions.Equals("P", StringComparison.OrdinalIgnoreCase))
                    sEngine.OutStandingItemsToPrinter(supcode);
                else if (printerOptions.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    sEngine.OutStandingItemsToFile(supcode);
                    frmReportViewer frv = new frmReportViewer(ReportType.OutStandingItems);
                    frv.ShowDialog();
                }

                this.SkipCode = "N";
                this.Close();
            }
        }

        void frmOutstandingItemsSetup_GotFocus(object sender, EventArgs e)
        {
            InputTextBox("SORP").SelectAll();
        }

        void SorpKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("SUPCODE").Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (InputTextBox("SORP").Text == "P")
                {
                    sEngine.OutStandingItemsToPrinter(InputTextBox("SUPCODE").Text.ToUpper());
                }
                else
                {
                    sEngine.OutStandingItemsToFile(InputTextBox("SUPCODE").Text.ToUpper());
                    frmReportViewer frv = new frmReportViewer(ReportType.OutStandingItems);
                    frv.ShowDialog();
                }
                this.Close();
            }
        }

        void frmOutstandingItemsSetup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                flos.ShowDialog();
                if (flos.sSelectedSupplierCode != "NULL")
                {
                    InputTextBox("SUPCODE").Text = flos.sSelectedSupplierCode;
                    InputTextBox("SORP").Focus();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
