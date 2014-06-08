using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmVATRateEdit : ScalableForm
    {
        StockEngine sEngine;
        TextBox[] tbCodes;
        TextBox[] tbNames;
        TextBox[] tbRates;

        public frmVATRateEdit(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new System.Drawing.Size(400, 70);
            SetupForm();
            
        }

        public void SetupForm()
        {
            AddMessage("VAT_CODE", "VAT Code", new System.Drawing.Point(10, 10));
            AddMessage("VAT_NAME", "VAT Name", new System.Drawing.Point(75, 10));
            AddMessage("VAT_RATE", "VAT Rate", new System.Drawing.Point(275, 10));
            tbCodes = new TextBox[sEngine.NumberOfVATRates];
            tbNames = new TextBox[sEngine.NumberOfVATRates];
            tbRates = new TextBox[sEngine.NumberOfVATRates];
            for (int i = 0; i < sEngine.NumberOfVATRates; i++)
            {
                tbCodes[i] = new TextBox();
                tbCodes[i].Location = new System.Drawing.Point(10, BelowLastControl);
                tbCodes[i].Width = 50;
                tbCodes[i].KeyDown += new KeyEventHandler(tbCodes_KeyDown);
                this.Controls.Add(tbCodes[i]);
                tbCodes[i].Tag = i.ToString();

                tbNames[i] = new TextBox();
                tbNames[i].Location = new System.Drawing.Point(10 + tbCodes[i].Width + 10, tbCodes[i].Top);
                tbNames[i].Width = 180;
                tbNames[i].KeyDown += new KeyEventHandler(tbNames_KeyDown);
                this.Controls.Add(tbNames[i]);
                tbNames[i].Tag = i.ToString();

                tbRates[i] = new TextBox();
                tbRates[i].Location = new System.Drawing.Point(tbNames[i].Left + tbNames[i].Width + 10, tbCodes[i].Top);
                tbRates[i].Width = this.ClientSize.Width - 10 - 180 - 50;
                tbRates[i].KeyDown += new KeyEventHandler(tbRates_KeyDown);
                this.Controls.Add(tbRates[i]);
                tbRates[i].Tag = i.ToString();
            }

            this.Height += (tbRates[0].Height + 10) * sEngine.NumberOfVATRates;

            string[,] sVATRates = sEngine.VATRates;
            for (int x = 0; x < sEngine.NumberOfVATRates && x < tbCodes.Length; x++)
            {
                tbCodes[x].Text = sVATRates[x, 0];
                tbNames[x].Text = sVATRates[x, 1];
                tbRates[x].Text = sVATRates[x, 2];
            }
            this.Text = "VAT Rate Edit";
        }

        void tbRates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Convert.ToInt32(((TextBox)sender).Tag) + 1 == sEngine.NumberOfVATRates)
                    Cancel();
                else
                    tbCodes[Convert.ToInt32(((TextBox)sender).Tag) + 1].Focus();
            }
            else if (e.KeyCode == Keys.Escape)
                Cancel();
            else if (e.KeyCode == Keys.Insert)
                AddVATCode();
        }

        void AddVATCode()
        {
            frmSingleInputBox fsiGetCode = new frmSingleInputBox("Enter the 2 character code to use :", ref sEngine);
            fsiGetCode.ShowDialog();
            if (fsiGetCode.Response != "$NONE")
            {
                frmSingleInputBox fsiGetName = new frmSingleInputBox("Enter a description of the new rate :", ref sEngine);
                fsiGetName.ShowDialog();
                if (fsiGetName.Response != "$NONE")
                {
                    frmSingleInputBox fsiGetRate = new frmSingleInputBox("Enter the new rate (%):", ref sEngine);
                    fsiGetRate.ShowDialog();
                    if (fsiGetRate.Response != "$NONE")
                    {
                        bool bOk = false;
                        try
                        {
                            Convert.ToDecimal(fsiGetRate.Response);
                            bOk = true;
                        }
                        catch
                        {
                            ;
                        }
                        if (bOk)
                        {
                            Array.Resize<TextBox>(ref tbCodes, tbCodes.Length + 1);
                            Array.Resize<TextBox>(ref tbNames, tbNames.Length + 1);
                            Array.Resize<TextBox>(ref tbRates, tbRates.Length + 1);

                            tbCodes[tbCodes.Length - 1] = new TextBox();
                            tbNames[tbNames.Length - 1] = new TextBox();
                            tbRates[tbRates.Length - 1] = new TextBox();

                            this.Controls.Add(tbCodes[tbRates.Length - 1]);
                            this.Controls.Add(tbNames[tbRates.Length - 1]);
                            this.Controls.Add(tbRates[tbRates.Length - 1]);

                            tbCodes[tbCodes.Length - 1].Width = 50;
                            tbNames[tbNames.Length - 1].Width = 200;
                            tbRates[tbRates.Length - 1].Width = 100;

                            tbCodes[tbCodes.Length - 1].Location = new System.Drawing.Point(10, tbCodes[tbCodes.Length - 2].Top + tbCodes[tbCodes.Length - 2].Height + 10);
                            tbNames[tbNames.Length - 1].Location = new System.Drawing.Point(tbCodes[tbCodes.Length - 1].Width + tbCodes[tbCodes.Length - 1].Left + 10, tbCodes[tbCodes.Length - 1].Top);
                            tbRates[tbRates.Length - 1].Location = new System.Drawing.Point(tbNames[tbNames.Length - 1].Left + tbNames[tbNames.Length - 1].Width + 10, tbNames[tbNames.Length - 1].Top);

                            this.Height += tbCodes[0].Height;

                            tbCodes[tbCodes.Length - 1].Text = fsiGetCode.Response;
                            tbNames[tbNames.Length - 1].Text = fsiGetName.Response;
                            tbRates[tbRates.Length - 1].Text = fsiGetRate.Response;
                        }
                    }
                }
            }
        }

        void DeleteVATCode(string sCode)
        {
            if (MessageBox.Show("To delete a VAT code, you must have another code that items belonging to the old code can use. Also, any other changes will first be saved. Continue?", "VAT Code Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Save();
                frmSingleInputBox fsiGetNewCode = new frmSingleInputBox("Enter the code of the VAT rate replacing " + sCode, ref sEngine);
                fsiGetNewCode.ShowDialog();
                if (fsiGetNewCode.Response != "$NONE")
                {
                    if (sCode != fsiGetNewCode.Response)
                    {
                        bool bFound = false;
                        for (int i = 0; i < tbCodes.Length; i++)
                        {
                            if (fsiGetNewCode.Response.ToUpper() == tbCodes[i].Text.ToUpper())
                            {
                                bFound = true;
                                break;
                            }
                        }
                        if (bFound)
                        {
                            sEngine.ChangeItemsVATCodes(sCode, fsiGetNewCode.Response);
                            int nCodeLoc = -1;
                            for (int i = 0; i < tbCodes.Length; i++)
                            {
                                if (tbCodes[i].Text.ToUpper() == sCode)
                                {
                                    nCodeLoc = i;
                                    break;
                                }
                            }
                            if (nCodeLoc != -1)
                            {
                                for (int i = nCodeLoc; i < tbCodes.Length - 1; i++)
                                {
                                    tbCodes[i].Text = tbCodes[i + 1].Text;
                                    tbNames[i].Text = tbNames[i + 1].Text;
                                    tbRates[i].Text = tbRates[i+1].Text;
                                }

                                this.Controls.Remove(tbCodes[tbCodes.Length - 1]);
                                this.Controls.Remove(tbNames[tbNames.Length - 1]);
                                this.Controls.Remove(tbRates[tbRates.Length - 1]);
                                
                                Array.Resize<TextBox>(ref tbCodes, tbCodes.Length - 1);
                                Array.Resize<TextBox>(ref tbNames, tbNames.Length - 1);
                                Array.Resize<TextBox>(ref tbRates, tbRates.Length - 1);
                                this.Height -= tbCodes[0].Height;
                            }
                        }
                    }
                }
            }
        }

        void tbNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tbRates[Convert.ToInt32(((TextBox)sender).Tag)].Focus();
            else if (e.KeyCode == Keys.Escape)
                Cancel();
            else if (e.KeyCode == Keys.Insert)
                AddVATCode();
        }

        void tbCodes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tbNames[Convert.ToInt32(((TextBox)sender).Tag)].Focus();
            else if (e.KeyCode == Keys.Escape)
                Cancel();
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                DeleteVATCode(((TextBox)sender).Text);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Insert)
                AddVATCode();
        }

        void Save()
        {
            string[] sCodes = new string[tbCodes.Length];
            string[] sNames = new string[tbNames.Length];
            decimal[] dRates = new decimal[tbRates.Length];
            for (int i = 0; i < tbCodes.Length; i++)
            {
                sCodes[i] = tbCodes[i].Text;
                sNames[i] = tbNames[i].Text;
                dRates[i] = Convert.ToDecimal(tbRates[i].Text);
            }
            sEngine.UpdateVATRates(sCodes, sNames, dRates);
            if (MessageBox.Show("Would you like to upload any changes to all tills?", "Upload", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                sEngine.CopyWaitingFilesToTills();
            }
        }


        void Cancel()
        {
            if (MessageBox.Show("Do you want to save any changes?", "Save Changes", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                this.Close();
            }
            else
            {
                Save();
                this.Close();
            }
        }

    }
}
