using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmBatchOptions : ScalableForm
    {
        StockEngine sEngine;
        bool bShopCodeOpen = false;
        bool bItemTypeOpen = false;
        bool bVATOpen = false;

        public frmBatchOptions(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.VisibleChanged += new EventHandler(frmBatchOptions_VisibleChanged);
            this.Size = new Size(1024, 445);
            this.Text = "Batch Add Item Options";
            AddMessage("TOPINST", "Any items after & including the current item will use the following details by default :", new Point(10, 10));

            AddInputControl("SHOPCODE", "Shop Code :", new Point(10, BelowLastControl), 250);
            InputTextBox("SHOPCODE").GotFocus += new EventHandler(ShopCodeGotFocus);
            AddInputControl("BARCODE", "Barcode Start :", new Point(10, BelowLastControl), 250, "If all the products' barcodes start the same (and you're not using barcode scanner). Eg for ET1, ET2... enter ET here");
            InputTextBox("BARCODE").MaxCharCount = 12;
            InputTextBox("BARCODE").KeyDown += new KeyEventHandler(BarcodeKeyDown);
            AddInputControl("DESCRIPTION", "Description Start :", new Point(10, BelowLastControl), 250, "As above, but with the description");
            InputTextBox("DESCRIPTION").MaxCharCount = 30;
            AddInputControl("RRP", "Selling Price :", new Point(10, BelowLastControl), 250, "Enter a price for the items");
            AddInputControl("SUPPLIER", "Supplier Code :", new Point(10, BelowLastControl), 250, "Press F5 for a list of suppliers, F6 for a list of comissioners.");
            InputTextBox("SUPPLIER").MaxCharCount = 6;
            InputTextBox("SUPPLIER").KeyDown += new KeyEventHandler(SupplierKeyDown);
            InputTextBox("SUPPLIER").AutoCompleteMode = AutoCompleteMode.Append;
            InputTextBox("SUPPLIER").AutoCompleteSource = AutoCompleteSource.CustomSource;
            InputTextBox("SUPPLIER").AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());
            AddInputControl("TYPE", "Item Type :", new Point(10, BelowLastControl), 250, "Press F5 to choose from possible item types.");
            InputTextBox("TYPE").KeyDown += new KeyEventHandler(TypeKeyDown);
            InputTextBox("TYPE").GotFocus += new EventHandler(TypeGotFocus);
            AddInputControl("CATEGORY", "Category :", new Point(10, BelowLastControl), 250, "Press F5 to select a category");
            InputTextBox("CATEGORY").KeyDown += new KeyEventHandler(CategoryKeyDown);
            AddInputControl("VAT", "VAT Rate :", new Point(10, BelowLastControl), 250, "Press F5 to select a VAT rate");
            InputTextBox("VAT").GotFocus += new EventHandler(VATGotFocus);
            InputTextBox("VAT").KeyDown += new KeyEventHandler(VATKeyDown);
            AddInputControl("MINQTY", "Minimum Order Quantity :", new Point(10, BelowLastControl), 250, "Enter the minimum that can be ordered. Eg 1MTR, 6PACK etc)");
            InputTextBox("MINQTY").MaxCharCount = 6;
            AddInputControl("SUPCOST", "Supplier Cost :", new Point(10, BelowLastControl), 250, "Enter the cost from the supplier");
            AddInputControl("SUPCODE", "Product Code :", new Point(10, BelowLastControl), 250, "Enter the product code");
            InputTextBox("SUPCODE").KeyDown += new KeyEventHandler(MinQtyKeyDown);

            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.GotFocus += new EventHandler(AllTBGotFocus);
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(AllTBKeyDown);
            }

            this.Text = "Batch Add Options";
        }

        void VATKeyDown(object sender, KeyEventArgs e)
        {
            if (!bVATOpen && e.KeyCode == Keys.F5)
            {
                frmListOfVATRates flovr = new frmListOfVATRates(ref sEngine);
                bVATOpen = true;
                flovr.ShowDialog();
                if (flovr.sSelectedVATCode != "NULL")
                {
                    InputTextBox("VAT").Text = flovr.sSelectedVATCode;
                    InputTextBox("MINQTY").Focus();
                }
                else
                {
                    InputTextBox("CATEGORY").Focus();
                }
                bVATOpen = false;
            }
        }

        void TypeKeyDown(object sender, KeyEventArgs e)
        {
            if (!bItemTypeOpen && e.KeyCode == Keys.F5)
            {
                frmListOfItemTypes flit = new frmListOfItemTypes();
                bItemTypeOpen = true;
                try
                {
                    if (InputTextBox("TYPE").Text.Length != 0)
                        flit.SelectedItemType = Convert.ToInt32(InputTextBox("TYPE").Text);
                }
                catch
                {
                    InputTextBox("TYPE").Text = "";
                }
                flit.ShowDialog();
                if (flit.SelectedItemType == -1)
                {
                    InputTextBox("SUPPLIER").Focus();
                }
                else
                {
                    InputTextBox("TYPE").Text = flit.SelectedItemType.ToString();
                    InputTextBox("CATEGORY").Focus();
                }
                bItemTypeOpen = false;
            }
        }

        void frmBatchOptions_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && InputTextBox("SHOPCODE").Text != "")
                InputTextBox("BARCODE").Focus();

            this.AlignInputTextBoxes();
        }

        void VATGotFocus(object sender, EventArgs e)
        {
        }

        public string SupplierCost
        {
            get
            {
                return FormatMoneyForDisplay(InputTextBox("SUPCOST").Text);
            }
        }
        public string SupplierCode
        {
            get
            {
                return InputTextBox("SUPCODE").Text;
            }
        }
        public string ShopCode
        {
            get
            {
                return InputTextBox("SHOPCODE").Text;
            }
            set
            {
                InputTextBox("SHOPCODE").Text = value;
            }
        }
        public string BarcodeStart
        {
            get
            {
                return InputTextBox("BARCODE").Text;
            }
            set
            {
                InputTextBox("BARCODE").Text = value;
            }
        }
        public string Supplier
        {
            get
            {
                return InputTextBox("SUPPLIER").Text;
            }
            set
            {
                InputTextBox("SUPPLIER").Text = value;
            }
        }
        public string Type
        {
            get
            {
                return InputTextBox("TYPE").Text;
            }
            set
            {
                InputTextBox("TYPE").Text = value;
            }
        }
        public string Category
        {
            get
            {
                return InputTextBox("CATEGORY").Text;
            }
            set
            {
                InputTextBox("CATEGORY").Text = value;
            }
        }
        public string MinimumQuantity
        {
            get
            {
                return InputTextBox("MINQTY").Text;
            }
            set
            {
                InputTextBox("MINQTY").Text = value;
            }
        }
        public string Description
        {
            get
            {
                return InputTextBox("DESCRIPTION").Text;
            }
        }
        public string VATRate
        {
            get
            {
                return InputTextBox("VAT").Text;
            }
            set
            {
                InputTextBox("VAT").Text = value;
            }
        }
        public string RRP
        {
            get
            {
                try
                {
                    return FormatMoneyForDisplay(InputTextBox("RRP").Text);
                }
                catch
                {
                    return "";
                }
            }
        }

        void AllTBKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void MinQtyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.Close();
            }
        }

        void CategoryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
                fcs.ShowDialog();
                if (fcs.SelectedItemCategory != "$NULL")
                {
                    InputTextBox("CATEGORY").Text = fcs.SelectedItemCategory;
                    InputTextBox("VAT").Focus();
                }
            }
        }

        void AllTBGotFocus(object sender, EventArgs e)
        {
            // Move cursor to the end
            ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
        }

        void TypeGotFocus(object sender, EventArgs e)
        {
            /*
            if (!bItemTypeOpen)
            {
                frmListOfItemTypes flit = new frmListOfItemTypes();
                bItemTypeOpen = true;
                try
                {
                    if (InputTextBox("TYPE").Text.Length != 0)
                        flit.SelectedItemType = Convert.ToInt32(InputTextBox("TYPE").Text);
                }
                catch
                {
                    InputTextBox("TYPE").Text = "";
                }
                flit.ShowDialog();
                if (flit.SelectedItemType == -1)
                {
                    InputTextBox("SUPPLIER").Focus();
                }
                else
                {
                    InputTextBox("TYPE").Text = flit.SelectedItemType.ToString();
                    InputTextBox("CATEGORY").Focus();
                }
                bItemTypeOpen = false;
            }*/
        }

        void BarcodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("BARCODE").Text = fsfi.GetItemBarcode();
                    // Move cursor to the end
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }

        void ShopCodeGotFocus(object sender, EventArgs e)
        {
            if (!bShopCodeOpen)
            {
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                bShopCodeOpen = true;
                while (flos.SelectedShopCode == "$NONE")
                    flos.ShowDialog();
                InputTextBox("SHOPCODE").Text = flos.SelectedShopCode;
                InputTextBox("BARCODE").Focus();
                bShopCodeOpen = false;
            }
        }

        void SupplierKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                flos.ShowDialog();
                if (flos.sSelectedSupplierCode != "NULL")
                {
                    InputTextBox("SUPPLIER").MaxCharCount = 6;
                    InputTextBox("SUPPLIER").Text = flos.sSelectedSupplierCode;
                    InputTextBox("TYPE").Focus();
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
                frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                floc.ShowDialog();
                if (floc.Commissioner != "$NONE")
                {
                    InputTextBox("SUPPLIER").MaxCharCount = 8;
                    InputTextBox("SUPPLIER").Text = floc.Commissioner;
                    InputTextBox("TYPE").Focus();
                }
            }
            else if (e.KeyCode == Keys.Enter && InputTextBox("SUPPLIER").Text != "" && sEngine.GetSupplierDetails(InputTextBox("SUPPLIER").Text.ToUpper())[0] == null)
            {
                if (MessageBox.Show("Supplier not found! Would you like to add a supplier?", "Supplier Code", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    frmAddSupplier fas = new frmAddSupplier(ref sEngine);
                    fas.ShowDialog();
                }
                else
                {
                    InputTextBox("SUPPLIER").Focus();
                }
            }
        }
    }
}
