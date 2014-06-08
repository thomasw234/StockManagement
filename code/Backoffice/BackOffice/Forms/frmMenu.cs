using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmMenu : ScalableForm
    {
        StockEngine sEngine;
        char[] cKeys = { '1', '2', '3', '4', '=', '5', '6', 'Z', 'Q' };
        public string shortcutString = null;

        public frmMenu(string shortcutString)
        {
            this.Disposed += new EventHandler(frmMenu_Disposed);
            sEngine = new StockEngine();
            if (!sEngine.GotEmailSupportAddress())
            {
                frmSingleInputBox fsfiGetEmail = new frmSingleInputBox("Please enter an e-mail address. Leave blank if you like.", ref sEngine);
                fsfiGetEmail.ShowDialog();
                if (fsfiGetEmail.Response != "$NONE")
                {
                    sEngine.SetEmailSupportAddress(fsfiGetEmail.Response);
                }
                
            }
            ScalableForm.TitleAddition = sEngine.CompanyName;
            SetupMenu();
            this.Text = "Main Menu";
            this.Focus();
            this.VisibleChanged += frmMenu_VisibleChanged;
            this.shortcutString = shortcutString;
        }

        void frmMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (shortcutString != null)
            {
                this.InputTextBox("CHOICE").Text = shortcutString;
                SendKeys.Send("~");
            }
        }

        void frmMenu_Disposed(object sender, EventArgs e)
        {
            sEngine.DisposeOfTables();
        }

        void SetupMenu()
        {
            this.Font = new Font(this.Font.FontFamily, 16.0f);
            this.WindowState = FormWindowState.Maximized;
            string[] sOptions = { "Stock Management", "Reports", "Order Management", "View Till Transactions", "Collect Daily Sales", "Setup", "Weekly Sales Summary","Backup to USB", "Quit" };
            if (sEngine.UpdateAvailable())
            {
                Array.Resize<String>(ref sOptions, sOptions.Length + 1);
                sOptions[sOptions.Length - 1] = sOptions[sOptions.Length - 2];
                sOptions[sOptions.Length - 2] = "View Update Details";
                Array.Resize<char>(ref cKeys, cKeys.Length + 1);
                cKeys[cKeys.Length - 1] = cKeys[cKeys.Length - 2];
                cKeys[cKeys.Length - 2] = 'U';

            }
            AddMessage("TITLE", "Select an option from the following...", new Point(this.Width / 3, this.Height / 4));
            for (int i = 0; i < sOptions.Length; i++)
            {
                AddMessage("OPTION" + i.ToString(), cKeys[i].ToString() + ".. " + sOptions[i], new Point((this.Width / 3), BelowLastControl));
                MessageLabel("OPTION" + i.ToString()).AccessibleDescription = i.ToString();
                MessageLabel("OPTION" + i.ToString()).Click += new EventHandler(OptionDoubleClick);
                if (sEngine.UpdateAvailable() && i == sOptions.Length - 2)
                {
                    MessageLabel("OPTION" + i.ToString()).ForeColor = Color.Red;
                }
            }
            AddInputControl("CHOICE", "Enter selected option: ", new Point(this.Width / 3, BelowLastControl + 50), 300);
            InputTextBox("CHOICE").KeyUp += new KeyEventHandler(Choice_KeyDown);
            InputTextBox("CHOICE").Focus();
            this.Paint += new PaintEventHandler(frmMenu_Paint);
        }

        void frmMenu_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(sEngine.CompanyName, new Font("Franklin Gothic Medium", 20.0f), new SolidBrush(Color.Black), new PointF((this.Width / 2) - (e.Graphics.MeasureString(sEngine.CompanyName, new Font("Franklin Gothic Medium", 20.0f)).Width / 2), (this.Height / 4) - 50));
        }

        void OptionDoubleClick(object sender, EventArgs e)
        {
            int nNum = Convert.ToInt32(((Label)sender).AccessibleDescription);
            InputTextBox("CHOICE").Text = cKeys[nNum].ToString();
            InputTextBox("CHOICE").Focus();
            SendKeys.Send("{ENTER}");
        }

        void Choice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && InputTextBox("CHOICE").Text.StartsWith("."))
            {
                shortcutString = InputTextBox("CHOICE").Text.Substring(2, InputTextBox("CHOICE").Text.Length - 2);
                InputTextBox("CHOICE").Text = InputTextBox("CHOICE").Text[1].ToString();
            }
            else if ((e.KeyCode == Keys.OemPeriod || InputTextBox("CHOICE").Text.Length >= 2))
                return;
            switch (InputTextBox("CHOICE").Text.ToUpper())
            {
                case "1":
                    frmSubMenu fStockMenu = new frmSubMenu(frmSubMenu.SubMenuType.Stock, ref sEngine);
                    fStockMenu.shortcutString = shortcutString;
                    fStockMenu.ShowDialog();
                    fStockMenu.Dispose();
                    break;
                case "2":
                    frmSubMenu fReportMenu = new frmSubMenu(frmSubMenu.SubMenuType.Reports, ref sEngine);
                    fReportMenu.shortcutString = shortcutString;
                    fReportMenu.ShowDialog();
                    fReportMenu.Dispose();
                    break;
                case "3":
                    frmSubMenu frmOrderMenu = new frmSubMenu(frmSubMenu.SubMenuType.Orders, ref sEngine);
                    frmOrderMenu.shortcutString = shortcutString;
                    frmOrderMenu.ShowDialog();
                    break;
                case "4":
                    frmViewTillTransactions fvtt = new frmViewTillTransactions(ref sEngine);
                    fvtt.ShowDialog();
                    break;
                case "5":
                    frmSubMenu fSettingsMenu = new frmSubMenu(frmSubMenu.SubMenuType.Setup, ref sEngine);
                    fSettingsMenu.ShowDialog();
                    break;
                case "Q":
                    this.Close();
                    break;
                case "=":
                    frmCollect fCollect = new frmCollect(ref sEngine);
                    this.Visible = false;
                    fCollect.ShowDialog();
                    this.Show();
                    break;
                case "6":
                    frmWeeklySalesSummary fwss = new frmWeeklySalesSummary(ref sEngine);
                    fwss.ShowDialog();
                    break;
                case "Z":
                    if (MessageBox.Show("Insert the USB drive to back up to now.", "Backup", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        FileManagementEngine.BackupToUSBPen(sEngine.CompanyName);
                    }
                    break;
                case "U":
                    frmUpdater fu = new frmUpdater(ref sEngine);
                    fu.ShowDialog();
                    break;
                case "-1":
                    sEngine.SendCommandToTill("OpenTillDrawer");
                    break;
            }
            if (InputTextBox("CHOICE").Text != "-")
                InputTextBox("CHOICE").Text = "";
            this.shortcutString = null;
        }

    }
}
