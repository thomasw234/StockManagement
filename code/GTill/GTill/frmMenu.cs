using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace GTill
{
    class frmMenu : Form
    {
        /// <summary>
        /// The possible types of menu
        /// </summary>
        public enum MenuType { MainMenu, AdminMenu };
        /// <summary>
        /// The type of menu that this instance is
        /// </summary>
        MenuType currentMenuType;
        /// <summary>
        /// The name of the shop
        /// </summary>
        string sShopName;
        /// <summary>
        /// The shop name label
        /// </summary>
        Label lblShopName;
        /// <summary>
        /// The menu title label
        /// </summary>
        Label lblMenuTitle;
        /// <summary>
        /// The form's forecolour (text colour)
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// The form's backcolour
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The listbox of menu items to choose from
        /// </summary>
        ListBox lbMenuItems;
        /// <summary>
        /// The listbox of menu item numbers
        /// </summary>
        ListBox lbMenuNumbers;
        /// <summary>
        /// The rectangle around the listboxes
        /// </summary>
        Rectangle rAroundListboxes;
        /// <summary>
        /// The current date and time label
        /// </summary>
        Label lblDateTime;
        /// <summary>
        /// The timer to update the date and time label
        /// </summary>
        Timer tmrUpdateDateTime;
        /// <summary>
        /// How long before the menu automatically closes
        /// </summary>
        int nTimeUntilTimeOut = 60;
        /// <summary>
        /// The label that shows how long before the menu is automatically closed
        /// </summary>
        Label lblTimeoutCountdown;
        /// <summary>
        /// The form to display if cash paid out is selected
        /// </summary>
        frmInput fiCashPaidOut;
        /// <summary>
        /// The form for selecting which transaction to remove (Admin Menu)
        /// </summary>
        frmLookupTransactions fltTransToRemove;
        /// <summary>
        /// The form for viewing transactions (Main Menu)
        /// </summary>
        frmLookupTransactions fltTransactions;
        /// <summary>
        /// The form for selecting the transaction to void (Main Menu)
        /// </summary>
        frmLookupTransactions fltTransToVoid;
        /// <summary>
        /// The form for selecting the receipt to reprint (Main Menu)
        /// </summary>
        frmLookupTransactions fltReprintReceipt;
        /// <summary>
        /// The form for entering refund information (Main Menu)
        /// </summary>
        frmRefund frRefund;
        /// <summary>
        /// The form for receiving money on account (Main Menu)
        /// </summary>
        frmReceiveOnAccount froaReceiveMoney;
        /// <summary>
        /// A reference to the main TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;
        /// <summary>
        /// The form to show the amount of money in the till (Main Menu)
        /// </summary>
        frmMoneyInTill fmitMoneyInTill;
        /// <summary>
        /// The form for viewing or changing the preset function keys (Main Menu)
        /// </summary>
        frmPresetKeys fpkPresets;
        /// <summary>
        /// Whether or not another form is open on top of this, if so, the countdown timer stops
        /// </summary>
        bool bOtherFormOpen = false;
        /// <summary>
        /// The name of the font to use on this form
        /// </summary>
        string sFontName;

        /// <summary>
        /// Initialises the menu
        /// </summary>
        /// <param name="mtMenuType">The type of menu to show</param>
        /// <param name="s">The size of the menu</param>
        /// <param name="t">A reference to the TillEngine</param>
        public frmMenu(MenuType mtMenuType, Size s, ref TillEngine.TillEngine t)
        {
            rAroundListboxes = new Rectangle(0, 0, 0, 0);
            this.Size = s;
            this.StartPosition = FormStartPosition.CenterScreen;
            currentMenuType = mtMenuType;
            sFontName = Properties.Settings.Default.sFontName;
            sShopName = t.ShopName;
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            this.BackColor = cFrmBackColour;
            this.ForeColor = cFrmForeColour;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ForeColor = cFrmForeColour;
            tEngine = t;
            CreateMenu(mtMenuType);
            this.Paint += new PaintEventHandler(frmMenu_Paint);
        }

        /// <summary>
        /// The form being repainted handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMenu_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), rAroundListboxes);
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour,2.0f), new Rectangle(rAroundListboxes.X - 4, rAroundListboxes.Y - 10 - lblMenuTitle.Height, rAroundListboxes.Width + 8, rAroundListboxes.Height + 14 + lblMenuTitle.Height));
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(4,4, this.Width - 8, 50));
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(8, 8, this.Width - 16, 42));
        }

        /// <summary>
        /// Sets up the menu's components and adds items to the menu
        /// </summary>
        /// <param name="mtType"></param>
        void CreateMenu(MenuType mtType)
        {
            lblShopName = new Label();
            lblShopName.ForeColor = cFrmForeColour;
            lblShopName.BackColor = cFrmBackColour;
            lblShopName.Location = new Point(10, 10);
            lblShopName.AutoSize = true;
            lblShopName.Font = new Font(sFontName, 14.0f);
            lblShopName.Text = sShopName;
            lblShopName.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblShopName);
            lblShopName.Top = (30 - lblShopName.Height / 2);

            lbMenuItems = new ListBox();
            lbMenuItems.BackColor = cFrmBackColour;
            lbMenuItems.ForeColor = cFrmForeColour;
            lbMenuItems.BorderStyle = BorderStyle.None;
            lbMenuItems.Width = (this.Width / 4) * 2;
            lbMenuItems.Height = (this.Height / 3) * 2;
            lbMenuItems.Left = (this.Width / 2) - (lbMenuItems.Width / 2) + 25;
            lbMenuItems.Top = (this.Height / 2) - (lbMenuItems.Height / 2) + lblShopName.Height + 40;
            lbMenuItems.Font = new Font(sFontName, 20.0f);
            lbMenuItems.SelectedIndexChanged += new EventHandler(lbMenuItems_SelectedIndexChanged);
            lbMenuItems.KeyDown += new KeyEventHandler(lbMenuItems_KeyDown);
            this.Controls.Add(lbMenuItems);

            lbMenuNumbers = new ListBox();
            lbMenuNumbers.BackColor = cFrmBackColour;
            lbMenuNumbers.ForeColor = cFrmForeColour;
            lbMenuNumbers.BorderStyle = BorderStyle.None;
            lbMenuNumbers.Width = 150;
            lbMenuNumbers.Height = lbMenuItems.Height;
            lbMenuNumbers.Top = lbMenuItems.Top;
            lbMenuNumbers.Left = lbMenuItems.Left - 50;
            lbMenuNumbers.Font = new Font(sFontName, 20.0f);
            this.Controls.Add(lbMenuNumbers);

            lblMenuTitle = new Label();
            lblMenuTitle.Font = new Font(sFontName, 20.0f);
            lblMenuTitle.BackColor = cFrmBackColour;
            lblMenuTitle.ForeColor = cFrmForeColour;
            lblMenuTitle.Left = lbMenuNumbers.Left;
            lblMenuTitle.Width = 50 + lbMenuItems.Width;
            lblMenuTitle.Height = 35;
            lblMenuTitle.Top = lbMenuItems.Top - 45;
            lblMenuTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblMenuTitle);

            lblDateTime = new Label();
            lblDateTime.ForeColor = cFrmForeColour;
            lblDateTime.BackColor = cFrmBackColour;
            lblDateTime.Text = DateTime.Now.ToString();
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font(sFontName, 14.0f);
            this.Controls.Add(lblDateTime);
            lblDateTime.Location = new Point(this.Width - lblDateTime.Width - 10, (30 - lblShopName.Height / 2));

            tmrUpdateDateTime = new Timer();
            tmrUpdateDateTime.Tick += new EventHandler(tmrUpdateDateTime_Tick);
            tmrUpdateDateTime.Interval = 1000;
            tmrUpdateDateTime.Enabled = true;

            lblTimeoutCountdown = new Label();
            lblTimeoutCountdown.Left = lblShopName.Left + lblShopName.Width;
            lblTimeoutCountdown.Top = lblShopName.Top;
            lblTimeoutCountdown.Width = lblDateTime.Left - lblTimeoutCountdown.Left;
            lblTimeoutCountdown.TextAlign = ContentAlignment.MiddleCenter;
            lblTimeoutCountdown.BackColor = cFrmBackColour;
            lblTimeoutCountdown.ForeColor = cFrmForeColour;
            lblTimeoutCountdown.Font = new Font(sFontName, 14.0f);
            this.Controls.Add(lblTimeoutCountdown);

            switch (mtType)
            {
                case MenuType.MainMenu:
                    lbMenuItems.Items.Add("Enable/Disable Printer");
                    lbMenuItems.Items.Add("Reprint a Receipt");
                    lbMenuItems.Items.Add("Cash Paid Out");
                    lbMenuItems.Items.Add("Void Transactions");
                    lbMenuItems.Items.Add("Look Up Transactions");
                    lbMenuItems.Items.Add("Money in Till");
                    lbMenuItems.Items.Add("Refund");
                    lbMenuItems.Items.Add("Define Preset Keys");
                    lbMenuItems.Items.Add("X-Register Report");
                    lbMenuItems.Items.Add("Received On Account");
                    lbMenuItems.Items.Add("Print Stock Levels Of A Category");
                    lbMenuItems.Items.Add("Add An Order Reminder");
                    lbMenuItems.Items.Add("Print A Barcode");
                    lbMenuItems.Items.Add("Exit Till System");
                    lblMenuTitle.Text = "Control Menu";
                    break;
                case MenuType.AdminMenu:
                    lbMenuItems.Items.Add("Remove A Transaction");
                    lbMenuItems.Items.Add("Undo Previous Transaction");
                    lbMenuItems.Items.Add("Print Receipt Header");
                    lbMenuItems.Items.Add("Print Receipt Footer");
                    lbMenuItems.Items.Add("Process Files in INGNG");
                    lbMenuItems.Items.Add("About GTILL");
                    lbMenuItems.Items.Add("Empty Printer Buffer");
                    lbMenuItems.Items.Add("Configure GTILL");
                    lbMenuItems.Items.Add("Change Sales Date in REPDATA");
                    if (tEngine.DemoMode)
                    {
                        lbMenuItems.Items.Add("Disable Demonstration Mode");
                    }
                    else
                    {
                        lbMenuItems.Items.Add("Enable Demonstration Mode");
                    }
                    lblMenuTitle.Text = "Administrator Menu";
                    lbMenuItems.Items.Add("Crash GTill");
                    lbMenuItems.Items.Add("Process Card Discounts");
                    lbMenuItems.Items.Add("Send Hex Codes to Printer");
                    lbMenuItems.Items.Add("Print Register Report From Backup");
                    break;
            }

            for (int i = 0; i < lbMenuItems.Items.Count; i++)
            {
                string sToAdd = (i+1).ToString();
                if (i < 9)
                    sToAdd = " " + sToAdd;
                sToAdd += ".";
                lbMenuNumbers.Items.Add(sToAdd);
            }

            rAroundListboxes = new Rectangle(lbMenuNumbers.Left - 2, lbMenuNumbers.Top - 2, 50 + lbMenuItems.Width + 4, lbMenuNumbers.Height + 4);
            lbMenuItems.SelectedIndex = 0;
            
        }

        /// <summary>
        /// The handler for when a key is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbMenuItems_KeyDown(object sender, KeyEventArgs e)
        {
            nTimeUntilTimeOut = 60;
            lblTimeoutCountdown.Text = "";
            if (e.KeyCode == Keys.Escape)
                this.Close();
            else if (e.KeyCode == Keys.Enter)
            {
                if (currentMenuType == MenuType.MainMenu)
                {
                    switch (lbMenuItems.SelectedIndex)
                    {
                        case 0:
                            // Toggle printer status
                            tEngine.TogglePrinterStatus();
                            this.Close();
                            break;
                        case 1:
                            //Reprint a receipt
                            fltReprintReceipt = new frmLookupTransactions(new Size(this.Width, this.Height - 56), ref tEngine, new Point(this.Left, this.Top + 56), "REPRINT_RECEIPT");
                            fltReprintReceipt.Show();
                            fltReprintReceipt.FormClosing += new FormClosingEventHandler(fltReprintReceipt_FormClosing);
                            bOtherFormOpen = true;
                            break;
                        case 2:
                            // Cash paid out
                            fiCashPaidOut = new frmInput(frmInput.FormType.CashPaidOut, new Point(this.Left, this.Height + this.Top - 150), new Size(this.Width, 150), new string[0]);
                            fiCashPaidOut.Show();
                            fiCashPaidOut.FormClosing += new FormClosingEventHandler(fiCashPaidOut_FormClosing);
                            bOtherFormOpen = true;
                            break;
                        case 3:
                            // Void Transaction
                            fltTransToVoid = new frmLookupTransactions(new Size(this.Width, this.Height - 56), ref tEngine, new Point(this.Left, this.Top + 56), "VOID_TRANSACTION");
                            fltTransToVoid.Show();
                            fltTransToVoid.FormClosing += new FormClosingEventHandler(fltTransToVoid_FormClosing);
                            bOtherFormOpen = true;
                            break;
                        case 4:
                            // Look up transactions
                            fltTransactions = new frmLookupTransactions(new Size(this.Width, this.Height - 56), ref tEngine, new Point(this.Left, this.Top + 56), null);
                            fltTransactions.Show();
                            bOtherFormOpen = true;
                            fltTransactions.FormClosing += new FormClosingEventHandler(fltTransactions_FormClosing);
                            break;
                        case 5:
                            // Money In Till
                            fmitMoneyInTill = new frmMoneyInTill(ref tEngine, new Point(this.Left, this.Top + 56), new Size(this.Width, this.Height - 56));
                            fmitMoneyInTill.Show();
                            bOtherFormOpen = true;
                            fmitMoneyInTill.FormClosing += new FormClosingEventHandler(fmitMoneyInTill_FormClosing);
                            break;
                        case 6:
                            // Refund
                            frRefund = new frmRefund(new Size(this.Width, this.Height - 56), new Point(this.Left, this.Top + 56), ref tEngine);
                            frRefund.Show();
                            bOtherFormOpen = true;
                            frRefund.FormClosing += new FormClosingEventHandler(frRefund_FormClosing);
                            break;
                        case 7:
                            // Define Preset Keys
                            fpkPresets = new frmPresetKeys(new Point(this.Left, this.Top + 56), new Size(this.Width, this.Height - 56), ref tEngine);
                            fpkPresets.Show();
                            fpkPresets.FormClosing += new FormClosingEventHandler(fpkPresets_FormClosing);
                            bOtherFormOpen = true;
                            break;
                        case 8:
                            // Print a register report
                            tEngine.PrintRegisterReport();
                            break;
                        case 9:
                            // Receive On Account
                            froaReceiveMoney = new frmReceiveOnAccount(new Point(this.Left, this.Top + this.Height - 150), new Size(this.Width, 150), ref tEngine);
                            bOtherFormOpen = true;
                            froaReceiveMoney.FormClosing += new FormClosingEventHandler(froaReceiveMoney_FormClosing);
                            break;
                        case 10:
                            // Print Stock Levels
                            frmCategorySelect fcSelect = new frmCategorySelect(ref tEngine);
                            fcSelect.ShowDialog();
                            if (fcSelect.SelectedCategory != "NONE_SELECTED")
                            {
                                tEngine.PrintStockLevelsOfCategory(fcSelect.SelectedCategory);
                            }
                            break;
                        case 11:
                            frmAddToOrder fato = new frmAddToOrder(ref tEngine);
                            fato.ShowDialog();
                            break;
                        case 13:
                            Application.ExitThread();
                            //Application.Exit();
                            break;
                        case 12:
                            tmrUpdateDateTime.Enabled = false;
                            frmSearchForItemV2 fsfBarcode = new frmSearchForItemV2(ref tEngine);
                            fsfBarcode.ShowDialog();
                            if (fsfBarcode.GetItemBarcode() != "NONE_SELECTED")
                            {
                                tEngine.PrintBarcode(fsfBarcode.GetItemBarcode(), false);
                            }
                            tmrUpdateDateTime.Enabled = true;
                            break;
                    }
                }
                else if (currentMenuType == MenuType.AdminMenu)
                {
                    switch (lbMenuItems.SelectedIndex)
                    {
                        case 0:
                            // Remove a transaction
                            fltTransToRemove = new frmLookupTransactions(new Size(this.Width, this.Height - 56), ref tEngine, new Point(this.Left, this.Top + 56), "REMOVE_TRANSACTION");
                            try
                            {
                                fltTransToRemove.Show();
                                fltTransToRemove.FormClosing += new FormClosingEventHandler(fltTransToRemove_FormClosing);
                                bOtherFormOpen = true;
                            }
                            catch
                            {
                                // User backed out of removal
                                ;
                            }
                            break;
                        case 1:
                            if (File.Exists("REPDATA_PREV.DBF") && File.Exists("TDATA_PREV.DBF") && File.Exists("THDR_PREV.DBF") && UserCertain("undo the previous transaction?"))
                            {
                                File.Delete(GTill.Properties.Settings.Default.sRepDataLoc);
                                File.Delete(GTill.Properties.Settings.Default.sTDataLoc);
                                File.Delete(GTill.Properties.Settings.Default.sTHdrLoc);
                                File.Copy("REPDATA_PREV.DBF", GTill.Properties.Settings.Default.sRepDataLoc);
                                File.Copy("TDATA_PREV.DBF", GTill.Properties.Settings.Default.sTDataLoc);
                                File.Copy("THDR_PREV.DBF", GTill.Properties.Settings.Default.sTHdrLoc);
                                tEngine.LoadTable("REPDATA");
                                tEngine.LoadTable("TDATA");
                                tEngine.LoadTable("THDR");
                                this.Close();
                            }
                            break;
                        case 2:
                            // Print receipt header
                            tEngine.PrintReceiptHeader();
                            tEngine.EmptyPrinterBuffer();
                            this.Close();
                            break;
                        case 3:
                            // Print receipt footer
                            string sDateTime = DateTime.Now.Day + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString() + ", " + DateTime.Now.TimeOfDay.Hours.ToString() + ":" + DateTime.Now.TimeOfDay.Minutes.ToString();
                            string sStaffName = tEngine.GetCurrentStaffName();
                            string sTransactionNumber = tEngine.GetNextTransactionNumber().ToString();
                            tEngine.PrintReceiptFooter(sStaffName, sDateTime, sTransactionNumber);
                            tEngine.EmptyPrinterBuffer();
                            this.Close();
                            break;
                        case 4:
                            // Process file in INGNG folder
                            if (GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy)
                            {
                                tEngine.CollectFilesFromDosDrive();
                            }
                            if (tEngine.ProcessFilesInINGNG())
                            {
                                if (MessageBox.Show("Not all changes will take effect until GTill has restarted. Restart now?", "Restart Now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    Application.Restart();
                                }
                                else
                                {
                                    MessageBox.Show("Files from INGNG have been processed");
                                }
                            }
                            this.Close();
                            break;
                        case 5:
                            // About GTill
                            frmAbout fAbt = new frmAbout();
                            fAbt.ShowDialog();
                            break;
                        case 6:
                            // Empty Printer Buffer
                            tEngine.EmptyPrinterBuffer();
                            this.Close();
                            break;
                        case 7:
                            // Show the configuration window
                            frmConfig cfg = new frmConfig();
                            Cursor.Show();
                            cfg.ShowDialog();
                            Cursor.Hide();
                            break;
                        case 8:
                            // Change sales date in REPDATA
                            frmDateInput fdi = new frmDateInput(this.Size);
                            fdi.Visible = false;
                            fdi.ShowDialog();
                            tEngine.SetDateInRepData(fdi.DateTimeInput);
                            MessageBox.Show("Date was successfully changed to " + fdi.DateTimeInput);
                            this.Close();
                            break;
                        case 9:
                            tEngine.DemoMode = !tEngine.DemoMode;
                            if (tEngine.DemoMode)
                                MessageBox.Show("Demonstration Mode has been enabled. No standard transactions will be saved whilst in demonstration mode, although refunds, transaction removals, void transactions etc will still be counted!! Please remember to disable it when you have finished.");
                            else
                                MessageBox.Show("Demonstration Mode has been disabled.");
                            this.Close();
                            break;
                        case 10:
                            throw new NotSupportedException("Test error!");
                            break;
                        case 11:
                            tEngine.ApplyCreditCardDiscs();
                            this.Close();
                            tEngine.PrintRegisterReport();
                            break;
                        case 12:
                            frmSingleInputBox fsiGetHex = new frmSingleInputBox("Enter 1 byte hex codes, seperated by spaces (e.g. 01 AF F2)");
                            tmrUpdateDateTime.Enabled = false;
                            fsiGetHex.ShowDialog();
                            if (fsiGetHex.Response != "$NONE")
                            {
                                tEngine.PrintTestBarcode(fsiGetHex.Response);
                            }
                            break;
                        case 13:
                            frmBackupDates fbd = new frmBackupDates();
                            fbd.ShowDialog();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// The handler for when the reprint receipt form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fltReprintReceipt_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
            if (fltReprintReceipt.TransactionNumber != "CANCELLED")
            {
                int nToReprint = Convert.ToInt32(fltReprintReceipt.TransactionNumber);
                tEngine.ReprintTransactionReceipt(nToReprint);
            }
        }

        /// <summary>
        /// The handler for when the receive money on account form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void froaReceiveMoney_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
        }

        /// <summary>
        /// The handler for when the preset keys form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fpkPresets_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
        }

        /// <summary>
        /// The handler for when the refund form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frRefund_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
        }

        /// <summary>
        /// The handler for when the void a transaction form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fltTransToVoid_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
            if (fltTransToVoid.TransactionNumber != "CANCELLED")
            {
                if (UserCertain("void transaction number " + fltTransToVoid.TransactionNumber))
                {
                    int nToRemove = Convert.ToInt32(fltTransToVoid.TransactionNumber);
                    tEngine.VoidTransaction(nToRemove, tEngine.CurrentStaffNumber);
                }
                else
                {
                    MessageBox.Show("Void transaction cancelled");
                }
            }
        }

        /// <summary>
        /// The handler for when the money in the till form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fmitMoneyInTill_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
        }

        /// <summary>
        /// Gets whether of not the user is certain that they want to perform
        /// </summary>
        /// <param name="sTask"></param>
        /// <returns></returns>
        bool UserCertain(string sTask)
        {
            if (MessageBox.Show("Are you sure you want to " + sTask, "GTill For Windows", MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            else
                return false;
        }

        /// <summary>
        /// The transaction to remove form closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fltTransToRemove_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sToRemove = fltTransToRemove.TransactionNumber;
            if (sToRemove != "CANCELLED")
            {
                ErrorHandler.LogMessage(tEngine.GetStaffName(tEngine.CurrentStaffNumber) + " is decided whether or not to remove transaction number " + sToRemove);
                if (MessageBox.Show("Are you sure you wish to remove transaction number " + sToRemove + "? This removal will be logged.", "Final Chance!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    int nToRemove = Convert.ToInt32(sToRemove);
                    tEngine.RemoveTransactionFromDatabases(nToRemove);
                    ErrorHandler.LogMessage(tEngine.GetStaffName(tEngine.CurrentStaffNumber) + " removed transaction number " + sToRemove);
                }
                else
                {
                    ErrorHandler.LogMessage(tEngine.GetStaffName(tEngine.CurrentStaffNumber) + " cancelled the removal of transaction number " + sToRemove);
                    MessageBox.Show("Transaction Removal cancelled");
                }
            }
        }

        /// <summary>
        /// The lookup transactions form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fltTransactions_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
        }
        
        /// <summary>
        /// The cash paid out form closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fiCashPaidOut_FormClosing(object sender, FormClosingEventArgs e)
        {
            bOtherFormOpen = false;
            string sCode = fiCashPaidOut.sGetDataToReturn();
            if (sCode != "CANCELLED")
            {
                tEngine.OpenTillDrawer(false);
                tEngine.PayCashOut(sCode);
                this.Close();
            }
        }

        /// <summary>
        /// Updates the date and time at the top of the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmrUpdateDateTime_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = DateTime.Now.ToString();
            if (!bOtherFormOpen)
                nTimeUntilTimeOut -= 1;
            if (nTimeUntilTimeOut == 0)
                this.Close();
            if (nTimeUntilTimeOut < 50)
                lblTimeoutCountdown.Text = "Time unti menu closes: " + nTimeUntilTimeOut.ToString();
            else
                lblTimeoutCountdown.Text = "";
        }

        /// <summary>
        /// When the user presses up or down, both listboxes move up or down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbMenuItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbMenuNumbers.SelectedIndex = lbMenuItems.SelectedIndex;
        }
    }
}
