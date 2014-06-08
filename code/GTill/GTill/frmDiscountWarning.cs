using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmDiscountWarning : Form
    {
        public bool Continue = false;
        string sDesc = "";
        Label lblWarning;

        public frmDiscountWarning(string sDesc)
        {
            this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, 125);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, (Screen.PrimaryScreen.Bounds.Height / 2) - 65);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Red;
            this.ForeColor = Color.White;
            this.sDesc = sDesc;
            this.Paint +=new PaintEventHandler(frmDiscountWarning_Paint);
            this.KeyDown += new KeyEventHandler(frmDiscountWarning_KeyDown);

            lblWarning = new Label();
            lblWarning.Location = new Point(0, 0);
            lblWarning.Size = new Size(this.Width, this.Height);
            lblWarning.Text = "High discount on " + sDesc + ".\nPress Enter to continue or Escape to cancel";
            lblWarning.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblWarning);
            lblWarning.Font = new Font("Franklin Gothic Medium", 30.0f, FontStyle.Bold);
            lblWarning.ForeColor = Color.White;
        }

        void frmDiscountWarning_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Continue = false;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                Continue = true;
                this.Close();
            }
        }

        void frmDiscountWarning_Paint(object sender, PaintEventArgs e)
        {
            //this.CreateGraphics().DrawString("High discount on " + sDesc + ".\nPress Enter to continue or Escape to cancel", new Font("Franklin Gothic Medium", 30.0f), new SolidBrush(Color.White), new PointF(0, 0));
        }
    }
}
