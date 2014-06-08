using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmSettings : ScalableForm
    {
        StockEngine sEngine;
        
        public frmSettings(ref StockEngine se)
        {
            sEngine = se;
            this.Text = "Settings";
            AddInputControl("COMPANY_NAME", "Company Name : ", new Point(10, 10), 500);
            AddInputControl("SHOP_COUNT", "Number Of Shops : ", new Point(10, BelowLastControl), 200, "Press F5 to Add/Edit Shops");
            AddInputControl("VAT_RATE_COUNT", "Number of VAT Rates : ", new Point(10, BelowLastControl), 200, "Press F5 to edit VAT rates");
            AddInputControl("CREDIT_CARD_COUNT", "Number of card types : ", new Point(10, BelowLastControl), 200, "Press F5 to edit the possible credit / debit cards");
            AddInputControl("VAT_NUMBER", "Company VAT Number : ", new Point(10, BelowLastControl), 400);
            AddInputControl("EMAIL", "E-Mail Support Address : ", new Point(10, BelowLastControl), 400);
            InputTextBox("SHOP_COUNT").KeyDown += new KeyEventHandler(ShopKeyDown);
            InputTextBox("VAT_RATE_COUNT").KeyDown += new KeyEventHandler(tbInput_KeyDown);
            InputTextBox("CREDIT_CARD_COUNT").KeyDown += new KeyEventHandler(tbCardKeyDown);
            InputTextBox("VAT_NUMBER").KeyDown += new KeyEventHandler(VATKeyDown);
            InputTextBox("EMAIL").KeyDown += new KeyEventHandler(Email_KeyDown);
            LoadExistingSettings();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown +=new KeyEventHandler(tbInputAll_KeyDown);
            }
        }

        void Email_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CloseAndQuit();
            }
        }

        void VATKeyDown(object sender, KeyEventArgs e)
        {
        }

        void ShopKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmAddEditShop faes = new frmAddEditShop(ref sEngine);
                faes.ShowDialog();
            }
        }

        void CloseAndQuit()
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

        void tbInputAll_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseAndQuit();
            }
        }

        


        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            // V.A.T Rate Entry
            if (e.KeyCode == Keys.F5)
            {
                int nToShow = Convert.ToInt32(InputTextBox("VAT_RATE_COUNT").Text);
                frmVATRateEdit fv = new frmVATRateEdit(ref sEngine);
                fv.ShowDialog();
            }
        }

        void tbCardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                int nOfCards = Convert.ToInt32(InputTextBox("CREDIT_CARD_COUNT").Text);
                frmCreditCardEdit fc = new frmCreditCardEdit(ref sEngine);
                fc.ShowDialog();
                InputTextBox("CREDIT_CARD_COUNT").Text = sEngine.NumberOfCards.ToString();
            }
        }

        void LoadExistingSettings()
        {
            InputTextBox("COMPANY_NAME").Text = sEngine.CompanyName;
            InputTextBox("COMPANY_NAME").SelectionStart = ibArray[0].tbInput.Text.Length;
            InputTextBox("VAT_NUMBER").Text = sEngine.VATNumber;
            InputTextBox("VAT_RATE_COUNT").Text = sEngine.NumberOfVATRates.ToString();
            InputTextBox("CREDIT_CARD_COUNT").Text = sEngine.NumberOfCards.ToString();
            InputTextBox("SHOP_COUNT").Text = sEngine.NumberOfShops.ToString();
            InputTextBox("EMAIL").Text = sEngine.GetEmailSupportAddress();
        }

        void SaveSettings()
        {
            sEngine.CompanyName = InputTextBox("COMPANY_NAME").Text;
            sEngine.VATNumber = InputTextBox("VAT_NUMBER").Text;
            sEngine.SetEmailSupportAddress(InputTextBox("EMAIL").Text);
        }
    }
}
