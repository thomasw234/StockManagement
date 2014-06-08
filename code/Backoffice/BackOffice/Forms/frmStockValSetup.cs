using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmStockValSetup : ScalableForm
    {
        StockEngine sEngine;
        StockEngine sOldEngine;
        public string SkipSettings = "N";

        public frmStockValSetup(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(850, 70);
            this.Text = "Stock Valuation Setup";

            AddInputControl("DATE", "Date of Stock Valuation : ", new Point(10, 10), 500, "Press F5 for a list of dates, leave blank for as of today.");
            InputTextBox("DATE").KeyDown += new KeyEventHandler(frmStockValSetup_KeyDown);

            this.VisibleChanged += frmStockValSetup_VisibleChanged;
        }

        void frmStockValSetup_VisibleChanged(object sender, EventArgs e)
        {
            if (SkipSettings != null && SkipSettings != "N" && SkipSettings.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                sEngine.StockValuationToPrinter();
                this.SkipSettings = "N";
                this.Close();
            }
        }

        void frmStockValSetup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmReportDates frd = new frmReportDates(StockEngine.Period.Monthly);
                frd.ShowDialog();
                if (frd.SelectedFolder != "$NONE")
                {
                    InputTextBox("DATE").Text = frd.SelectedFolder;
                    SendKeys.Send("{ENTER}");
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (InputTextBox("DATE").Text != "")
                {
                    sOldEngine = new StockEngine(InputTextBox("DATE").Text);
                    sOldEngine.StockValuationToPrinter();
                }
                else
                {
                    sEngine.StockValuationToPrinter();
                }
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
