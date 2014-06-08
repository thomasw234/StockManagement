using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;

namespace GTill
{
    class frmSingleInputBox : Form
    {
        public string Response = "$NONE";
        public string ProductLookupHelp = "";
        public bool GettingCategory = false;
        Label lblQuestion;
        public TextBox tbResponse;
        Button bOK;

        public frmSingleInputBox(string sQuestion)
        {
            this.Size = new Size(500, 100);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            lblQuestion = new Label();
            lblQuestion.Location = new Point(10, 10);
            lblQuestion.Text = sQuestion;
            lblQuestion.AutoSize = true;
            this.Controls.Add(lblQuestion);

            tbResponse = new TextBox();
            tbResponse.Location = new Point(10, 20);
            tbResponse.Size = new Size(400, 25);
            this.Controls.Add(tbResponse);
            tbResponse.KeyDown += new KeyEventHandler(tbResponse_KeyDown);

            bOK = new Button();
            bOK.Location = new Point(410, tbResponse.Top);
            bOK.Size = new Size(65, 25);
            bOK.Text = "OK";
            this.Controls.Add(bOK);
            bOK.Click += new EventHandler(bOK_Click);
        }

        void bOK_Click(object sender, EventArgs e)
        {
            if (tbResponse.Text == "")
            {
                Response = "$NONE";
            }
            else
            {
                Response = tbResponse.Text;
            }
            this.Close();
        }

        void bOK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbResponse.Text == "")
                {
                    Response = "$NONE";
                }
                else
                {
                    Response = tbResponse.Text;
                }
                this.Close();
            }
        }

        void tbResponse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbResponse.Text == "")
                {
                    Response = "$NONE";
                }
                else
                {
                    Response = tbResponse.Text;
                }
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
