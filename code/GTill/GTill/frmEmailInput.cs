using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TillEngine;

namespace GTill
{
    class frmEmailInput : Form
    {
        TillEngine.TillEngine tEngine;
        TextBox tbTitle;
        TextBox tbForeName;
        TextBox tbSurname;
        TextBox tbEMail;
        TextBox tbAddress;
        TextBox tbItem;
        Label[] lblInsts;

        public frmEmailInput(ref TillEngine.TillEngine te)
        {
            tEngine = te;

            this.BackColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmBackColour;

            lblInsts = new Label[6];
            for (int i = 0; i < lblInsts.Length; i++)
            {
                lblInsts[i] = new Label();
                lblInsts[i].Location = new Point(10, 10 + (i * 35));
                lblInsts[i].BackColor = this.BackColor;
                lblInsts[i].ForeColor = this.ForeColor;
                lblInsts[i].Font = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
                lblInsts[i].AutoSize = true;
                this.Controls.Add(lblInsts[i]);
            }

            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(630, 240);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblInsts[0].Text = "Title : ";
            lblInsts[1].Text = "Forename : ";
            lblInsts[2].Text = "Surname : ";
            lblInsts[3].Text = "E-Mail Address : ";
            lblInsts[4].Text = "Home Address : ";
            lblInsts[5].Text = "Item Barcode : ";

            tbTitle = new TextBox();
            tbTitle.Location = new Point(200, lblInsts[0].Top);
            tbTitle.Size = new Size(100, lblInsts[0].Height);
            tbTitle.Font = lblInsts[0].Font;
            tbTitle.KeyDown += new KeyEventHandler(tbTitle_KeyDown);
            this.Controls.Add(tbTitle);

            tbForeName = new TextBox();
            tbForeName.Location = new Point(200, lblInsts[1].Top);
            tbForeName.Size = new Size(300, lblInsts[1].Height);
            tbForeName.Font = lblInsts[1].Font;
            tbForeName.KeyDown += new KeyEventHandler(tbForeName_KeyDown);
            this.Controls.Add(tbForeName);

            tbSurname = new TextBox();
            tbSurname.Location = new Point(200, lblInsts[2].Top);
            tbSurname.Size = new Size(300, lblInsts[2].Height);
            tbSurname.Font = lblInsts[2].Font;
            tbSurname.KeyDown += new KeyEventHandler(tbSurname_KeyDown);
            this.Controls.Add(tbSurname);

            tbEMail = new TextBox();
            tbEMail.Location = new Point(200, lblInsts[3].Top);
            tbEMail.Size = new Size(400, lblInsts[3].Height);
            tbEMail.Font = lblInsts[3].Font;
            tbEMail.KeyDown += new KeyEventHandler(tbEMail_KeyDown);
            this.Controls.Add(tbEMail);

            tbAddress = new TextBox();
            tbAddress.Location = new Point(200, lblInsts[4].Top);
            tbAddress.Size = new Size(400, lblInsts[4].Height);
            tbAddress.Font = lblInsts[4].Font;
            tbAddress.KeyDown += new KeyEventHandler(tbAddress_KeyDown);
            this.Controls.Add(tbAddress);

            tbItem = new TextBox();
            tbItem.Location = new Point(200, lblInsts[5].Top);
            tbItem.Size = new Size(400, lblInsts[5].Height);
            tbItem.Font = lblInsts[5].Font;
            tbItem.KeyDown += new KeyEventHandler(tbItem_KeyDown);
            this.Controls.Add(tbItem);
        }

        void tbItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tEngine.AddEmail(tbTitle.Text, tbForeName.Text, tbSurname.Text, tbEMail.Text, tbAddress.Text, tbItem.Text);
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbSurname.Focus();
                tbSurname.SelectionStart = tbSurname.Text.Length;
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref tEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    tbItem.Text = fsfi.GetItemBarcode();
                }
            }
        }

        void tbAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbItem.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbSurname.Focus();
                tbSurname.SelectionStart = tbSurname.Text.Length;
            }
        }

        void tbEMail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbAddress.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbSurname.Focus();
                tbSurname.SelectionStart = tbSurname.Text.Length;
            }
        }

        void tbSurname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbEMail.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbForeName.Focus();
                tbForeName.SelectionStart = tbSurname.Text.Length;
            }
        }

        void tbForeName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbSurname.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbTitle.Focus();
                tbTitle.SelectionStart = tbForeName.Text.Length;
            }
        }

        void tbTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbForeName.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
