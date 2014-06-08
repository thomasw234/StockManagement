using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmCustEmails : ScalableForm
    {
        StockEngine sEngine;

        ListBox lbTitle;
        ListBox lbForeName;
        ListBox lbSurName;
        ListBox lbEmail;
        ListBox lbDate;

        public frmCustEmails(ref StockEngine se)
        {
            sEngine = se;
            sEngine.CollectEmailsFromTills();
            this.AllowScaling = false;
            AddMessage("TITLE", "Title", new Point(10, 10));
            AddMessage("FORENAME", "Forename", new Point(50, 10));
            AddMessage("SURNAME", "Surname", new Point(200, 10));
            AddMessage("EMAIL", "E-Mail", new Point(350, 10));
            AddMessage("DATE", "Date Added", new Point(600, 10));

            lbTitle = new ListBox();
            lbTitle.Location = new Point(10, BelowLastControl);
            lbTitle.Size = new Size(40, 300);
            lbTitle.BorderStyle = BorderStyle.None;
            lbTitle.KeyDown += new KeyEventHandler(lbKeyDown);
            lbTitle.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbTitle);

            lbForeName = new ListBox();
            lbForeName.Location = new Point(50, lbTitle.Top);
            lbForeName.Size = new Size(150, 300);
            lbForeName.BorderStyle = BorderStyle.None;
            lbForeName.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbForeName.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbForeName);

            lbSurName = new ListBox();
            lbSurName.Location = new Point(200, lbTitle.Top);
            lbSurName.Size = new Size(150, 300);
            lbSurName.BorderStyle = BorderStyle.None;
            lbSurName.KeyDown += new KeyEventHandler(lbKeyDown);
            lbSurName.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbSurName);

            lbEmail = new ListBox();
            lbEmail.Location = new Point(350, lbTitle.Top);
            lbEmail.Size = new Size(250, 300);
            lbEmail.BorderStyle = BorderStyle.None;
            lbEmail.KeyDown += new KeyEventHandler(lbKeyDown);
            lbEmail.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbEmail);

            lbDate = new ListBox();
            lbDate.Location = new Point(600, lbTitle.Top);
            lbDate.Size = new Size(150, 300);
            lbDate.BorderStyle = BorderStyle.None;
            lbDate.KeyDown += new KeyEventHandler(lbKeyDown);
            lbDate.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbDate);

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(768, 400);

            string[] sTitles = new string[0];
            string[] sForeNames = new string[0];
            string[] sSurNames = new string[0];
            string[] sEmails = new string[0];
            string[] sDates = new string[0];
            sEngine.GetEmailAddresses(ref sTitles, ref sForeNames, ref sSurNames, ref sEmails, ref sDates);

            for (int i = 0; i < sTitles.Length; i++)
            {
                lbTitle.Items.Add(sTitles[i]);
                lbForeName.Items.Add(sForeNames[i]);
                lbSurName.Items.Add(sSurNames[i]);
                lbEmail.Items.Add(sEmails[i]);
                lbDate.Items.Add(sDates[i]);
            }

            if (sTitles.Length > 0)
                lbTitle.SelectedIndex = 0;

            this.Text = "Customer E-Mail addresses";
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbTitle.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbForeName.SelectedIndex =((ListBox)sender).SelectedIndex;
            lbSurName.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbEmail.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDate.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
