using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmWeeklySalesSummary : ScalableForm
    {
        StockEngine sEngine;

        CListBox lbWeekNum;
        CListBox lbLastYear;
        CListBox lbThisYear;
        CListBox lbWeekCommencing;
        Label lbAverage;

        public frmWeeklySalesSummary(ref StockEngine se)
        {
            sEngine = se;
            this.Size = new Size(700, 590);
            this.SurroundListBoxes = true;

            lbWeekNum = new CListBox();
            lbWeekNum.Location = new Point(10, 31);
            lbWeekNum.Size = new Size(50, 500);
            lbWeekNum.BorderStyle = BorderStyle.None;
            lbWeekNum.KeyDown +=new KeyEventHandler(lbLastYear_KeyDown);
            lbWeekNum.SelectedIndexChanged +=new EventHandler(lbLastYear_SelectedIndexChanged);
            this.Controls.Add(lbWeekNum);
            AddMessage("WEEK", "Week", new Point(10, 10));

            lbLastYear = new CListBox();
            lbLastYear.Location = new Point(60, 31);
            lbLastYear.Size = new Size(150, 500);
            lbLastYear.BorderStyle = BorderStyle.None;
            lbLastYear.KeyDown += new KeyEventHandler(lbLastYear_KeyDown);
            lbLastYear.SelectedIndexChanged += new EventHandler(lbLastYear_SelectedIndexChanged);
            lbLastYear.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbLastYear);
            AddMessage("LY", "Last Year", new Point(60, 10));

            lbThisYear = new CListBox();
            lbThisYear.Location = new Point(210, 31);
            lbThisYear.Size = new Size(150, 500);
            lbThisYear.BorderStyle = BorderStyle.None;
            lbThisYear.KeyDown +=new KeyEventHandler(lbLastYear_KeyDown);
            lbThisYear.RightToLeft = RightToLeft.Yes;
            lbThisYear.SelectedIndexChanged +=new EventHandler(lbLastYear_SelectedIndexChanged);
            this.Controls.Add(lbThisYear);
            AddMessage("TY", "This Year", new Point(210, 10));

            lbWeekCommencing = new CListBox();
            lbWeekCommencing.Location = new Point(400, 31);
            lbWeekCommencing.Size = new Size(this.ClientSize.Width - 10 - lbWeekCommencing.Left, 500);
            lbWeekCommencing.BorderStyle = BorderStyle.None;
            lbWeekCommencing.KeyDown +=new KeyEventHandler(lbLastYear_KeyDown);
            lbWeekCommencing.SelectedIndexChanged +=new EventHandler(lbLastYear_SelectedIndexChanged);
            this.Controls.Add(lbWeekCommencing);
            AddMessage("WC", "Week Commencing", new Point(360, 10));

            lbAverage = new Label();
            lbAverage.Location = new Point(lbWeekNum.Left, lbWeekNum.Top + lbWeekNum.Height);
            lbAverage.AutoSize = true;
            this.Controls.Add(lbAverage);

            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Weekly Sales Summary";

            LoadStats();
            //lbWeekNum.SelectedIndex = sEngine.WeekCalc(DateTime.Now.Day.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString()) - 1;
        }

        private DateTime GetStartOfWeekDate()
        {
            DateTime dt = DateTime.Now;
            while (dt.DayOfWeek != DayOfWeek.Sunday)
                dt = dt.AddDays(-1);
            return dt;
        }

        void LoadStats()
        {
            DateTime dtStartOfWeek = GetStartOfWeekDate();

            string sLastYearWeeek = "";
            string sThisYearWeek = "";
            try
            {
                sEngine.GetWeeklySales(1, dtStartOfWeek.Year, ref sThisYearWeek);
                sEngine.GetWeeklySales(1, dtStartOfWeek.Year - 1, ref sLastYearWeeek);
            }
            catch
            {
                ;
            }
            int nLastDiff = 0;
            int nThisDiff = 0;
            if (sLastYearWeeek != "" && sThisYearWeek != "")
            {
                DateTime dLastYear = DateTime.Parse(FormatDate(sLastYearWeeek));
                DateTime dThisYear = DateTime.Parse(FormatDate(sThisYearWeek));
                if (dLastYear.Day - dThisYear.Day >= 5)
                {
                    nLastDiff -= 1;
                }
                else if (dThisYear.Day - dLastYear.Day >= 5)
                {
                    nThisDiff -= 1;
                }
            }
            decimal dPercTotal = 0;
            for (int i = 1; i <= 53; i++)
            {
                sLastYearWeeek = "";
                sThisYearWeek = "";
                lbWeekNum.Items.Add(i.ToString());
                lbLastYear.Items.Add(FormatMoneyForDisplay(sEngine.GetWeeklySales(i + nLastDiff, (dtStartOfWeek.Year - 1), ref sLastYearWeeek)));
                lbThisYear.Items.Add(FormatMoneyForDisplay(sEngine.GetWeeklySales(i + nThisDiff, dtStartOfWeek.Year, ref sThisYearWeek)));
                lbWeekCommencing.Items.Add(FormatDate(sLastYearWeeek) + " & " + FormatDate(sThisYearWeek));
                if (lbLastYear.Items[i-1].ToString() != "" && lbThisYear.Items[i-1].ToString() != "")
                {
                    decimal dLastYearWeek = Convert.ToDecimal(lbLastYear.Items[i-1].ToString());
                    decimal dThisYearWeek = Convert.ToDecimal(lbThisYear.Items[i-1].ToString());
                    decimal dPercentage = 0;
                    if (dLastYearWeek != 0)
                        dPercentage = (100 / dLastYearWeek) * (dThisYearWeek - dLastYearWeek);

                    if (dPercentage > -99)
                    {
                        dPercTotal += dPercentage;
                    }

                    if (dPercentage >= 0)
                    {
                        if (dPercentage < 10)
                        {
                            lbWeekCommencing.Items[i - 1] += " - 0" + FormatMoneyForDisplay(dPercentage) + "% up";
                        }
                        else
                        {
                            lbWeekCommencing.Items[i - 1] += " - " + FormatMoneyForDisplay(dPercentage) + "% up";
                        }
                    }
                    else
                    {
                        dPercentage *= -1;
                        if (dPercentage < 10)
                        {
                            lbWeekCommencing.Items[i - 1] += " - 0" + FormatMoneyForDisplay(dPercentage) + "% down";
                        }
                        else
                        {
                            lbWeekCommencing.Items[i - 1] += " - " + FormatMoneyForDisplay(dPercentage) + "% down";
                        }

                    }
                }
            }
            lbWeekNum.SelectedIndex = sEngine.WeekCalc(dtStartOfWeek.Day.ToString() + "/" + dtStartOfWeek.Month.ToString() + "/" + dtStartOfWeek.Year.ToString()) + nThisDiff;
            try
            {
                decimal dLastYearTotal = 0;
                decimal dThisYearTotal = 0;

                for (int i = 0; i < lbWeekNum.SelectedIndex; i++)
                {
                    dLastYearTotal += Convert.ToDecimal(lbLastYear.Items[i].ToString());
                    dThisYearTotal += Convert.ToDecimal(lbThisYear.Items[i].ToString());

                    decimal dPercentage = ((100 / dLastYearTotal) * dThisYearTotal) - 100;
                    dPercentage = Math.Round(dPercentage, 2);

                    decimal dCorrectedPercentage = dPercentage;
                    if (dCorrectedPercentage < 0)
                        dCorrectedPercentage *= -1;

                    lbAverage.Text = "The sales for the Year To Date (to the end of last week) are " + FormatMoneyForDisplay(dCorrectedPercentage);

                    // Let's be optimistic (>= 0 means up!)
                    if (dPercentage >= 0)
                    {
                        lbAverage.Text += "% up on last year.";
                    }
                    else
                    {
                        lbAverage.Text += "% down on last year.";
                    }
                }
            }
            catch
            {
                lbAverage.Text = "Unable to calculate any figures";
            }
        }

        string FormatDate(string sDate)
        {
            try
            {
                string sYear = sDate[0].ToString() + sDate[1].ToString() + sDate[2].ToString() + sDate[3].ToString();
                string sMonth = sDate[4].ToString() + sDate[5].ToString();
                string sDay = sDate[6].ToString() + sDate[7].ToString();
                return sDay + "/" + sMonth + "/" + sYear;
            }
            catch
            {
                return "";
            }
        }

        void lbLastYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbLastYear.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbThisYear.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbWeekNum.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbWeekCommencing.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void lbLastYear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

    }
}
