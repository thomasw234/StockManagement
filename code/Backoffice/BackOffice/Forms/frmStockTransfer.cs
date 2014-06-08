using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmStockTransfer : ScalableForm
    {
        StockEngine sEngine;
        bool bShowingShops = false;

        public frmStockTransfer(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(1024, 300);
            AddInputControl("FROM", "Shop Code From :", new Point(10, 20), 200, "The code of the shop that the item is coming from");
            AddInputControl("TO", "Shop Code To :", new Point(10, BelowLastControl), 200, "The code of the shop that the item is going to");
            AddInputControl("BARCODE", "Barcode :", new Point(10, BelowLastControl), 200, "The item's barcode");
            AddInputControl("QTY", "Quantity :", new Point(10, BelowLastControl), 200, "The quantity to transfer");
            InputTextBox("QTY").KeyDown += new KeyEventHandler(QtyKeyDown);
            InputTextBox("BARCODE").KeyDown += new KeyEventHandler(BarcodeKeyDown);
            InputTextBox("FROM").GotFocus += new EventHandler(FromGotFocus);
            InputTextBox("TO").KeyDown += new KeyEventHandler(ToKeyDown);
            this.Text = "Stock Transfer";
        }

        void QtyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                frmSingleInputBox fGetPassword = new frmSingleInputBox("Please enter the administrator password :", ref sEngine);
                fGetPassword.tbResponse.PasswordChar = ' ';
                fGetPassword.ShowDialog();
                if (fGetPassword.Response == "$NONE" || fGetPassword.Response.ToUpper() != sEngine.GetPasswords(2).ToUpper())
                {
                    MessageBox.Show("Incorrect password");
                    this.Close();
                }
                else
                {
                    sEngine.TransferStockItem(InputTextBox("FROM").Text, InputTextBox("TO").Text, InputTextBox("BARCODE").Text, Convert.ToDecimal(InputTextBox("QTY").Text), false);
                    if (MessageBox.Show("Would you like to upload all changes to tills?", "Upload?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        sEngine.CopyWaitingFilesToTills();
                    }
                    this.Close();
                }
            }
        }

        void ToKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                flos.ShowDialog();
                if (flos.SelectedShopCode != "$NONE")
                {
                    InputTextBox("TO").Text = flos.SelectedShopCode;
                    InputTextBox("BARCODE").Focus();
                }
            }
        }

        void FromGotFocus(object sender, EventArgs e)
        {
            if (!bShowingShops)
            {
                bShowingShops = true;
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                while (flos.SelectedShopCode == "$NONE")
                    flos.ShowDialog();
                InputTextBox("FROM").Text = flos.SelectedShopCode;
                InputTextBox("TO").Focus();
            }
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
                    InputTextBox("QTY").Focus();
                }
            }
        }
    }
}
