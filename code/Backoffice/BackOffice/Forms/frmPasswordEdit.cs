using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmPasswordEdit : ScalableForm
    {
        StockEngine sEngine;

        public frmPasswordEdit(ref StockEngine se)
        {
            sEngine = se;
            AddInputControl("LVL1", "Till's Control Menu Password : ", new System.Drawing.Point(10, 10), 300, "Control Menu Password");
            InputTextBox("LVL1").MaxCharCount = 6;
            InputTextBox("LVL1").KeyDown += new KeyEventHandler(LVL1KEYDOWN);
            InputTextBox("LVL1").Text = sEngine.GetPasswords(1);
            AddInputControl("LVL2", "Till's Administrator Password :", new System.Drawing.Point(10, BelowLastControl), 300, "Admin Menu Password");
            InputTextBox("LVL2").MaxCharCount = 6;
            InputTextBox("LVL2").KeyDown += new KeyEventHandler(frmPasswordEdit_KeyDown);
            InputTextBox("LVL2").Text = sEngine.GetPasswords(2);
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new System.Drawing.Size(500, 150);

            this.Text = "Password Edit";
        }

        void LVL1KEYDOWN(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void frmPasswordEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Save();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("LVL1").Focus();
            }
        }

        void Save()
        {
            sEngine.SetPassword(1, InputTextBox("LVL1").Text);
            sEngine.SetPassword(2, InputTextBox("LVL2").Text);
            sEngine.GenerateDetailsForAllTills();
            sEngine.CopyWaitingFilesToTills();
        }
    }
}
