using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmBatchEditOptions : ScalableForm
    {
        StockEngine sEngine;
        bool bShopCodeOpen = false;
        public bool bFinished = false;

        public string ShopCode
        {
            get
            {
                return InputTextBox("SHOPCODE").Text;
            }
        }
        public string CategoryCode
        {
            get
            {
                return InputTextBox("CAT").Text;
            }
        }
        public string SupplierCode
        {
            get
            {
                return InputTextBox("SUPCODE").Text;
            }
        }
        public bool EditDesc
        {
            get
            {
                if (InputTextBox("EDITDESC").Text == "Y")
                    return true;
                else
                    return false;
            }
        }
        public bool EditType
        {
            get
            {
                if (InputTextBox("EDITTYPE").Text == "Y")
                    return true;
                else
                    return false;
            }
        }
        public bool EditCat
        {
            get
            {
                if (InputTextBox("EDITCAT").Text == "Y")
                {
                    return true;
                }
                else
                    return false;
            }
        }
        public bool EditRRP
        {
            get
            {
                if (InputTextBox("EDITRRP").Text.ToUpper() == "Y")
                    return true;
                else
                    return false;
            }
        }
        public bool EditVAT
        {
            get
            {
                if (InputTextBox("EDITVAT").Text.ToUpper() == "Y")
                    return true;
                else
                    return false;
            }

        }
        public bool EditSupCost
        {
            get
            {
                if (InputTextBox("EDITSUPCOST").Text.ToUpper() == "Y")
                    return true;
                else
                    return false;
            }
        }

        public frmBatchEditOptions(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new Size(1024, 400);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            AddInputControl("SHOPCODE", "Shop Code :", new Point(10, 20), 300, "Enter the Shop Code, F5 for a list");
            InputTextBox("SHOPCODE").GotFocus += new EventHandler(ShopCodeGotFoucs);
            AddInputControl("SUPCODE", "Supplier Code :", new Point(10, BelowLastControl), 300, "Press F5 for a list of suppliers");
            InputTextBox("SUPCODE").MaxCharCount = 6;
            InputTextBox("SUPCODE").KeyDown += new KeyEventHandler(SupCodeKeyDown);
            InputTextBox("SUPCODE").AutoCompleteMode = AutoCompleteMode.Append;
            InputTextBox("SUPCODE").AutoCompleteSource = AutoCompleteSource.CustomSource;
            InputTextBox("SUPCODE").AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());
            AddInputControl("CAT", "Category :", new Point(10, BelowLastControl), 300, "Press F5 to select a category");
            InputTextBox("CAT").KeyDown += new KeyEventHandler(CatKeyDown);
            AddInputControl("EDITDESC", "Edit Description :", new Point(10, BelowLastControl), 200, "Edit the items' descriptions? [Y/N]");
            AddInputControl("EDITTYPE", "Edit Item Type :", new Point(10, BelowLastControl), 200, "Edit the items' item types? [Y/N]");
            AddInputControl("EDITCAT", "Edit Item Category :", new Point(10, BelowLastControl), 200, "Edit the items' categories? [Y/N]");
            AddInputControl("EDITRRP", "Edit Item RRP :", new Point(10, BelowLastControl), 200, "Edit the items' prices? [Y/N]");
            AddInputControl("EDITVAT", "Edit Item VAT :", new Point(10, BelowLastControl), 200, "Edit the items' VAT types? [Y/N]");
            AddInputControl("EDITSUPCOST", "Edit Item Supplier Cost :", new Point(10, BelowLastControl), 200, "Edit the items' supplier costs? [Y/N]");
            InputTextBox("EDITSUPCOST").KeyDown += new KeyEventHandler(EditSupCostKeyDown);
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (i >= 3)
                {
                    ibArray[i].tbInput.Text = "Y";
                    ibArray[i].tbInput.GotFocus += new EventHandler(ibArrayGotFocus);
                }
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(tbKeyDown);
            }

            this.Text = "Batch Edit Options";
        }

        void tbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void ShopCodeGotFoucs(object sender, EventArgs e)
        {
            if (!bShopCodeOpen)
            {
                bShopCodeOpen = true;
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                while (flos.SelectedShopCode == "$NONE")
                    flos.ShowDialog();
                InputTextBox("SHOPCODE").Text = flos.SelectedShopCode;
                InputTextBox("SUPCODE").Focus();
            }
        }

        void EditSupCostKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bFinished = true;
                this.Close();
            }
        }

        void ibArrayGotFocus(object sender, EventArgs e)
        {
            ((CTextBox)sender).SelectAll();
        }

        void CatKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fCat = new frmCategorySelect(ref sEngine);
                fCat.ShowDialog();
                if (fCat.SelectedItemCategory != "$NULL")
                {
                    InputTextBox("CAT").Text = fCat.SelectedItemCategory;
                    InputTextBox("EDITDESC").Focus();
                }
            }
        }

        void SupCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                flos.ShowDialog();
                if (flos.sSelectedSupplierCode != "NULL")
                {
                    InputTextBox("SUPCODE").Text = flos.sSelectedSupplierCode;
                    InputTextBox("CAT").Focus();
                }
            }
        }
    }
}
