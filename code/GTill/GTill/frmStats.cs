using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmStats : Form
    {
        TillEngine.TillEngine tEngine;
        Color[] cPieColours = { Color.Red, Color.Orange, Color.Yellow, Color.LightGreen, Color.Green, Color.LightBlue, Color.Blue, Color.Indigo, Color.Violet };
        Label lblTitle;
        Label lblExit;
        float fPieRotationAngle = 0;
        ChartType cChartType;
        public enum ChartType { Pie, CumulativeSales, AverageHourly, WeeklyCumulative };

        public frmStats(ref TillEngine.TillEngine tE, Size s, Point p, ChartType cTypes)
        {
            this.Size = s;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Location = p;
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
            tEngine = tE;
            lblTitle = new Label();
            lblTitle.Text = "Sales Analysis - Amount Taken";
            lblTitle.AutoSize = false;
            lblTitle.Top = 0;
            lblTitle.Left = 0;
            lblTitle.Width = this.Width;
            lblTitle.Height = 40;
            lblTitle.Font = new Font(GTill.Properties.Settings.Default.sFontName, 20.0f);
            lblTitle.TextAlign = ContentAlignment.TopCenter;
            lblExit = new Label();
            lblExit.Text = "Press any key to close.";
            lblExit.Location = new Point(0, this.Height - 50);
            lblExit.Size = new Size(this.Width, 30);
            cChartType = cTypes;
            lblExit.Font = new Font(GTill.Properties.Settings.Default.sFontName, 20.0f);
            lblExit.TextAlign = ContentAlignment.TopCenter;
            WorkOutAngles();
            //this.Controls.Add(lblExit);
            //this.Controls.Add(lblTitle);
            this.KeyDown += new KeyEventHandler(frmStats_KeyDown);
            this.Paint += new PaintEventHandler(frmStats_Paint);
        }

        void frmStats_Paint(object sender, PaintEventArgs e)
        {
            //DrawPieChart(this.CreateGraphics());
            if (cChartType == ChartType.CumulativeSales)
                DrawGraph(this.CreateGraphics());
            else if (cChartType == ChartType.Pie)
                DrawPieChart(this.CreateGraphics());
            else if (cChartType == ChartType.AverageHourly)
                DrawAverageHourlyGraph(this.CreateGraphics());
            else if (cChartType == ChartType.WeeklyCumulative)
                DrawWeekGraph(this.CreateGraphics());
        }

        void frmStats_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && cChartType == ChartType.Pie)
            {
                fPieRotationAngle += 1.0f;
                DrawPieChart(this.CreateGraphics());
            }
            else if (e.KeyCode == Keys.Left && cChartType == ChartType.Pie)
            {
                fPieRotationAngle -= 1.0f;
                DrawPieChart(this.CreateGraphics());
            }
            else
                this.Close();
        }

        float[] fAngles;
        float[] fStaffSales;
        void WorkOutAngles()
        {
            fStaffSales = Statistics.GetAmountsThatStaffHaveSold(ref tEngine);
            string[] sNoStats = GTill.Properties.Settings.Default.sNoStatsAbout.Split(',');
            int[] nNoStats = new int[sNoStats.Length];
            for (int i = 0; i < sNoStats.Length; i++)
            {
                try
                {
                    nNoStats[i] = Convert.ToInt32(sNoStats[i]);
                }
                catch
                {
                    nNoStats[i] = 0;
                }
            }
            float fTotalAmount = 0.0f;
            for (int i = 0; i < fStaffSales.Length; i++)
            {
                bool bStaffOptedOut = false;
                for (int x = 0; x < nNoStats.Length; x++)
                {
                    if (i == nNoStats[x])
                        bStaffOptedOut = true;
                }
                if (!bStaffOptedOut)
                    fTotalAmount += fStaffSales[i];
            }
            fAngles = new float[100];
            for (int i = 0; i < 100; i++)
            {
                fAngles[i] = (360 / fTotalAmount) * fStaffSales[i];
            }
        }

        void DrawPieChart(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            string[] sNoStats = GTill.Properties.Settings.Default.sNoStatsAbout.Split(',');
            int[] nNoStats = new int[sNoStats.Length];
            for (int i = 0; i < sNoStats.Length; i++)
            {
                try
                {
                    nNoStats[i] = Convert.ToInt32(sNoStats[i]);
                }
                catch
                {
                    nNoStats[i] = 0;
                }
            }
            int xOffset = (this.Width / 2) - 350, yOffset = (this.Height / 2) - 250;
            int nCurrentPieColour = 0;
            float nCurrentDegree = fPieRotationAngle;
            int nStaffHeightLoc = yOffset;
            for (int i = 0; i < fAngles.Length;)
            {
                if (nCurrentDegree < 360 + fPieRotationAngle)
                {
                    g.FillPie(new SolidBrush(cPieColours[nCurrentPieColour]), new Rectangle(xOffset, yOffset, 500, 500), nCurrentDegree, fAngles[i]);
                    if (fAngles[i] > 0.0f)  
                    {
                        g.DrawString(tEngine.GetStaffName(i) + " - " + tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fStaffSales[i]), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(cPieColours[nCurrentPieColour]), new PointF(xOffset + 500, (float)nStaffHeightLoc));
                        nStaffHeightLoc += (int)g.MeasureString(tEngine.GetStaffName(i), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Height + 5;
                        nCurrentPieColour++;
                        if (nCurrentPieColour >= cPieColours.Length)
                            nCurrentPieColour = 0;
                    }
                    nCurrentDegree += fAngles[i];
                }

                bool bStaffOptedOut = false;
                do
                {
                    i++;
                    bStaffOptedOut = false;
                    if (i >= fAngles.Length)
                        break;
                    for (int x = 0; x < nNoStats.Length; x++)
                    {
                        if (i == nNoStats[x])
                            bStaffOptedOut = true;
                    }
                } while (bStaffOptedOut);
                g.DrawString("Pie Chart", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - (g.MeasureString("Hourly Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width / 2), g.MeasureString("ThomasIsMyName", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Height));
           
            }
        }

        void DrawGraph(Graphics g)
        {
            try
            {
                string[] sNoStats = GTill.Properties.Settings.Default.sNoStatsAbout.Split(',');
                int[] nNoStats = new int[sNoStats.Length];
                int nOfHoursToShow = DateTime.Now.Hour - 8;
                for (int i = 0; i < sNoStats.Length; i++)
                {
                    try
                    {
                        nNoStats[i] = Convert.ToInt32(sNoStats[i]);
                    }
                    catch
                    {
                        nNoStats[i] = 0;
                    }
                }
                int nNameLeft = 0;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int nCurrentPieColour = 0;
                Point pBottomLeft = new Point((this.Width / 8), this.Height - (this.Height / 8));
                float fTotalAmount = 0.0f;
                for (int i = 0; i < fStaffSales.Length; i++)
                {
                    bool bStaffOptedOut = false;
                    for (int x = 0; x < nNoStats.Length; x++)
                    {
                        if (i == nNoStats[x])
                            bStaffOptedOut = true;
                    }
                    if (fTotalAmount < fStaffSales[i] && !bStaffOptedOut)
                        fTotalAmount = fStaffSales[i];
                }
                float fAmountEachYPixel = (float)fTotalAmount/ (float)((this.Height / 4) * 3);
                float fAmountEachXPixel = (float)(nOfHoursToShow * 60) / (float)((this.Width / 4) * 3); // 9 hours @ 60 mins each
                // Go through each sale
                string[] sSales = tEngine.GetListOfTransactionNumbers();
                for (int x = 0; x < 10; x++)
                {
                    float fCumulativeTotal = 0.0f;
                    Point pLastPoint = pBottomLeft;
                    string sStaffName = "";
                    sStaffName = tEngine.GetStaffName(x);
                    bool bStaffOptedOut = false;
                    for (int i = 0; i < nNoStats.Length; i++)
                    {
                        if (x == nNoStats[i])
                            bStaffOptedOut = true;
                    }
                    if (sStaffName != "" && !bStaffOptedOut)
                    {
                        for (int i = 0; i < sSales.Length; i++)
                        {
                            string[,] sTransactionInfo = tEngine.GetTransactionInfo(sSales[i]);
                            if (sStaffName == tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1])
                            {
                                // This is one of the staff member's transactions, so plot a point
                                // Work out the value of the transaction
                                float fTransactionValue = 0.0f;
                                int nOfItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                                int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
                                for (int z = 1; z <= nOfItemsInTransaction; z++)
                                {
                                    fTransactionValue += (float)Convert.ToDecimal(sTransactionInfo[z, 2]);
                                }
                                for (int z = nOfItemsInTransaction; z <= nOfItemsInTransaction + nOfPaymentMethods; z++)
                                {
                                    if (sTransactionInfo[z, 0] == "DEPO")
                                    {
                                        fTransactionValue -= (float)Convert.ToDecimal(sTransactionInfo[z, 1]);
                                    }
                                }
                                if (sTransactionInfo[0, 3] != "SALE" && sTransactionInfo[0,3] != "SPECIFICREFUND")
                                {
                                    fTransactionValue = 0;
                                }
                                fTransactionValue = TillEngine.TillEngine.FixFloatError(fTransactionValue);
                                fCumulativeTotal += fTransactionValue;
                                fCumulativeTotal = TillEngine.TillEngine.FixFloatError(fCumulativeTotal);
                                int yPos = this.Height - (this.Height / 8) - (int)Math.Round(fCumulativeTotal / fAmountEachYPixel, 0);
                                string stime = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[0].Split(' ')[1].Replace(':', '.');
                                int nMinutesTotal = 0;
                                nMinutesTotal += (Convert.ToInt32(stime.Split('.')[0]) * 60) - (9 * 60);
                                nMinutesTotal += Convert.ToInt32(stime.Split('.')[1]);
                                int xPos = (int)Math.Round(nMinutesTotal / fAmountEachXPixel) + (this.Width / 8);
                                g.DrawLine(new Pen(cPieColours[nCurrentPieColour], 3.0f), pLastPoint, new Point(xPos, yPos));
                                pLastPoint = new Point(xPos, yPos);
                            }
                        }
                        if (fStaffSales[x] > 0.0f)
                        {
                            g.DrawString(tEngine.GetStaffName(x), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(cPieColours[nCurrentPieColour]), new PointF((float)nNameLeft, 0.0f));
                            nNameLeft += (int)g.MeasureString(tEngine.GetStaffName(x), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width;
                            nCurrentPieColour++;
                            if (nCurrentPieColour >= cPieColours.Length)
                                nCurrentPieColour = 0;
                        }
                    }
                }
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(this.Width / 8, this.Height / 8));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(this.Width - (this.Width / 8), this.Height - (this.Height / 8)));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point((this.Width / 8) - 15, this.Height - (this.Height / 8)));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(this.Width / 8, this.Height / 8), new Point((this.Width / 8) - 15, this.Height / 8));
                g.DrawString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - g.MeasureString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, pBottomLeft.Y - g.MeasureString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2));
                g.DrawString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - g.MeasureString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, (this.Height / 8) - g.MeasureString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2));
                int nDiff = ((this.Width / 4) * 3) / nOfHoursToShow;
                for (int t = 0; t <= 9; t++)
                {
                    g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point((this.Width / 8) + (nDiff * t), pBottomLeft.Y), new Point((this.Width / 8) + (nDiff * t), pBottomLeft.Y + 15));
                    g.DrawString((t + 9).ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 8) + (nDiff * t) - (g.MeasureString((9 + t).ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2), this.Height - (this.Height / 8) + 20));
                }
                g.DrawString("Hour", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - 15, this.Height - 50));
                g.DrawString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(this.Width / 8 - g.MeasureString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width - 15, (this.Height / 8) - 50));
                g.DrawString("Cumulative Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - (g.MeasureString("Hourly Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width / 2), g.MeasureString("ThomasIsMyName", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Height));
                for (int t = 0; t <= 9; t++)
                {
                    g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(pBottomLeft.X - 15, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10))), new Point(pBottomLeft.X, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10))));
                    g.DrawString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay((fTotalAmount / 10) * t), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - (int)g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay((fTotalAmount / 10) * t), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10)) - (g.MeasureString(tEngine.CurrencySymbol.ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2)));
 
                }

            }
            catch (Exception ex)
            {
                ;
            }
        
        }

        void DrawAverageHourlyGraph(Graphics g)
        {
            // First work out hourly sales

            string[] sTransaction = tEngine.GetListOfTransactionNumbers();
            string[] sStaff = new string[10];
            for (int i = 0; i < 10; i++)
            {
                sStaff[i] = tEngine.GetStaffName(i);
            }
            float[,] fStaffAverages = new float[10, 9]; // staffnum,hour
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    fStaffAverages[x, y] = 0;
                }
            }
            for (int i = 0; i < sTransaction.Length; i++)
            {
                string[,] sTransactionInfo = tEngine.GetTransactionInfo(sTransaction[i]);
                int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
                string[] sSalesData = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2]);
                string sTime = sSalesData[0].Split(' ')[1];
                string sHour = sTime.Split(':')[0];
                int nHour = Convert.ToInt32(sHour);
                string sStaffName = sSalesData[1].TrimEnd(' ').ToUpper();
                int nStaffNum = 0;
                for (int z = 0; z < sStaff.Length; z++)
                {
                    if (sStaffName.ToUpper() == sStaff[z].ToUpper())
                    {
                        nStaffNum = z;
                        break;
                    }
                }
                float fAmountSold = 0.0f;
                for (int x = 1; x <= nOfItems; x++)
                {
                    fAmountSold += (float)Convert.ToDecimal(sTransactionInfo[x, 2]);
                }
                fAmountSold = tEngine.fFixFloatError(fAmountSold);
                fStaffAverages[nStaffNum, nHour - 9] += fAmountSold;
                fStaffAverages[nStaffNum, nHour - 9] = tEngine.fFixFloatError(fStaffAverages[nStaffNum, nHour - 9]);

            }

            // Work out highest on y axis

            float fHighest = 0.0f;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (fStaffAverages[x,y] > fHighest)
                        fHighest = fStaffAverages[x,y];
                }
            }
            float fAmountEachYPixel = fHighest / (((float)this.Height / 4) * 3);
            float fAmountEachXPixel = 9.0f / (((float)this.Width / 4) * 3);
            int nColourNumber = 0;
            int nNameLeft = 0;
            bool[] bDrawingStaff = new bool[sStaff.Length];
            for (int i = 0; i < bDrawingStaff.Length; i++)
            {
                bDrawingStaff[i] = false;
            }
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (fStaffAverages[x, y] != 0.0f)
                        bDrawingStaff[x] = true;
                }
            }
            
            // Draw the graph
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Point pBottomLeft = new Point((this.Width / 8), this.Height - (this.Height / 8));
            for (int i = 1; i < 10; i++) // Staff
            {
                if (bDrawingStaff[i])
                {
                    Point pLastPoint = pBottomLeft;
                    for (int x = 0; x < 9; x++) // Hours
                    {
                        Point pNewPoint = new Point(Convert.ToInt32(pBottomLeft.X + (x / fAmountEachXPixel) + (0.5 / fAmountEachXPixel)), Convert.ToInt32(pBottomLeft.Y - (fStaffAverages[i, x] / fAmountEachYPixel)));
                        g.DrawLine(new Pen(cPieColours[nColourNumber], 2.0f), pLastPoint, pNewPoint);
                        pLastPoint = pNewPoint;
                    }
                    g.DrawString(tEngine.GetStaffName(i), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(cPieColours[nColourNumber]), new PointF(nNameLeft, 0));
                    nNameLeft += Convert.ToInt32(g.MeasureString(tEngine.GetStaffName(i), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width);
                    nColourNumber++;
                }
            }

            // Draw the axes

            g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(pBottomLeft.X, pBottomLeft.Y - ((this.Height / 4) * 3)));
            g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(pBottomLeft.X + ((this.Width / 4) * 3), pBottomLeft.Y));
            int nYDiff = ((this.Height / 4) * 3) / 10;
            int nXDiff = ((this.Width / 4) * 3) / 9;
            for (int i = 0; i < 10; i++)
            {
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(pBottomLeft.X - 15, pBottomLeft.Y - (nYDiff * i)), new Point(pBottomLeft.X, pBottomLeft.Y - (nYDiff * i)));
                g.DrawString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay((fHighest / 10) * i), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay((fHighest / 10) * i), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width - 15, pBottomLeft.Y - (nYDiff * i) - (g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay((fHighest / 10) * i), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2)));
            }
            g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(pBottomLeft.X - 15, pBottomLeft.Y - ((this.Height / 4) * 3)), new Point(pBottomLeft.X, pBottomLeft.Y - ((this.Height / 4) * 3)));
            g.DrawString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay(fHighest), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay(fHighest), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width - 15, pBottomLeft.Y - ((this.Height / 4) * 3) - (g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay(fHighest), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2)));

            for (int i = 0; i < 9; i++)
            {
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(pBottomLeft.X + (i * nXDiff) + Convert.ToInt32(0.5 / fAmountEachXPixel), pBottomLeft.Y), new Point(pBottomLeft.X + (i * nXDiff) + Convert.ToInt32(0.5 / fAmountEachXPixel), pBottomLeft.Y + 15));
                g.DrawString((i + 9).ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X + (i * nXDiff) + Convert.ToInt32(0.5 / fAmountEachXPixel) - 15, pBottomLeft.Y + 15));
            }

            // Draw the title

            g.DrawString("Hourly Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - (g.MeasureString("Hourly Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width / 2), g.MeasureString("ThomasIsMyName", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Height));
            g.DrawString("Hour", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - 15, this.Height - 50));
            g.DrawString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(this.Width / 8 - g.MeasureString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width - 15, (this.Height / 8) - 50));
        }


        void DrawWeekGraph(Graphics g)
        {
                string[] sNoStats = GTill.Properties.Settings.Default.sNoStatsAbout.Split(',');
                int[] nNoStats = new int[sNoStats.Length];
                int nOfHoursToShow = 63;
                for (int i = 0; i < sNoStats.Length; i++)
                {
                    try
                    {
                        nNoStats[i] = Convert.ToInt32(sNoStats[i]);
                    }
                    catch
                    {
                        nNoStats[i] = 0;
                    }
                }
                int nNameLeft = 0;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int nCurrentPieColour = 0;
                Point pBottomLeft = new Point((this.Width / 8), this.Height - (this.Height / 8));
                float fTotalAmount = 0.0f;
                for (int i = 0; i < fStaffSales.Length; i++)
                {
                    bool bStaffOptedOut = false;
                    for (int x = 0; x < nNoStats.Length; x++)
                    {
                        if (i == nNoStats[x])
                            bStaffOptedOut = true;
                    }
                    if (fTotalAmount < fStaffSales[i] && !bStaffOptedOut)
                        fTotalAmount = fStaffSales[i];
                }
                float fAmountEachYPixel = (float)fTotalAmount / (float)((this.Height / 4) * 3);
                float fAmountEachXPixel = (float)(nOfHoursToShow * 60) / (float)((this.Width / 4) * 3) / 7; // 9 hours @ 60 mins each
                // Go through each sale
                
                //Set up array of tillEngines
                TillEngine.TillEngine[] tEngines = new TillEngine.TillEngine[7];
                for (int i = 0; i < 7; i++)
                {
                    tEngines[i] = new TillEngine.TillEngine();
                    tEngines[i].LoadTable(GTill.Properties.Settings.Default.sOUTGNGDir + "\\REPDATA" + (i + 1).ToString() + ".DBF");
                    tEngines[i].LoadTable(GTill.Properties.Settings.Default.sOUTGNGDir + "\\TDATA" + (i + 1).ToString() + ".DBF");
                    tEngines[i].LoadTable(GTill.Properties.Settings.Default.sOUTGNGDir + "\\THDR" + (i + 1).ToString() + ".DBF");
                }
                int nToday = tEngine.GetDayNumberFromRepData();
            
                for (int nDayNum = (nToday + 1) % 7; nDayNum < 7 + ((nToday + 1) % 7); nDayNum++)
                {

                    string[] sSales = tEngines[nDayNum].GetListOfTransactionNumbers();
                    for (int x = 0; x < 10; x++)
                    {
                        float fCumulativeTotal = 0.0f;
                        Point pLastPoint = pBottomLeft;
                        string sStaffName = "";
                        sStaffName = tEngine.GetStaffName(x);
                        bool bStaffOptedOut = false;
                        for (int i = 0; i < nNoStats.Length; i++)
                        {
                            if (x == nNoStats[i])
                                bStaffOptedOut = true;
                        }
                        if (sStaffName != "" && !bStaffOptedOut)
                        {
                            for (int i = 0; i < sSales.Length; i++)
                            {
                                string[,] sTransactionInfo = tEngines[nDayNum].GetTransactionInfo(sSales[i]);
                                if (sStaffName == tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1])
                                {
                                    // This is one of the staff member's transactions, so plot a point
                                    // Work out the value of the transaction
                                    float fTransactionValue = 0.0f;
                                    int nOfItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                                    for (int z = 1; z <= nOfItemsInTransaction; z++)
                                    {
                                        fTransactionValue += (float)Convert.ToDecimal(sTransactionInfo[z, 2]);
                                    }
                                    fTransactionValue = TillEngine.TillEngine.FixFloatError(fTransactionValue);
                                    fCumulativeTotal += fTransactionValue;
                                    fCumulativeTotal = TillEngine.TillEngine.FixFloatError(fCumulativeTotal);
                                    int yPos = this.Height - (this.Height / 8) - (int)Math.Round(fCumulativeTotal / fAmountEachYPixel, 0);
                                    string stime = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[0].Split(' ')[1].Replace(':', '.');
                                    int nMinutesTotal = 0;
                                    nMinutesTotal += (Convert.ToInt32(stime.Split('.')[0]) * 60) - (9 * 60);
                                    nMinutesTotal += Convert.ToInt32(stime.Split('.')[1]);
                                    int xPos = (int)Math.Round(nMinutesTotal / fAmountEachXPixel) + (this.Width / 8);
                                    g.DrawLine(new Pen(cPieColours[nCurrentPieColour], 3.0f), pLastPoint, new Point(xPos, yPos));
                                    pLastPoint = new Point(xPos, yPos);
                                }
                            }
                            if (fStaffSales[x] > 0.0f)
                            {
                                g.DrawString(tEngine.GetStaffName(x), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(cPieColours[nCurrentPieColour]), new PointF((float)nNameLeft, 0.0f));
                                nNameLeft += (int)g.MeasureString(tEngine.GetStaffName(x), new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width;
                                nCurrentPieColour++;
                                if (nCurrentPieColour >= cPieColours.Length)
                                    nCurrentPieColour = 0;
                            }
                        }
                    }
                }
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(this.Width / 8, this.Height / 8));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point(this.Width - (this.Width / 8), this.Height - (this.Height / 8)));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), pBottomLeft, new Point((this.Width / 8) - 15, this.Height - (this.Height / 8)));
                g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(this.Width / 8, this.Height / 8), new Point((this.Width / 8) - 15, this.Height / 8));
                g.DrawString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - g.MeasureString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, pBottomLeft.Y - g.MeasureString(tEngine.CurrencySymbol.ToString() + "0.00", new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2));
                g.DrawString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - g.MeasureString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, (this.Height / 8) - g.MeasureString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(fTotalAmount), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2));
                int nDiff = ((this.Width / 4) * 3) / nOfHoursToShow;
                for (int t = 0; t <= 9; t++)
                {
                    g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point((this.Width / 8) + (nDiff * t), pBottomLeft.Y), new Point((this.Width / 8) + (nDiff * t), pBottomLeft.Y + 15));
                    g.DrawString((t + 9).ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 8) + (nDiff * t) - (g.MeasureString((9 + t).ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2), this.Height - (this.Height / 8) + 20));
                }
                g.DrawString("Hour", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - 15, this.Height - 50));
                g.DrawString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(this.Width / 8 - g.MeasureString("Amount", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width - 15, (this.Height / 8) - 50));
                g.DrawString("Cumulative Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF((this.Width / 2) - (g.MeasureString("Hourly Sales", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Width / 2), g.MeasureString("ThomasIsMyName", new Font(GTill.Properties.Settings.Default.sFontName, 20.0f)).Height));
                for (int t = 0; t <= 9; t++)
                {
                    g.DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour, 2.0f), new Point(pBottomLeft.X - 15, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10))), new Point(pBottomLeft.X, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10))));
                    g.DrawString(tEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay((fTotalAmount / 10) * t), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f), new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(pBottomLeft.X - 15 - (int)g.MeasureString(tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay((fTotalAmount / 10) * t), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Width, pBottomLeft.Y - (t * (((this.Height / 4) * 3) / 10)) - (g.MeasureString(tEngine.CurrencySymbol.ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 16.0f)).Height / 2)));

                }

        }

    }
}
