using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmAccountEdit : ScalableForm
    {
        StockEngine sEngine;

        public frmAccountEdit(ref StockEngine se)
        {
            sEngine = se;

            AddInputControl("ACCCODE", "Account Code : ", new Point(10, 10), 300, "Press F5 to select an account");
            InputTextBox("ACCCODE").MaxCharCount = 6;
            InputTextBox("ACCCODE").KeyDown += new KeyEventHandler(AccCodeKeyDown);
            AddInputControl("ACCNAME", "Account Name : ", new Point(10, BelowLastControl), 400, "");
            InputTextBox("ACCNAME").MaxCharCount = 35;
            AddInputControl("ADDR1", "Address 1 : ", new Point(10, BelowLastControl), 400, "");
            InputTextBox("ADDR1").MaxCharCount = 35;
            AddInputControl("ADDR2", "Address 2 : ", new Point(10, BelowLastControl), 400, "");
            InputTextBox("ADDR2").MaxCharCount = 35;
            AddInputControl("ADDR3", "Address 3 : ", new Point(10, BelowLastControl), 400, "");
            InputTextBox("ADDR3").MaxCharCount = 30;
            AddInputControl("ADDR4", "Address 4 : ", new Point(10, BelowLastControl), 400, "");
            InputTextBox("ADDR4").MaxCharCount = 30;
            InputTextBox("ADDR4").KeyDown += new KeyEventHandler(frmAccountEdit_KeyDown);

            this.WindowState = FormWindowState.Maximized;
            this.Text = "Account Entry / Editing";
        }

        void frmAccountEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string[] sAccInfo = sEngine.GetAccountRecord(InputTextBox("ACCCODE").Text);
                if (sAccInfo.Length <= 1)
                {
                    sAccInfo = new string[9];
                }
                sAccInfo[1] = "B";
                sAccInfo[2] = InputTextBox("ACCNAME").Text;
                sAccInfo[3] = InputTextBox("ADDR1").Text;
                sAccInfo[4] = InputTextBox("ADDR2").Text;
                sAccInfo[5] = InputTextBox("ADDR3").Text;
                sAccInfo[6] = InputTextBox("ADDR4").Text;
                sAccInfo[7] = "";
                sAccInfo[8] = "";
                sAccInfo[9] = "";
                sEngine.AddEditAccountRecord(sAccInfo);
                this.Close();
            }
        
        }

        void AccCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfAccounts floa = new frmListOfAccounts(ref sEngine);
                floa.ShowDialog();
                if (floa.AccountCode != "$NONE")
                {
                    InputTextBox("ACCCODE").Text = floa.AccountCode;
                    LoadAccount();
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                LoadAccount();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void LoadAccount()
        {
            string[] sAccRec = sEngine.GetAccountRecord(InputTextBox("ACCCODE").Text);
            InputTextBox("ACCNAME").Text = sAccRec[2];
            InputTextBox("ADDR1").Text = sAccRec[3];
            InputTextBox("ADDR2").Text = sAccRec[4];
            InputTextBox("ADDR3").Text = sAccRec[5];
            InputTextBox("ADDR4").Text = sAccRec[6];
            InputTextBox("ACCNAME").Focus();
        }


    }
}
