using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmShutDownWait : Form
    {
        TillEngine.TillEngine tEngine;
        Timer tmr = new Timer();
        public frmShutDownWait(ref TillEngine.TillEngine te)
        {
            tEngine = te;
            this.Paint += new PaintEventHandler(frmShutDownWait_Paint);
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.Font = new Font(GTill.Properties.Settings.Default.sFontName, 20.0f, FontStyle.Bold);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            tmr.Interval = 1000;
            tmr.Tick += new EventHandler(tmr_Tick);
            tmr.Enabled = true;
            this.KeyDown += new KeyEventHandler(frmShutDownWait_KeyDown);
        }

        void frmShutDownWait_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.ExitThread();
                Application.Exit();
            }
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            tEngine.ProcessCommands();
        }

        void frmShutDownWait_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("Waiting for signal to shut down...", this.Font, new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - (e.Graphics.MeasureString("Waiting for signal to shut down...", this.Font).Width / 2), this.Height / 2));
        }
    }
}
