using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmGetBarcode : ScalableForm
    {
        StockEngine sEngine;
        public string Barcode;
        public string LastCategory = "";
        public string shortcutString { get; set; }

        public frmGetBarcode(ref StockEngine se)
        {
            sEngine = se;
            SetupForm();
            this.FormClosing += new FormClosingEventHandler(frmGetBarcode_FormClosing);
            this.VisibleChanged += frmGetBarcode_VisibleChanged;
        }

        void frmGetBarcode_VisibleChanged(object sender, EventArgs e)
        {
            if (shortcutString != null)
            {
                InputTextBox("GETCODE").Text = shortcutString;
                SendKeys.Send("~");
            }
        }

        void frmGetBarcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Fixes a bug where if a dud barcode was in the text box and the cross was clicked then the dud barcode was used, causing a crash
            if (sEngine.GetMainStockInfo(InputTextBox("GETCODE").Text).Length == 1)
            {
                Barcode = "";
                InputTextBox("GETCODE").Text = "";
            }
        }

        void SetupForm()
        {
            this.AllowScaling = false;
            AddInputControl("GETCODE", "Enter Item Code :", new Point(10, 10), 275);
            InputTextBox("GETCODE").Click += new EventHandler(frmGetBarcode_Click);
            AddMessage("F5", "F5 = Select from categories", new Point(10, 40));
            MessageLabel("F5").Click += new EventHandler(F5Click);
            AddMessage("F6", "F6 = Use previous category", new Point(10, 60));
            MessageLabel("F6").Click += new EventHandler(F6Click);
            AddMessage("F7", "F7 = Search by description", new Point(10, 80));
            MessageLabel("F7").Click += new EventHandler(F7Click);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(315, 145);
            InputTextBox("GETCODE").KeyDown += new KeyEventHandler(frmGetBarcode_KeyDown);
        }

        void frmGetBarcode_Click(object sender, EventArgs e)
        {
            F5Click(sender, e);
        }

        void F7Click(object sender, EventArgs e)
        {
            frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
            fsfi.ShowDialog();
            if (fsfi.GetItemBarcode() != "NONE_SELECTED")
            {
                InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                Barcode = fsfi.GetItemBarcode();
                this.Close();
            }
        }

        void F6Click(object sender, EventArgs e)
        {
            // Show last category
            LastCategory = sEngine.LastCategoryCode;
            if (LastCategory != "")
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.CheckForPartialBarcodeFromScanner("CAT:" + LastCategory);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                    Barcode = fsfi.GetItemBarcode();
                    fsfi.Dispose();
                    this.Close();
                }
                else
                {
                    fsfi.Dispose();
                }
            }
        }

        void F5Click(object sender, EventArgs e)
        {
            frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
            fcs.ShowDialog();
            string sSelectedCategory = fcs.SelectedItemCategory;
            if (sSelectedCategory != "$NULL")
            {
                LastCategory = sSelectedCategory;
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.CheckForPartialBarcodeFromScanner("CAT:" + sSelectedCategory);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                    Barcode = fsfi.GetItemBarcode();
                    fsfi.Dispose();
                    this.Close();
                }
                else
                {
                    fsfi.Dispose();
                }
            }
        }

        void frmGetBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Barcode = InputTextBox("GETCODE").Text;
                if (sEngine.GetMainStockInfo(Barcode)[0] == null && InputTextBox("GETCODE").Text != "")
                {
                    frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                    fsfi.CheckForPartialBarcodeFromScanner(Barcode, true);
                    if (!fsfi.FoundScannerResults)
                    {
                        if (MessageBox.Show("This item doesn't exist! Would you like to create it?", "Not found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Barcode = "";
                            frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                            faei.AddingBarcode = InputTextBox("GETCODE").Text;
                            faei.ShowDialog();
                            faei.Dispose();
                            InputTextBox("GETCODE").Focus();
                        }
                        else
                        {
                            InputTextBox("GETCODE").Text = "";
                            InputTextBox("GETCODE").Focus();
                            Barcode = "";
                        }
                    }
                    else
                    {
                        fsfi.Hide();
                        fsfi.ShowDialog();
                        if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                        {
                            InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                            InputTextBox("GETCODE").Focus();
                            Barcode = fsfi.GetItemBarcode();
                            this.Close();
                        }
                        else
                        {
                            InputTextBox("GETCODE").Focus();
                        }
                    }

                }
                else
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                // Show categories
                frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
                fcs.ShowDialog();
                string sSelectedCategory = fcs.SelectedItemCategory;
                if (sSelectedCategory != "$NULL")
                {
                    LastCategory = sSelectedCategory;
                    frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                    fsfi.CheckForPartialBarcodeFromScanner("CAT:" + sSelectedCategory);
                    fsfi.ShowDialog();
                    if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                    {
                        InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                        Barcode = fsfi.GetItemBarcode();
                        fsfi.Dispose();
                        this.Close();
                    }
                    else
                    {
                        fsfi.Dispose();
                    }
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
                // Show last category
                LastCategory = sEngine.LastCategoryCode;
                if (LastCategory != "")
                {
                    frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                    fsfi.CheckForPartialBarcodeFromScanner("CAT:" + LastCategory);
                    fsfi.ShowDialog();
                    if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                    {
                        InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                        Barcode = fsfi.GetItemBarcode();
                        fsfi.Dispose();
                        this.Close();
                    }
                    else
                    {
                        fsfi.Dispose();
                    }
                }
            }
            else if (e.KeyCode == Keys.F7)
            {
                // Description search
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("GETCODE").Text = fsfi.GetItemBarcode();
                    Barcode = fsfi.GetItemBarcode();
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("GETCODE").Text = "";
                Barcode = "";
                this.Close();
            }
        }
    }
}
