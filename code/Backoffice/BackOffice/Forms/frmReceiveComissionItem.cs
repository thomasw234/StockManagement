using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmReceiveComissionItem : ScalableForm
    {
        StockEngine sEngine;
        bool bShopCodeOpen = false;
        bool bReceiving = false;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="se">The stockengine to use</param>
        /// <param name="bReceiving">True if receiving, false if returning</param>
        public frmReceiveComissionItem(ref StockEngine se, bool bReceiving)
        {
            this.bReceiving = bReceiving;
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(750, 140);
            if (bReceiving)
            {
                AddInputControl("SHOPCODE", "Shop Code : ", new Point(10, 10), 300, "Enter the code of the shop to receive the item to");
                InputTextBox("SHOPCODE").GotFocus += new EventHandler(ShopCodeGotFocus);
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the comission item to receive. Press F5 to search.");
                InputTextBox("BARCODE").KeyDown += new KeyEventHandler(BarcodeKeyDown);
                AddInputControl("QTY", "Quantity Receiving : ", new Point(10, BelowLastControl), 300, "Enter the quantity of this item to receive");
                InputTextBox("QTY").KeyDown += new KeyEventHandler(QtyKeyDown);

                this.Text = "Receive A Commission Item";
            }
            else
            {
                AddInputControl("SHOPCODE", "Shop Code : ", new Point(10, 10), 300, "Enter the code of the shop that the item is being returned from");
                InputTextBox("SHOPCODE").GotFocus += new EventHandler(ShopCodeGotFocus);
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the comission item to return. Press F5 to search");
                InputTextBox("BARCODE").KeyDown += new KeyEventHandler(BarcodeKeyDown);
                AddInputControl("QTY", "Quantity Returning : ", new Point(10, BelowLastControl), 300, "Enter the quantity of this item to return");
                InputTextBox("QTY").KeyDown += new KeyEventHandler(QtyKeyDown);

                this.Text = "Return A Commission Item";
            }
            AlignInputTextBoxes();
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
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void ShopCodeGotFocus(object sender, EventArgs e)
        {
            if (!bShopCodeOpen)
            {
                bShopCodeOpen = true;
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                while (flos.SelectedShopCode == "$NONE")
                {
                    flos.ShowDialog();
                }
                InputTextBox("SHOPCODE").Text = flos.SelectedShopCode;
                InputTextBox("BARCODE").Focus();
            }
        }

        void QtyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (bReceiving)
                {
                    if (sEngine.ReceiveComissionItem(InputTextBox("BARCODE").Text, InputTextBox("QTY").Text, InputTextBox("SHOPCODE").Text))
                    {
                        if (MessageBox.Show("Would you like to upload the changes to stock levels to all tills?", "Upload", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            sEngine.CopyWaitingFilesToTills();
                        }
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add! Are you sure this is a comission item?");
                        InputTextBox("BARCODE").Focus();
                    }
                }
                else
                {
                    decimal dQtyToReturn = 0;
                    try
                    {
                        dQtyToReturn = Convert.ToDecimal(InputTextBox("QTY").Text);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid Quantity Entered");
                        return;
                    }
                    decimal dQtyReturned = sEngine.ReturnCommissionItems(InputTextBox("BARCODE").Text, Convert.ToDecimal(InputTextBox("QTY").Text), InputTextBox("SHOPCODE").Text);
                    if (dQtyReturned != dQtyToReturn)
                    {
                        if (dQtyReturned == 1)
                        {
                            MessageBox.Show("Only 1 was returned, as there was only 1 in stock");
                        }
                        else
                        {
                            MessageBox.Show("Only " + dQtyReturned.ToString() + " were returned, as there was only " + dQtyReturned.ToString() + " in stock");
                        }
                    }
                    else
                    {
                        MessageBox.Show("All returned ok!");
                    }

                }

            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("BARCODE").Focus();
            }
        }
    }
}
