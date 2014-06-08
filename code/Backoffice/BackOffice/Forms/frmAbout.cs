using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAbout : Form
    {
        PictureBox pbImage;
        Random r;
        public frmAbout()
        {
            /*this.WindowState = FormWindowState.Maximized;
            this.SizeChanged += new EventHandler(frmAbout_SizeChanged);
            this.KeyDown += new KeyEventHandler(frmAbout_KeyDown);
            this.FormBorderStyle = FormBorderStyle.None;
            pbImage = new PictureBox();
            pbImage.Location = new Point(0, 0);
            pbImage.Size = new Size(this.Width, this.Height);
            r = new Random();
            switch (r.Next(1, 3))
            {
                case 1:
                    pbImage.Image = (Image)BackOffice.Properties.Resources.AboutPic1;
                    break;
                case 2:
                    pbImage.Image = (Image)BackOffice.Properties.Resources.AboutPic2;
                    break;
            }
            this.Controls.Add(pbImage);
            pbImage.SizeMode = PictureBoxSizeMode.Zoom;*/
        }

        void frmAbout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void frmAbout_SizeChanged(object sender, EventArgs e)
        {
            pbImage.Size = this.Size;
        }
    }
}
