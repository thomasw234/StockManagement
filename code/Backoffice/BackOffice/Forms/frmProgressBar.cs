using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace BackOffice
{
    class frmProgressBar : Form
    {
        public ProgressBar pb;
        public Label lbInfo;

        public frmProgressBar()
        {
            this.BackColor = frmMenu.BackGroundColour;
            this.Size = new System.Drawing.Size(230, 70);
            pb = new ProgressBar();
            pb.Location = new System.Drawing.Point(5, 5);
            pb.Size = new System.Drawing.Size(this.ClientSize.Width - 10, this.ClientSize.Height - 10);
            this.Controls.Add(pb);
            pb.Value = 0;
            pb.Style = ProgressBarStyle.Continuous;
           
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        public frmProgressBar(string sDoing)
        {
            /*lbInfo = new Label();
            lbInfo.Text = sDoing;
            lbInfo.Location = new System.Drawing.Point(10, 10);
            lbInfo.AutoSize = true;
            this.Controls.Add(lbInfo);
            this.Refresh();*/
            
            this.Text = sDoing;

            pb = new ProgressBar();
            pb.Location = new System.Drawing.Point(5, 5);
            this.Controls.Add(pb);
            pb.Value = 0;
            pb.Style = ProgressBarStyle.Continuous;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new System.Drawing.Size(230, 70);
            pb.Size = new System.Drawing.Size(this.ClientSize.Width - 10, this.ClientSize.Height - 10);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
