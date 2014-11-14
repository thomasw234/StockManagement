using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;

namespace BackOffice.Forms
{
    class frmCommissionPeriods : ScalableForm
    {
        // Allows the user to select which length of time the report should be from

        private ListBox lbPeriods;
        public bool Chosen = false;

        public frmCommissionPeriods()
        {
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            this.Text = "Commission Report Period";

            lbPeriods = new ListBox();
            lbPeriods.Items.Add("Daily");
            lbPeriods.Items.Add("Weekly");
            lbPeriods.Items.Add("Monthly");
            lbPeriods.Items.Add("Yearly");
            lbPeriods.Items.Add("Custom");
            lbPeriods.SelectedIndex = 0;
            this.Controls.Add(lbPeriods);

            lbPeriods.Location = new System.Drawing.Point(10, 10);
            lbPeriods.Size = new System.Drawing.Size(150, 100);

            this.Size = new System.Drawing.Size(175, 130);

            lbPeriods.KeyDown += new KeyEventHandler(lbPeriods_KeyDown);
            lbPeriods.BorderStyle = BorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        void lbPeriods_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Chosen = true;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Get the period that was chosen
        /// </summary>
        public Period ChosenPeriod
        {
            get
            {
                switch (lbPeriods.SelectedIndex)
                {
                    case 0:
                        return Period.Daily;
                        break;
                    case 1:
                        return Period.Weekly;
                        break;
                    case 2:
                        return Period.Monthly;
                        break;
                    case 3:
                        return Period.Yearly;
                        break;
                    case 4:
                        return Period.Other;
                        break;
                }
                return Period.Other;
            }
        }

        private string dateAsDDMMYY(DateTime dt)
        {
            string toReturn = dt.Day.ToString();
            if (dt.Day < 10)
                toReturn = "0" + toReturn;

            if (dt.Month < 10)
                toReturn += "0";
            toReturn += dt.Month.ToString();

            if (dt.AddYears(-2000).Year < 10)
                toReturn += "0";
            toReturn += dt.Year.ToString()[2].ToString() + dt.Year.ToString()[3].ToString();


            return toReturn;
        }

        /// <summary>
        /// Commission sales report requires today's date, and the date to start the report from, so this function generates the start date based on the chosen option
        /// </summary>
        /// <returns>The start date for the report</returns>
        public string getStartPeriodDate()
        {
            return dateAsDDMMYY(this.getStartPeriodDateAsDT());

        }

        /// <summary>
        /// An internal method that calculates the start date, and returns as a DateTime
        /// </summary>
        /// <returns>The start date for this report</returns>
        private DateTime getStartPeriodDateAsDT()
        {
            DateTime dt = DateTime.Now;
            switch (this.ChosenPeriod)
            {
                case Period.Daily:
                    dt = dt.AddDays(-1);
                    break;
                case Period.Weekly:
                    dt = dt.AddDays(-7);
                    // Roll back until the start of the previous week
                    while (dt.DayOfWeek != DayOfWeek.Monday)
                        dt = dt.AddDays(-1);
                    break;
                case Period.Monthly:
                    dt = dt.AddMonths(-1);
                    // Roll back until the start of the previous month
                    while (dt.Day != 1)
                        dt = dt.AddDays(-1);
                    break;
                case Period.Yearly:
                // No need to actually go to last year, more like Year To Date    
                // dt = dt.AddYears(-1);
                    // Roll back until the start of the previous year
                    while (dt.Month != 1)
                        dt = dt.AddMonths(-1);
                    while (dt.Day != 1)
                        dt = dt.AddDays(-1);
                    break;
            }
            return dt;
        }

        /// <summary>
        /// Gets the end date for this report, by getting the start date and adding the relevant amount of time
        /// </summary>
        /// <returns>The end date for this report</returns>
        public string getEndDate()
        {
            DateTime dt = getStartPeriodDateAsDT();
            switch (this.ChosenPeriod)
            {
                case Period.Daily:
                    dt = dt.AddDays(1);
                    break;
                case Period.Weekly:
                    dt = dt.AddDays(7);
                    break;
                case Period.Monthly:
                    dt = dt.AddMonths(1);
                    break;
                case Period.Yearly:
                    dt = DateTime.Now; // Because it's Year To Date really
                    break;
            }

            return dateAsDDMMYY(dt);
        }
    }
}
