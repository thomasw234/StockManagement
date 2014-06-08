using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmNewBatchEditOptions : ScalableForm
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
        

        public frmNewBatchEditOptions(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new Size(550, 150);
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
            
            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(tbKeyDown);
            }

            AlignInputTextBoxes();

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
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (InputTextBox("CAT").Text == "" && InputTextBox("SUPCODE").Text == "")
                {
                    if (MessageBox.Show("This will load every single item in the database. Continue?", "Ahem", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        bFinished = true;
                        this.Close();
                    }
                }

                else
                {
                    bFinished = true;
                    this.Close();
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
