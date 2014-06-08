using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GTill
{
    public partial class frmConfig : Form
    {
        public frmConfig()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save settings
            Properties.Settings.Default.bAutoLowercaseItems = cbAutoLowercase.Checked;
            Properties.Settings.Default.bDoBackups = cbDoBackups.Checked;
            Properties.Settings.Default.bUseFloppyCashup = cbFloppy.Checked;
            Properties.Settings.Default.bUseVirtualPrinter = cbUseVirtualPrinter.Checked;
            Properties.Settings.Default.nLinesAfterReigsterReport = (int)nLinesAfterRegisterReport.Value;
            Properties.Settings.Default.nPriceLabelOffset = (int)nPriceLabelOffset.Value;
            Properties.Settings.Default.nPrinterCharWidth = (int)nPrinterCharWidth.Value;
            Properties.Settings.Default.nSpacesBetweenReceipts = (int)nLinesBetweenReceipt.Value;
            Properties.Settings.Default.sBackupLocation = sBackupLocation.Text;
            Properties.Settings.Default.sFloppyLocation = sFloppyDiscLocation.Text;
            Properties.Settings.Default.sPrinterOutputPort = sPrinterOutputPort.Text;
            Properties.Settings.Default.sFontName = sFontName.Text;
            Properties.Settings.Default.bAllowStats = cSalesStats.Checked;
            Properties.Settings.Default.sNoStatsAbout = tbNoStats.Text;
            Properties.Settings.Default.bWaitForShutDown = checkBox1.Checked;
            Properties.Settings.Default.Save();
            if (MessageBox.Show("For the changes to take effect you must restart GTill. Would you like to do that now?", "Restart GTill", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.Restart();
            }
            else
            {
                this.Close();
            }
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            // Load settings
            Cursor.Show();
            cbAutoLowercase.Focus();
            cbAutoLowercase.Checked = Properties.Settings.Default.bAutoLowercaseItems;
            cbDoBackups.Checked = Properties.Settings.Default.bDoBackups;
            cbFloppy.Checked = Properties.Settings.Default.bUseFloppyCashup;
            cbUseVirtualPrinter.Checked = Properties.Settings.Default.bUseVirtualPrinter;
            nLinesAfterRegisterReport.Value = Properties.Settings.Default.nLinesAfterReigsterReport;
            nPriceLabelOffset.Value = Properties.Settings.Default.nPriceLabelOffset;
            nPrinterCharWidth.Value = Properties.Settings.Default.nPrinterCharWidth;
            nLinesBetweenReceipt.Value = Properties.Settings.Default.nSpacesBetweenReceipts;
            sBackupLocation.Text = Properties.Settings.Default.sBackupLocation;
            sFloppyDiscLocation.Text = Properties.Settings.Default.sFloppyLocation;
            sPrinterOutputPort.Text = Properties.Settings.Default.sPrinterOutputPort;
            sFontName.Text = Properties.Settings.Default.sFontName;
            cSalesStats.Checked = Properties.Settings.Default.bAllowStats;
            tbNoStats.Text = Properties.Settings.Default.sNoStatsAbout;
            checkBox1.Checked = Properties.Settings.Default.bWaitForShutDown;
            cSalesStats.Focus();
        }

        private void btnPresetFloppy_Click(object sender, EventArgs e)
        {
            cbFloppy.Checked = true;
            sFloppyDiscLocation.Text = "A:";
        }

        private void btnPresetDOS_Click(object sender, EventArgs e)
        {
            cbFloppy.Checked = true;
            sFloppyDiscLocation.Text = "C:";
        }

        private void btnPresetNothing_Click(object sender, EventArgs e)
        {
            cbFloppy.Checked = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cbAutoLowercase.Checked = false;
            cbDoBackups.Checked = false;
            cbFloppy.Checked = true;
            cbUseVirtualPrinter.Checked = false;
            nLinesAfterRegisterReport.Value = 12;
            nLinesBetweenReceipt.Value = 1;
            nPriceLabelOffset.Value = 50;
            nPrinterCharWidth.Value = 40;
            sBackupLocation.Text = "F:\\till_backup";
            sFloppyDiscLocation.Text = "A:";
            sFontName.Text = "Franklin Gothic Medium";
            sPrinterOutputPort.Text = "LPT1";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbUsingDOSInsteadOfFloppy_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void cbAutoSwapBootFiles_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
