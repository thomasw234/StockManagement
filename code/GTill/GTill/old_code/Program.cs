using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TillEngine;
using System.IO;

namespace GTill
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
            //Application.Run(new EasterEggs.SnakeGame(new System.Drawing.Size(800, 600), new System.Drawing.Point(200, 200)));
            //Application.Run(new frmDateInput(System.Drawing.Color.Black, System.Drawing.Color.White, "Franklin Gothic Medium", new System.Drawing.Size(1280,1024)));
            /*
            TillEngine.TillEngine te = new TillEngine.TillEngine();
            te.LoadTable("REPDATA");
            te.LoadTable("THDR");
            te.LoadTable("TDATA");
            te.LoadTable("STOCK");
            te.LoadTable("STKLEVEL");
            te.LoadTable("DETAILS");
            Application.Run(new frmRefund(System.Drawing.Color.Black, System.Drawing.Color.White, new System.Drawing.Size(800, 600), new System.Drawing.Point(100, 100), "Franklin Gothic Medium", ref te));
            */
            
        }
    }
}
