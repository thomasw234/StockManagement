using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddTill : ScalableForm
    {
        StockEngine sEngine;
        string sShopCode;

        public frmAddTill(ref StockEngine se, string ShopCode)
        {
            sEngine = se;
            sShopCode = ShopCode;
            this.AllowScaling = false;
            this.Size = new Size(700, 275);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Enter or Edit Till Details";

            AddInputControl("TILL_NUM", "Till Number : ", new Point(10, 10), 300, "Enter the till's number");
            AddInputControl("TILL_NAME", "Till Name : ", new Point(10, BelowLastControl), 300, "Enter the till's name");
            AddInputControl("RLINE_1", "Receipt Line 1 : ", new Point(10, BelowLastControl), 300, "Enter the first receipt message line");
            AddInputControl("RLINE_2", "Receipt Line 2 : ", new Point(10, BelowLastControl), 300, "Enter the second receipt message line");
            AddInputControl("RLINE_3", "Receipt Line 3 : ", new Point(10, BelowLastControl), 300, "Enter the third receipt message line");
            AddInputControl("COLLECTION", "Collection Map : ", new Point(10, BelowLastControl), 300, "YYYYYNN means collect Monday - Friday, YYYYYYN means Monday - Saturday");
            AddInputControl("TILL_LOC", "Till's Location : ", new Point(10, BelowLastControl), 300, "Enter the path of the INGNG, OUTGNG and TILL folders file. Press F5 for file dialog");
            InputTextBox("TILL_LOC").KeyDown +=new KeyEventHandler(frmAddTillLoc_KeyDown);
            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
            }
        }

        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                SaveQuestion();
            }
        }

        void SaveQuestion()
        {
            switch (MessageBox.Show("Would you like to save this till information?", "Save changes?", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    string[] sToAdd = { InputTextBox("RLINE_1").Text, InputTextBox("RLINE_2").Text, InputTextBox("RLINE_3").Text };
                    sEngine.AddTill(InputTextBox("TILL_NUM").Text, InputTextBox("TILL_NAME").Text, sToAdd, InputTextBox("TILL_LOC").Text, InputTextBox("COLLECTION").Text, sShopCode);
                    if (MessageBox.Show("Would you like to upload changes to the till now?", "Upload Now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        sEngine.CopyWaitingFilesToTills();
                    }
                    this.Close();
                    break;
                case DialogResult.No:
                    this.Close();
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }

        void frmAddTillLoc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                FolderBrowserDialog f = new FolderBrowserDialog();
                f.ShowDialog();
                InputTextBox("TILL_LOC").Text = f.SelectedPath;
                InputTextBox("TILL_LOC").SelectionStart = InputTextBox("TILL_LOC").Text.Length;
            } 
            else if (e.KeyCode == Keys.Enter)
            {
                SaveQuestion();                
            }
        }

        public void ShowTillDetails(string sTillCode)
        {
            string[] sTillDetails = sEngine.GetTillData(sTillCode);
            InputTextBox("TILL_NUM").Text = sTillDetails[0];
            InputTextBox("TILL_NUM").SelectionStart = InputTextBox("TILL_NUM").Text.Length;
            InputTextBox("TILL_NAME").Text = sTillDetails[1];
            InputTextBox("RLINE_1").Text = sTillDetails[3];
            InputTextBox("RLINE_2").Text = sTillDetails[4];
            InputTextBox("RLINE_3").Text = sTillDetails[5];
            InputTextBox("TILL_LOC").Text = sTillDetails[2];
            InputTextBox("COLLECTION").Text = sTillDetails[8];
        }

    }
}
