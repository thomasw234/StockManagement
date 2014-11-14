using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;
using System.Drawing.Printing;

namespace BackOffice
{
    class frmStockLevelReportConfig : ScalableForm
    {
        StockEngine sEngine;
        bool bUsingCatGroups = false;
        public string SkipSettings = "N";

        public frmStockLevelReportConfig(ref StockEngine se)
        {
            sEngine = se;
            SetupForm();
            this.AllowScaling = false;
            this.Size = new Size(750, 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.VisibleChanged += frmStockLevelReportConfig_VisibleChanged;
        }

        void frmStockLevelReportConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (SkipSettings != null && SkipSettings != "N")
            {
                if (SkipSettings == "")
                    SkipSettings = "S";
                this.CreateReport(ReportOrderedBy.DescAlphabetical, true, SkipSettings);
                SkipSettings = "N";
            }
        }


        void SetupForm()
        {
            AddInputControl("CAT", "Enter the category to view stock levels of:", new Point(10, BelowLastControl + 5), 350, "Press F5 to select a category, or F6 to select a category group.");
            AddInputControl("SORP", "<S>creen or <P>rinter", new Point(10, BelowLastControl), 350);
            InputTextBox("SORP").Text = "S";
            InputTextBox("SORP").KeyDown += new KeyEventHandler(frmStockLevelReportConfigSORP_KeyDown);
            InputTextBox("CAT").KeyDown += new KeyEventHandler(frmStockLevelReportConfig_KeyDown);
            InputTextBox("SORP").GotFocus += frmStockLevelReportConfig_GotFocus;
            AlignInputTextBoxes();
            this.Text = "Stock Level Report Settings";
        }

        void frmStockLevelReportConfig_GotFocus(object sender, EventArgs e)
        {
            InputTextBox("SORP").SelectAll();
        }

        private void CreateReport(ReportOrderedBy sOrder, bool includeZeroItems, string output)
        {
            if (!bUsingCatGroups)
            {
                string[] sCats = { InputTextBox("CAT").Text };
                if (string.Equals(output, "P", StringComparison.OrdinalIgnoreCase))
                {
                    sEngine.StockReportToPrinter(sCats, sOrder, "", includeZeroItems);
                }
                else if (string.Equals(output, "S", StringComparison.OrdinalIgnoreCase))
                {
                    sEngine.StockReportToFile(sCats, sOrder, "", includeZeroItems);
                    frmReportViewer fViewer = new frmReportViewer(ReportType.StockLevelReport);
                    fViewer.ShowDialog();
                }
            }
            else
            {
                string[] sCats = sEngine.GetListOfCatGroupCategories(InputTextBox("CAT").Text);
                if (string.Equals(output, "P", StringComparison.OrdinalIgnoreCase))
                {
                    sEngine.StockReportToPrinter(sCats, sOrder, InputTextBox("CAT").Text, includeZeroItems);
                }
                else if (string.Equals(output, "S", StringComparison.OrdinalIgnoreCase))
                {
                    sEngine.StockReportToFile(sCats, sOrder, InputTextBox("CAT").Text, includeZeroItems);
                    frmReportViewer fViewer = new frmReportViewer(ReportType.StockLevelReport);
                    fViewer.ShowDialog();
                }
            }
            this.Close();
        }

        void frmStockLevelReportConfigSORP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReportOrderedBy sOrder;
                frmStockReportOrder fOrder = new frmStockReportOrder();
                fOrder.ShowDialog();
                if (fOrder.OptionSelected)
                {
                    sOrder = fOrder.SelectedOrder;
                    bool bIncludeZeroItems = true;
                    if ((MessageBox.Show("Include items with a stock level of zero in all shops?", "Include Zero Items?", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    {
                        bIncludeZeroItems = true;
                    }
                    else
                    {
                        bIncludeZeroItems = false;
                    }
                    this.CreateReport(sOrder, bIncludeZeroItems, InputTextBox("SORP").Text);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("CAT").Focus();
            }
            else
            {
                InputTextBox("SORP").Text = "";
            }
        }

        void frmStockLevelReportConfig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcat = new frmCategorySelect(ref sEngine);
                fcat.ShowDialog();
                if (fcat.SelectedItemCategory != "$NULL")
                {
                    bUsingCatGroups = false;
                    InputTextBox("CAT").Text = fcat.SelectedItemCategory;
                    InputTextBox("SORP").Focus();
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
                frmListOfCategoryGroups flcat = new frmListOfCategoryGroups(ref sEngine);
                flcat.ShowDialog();
                if (flcat.SelectedCategoryGroup != "$NONE")
                {
                    bUsingCatGroups = true;
                    InputTextBox("CAT").Text = flcat.SelectedCategoryGroup;
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
