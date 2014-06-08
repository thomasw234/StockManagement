using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmReportViewer : Form
    {
        string[] sReport;
        int nTopLine = 7;
        int nBottomLine = 6;
        Label lblInstructions;
        string ReportTitle;
        int nOfLinesDrawn = 0;
        int nMaxLines = 0;
        int nLineSelected = 0;
        decimal dLinesStartPos = 0;
        decimal dLineHeight = 0;
        int[] nLineTop;
        int[] nLineHeight;

        StockEngine.ReportType rType;
        public frmReportViewer(StockEngine.ReportType r)
        {
            rType = r;
            if (rType == StockEngine.ReportType.StockLevelReport)
            {
                nTopLine = 6;
                nBottomLine = 5;
            }
            else if (rType == StockEngine.ReportType.ComissionReport)
            {
                nTopLine = 5;
                nBottomLine = 4;
            }
            else if (rType == StockEngine.ReportType.CommissionSummaryReport)
            {
                nTopLine = 4;
                nBottomLine =3;
            }
            else if (rType == StockEngine.ReportType.OutStandingItems)
            {
                nTopLine = 6;
                nBottomLine = 5;
            }
            //this.FormBorderStyle = FormBorderStyle.None;
            nLineSelected = nTopLine;
            this.WindowState = FormWindowState.Maximized;
            this.Size = new Size(1024, 768);
            TextReader tr = new StreamReader("REPORT.TXT");
            sReport = tr.ReadToEnd().Split('\n');
            nLineHeight = new int[sReport.Length];
            nLineTop = new int[sReport.Length];
            ReportTitle = sReport[1];

            while (nLineSelected != sReport.Length && sReport[nLineSelected].StartsWith("-"))
                nLineSelected++;

            if (rType == StockEngine.ReportType.ComissionReport || rType == StockEngine.ReportType.CommissionSummaryReport)
                ReportTitle = sReport[0];
            tr.Close();
            DrawNextPageOfReport(true);
            this.Paint += new PaintEventHandler(frmReportViewer_Paint);
            lblInstructions = new Label();
            lblInstructions.AutoSize = true;
            lblInstructions.Font = new Font("Consolas", 15.0f);
            lblInstructions.Text = "Press Page Up/Down to move up/down, or Esc to quit";
            this.Controls.Add(lblInstructions);
            this.KeyDown += new KeyEventHandler(frmReportViewer_KeyDown);
            this.Text = "Report Viewer";
            this.MouseDown += new MouseEventHandler(frmReportViewer_MouseDown);
            this.MouseWheel += new MouseEventHandler(frmReportViewer_MouseWheel);
        }

        void frmReportViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (nLineSelected + 1 < sReport.Length)
            {
                do
                {
                    nLineSelected += -(e.Delta / 120);
                    if (sReport.Length == nLineSelected)
                    {
                        nLineSelected--;
                        break;
                    }
                    if (nLineSelected >= nBottomLine - 1)
                    {
                        nBottomLine++;
                        nTopLine++;
                    }
                }
                while (sReport[nLineSelected].StartsWith("-"));
            }
            DrawNextPageOfReport(false);
        }

        void frmReportViewer_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < nLineTop.Length; i++)
            {
                if (nLineTop[i] < e.Y && nLineHeight[i] + nLineTop[i] > e.Y)
                {
                    nLineSelected = i;
                    break;
                }
            }
            DrawNextPageOfReport(false);
        }

        void frmReportViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.PageDown)
            {
                if (nBottomLine < (sReport.Length - 1))
                {
                    DrawNextPageOfReport(true);
                    nLineSelected = nTopLine;
                    DrawNextPageOfReport(false);
                }
                else
                {
                    if (e.KeyCode == Keys.Enter)
                        this.Close();
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                do
                {
                    nLineSelected++;
                    if (sReport.Length == nLineSelected)
                    {
                        nLineSelected--;
                        break;
                    }
                    if (nLineSelected >= nBottomLine - 1)
                    {
                        nBottomLine++;
                        nTopLine++;
                    }
                }
                while (sReport[nLineSelected].StartsWith("-"));
                DrawNextPageOfReport(false);
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                nTopLine -= nMaxLines;
                nBottomLine -= nMaxLines;
                if (nTopLine < 7)
                {
                    if (rType == StockEngine.ReportType.StockLevelReport)
                    {
                        nTopLine = 6;
                        nBottomLine = 5;
                    }
                    else if (rType == StockEngine.ReportType.ComissionReport)
                    {
                        nTopLine = 5;
                        nBottomLine = 4;
                    }
                    else
                    {
                        nTopLine = 7;
                        nBottomLine = 6;
                    }
                }
                DrawNextPageOfReport(false);
            }
            else if (e.KeyCode == Keys.Up)
            {
                do
                {
                    nLineSelected--;
                    if (nLineSelected < nTopLine)
                    {
                        nTopLine -= 1;
                        nBottomLine -= 1;
                    }
                    int nMaxTop = 0;

                    if (rType == StockEngine.ReportType.StockLevelReport)
                    {
                        nMaxTop = 6;
                    }
                    else if (rType == StockEngine.ReportType.ComissionReport)
                    {
                        nMaxTop = 5;
                    }
                    else
                    {
                        nMaxTop = 7;
                    }
                    while (nTopLine < nMaxTop)
                    {
                        nTopLine++;
                        nBottomLine++;
                        nLineSelected = nTopLine;
                        while (sReport[nLineSelected].StartsWith("-"))
                            nLineSelected++;
                    }

                }
                while (sReport[nLineSelected].StartsWith("-"));
                DrawNextPageOfReport(false);
            }
            else if (e.KeyCode == Keys.Q || e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void frmReportViewer_Paint(object sender, PaintEventArgs e)
        {
            DrawNextPageOfReport(false);
            lblInstructions.Location = new Point(10, this.Height - lblInstructions.Height - 10);
        }

        void DrawNextPageOfReport(bool bIncrementPage)
        {
            Bitmap bm = new Bitmap(this.Width, this.Height);
            Graphics e = Graphics.FromImage(bm);
            for (int i = 0; i < nLineTop.Length; i++)
            {
                nLineTop[i] = -1;
                nLineHeight[i] = 0;
            }
            Font fFont = new Font("Consolas", 9.5f);
            Color cBoxColor = Color.LightGray;
            int nTop = 10;
            //Graphics e = this.CreateGraphics();
            e.Clear(this.BackColor);
            if (bIncrementPage)
                nTopLine = nBottomLine + 1;
            int nBoxWidth = 0;
            if (rType == StockEngine.ReportType.SalesReport)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString("                                             Level     Sold         Sales       Sales                            Profit (%)   Sales (%)", fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight() * 2);
                e.DrawString("Barcode       Description                    Stock     Quantity     Gross       Net         Profit    Profit(%)  Relative     Relative", fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight());
                e.DrawString("                                             Level     Sold         Sales       Sales                            Profit (%)   Sales (%)", fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight()) * 2;
            }
            else if (rType == StockEngine.ReportType.StockLevelReport)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString(sReport[4], fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                e.DrawString(sReport[4], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight() * 2);
            }
            else if (rType == StockEngine.ReportType.ComissionReport)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString(sReport[2], fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                e.DrawString(sReport[3], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight() * 2);
            }
            else if (rType == StockEngine.ReportType.CommissionSummaryReport)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString(sReport[1], fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                e.DrawString(sReport[2], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight() * 2);
            }
            else if (rType == StockEngine.ReportType.OutStandingItems)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString(sReport[4], fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                e.DrawString(sReport[4], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight() * 2);
            }
            else if (rType == StockEngine.ReportType.OutOfStockLengthReport)
            {
                nBoxWidth = Convert.ToInt32(e.MeasureString(sReport[4], fFont).Width);
                e.DrawString(ReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((nBoxWidth / 2) - Convert.ToInt32((e.MeasureString(ReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nTop));
                nTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                e.DrawString(sReport[4], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                nTop += Convert.ToInt32(fFont.GetHeight() * 2);
            }
            nOfLinesDrawn = 0;
            int nOfReportLines = 0;
            dLinesStartPos = nTop;
            dLineHeight = (decimal)fFont.GetHeight();
            bool bColorBackground = false;
            for (int i = nTopLine; i < sReport.Length && nTop < this.Height - 100; i++)
            {
                if (sReport[i].StartsWith("-"))
                {
                    if (i + 1 == nLineSelected)
                    {
                        e.FillRectangle(new SolidBrush(Color.Blue), 10, nTop, nBoxWidth, fFont.GetHeight());
                        e.DrawString(sReport[i + 1], fFont, new SolidBrush(Color.White), new PointF(10, nTop));
                        nLineTop[i + 1] = nTop;
                        nLineHeight[i + 1] = Convert.ToInt32(fFont.GetHeight());
                        i += 2;
                        nOfReportLines += 2;
                    }
                    else
                    {
                        e.FillRectangle(new SolidBrush(cBoxColor), 10, nTop, nBoxWidth, fFont.GetHeight());
                        e.DrawString(sReport[i + 1], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                        nLineTop[i + 1] = nTop;
                        nLineHeight[i + 1] = Convert.ToInt32(fFont.GetHeight());
                        i += 2;
                        nOfReportLines += 2;
                    }

                }
                else
                {
                    if (i == nLineSelected)
                    {
                        e.FillRectangle(new SolidBrush(Color.Blue), 10, nTop, nBoxWidth, fFont.GetHeight());
                        e.DrawString(sReport[i], fFont, new SolidBrush(Color.LightSteelBlue), new PointF(10, nTop));
                        nLineTop[i] = nTop;
                        nLineHeight[i] = Convert.ToInt32(fFont.GetHeight());
                    }
                    else
                    {
                        if (bColorBackground)
                            e.FillRectangle(new SolidBrush(Color.White), 10, nTop, nBoxWidth, fFont.GetHeight());
                        //se
                            //e.FillRectangle(new SolidBrush(Color.White), 10, nTop, nBoxWidth, fFont.GetHeight());

                        e.DrawString(sReport[i], fFont, new SolidBrush(Color.Black), new PointF(10, nTop));
                        nLineTop[i] = nTop;
                        nLineHeight[i] = Convert.ToInt32(fFont.GetHeight());
                    }
                    bColorBackground = !bColorBackground;
                }
                nTop += Convert.ToInt32(fFont.GetHeight());
                nBottomLine = i;
                nOfLinesDrawn++;
                nOfReportLines++;
            }
            if (nOfReportLines > nMaxLines)
                nMaxLines = nOfReportLines;

            //this.CreateGraphics().Clear(this.BackColor);
            this.CreateGraphics().DrawImage(bm, new Point(0, 0));
        }

    }
}
