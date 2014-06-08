using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using ModernListBox;

namespace BackOffice
{
    class frmMain : Form
    {
        BackEngine bEngine;
        bool bDrawnMiddle = false;
        ModernListBox.ModernListBox mlbLoading;

        public frmMain()
        {
            bEngine = new BackEngine();
            //this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1024, 768);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(170, 0, 0);
            this.Paint += new PaintEventHandler(frmMain_Paint);
            this.Resize += new EventHandler(frmMain_Resize);
            this.Show();
            this.DoubleBuffered = true;
            AnimateMiddleExpanding(this.CreateGraphics());
            this.Refresh();
            this.Text = "BackOffice Menu";
            this.KeyDown += new KeyEventHandler(frmMain_KeyDown);
        }

        void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Oemplus)
            {
                // Collect/Upload daily sales
                ExpandMiddleFullScreen(this.CreateGraphics());
                frmCollect fCollect = new frmCollect(this.Size, ref bEngine);
                fCollect.ShowDialog();
            }
        }

        void frmMain_Resize(object sender, EventArgs e)
        {
            this.Refresh();
        }

        void frmMain_Paint(object sender, PaintEventArgs e)
        {
            Color cYellow = Color.FromArgb(255, 255, 85);
            Pen p = new Pen(cYellow, 3.0f);
            e.Graphics.DrawRectangle(p, new Rectangle(new Point(3, 3), new Size(this.Width - 9, this.Height - 9)));
            e.Graphics.DrawRectangle(p, new Rectangle(new Point(9, 9), new Size(this.Width - 21, this.Height - 21)));
            if (bDrawnMiddle)
            {
                int nFinalWidth = (this.Width / 100) * 65;
                int nFinalHeight = (this.Height / 100) * 65;
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(new Point((this.Width / 2) - (nFinalWidth / 2) + ((nFinalWidth / 100) * 5), (this.Height / 2) - (nFinalHeight / 2) + ((nFinalHeight / 100) * 5)), new Size(nFinalWidth, nFinalHeight)));
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 170)), new Rectangle(new Point((this.Width / 2) - (nFinalWidth / 2), (this.Height / 2) - (nFinalHeight / 2)), new Size(nFinalWidth, nFinalHeight)));
                e.Graphics.DrawRectangle(p, new Rectangle(new Point((this.Width - nFinalWidth + 6) / 2, (this.Height - nFinalHeight + 6) / 2), new Size(nFinalWidth - 9, nFinalHeight - 9)));
                e.Graphics.DrawRectangle(p, new Rectangle(new Point((this.Width - nFinalWidth + 18) / 2, (this.Height - nFinalHeight + 18) / 2), new Size(nFinalWidth - 21, nFinalHeight - 21)));
                e.Graphics.DrawString(" =.. Collect/Upload Daily Sales", new Font("Franklin Gothic Medium", 16.0f), new SolidBrush(Color.FromArgb(255, 85, 85)), new PointF((this.Width - nFinalWidth + 26) / 2, (this.Height - nFinalHeight + 26) / 2));
            }
        }

        void AnimateMiddleExpanding(Graphics g)
        {
            int nFinalWidth = (this.Width / 100) * 65;
            int nFinalHeight = (this.Height / 100) * 65;
            int nCurrentWidth = 1;
            int nCurrentHeight = 1;

            while (nCurrentHeight < nFinalHeight || nCurrentWidth < nFinalWidth)
            {
                if (nCurrentWidth < nFinalWidth)
                    nCurrentWidth+=10;
                if (nCurrentHeight < nFinalHeight)
                    nCurrentHeight+=10;
                System.Threading.Thread.Sleep(1);
                g.FillRectangle(new SolidBrush(Color.FromArgb(0,0,170)), new Rectangle(new Point((this.Width / 2) - (nCurrentWidth / 2), (this.Height / 2) - (nCurrentHeight / 2)), new Size(nCurrentWidth, nCurrentHeight)));
            }
            bDrawnMiddle = true;
        }

        void ExpandMiddleFullScreen(Graphics g)
        {
            int nFinalWidth = this.Width;
            int nFinalHeight = this.Height;
            int nCurrentWidth = (this.Width / 100) * 65;
            int nCurrentHeight = (this.Height / 100) * 65;
            while (nCurrentHeight < nFinalHeight || nCurrentWidth < nFinalWidth)
            {
                if (nCurrentWidth < nFinalWidth)
                    nCurrentWidth += 10;
                if (nCurrentHeight < nFinalHeight)
                    nCurrentHeight += 5;
                System.Threading.Thread.Sleep(1);
                g.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 170)), new Rectangle(new Point((this.Width / 2) - (nCurrentWidth / 2), (this.Height / 2) - (nCurrentHeight / 2)), new Size(nCurrentWidth, nCurrentHeight)));
            }
        }
            
    }
}
