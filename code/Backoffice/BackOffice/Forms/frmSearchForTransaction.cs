using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BackOffice
{
    public partial class frmSearchForTransaction : Form
    {
        enum WordType {PeriodIndicator, Date, PartDate, Unknown};

        public frmSearchForTransaction()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Attempt to parse what the user has input
            string sInput = "";

            // Remove useless characters and excessive spaces
            sInput = sInput.Replace(",","");
            sInput = sInput.Replace("  ", " ");
            sInput = sInput.Replace(".", "");

            // Put into uppercase
            sInput = sInput.ToUpper();

            // Split into words
            string[] sSplit = sInput.Split(' ');

            WordType[] words = new WordType[sSplit.Length];

            for (int i = 0; i < words.Length; i++)
            {
                if (sSplit[i] == "BEFORE" ||
                    sSplit[i] == "BETWEEN" ||
                    sSplit[i] == "AFTER" ||
                    sSplit[i] == "PRIOR")
                {
                    words[i] = WordType.PeriodIndicator;
                }
                else if 
                   (sSplit[i] == "MONDAY" ||
                    sSplit[i] == "TUESDAY" ||
                    sSplit[i] == "WEDNESDAY" ||
                    sSplit[i] == "THURSDAY" ||
                    sSplit[i] == "FRIDAY" ||
                    sSplit[i] == "SATURDAY" ||
                    sSplit[i] == "SUNDAY" ||
                    sSplit[i] == "LAST")

                {
                    words[i] = WordType.PartDate;
                }
            }
        }
    }
}
