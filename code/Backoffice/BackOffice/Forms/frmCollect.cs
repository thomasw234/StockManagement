using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmCollect : ScalableForm
    {
        StockEngine sEngine;
        Button btnOK;

        public frmCollect(ref StockEngine se)
        {
            sEngine = se;
            SetupForm();
            this.AllowScaling = false;
            this.Size = new System.Drawing.Size(590, 80);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Collect Till Data";
            btnOK = new Button();
            this.Controls.Add(btnOK);
            btnOK.Text = "OK";
            btnOK.Location = new System.Drawing.Point(500,10);
            btnOK.AutoSize = false;
            btnOK.Size = new System.Drawing.Size(this.ClientSize.Width - 10 - btnOK.Left, InputTextBox("GETDATE").Height);
            btnOK.Click += new EventHandler(btnOK_Click);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            ContinueCollect();
        }

        private void SetupForm()
        {
            AddInputControl("GETDATE", "Enter the sales date to collect information for (DDMMYY) :", new System.Drawing.Point(10, 10), 480);
            InputTextBox("GETDATE").KeyDown += new KeyEventHandler(frmCollect_KeyDown);
            InputTextBox("GETDATE").Text = sEngine.GetDDMMYYDate();
        }

        void ContinueCollect()
        {
            bool bUpdateDailySales = true;
            this.Hide();
            /*if (MessageBox.Show("Would you like to overwrite the daily sales information?", "Zeroing Daily Sales", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                bUpdateDailySales = false;
            }*/
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                && MessageBox.Show("It's Sunday, should I skip collection? (If you're unsure, choose yes)", "Sunday", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                if (MessageBox.Show("Would you like to shut down all tills?", "Shut Down?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    sEngine.SendCommandToTill("ShutDown");
                }
            }
            else if (sEngine.CollectDataFromTills(InputTextBox("GETDATE").Text, bUpdateDailySales))
            {
                if (MessageBox.Show("Collection Completed OK! Would you like to shutdown all tills?", "Shut Down?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.SendCommandToTill("ShutDown");
                }
            }
            else
            {
                if (MessageBox.Show("Collection finished with errors, please see the console for more information. Shut Down all tills anyway?", "Shut Down", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.SendCommandToTill("ShutDown");
                }
            }
            this.Close();
        }

        void frmCollect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ContinueCollect();
                
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
