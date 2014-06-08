using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmAddEditShop : ScalableForm
    {
        StockEngine sEngine;
        string sShopCode = "";

        public frmAddEditShop(ref StockEngine se)
        {
            sEngine = se;
            this.Text = "Shop Settings";

            AddInputControl("SHOP_CODE", "Shop's 2 Digit Code : ", new Point(10, 10), 200, "Press F5 to choose an existing shop");
            AddInputControl("SHOP_NAME", "Shop Name : ", new Point(10, BelowLastControl), 500);
            AddInputControl("ADDRESS1", "Address Line 1 : ", new Point(10, BelowLastControl), 500, "Property Number and Street Name");
            AddInputControl("ADDRESS2", "Address Line 2 : ", new Point(10, BelowLastControl), 500, "Town / City");
            AddInputControl("ADDRESS3", "Address Line 3 : ", new Point(10, BelowLastControl), 500, "Post Code");
            AddInputControl("ADDRESS4", "Address Line 4 : ", new Point(10, BelowLastControl), 500, "County");
            AddInputControl("TILL_COUNT", "Number of tills : ", new Point(10, BelowLastControl), 200, "Press F5 to add a new till, F6 edit an existing till, or F7 to remove a till. Do not alter the number of tills here");
            InputTextBox("TILL_COUNT").KeyDown += new KeyEventHandler(TillCount_KeyDown);
            InputTextBox("SHOP_CODE").KeyDown += new KeyEventHandler(ShopCodeKeyDown);
            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown +=new KeyEventHandler(AllTextBox_KeyDown);
            }
        }


        void AllTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                AskAndClose();
            }
        } 

        void ShopCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfShops fListOfshops = new frmListOfShops(ref sEngine);
                fListOfshops.ShowDialog();
                if (fListOfshops.SelectedShopCode != "$NONE")
                {
                    sShopCode = fListOfshops.SelectedShopCode;
                    InputTextBox("SHOP_CODE").Text = sShopCode;
                    LoadSettings();
                    InputTextBox("SHOP_NAME").Focus();
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (InputTextBox("SHOP_CODE").Text != "")
                {
                    sShopCode = InputTextBox("SHOP_CODE").Text;
                    LoadSettings();
                    InputTextBox("SHOP_NAME").Focus();
                }
                else
                {
                    this.Close();
                }
            }
        }

        void TillCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                // Add Till
                frmAddTill fat = new frmAddTill(ref sEngine, sShopCode);
                fat.ShowDialog();
                fat.Dispose();
            }
            else if (e.KeyCode == Keys.F6)
            {
                // Edit Till
                frmListOfTills ft = new frmListOfTills(ref sEngine, sShopCode);
                ft.ShowDialog();
                string sCode = ft.sSelectedTillCode;
                ft.Dispose();
                if (sCode != "NULL")
                {
                    frmAddTill fat = new frmAddTill(ref sEngine, sShopCode);
                    fat.ShowTillDetails(sCode);
                    fat.ShowDialog();
                    fat.Dispose();
                }
            }
            else if (e.KeyCode == Keys.F7)
            {
                // Remove Till
                frmListOfTills ft = new frmListOfTills(ref sEngine, sShopCode);
                ft.ShowDialog();
                string sCode = ft.sSelectedTillCode;
                ft.Dispose();
                if (sCode != "NULL")
                {
                    if (MessageBox.Show("Are you sure that you want to remove the till with code " + sCode + "?", "Remove TilL", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        {
                            // Remove till
                            sEngine.RemoveTill(sCode);
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                AskAndClose();
            }
            InputTextBox("TILL_COUNT").Text = sEngine.NumberOfTills(sShopCode).ToString();
        }

        void AskAndClose()
        {
                switch (MessageBox.Show("Would you like to save any changes made? (Any changes to till settings will have already been saved).", "Save Changes?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        SaveSettings();
                        this.Close();
                        break;
                    case DialogResult.No:
                        this.Close();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
        }


        void LoadSettings()
        {
            try
            {
                InputTextBox("SHOP_NAME").Text = sEngine.GetShopNameFromCode(sShopCode);
                string[] sAddress = sEngine.GetShopAddress(sShopCode);
                for (int i = 0; i < sAddress.Length; i++)
                {
                    InputTextBox("ADDRESS" + (i + 1).ToString()).Text = sAddress[i];
                }
                InputTextBox("TILL_COUNT").Text = sEngine.NumberOfTills(sShopCode).ToString();
            }
            catch
            {
                ;
            }
        }

        void SaveSettings()
        {
            string[] sAddress = { InputTextBox("ADDRESS1").Text, InputTextBox("ADDRESS2").Text, InputTextBox("ADDRESS3").Text, InputTextBox("ADDRESS4").Text };
            sEngine.AddShop(sShopCode, InputTextBox("SHOP_NAME").Text, sAddress);
        }
    }
}
