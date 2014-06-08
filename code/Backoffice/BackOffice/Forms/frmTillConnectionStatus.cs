using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;

namespace BackOffice
{
    class frmTillConnectionStatus : ScalableForm
    {
        StockEngine sEngine;
        Timer tmr;

        public frmTillConnectionStatus(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(1024, 200);
            this.KeyDown += new KeyEventHandler(frmTillConnectionStatus_KeyDown);

            tmr = new Timer();
            tmr.Tick += new EventHandler(tmr_Tick);
            tmr.Enabled = true;
            tmr.Interval = 1000;
        }

        void frmTillConnectionStatus_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                this.Close();
            }
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            int[] nCodes = new int[0];
            bool[] bCollectionStatus = sEngine.TillsConnected(ref nCodes);
            for (int i = 0; i < nCodes.Length; i++)
            {
                RemoveMessage("TILL_" + nCodes[i].ToString());
            }
            int nTop = 10;
            for (int i = 0; i < bCollectionStatus.Length; i++)
            {
                AddMessage("TILL_" + nCodes[i].ToString(), "Till " + nCodes[i].ToString() + " : ", new Point(10, nTop));
                if (bCollectionStatus[i])
                {
                    MessageLabel("TILL_" + nCodes[i].ToString()).Text += "Connected";
                }
                else
                {
                    MessageLabel("TILL_" + nCodes[i].ToString()).Text += "Not Found";
                }
            
                nTop += 20;
            }

        }
    }
}
