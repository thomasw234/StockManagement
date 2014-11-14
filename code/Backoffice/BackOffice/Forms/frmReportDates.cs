using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;
using System.IO;

namespace BackOffice
{
    class frmReportDates : ScalableForm
    {
        CListBox lbDates;
        public string SelectedFolder = "$NONE";
        string[] sDirs;

        public frmReportDates(Period pPeriod)
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(200, 500);
            AddMessage("HELP", "Date of period end:", new Point(10, 10));
            lbDates = new CListBox();
            lbDates.Location = new Point(10, BelowLastControl);
            lbDates.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 10 - lbDates.Top);
            lbDates.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lbDates);
            lbDates.KeyDown += new KeyEventHandler(lbDates_KeyDown);

            string sPeriod = "";
            switch (pPeriod)
            {
                case Period.Daily:
                    sPeriod = "Daily";
                    break;
                case Period.Monthly:
                    sPeriod = "Monthly";
                    break;
                case Period.Weekly:
                    sPeriod = "Weekly";
                    break;
                case Period.Yearly:
                    sPeriod = "Yearly";
                    break;
            }

            string sDir = "Archive\\" + sPeriod;
            if (Directory.Exists(sDir))
            {
                sDirs = Directory.GetDirectories(sDir);
            }
            else
                sDirs = new string[0];

            for (int i = 0; i < sDirs.Length; i++)
            {
                string sDate = sDirs[i].Split('\\')[sDirs[i].Split('\\').Length - 1];
                string sDayOfMonth = sDate[8].ToString() + sDate[9].ToString();
                string sMonth = sDate[5].ToString() + sDate[6].ToString();
                string sYear = sDate[0].ToString() + sDate[1].ToString() + sDate[2].ToString() + sDate[3].ToString();
                lbDates.Items.Add(sDayOfMonth + "/" + sMonth + "/" + sYear);
            }

            if (lbDates.Items.Count > 0)
            {
                lbDates.SelectedIndex = lbDates.Items.Count - 1;
            }
        }

        void lbDates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
            else if (e.KeyCode == Keys.Enter)
            {
                SelectedFolder = sDirs[lbDates.SelectedIndex];
                this.Close();
            }
        }

    }
}
