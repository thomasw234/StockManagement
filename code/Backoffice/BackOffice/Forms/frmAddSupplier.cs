using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddSupplier : ScalableForm
    {
        StockEngine sEngine;
        bool bLoaded = false;
        public string supplierCode = string.Empty;

        public frmAddSupplier(ref StockEngine se)
        {
            sEngine = se;
            AddInputControl("CODE", "Supplier Code : ", new Point(10, 10), 500, "Enter the supplier's code (max 6 characters). Press F5 for a list of suppliers");
            AddInputControl("SUPNAME", "Supplier Name : ", new Point(10, BelowLastControl), 500, "Enter the supplier's name");
            AddInputControl("ADDLINE1", "Address Line 1 : ", new Point(10, BelowLastControl), 500, "Enter supplier's address");
            AddInputControl("ADDLINE2", "Address Line 2 : ", new Point(10, BelowLastControl), 500, "Enter supplier's address");
            AddInputControl("ADDLINE3", "Address Line 3 : ", new Point(10, BelowLastControl), 500, "Enter supplier's address");
            AddInputControl("ADDLINE4", "Address Line 4 : ", new Point(10, BelowLastControl), 500, "Enter supplier's address");
            AddInputControl("POSTCODE", "Post Code : ", new Point(10, BelowLastControl), 500, "Enter supplier's post code");
            AddInputControl("PHONE", "Phone Number : ", new Point(10, BelowLastControl), 500, "Enter supplier's phone number");
            AddInputControl("FAX", "Fax Number : ", new Point(10, BelowLastControl), 500, "Enter supplier's fax number");
            AddInputControl("E-MAIL", "E-Mail address : ", new Point(10, BelowLastControl), 500, "Enter supplier's e-mail address");
            AddInputControl("OURREF", "Our Account Number : ", new Point(10, BelowLastControl), 500, "Enter our account number with the supplier");
            AddInputControl("CONTACT", "Contact : ", new Point(10, BelowLastControl), 500, "Enter the name of a contact");
            AddInputControl("LYEAR", "Contact's Number : ", new Point(10, BelowLastControl), 500, "Enter the amount purchased last year from the supplier");
            AddInputControl("CURRPUR", "Current Purchases : ", new Point(10, BelowLastControl), 500, "Enter the amount purchased this year from the supplier");
            AddAutoFill("LYEAR", "0.00");
            AddAutoFill("CURRPUR", "0.00");

            InputTextBox("CODE").KeyDown += new KeyEventHandler(frmAddSupplierCode_KeyDown);
            InputTextBox("CURRPUR").KeyDown += new KeyEventHandler(frmAddSupplierCurr_KeyDown);

            /*
            tbSupCode[tbSupCode.Length - 1].AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbSupCode[tbSupCode.Length - 1].AutoCompleteMode = AutoCompleteMode.Append;
            tbSupCode[tbSupCode.Length - 1].AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());*/

            InputTextBox("CODE").AutoCompleteSource = AutoCompleteSource.CustomSource;
            InputTextBox("CODE").AutoCompleteMode = AutoCompleteMode.Append;
            InputTextBox("CODE").AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());

            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
            }

            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(1000, 500);
            AlignInputTextBoxes();
            this.Text = "Add / Edit Suppliers";
        }

        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Check for changes made
                string[] sSaveSuppDetails = sEngine.GetSupplierDetails(ibArray[0].tbInput.Text);
                bool bChanges = false;
                if (sSaveSuppDetails[0] != null)
                {
                    for (int i = 0; i < ibArray.Length; i++)
                    {
                        if (ibArray[i].tbInput.Text.ToUpper() != sSaveSuppDetails[i])
                            bChanges = true;
                    }
                }
                if (!bChanges && bLoaded)
                {
                    this.Close();
                }
                else
                {
                    switch (MessageBox.Show("Do you want to save the changes to the supplier's details?", "Save Details?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            SaveSupplierDetails();
                            this.Close();
                            break;
                        case DialogResult.No:
                            this.Close();
                            break;
                        case DialogResult.Cancel:
                            break;
                    }
                }
            }
        }

        void frmAddSupplierCurr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Check for changes made
                string[] sSaveSuppDetails = sEngine.GetSupplierDetails(ibArray[0].tbInput.Text);
                bool bChanges = false;
                if (sSaveSuppDetails[0] != null)
                {
                    for (int i = 0; i < ibArray.Length; i++)
                    {
                        if (ibArray[i].tbInput.Text.ToUpper() != sSaveSuppDetails[i])
                            bChanges = true;
                    }
                }
                else
                    bChanges = true;
                if (!bChanges)
                {
                    this.Close();
                }
                else
                {
                    switch (MessageBox.Show("Do you want to save the changes to the supplier's details?", "Save Details?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            SaveSupplierDetails();
                            this.Close();
                            break;
                        case DialogResult.No:
                            this.Close();
                            break;
                        case DialogResult.Cancel:
                            break;
                    }
                }
            }
        }

        void frmAddSupplierCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string[] sData = sEngine.GetSupplierDetails(InputTextBox("CODE").Text);
                if (sData[0] != null)
                {
                    for (int i = 0; i < sData.Length - 2; i++)
                    {
                        ibArray[i].tbInput.Text = sData[i];
                    }
                }
                InputTextBox("SUPNAME").SelectionStart = InputTextBox("SUPNAME").Text.Length;
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmListOfSuppliers fls = new frmListOfSuppliers(ref sEngine);
                fls.ShowDialog();
                if (fls.sSelectedSupplierCode != "NULL")
                {
                    InputTextBox("CODE").Text = fls.sSelectedSupplierCode;
                    SendKeys.Send("{ENTER}");
                    bLoaded = true;
                }
            }
        }

        void SaveSupplierDetails()
        {
            string[] sToSave = new string[ibArray.Length + 2];
            for (int i = 0; i < ibArray.Length; i++)
            {
                sToSave[i] = ibArray[i].tbInput.Text;
            }
            sToSave[ibArray.Length] = "0.00"; // LYPurchases
            sToSave[ibArray.Length + 1] = "NO";
            sEngine.AddSupplier(sToSave);
            this.supplierCode = sToSave[0];
        }
    }
}
