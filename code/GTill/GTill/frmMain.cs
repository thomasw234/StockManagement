using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TillEngine;
using System.Runtime.InteropServices;
using ModernListBox;
using System.IO;

namespace GTill
{
    class frmMain : Form
    {
        #region GlobalVariables

        /// <summary>
        /// Creates a new TillEngine
        /// </summary>
        TillEngine.TillEngine tillEngine = new TillEngine.TillEngine();
        /// <summary>
        /// The display state that the form is currently in
        /// </summary>
        FormState fsCurrentFormState;
        /// <summary>
        /// The number of seconds since the mouse was last moved
        /// </summary>
        int nTimeSinceLastMouseMove = 0;
        /// <summary>
        /// Times the time since the last mouse move
        /// </summary>
        Timer tmrMouseMove;
        /// <summary>
        /// Checks for files in INGNG every 60 seconds
        /// </summary>
        Timer tmrFilesToUpdate;
        /// <summary>
        /// Whether or not the input box needs to seek the user's attention by flashing
        /// </summary>
        bool bFlashTextInputBox = false;

        // Start of Loading

        /// <summary>
        /// The background colour of the form
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The font colour of the form
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// A label showing the build number of the program
        /// </summary>
        Label lblBuildNum;
        /// <summary>
        /// A custom control that shows which database is currently loading
        /// </summary>
        ModernListBox.ModernListBox mlbLoading;
            
        // End of Loading

        // Start of MainSection
        
        /// <summary>
        /// The shop name at the top of the screen
        /// </summary>
        Label lblTitle;
        /// <summary>
        /// A description of what each column does (Item Number, Description, Price)
        /// </summary>
        Label[] lblDisplayDesc;
        /// <summary>
        /// Lines across the form seperating it into 3 parts
        /// </summary>
        Label[] lblLines;
        /// <summary>
        /// Tells the user what to do (Enter I.D. Number for example)
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// The textbox that the user enter's their I.D. or a shortcut key into
        /// </summary>
        TextBox tbInput;
        /// <summary>
        /// Some of the shortcut keys that can be entered are here (P = Reprint Receipt etc)
        /// </summary>
        Label lblHelp;
        /// <summary>
        /// Shows the current date and time at the bottom of the screen
        /// </summary>
        Label lblDateTime;
        /// <summary>
        /// A timer that updates the date and time at the bottom of the screen
        /// </summary>
        Timer tmrUpdatelblDateTime;
        /// <summary>
        /// Shows the name of the till at the bottom of the screen
        /// </summary>
        Label lblTillID;
        /// <summary>
        /// Shows the name of the current member of staff at the bottom of the screen
        /// </summary>
        Label lblStaffName;
        /// <summary>
        /// Shows the number of the next/current transaction at the bottom of the screen
        /// </summary>
        Label lblTransactionNumber;
        /// <summary>
        /// Shows whether the printer is enabled or not at the bottom of the screen
        /// </summary>
        Label lblPrinterStatus;
        /// <summary>
        /// The password input form
        /// </summary>
        frmPasswordInput fpiPassword;
        /// <summary>
        /// The menu form
        /// </summary>
        frmMenu fMenu;
        /// <summary>
        /// The form to change the payment method after a transaction has completed
        /// </summary>
        frmPaymentInput fpiChangePaymentMethod;
        /// <summary>
        /// The name of the font to use on the form
        /// </summary>
        string sFontName;
        
        // End of MainSection

        // Start of Lookup

        /// <summary>
        /// Tells the user what to do for a lookup
        /// </summary>
        Label lblLookupInstruction;
        /// <summary>
        /// The textbox that the user enters the code to lookup into
        /// </summary>
        TextBox tbLookupStockCode;
        /// <summary>
        /// Shows a description of the item that has been looked up if it has been found
        /// </summary>
        Label[] lblLookupDescription;
        
        // End of Lookup

        // Start of Transaction

        /// <summary>
        /// The custom control that shows the details of the current transaction
        /// </summary>
        TransactionView tvTransaction;
        /// <summary>
        /// The textbox that the user enters or scans the barcode of the next item into
        /// </summary>
        TextBox tbItemCode;
        /// <summary>
        /// The label showing the number of the next item
        /// </summary>
        Label lblNextItemNumber;
        /// <summary>
        /// The label showing the current total of the transaction
        /// </summary>
        Label lblCurrentTotal;
        /// <summary>
        /// The textbox where the description of an item is input
        /// </summary>
        TextBox tbItemDescInput;
        /// <summary>
        /// The textbox where the price of an item is input
        /// </summary>
        TextBox tbItemPriceInput;
            
        /// <summary>
        /// The form that gets the payment method from the user
        /// </summary>
        frmPaymentInput fpiGetPayment;
        /// <summary>
        /// The form that gets the amount to multiply by or discount by
        /// </summary>
        frmInput fiGetMulOrDisc;
        /// <summary>
        /// The form that looks up products
        /// </summary>
        frmSearchForItemV2 fsfiLookup;
        /// <summary>
        /// The lookup form for when an unrecognised barcode is entered
        /// </summary>
        frmSearchForItemV2 fsfiPartialBCode;
        /// <summary>
        /// The lookup for when the user is doing a product lookup
        /// </summary>
        frmSearchForItemV2 fsfiLookupLookup;
        /// <summary>
        /// The form showing all the possible accounts to charge
        /// </summary>
        frmAccSel fasChargeToAcc;

        /// <summary>
        /// An array of strings containing the non-item elements to show on the transaction display (such as payment methods)
        /// </summary>
        string[] sNonItemDisplayArray;
        /// <summary>
        /// Whether or not the shift key has been pressed (for the function key shortcuts)
        /// </summary>
        bool bShiftKeyDown = false;

        string sLookupBarcode = "";

        // End of Transaction

        #endregion

        /// <summary>
        /// The possible states of the form
        /// </summary>
        enum FormState { Loading, LoginScreen, Transaction, Payment, Lookup };

        /// <summary>
        /// Initialises the main form
        /// </summary>
        public frmMain()
        {
            // Set the form's font
            sFontName = Properties.Settings.Default.sFontName;
            // Change the form state to loading
            ChangeFormState(FormState.Loading);
            // Show the form
            this.Show();
            // Hide the cursor
            Cursor.Hide();
            // Load the till controls
            LoadTill(false);
            // Change the staff number to 0
            tillEngine.CurrentStaffNumber = 0;
            // Set the staff name (till name when staff number = 0)
            lblStaffName.Text = "ID = " + tillEngine.GetCurrentStaffName();
            // Add event handler for form resizing
            this.Resize += new EventHandler(frmMain_Resize);
            // Add event handler for form getting focus
            this.GotFocus += new EventHandler(frmMain_GotFocus);
            // Add event handler for this having a keydown
            this.KeyDown += new KeyEventHandler(frmMain_KeyDown);
            // Add event handler for this having mouse movement
            this.MouseMove += new MouseEventHandler(frmMain_MouseMove);
            // Add event handler for the ID input textbox losing focus
            tbInput.LostFocus += new EventHandler(tbInput_LostFocus);
            // Add event handler for the ID input textbox getting focus
            tbInput.GotFocus += new EventHandler(tbInput_GotFocus);
        }

        /// <summary>
        /// Handles the ID input textbox getting focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbInput_GotFocus(object sender, EventArgs e)
        {
            bFlashTextInputBox = false;
            tbInput.BackColor = cFrmBackColour;
        }

        /// <summary>
        /// Handles the ID input textbox losing focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbInput_LostFocus(object sender, EventArgs e)
        {
            if (fsCurrentFormState == FormState.LoginScreen)
            {
                tbInput.BackColor = Color.Red;
                bFlashTextInputBox = true;
            }
        }

        /// <summary>
        /// Handles the form having mouse movement (shows the mouse)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Show();
            nTimeSinceLastMouseMove = 0;
        }

        /// <summary>
        /// Handles the form having a key pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (fpiChangePaymentMethod != null && fpiChangePaymentMethod.Visible == true)
            {
                fpiChangePaymentMethod.Focus();
            }
            else if (fpiGetPayment != null && fpiGetPayment.Visible == true)
            {
                fpiGetPayment.Focus();
            }

            else if (fpiPassword != null && fpiPassword.Visible == true)
            {
                fpiPassword.Focus();
            }
            else if (fsfiLookup != null && fsfiLookup.Visible == true)
            {
                fsfiLookup.Focus();
            }
            else if (fsfiLookupLookup != null && fsfiLookupLookup.Visible == true)
            {
                fsfiLookupLookup.Focus();
            }
            else if (fsfiPartialBCode != null && fsfiPartialBCode.Visible == true)
            {
                fsfiPartialBCode.Focus();
            }
            else if (fsCurrentFormState == FormState.LoginScreen)
            {
                tbInput.Focus();
            }
            else if (fsCurrentFormState == FormState.Transaction)
            {
                tbItemCode.Focus();
            }
            else
            {
                tillEngine.StoreTransactionForLater();
                MessageBox.Show("Sorry, a problem has occured. To get back to the current transaction, re-enter your I.D. and press Enter");
                ChangeFormState(FormState.LoginScreen);
            }
        }

        /// <summary>
        /// Closes all open menus when the main form recaptures focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMain_GotFocus(object sender, EventArgs e)
        {
            if (fMenu != null)
            {
                fMenu.Close();
                fMenu.Dispose();
            }
            if (fpiPassword != null)
            {
                fpiPassword.Close();
                fpiPassword.Dispose();
            }
        }

        /// <summary>
        /// Redraws the form when the form size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMain_Resize(object sender, EventArgs e)
        {
            if (fsCurrentFormState == FormState.LoginScreen)
            {
                this.Text = "Please wait, resizing...";
                this.Refresh();
                DisposeOfAllFormControls();
                LoadTill(true);
                this.Text = "";
            }
        }

        /// <summary>
        /// Changes the layout of the form based on the new state
        /// </summary>
        /// <param name="stateToGoTo">The new form state</param>
        void ChangeFormState(FormState stateToGoTo)
        {
            fsCurrentFormState = stateToGoTo;
            try
            {
                tmrFilesToUpdate.Enabled = true;
            }
            catch
            {
                ;
            }
            // Show the loading screen (with list of dBase files to load)
            if (stateToGoTo == FormState.Loading)
            {
                this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                //this.Size = new Size(1366, 768);
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;

                cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
                cFrmForeColour = Properties.Settings.Default.cFrmForeColour;

                mlbLoading = new ModernListBox.ModernListBox(29.0f, 14.0f, false);
                mlbLoading.BackColor = cFrmBackColour;
                mlbLoading.ForeColor = cFrmForeColour;
                mlbLoading.Font = new Font(sFontName, 10.0f);
                mlbLoading.Location = new Point(0, (this.Height / 2) - 175);
                mlbLoading.Size = new Size(this.Width, 450);
                mlbLoading.Visible = false;
                this.Controls.Add(mlbLoading);

                this.BackColor = cFrmBackColour;

                lblBuildNum = new Label();
                lblBuildNum.Location = new Point(0, 0);
                lblBuildNum.Font = new Font(sFontName, 20.0f);
                if (File.Exists("buildNum.txt"))
                {
                    System.IO.TextReader tr = new System.IO.StreamReader("buildNum.txt");
                    lblBuildNum.Text = "Build " + tr.ReadLine();
                    tr.Close();
                }
                else
                    lblBuildNum.Text = "buildNum.txt not found";
                lblBuildNum.Width = this.Width;
                lblBuildNum.Height = 50;
                lblBuildNum.ForeColor = cFrmForeColour;
                lblBuildNum.BackColor = cFrmBackColour;
                lblBuildNum.AutoSize = true;
                this.Controls.Add(lblBuildNum);
                lblBuildNum.BringToFront();
            }
            // Shows the main screen which waits for the user to input their I.D. number.
            else if (stateToGoTo == FormState.LoginScreen)
            {
                tbItemCode.Visible = false;
                fsCurrentFormState = FormState.LoginScreen;
                lblInstruction.Visible = true;
                tbInput.Visible = true;
                tbInput.Left = lblInstruction.Width;
                tbInput.Top = lblInstruction.Top;
                tbInput.Height = lblInstruction.Height;
                tbInput.Width = 100;
                tbInput.Focus();
                tbInput.Text = "";
                lblHelp.Left = 0;
                lblHelp.Visible = true;
                lblLookupInstruction.Visible = false;
                tbLookupStockCode.Visible = false;
                tbItemCode.Visible = false;
                lblNextItemNumber.Visible = false;
                lblCurrentTotal.Visible = false;
                ReSortOutStatusLabels(true);
                tbInput.Focus();
                if (tillEngine.PrinterEnabled)
                {
                    lblPrinterStatus.Text = "Printer On";
                    lblPrinterStatus.Tag = "INVERTED";
                    lblPrinterStatus.BackColor = cFrmForeColour;
                    lblPrinterStatus.ForeColor = cFrmBackColour;
                }
                else
                {
                    lblPrinterStatus.Text = "Printer Off";
                    lblPrinterStatus.Tag = "";
                    lblPrinterStatus.ForeColor = cFrmForeColour;
                    lblPrinterStatus.BackColor = cFrmBackColour;
                }
            }
            // Hides the I.D. entry textbox, and shows a textbox asking for the next barcode to add to the transaction
            else if (stateToGoTo == FormState.Transaction)
            {
                tvTransaction.Visible = true;
                sNonItemDisplayArray = new string[0];
                ClearTransactionArea();
                fsCurrentFormState = FormState.Transaction;
                tillEngine.SetupNewTransaction();
                lblInstruction.Visible = false;
                tbInput.Visible = false;
                lblHelp.Visible = false;
                tbItemCode.Visible = true;
                lblNextItemNumber.Visible = true;
                lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();
                lblTransactionNumber.Text = "Transaction = " + (tillEngine.GetNextTransactionNumber()).ToString();
                tbItemDescInput.Visible = false;
                tbItemPriceInput.Visible = false;
                ReSortOutStatusLabels(true);
                SetFontSizeOnStatusLabels(true);
                tbItemCode.Focus();
            }
            // Asks for the barcode to lookup
            else if (stateToGoTo == FormState.Lookup)
            {
                tvTransaction.Visible = false;
                fsCurrentFormState = FormState.Lookup;
                ClearTransactionArea();
                tillEngine.CurrentStaffNumber = 0;
                lblStaffName.Text = "ID = " + tillEngine.GetCurrentStaffName();
                lblLookupInstruction.Visible = true;
                tbLookupStockCode.Visible = true;
                tbLookupStockCode.Text = "";
                tbItemCode.Visible = false;
                tbLookupStockCode.Focus();
            }
        }

        /// <summary>
        /// Initialises controls and places them on the form
        /// </summary>
        /// <param name="bJustRedrawing">Whether or not the dBase files have already been loaded</param>
        void LoadTill(bool bJustRedrawing)
        {
            float fStandardFontSize = Properties.Settings.Default.fMainScreenFontSize;
            if (!bJustRedrawing)
            {
                mlbLoading.AddItem("ACCSTAT.DBF");
                mlbLoading.AddItem("EMAIL.DBF");
                mlbLoading.AddItem("COMMISSION.DBF");
                mlbLoading.AddItem("CUSTREC.DBF");
                mlbLoading.AddItem("DETAILS.DBF");
                mlbLoading.AddItem("MULTIDAT.DBF");
                mlbLoading.AddItem("MULTIHDR.DBF");
                mlbLoading.AddItem("OFFERS.DBF");
                mlbLoading.AddItem("PRESETS.DBF");
                mlbLoading.AddItem("REPDATA.DBF");
                mlbLoading.AddItem("STAFF.DBF");
                mlbLoading.AddItem("STKLEVEL.DBF");
                mlbLoading.AddItem("STOCK.DBF");
                mlbLoading.AddItem("TDATA.DBF");
                mlbLoading.AddItem("THDR.DBF");
                mlbLoading.AddItem("TILLCAT.DBF");
                mlbLoading.AddItem("TORDERSU.DBF");
                mlbLoading.AddItem("VAT.DBF");
                if (DateTime.Now.DayOfYear == 28 && DateTime.Now.Month == 1)
                {
                    mlbLoading.AddItem("BIRTHDAY.DBF");
                }
                mlbLoading.MoveDown();
                mlbLoading.MoveUp();
                this.Refresh();

                // Fade the form loading controls in

                mlbLoading.ForeColor = cFrmBackColour;
                mlbLoading.Visible = false;
                this.Refresh();
                int nR = cFrmBackColour.R, nG = cFrmBackColour.G, nB = cFrmBackColour.B;
                int nDiffR = (cFrmForeColour.R - cFrmBackColour.R) / 10;
                int nDiffG = (cFrmForeColour.G - cFrmBackColour.G) / 10;
                int nDiffB = (cFrmForeColour.B - cFrmBackColour.B) / 10;

                for (int i = 0; i < 10; i++)
                {
                    nR += nDiffR;
                    nG += nDiffG;
                    nB += nDiffB;
                    Color cToChangeTo = Color.FromArgb(nR, nG, nB);
                    mlbLoading.ForeColor = cToChangeTo;
                    this.Refresh();
                    //System.Threading.Thread.Sleep(1);
                }

                nR = cFrmForeColour.R; nG = cFrmForeColour.G; nB = cFrmForeColour.B;
                nDiffR = (cFrmBackColour.R - cFrmForeColour.R) / 10;
                nDiffG = (cFrmBackColour.G - cFrmForeColour.G) / 10;
                nDiffB = (cFrmBackColour.B - cFrmForeColour.B) / 10;
                
                // End of fade

                // Load each of the tables

                bool bAllTablesLoaded = true;
                string sFailedToLoad = "";
                if (!tillEngine.LoadTable("ACCSTAT"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "ACCSTAT ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("EMAIL"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "EMAIL ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("COMMISSION"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "COMMISSION ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("CUSTREC"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "CUSTREC ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("DETAILS"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "DETAILS ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("MULTIDATA"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "MULTIDATA ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("MULTIHEADER"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "MULTIHEADER ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("OFFERS"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "OFFERS ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("PRESETS"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "PRESETS ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("REPDATA"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "REPDATA ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("STAFF"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "STAFF ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("STKLEVEL"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "STKLEVEL ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("STOCK"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "STOCK ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("TDATA"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "TDATA ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("THDR"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "THDR ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("TILLCAT"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "TILLCAT ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("TORDERSUG"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "TORDERSUG ";
                }
                mlbLoading.MoveDown();

                if (!tillEngine.LoadTable("VAT"))
                {
                    bAllTablesLoaded = false;
                    mlbLoading.ForeColor = Color.Red;
                    sFailedToLoad += "VAT ";
                }

                if (DateTime.Now.DayOfYear == 28 && DateTime.Now.Month == 1)
                {
                    mlbLoading.MoveDown();
                    System.Threading.Thread.Sleep(3000);
                }
                
                if (!bAllTablesLoaded) // One or more of the tables failed whilst loading
                {
                    MessageBox.Show("The following dBase files failed to load: " + sFailedToLoad + ". Please ensure they exist within the same directory as GTill.exe (with the correct file name), and that they are not corrupted (try importing into Microsoft Access).");
                    GTill.ErrorHandler.LogError("The following dBase files failed to load: " + sFailedToLoad + ". Please ensure they exist within the same directory as GTill.exe (with the correct file name), and that they are not corrupted (try importing into Microsoft Access).");
                    throw new Exception("The following dBase files failed to load: " + sFailedToLoad + ". Please ensure they exist within the same directory as GTill.exe (with the correct file name), and that they are not corrupted (try importing into Microsoft Access).");
                }

                // Fade the form loading controls out

                for (int i = 0; i < 10; i++)
                {
                    nR += nDiffR;
                    nG += nDiffG;
                    nB += nDiffB;
                    Color cToChangeTo = Color.FromArgb(nR, nG, nB);
                    mlbLoading.ForeColor = cToChangeTo;
                    lblBuildNum.ForeColor = cToChangeTo;
                    this.Refresh();
                    System.Threading.Thread.Sleep(1);
                }

                // End of fade out

                // If the date is required (first thing in a morning normally), then show the date input form
                if (tillEngine.RepdataNeedsDate())
                {
                    frmDateInput f = new frmDateInput(this.Size);
                    f.Visible = false;
                    f.ShowDialog();
                    tillEngine.SetDateInRepData(f.DateTimeInput);
                    if (Properties.Settings.Default.bUsingDosInsteadOfFloppy)
                    {
                        tillEngine.CollectFilesFromDosDrive();
                    }
                    if (GTill.Properties.Settings.Default.bDoBackups)
                    {
                        BackupEngine.FullBackup("Pre_INGNG_Processed");
                    }
                    tillEngine.ProcessFilesInINGNG();
                    if (GTill.Properties.Settings.Default.bDoBackups)
                    {
                        BackupEngine.FullBackup("Post_INGNG_Processed");
                    }
                    if (File.Exists("receipts.txt"))
                        File.Delete("receipts.txt");
                    if (File.Exists(Properties.Settings.Default.sOUTGNGDir + "\\prev_receipts.txt"))
                        File.Delete(Properties.Settings.Default.sOUTGNGDir + "\\prev_receipts.txt");
                    if (File.Exists(Properties.Settings.Default.sOUTGNGDir + "\\receipts.txt"))
                        File.Delete(Properties.Settings.Default.sOUTGNGDir + "\\receipts.txt");
                }
                if (Properties.Settings.Default.bDoBackups)
                {
                    frnBackup f = new frnBackup();
                    f.Size = this.Size;
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = this.Location;
                    f.FormBorderStyle = FormBorderStyle.None;
                    f.ShowDialog();
                }
            }

            // Initialise all controls and place them

            lblTitle = new Label();
            lblTitle.Text = tillEngine.ShopName.Replace(" & ", " && ");
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.Width, Convert.ToInt32(fStandardFontSize * 2));
            if (!bJustRedrawing)
            lblTitle.Font = new Font(sFontName, fStandardFontSize , FontStyle.Bold);
            lblTitle.ForeColor = cFrmForeColour;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblDisplayDesc = new Label[3];
            for (int i = 0; i < lblDisplayDesc.Length; i++)
            {
                lblDisplayDesc[i] = new Label();
                lblDisplayDesc[i].Top = (this.Height / 9);
                if (!bJustRedrawing)
                    lblDisplayDesc[i].Font = new Font(sFontName, fStandardFontSize);
                lblDisplayDesc[i].ForeColor = cFrmForeColour;
                lblDisplayDesc[i].BackColor = cFrmBackColour;
                lblDisplayDesc[i].AutoSize = true;
                this.Controls.Add(lblDisplayDesc[i]);
            }
            lblDisplayDesc[0].Left = 0;
            lblDisplayDesc[1].Left = (this.Width / 17) * 2;
            lblDisplayDesc[2].Left = ((this.Width / 17) * 15) - Properties.Settings.Default.nPriceLabelOffset;
            lblDisplayDesc[0].Text = "Line";
            lblDisplayDesc[1].Text = "Description";
            lblDisplayDesc[2].Text = "Price";

            lblLines = new Label[2];
            for (int i = 0; i < lblLines.Length; i++)
            {
                lblLines[i] = new Label();
                lblLines[i].ForeColor = cFrmForeColour;
                lblLines[i].Font = new Font(sFontName, fStandardFontSize);
                lblLines[i].Left = -2;
                lblLines[i].Width = this.Width + 4;
                lblLines[i].Height = 25;
                lblLines[i].AutoSize = true;
                this.Controls.Add(lblLines[i]);
                while (lblLines[i].Width < this.Width)
                    lblLines[i].Text += "_";
            }
            lblLines[0].Top = lblDisplayDesc[0].Top + lblDisplayDesc[0].Height;
            lblLines[1].Top = (this.Height / 9) * 7;

            lblInstruction = new Label();
            lblInstruction.Text = "Enter Identity Number:";
            if (!bJustRedrawing)
                lblInstruction.Font = new Font(sFontName, fStandardFontSize);
            lblInstruction.ForeColor = cFrmForeColour;
            lblInstruction.BackColor = cFrmBackColour;
            lblInstruction.Left = 0;
            lblInstruction.Top = lblLines[1].Top + lblLines[0].Height + 5;
            lblInstruction.AutoSize = true;
            this.Controls.Add(lblInstruction);

            tbInput = new TextBox();
            tbInput.BackColor = cFrmBackColour;
            tbInput.ForeColor = cFrmForeColour;
            if (!bJustRedrawing)
                tbInput.Font = new Font(sFontName, fStandardFontSize);
            tbInput.BorderStyle = BorderStyle.None;
            tbInput.Left = lblInstruction.Width;
            tbInput.Top = lblInstruction.Top;
            tbInput.Height = lblInstruction.Height;
            tbInput.Width = 25;
            tbInput.Visible = false;
            tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
            tbInput.KeyUp += new KeyEventHandler(tbInput_KeyUp);
            this.Controls.Add(tbInput);

            lblHelp = new Label();
            lblHelp.Text = "(P = Reprint Receipt, A = Email Addresses, L = Price Lookup + Stock Levels, D = Description Search)";
            if (!bJustRedrawing)
                lblHelp.Font = new Font(sFontName, fStandardFontSize);
            lblHelp.Top = (this.Height / 9) * 8;
            lblHelp.Left = 0;
            lblHelp.Visible = false;
            lblHelp.AutoSize = true;
            lblHelp.ForeColor = cFrmForeColour;
            lblHelp.BackColor = cFrmBackColour;
            this.Controls.Add(lblHelp);

            lblDateTime = new Label();
            tmrUpdatelblDateTime = new Timer();
            tmrUpdatelblDateTime.Enabled = true;
            tmrUpdatelblDateTime.Interval = 60000;
            tmrUpdatelblDateTime.Tick += new EventHandler(tmrUpdatelblDateTime_Tick);
            lblDateTime.Left = 5;
            lblDateTime.BackColor = cFrmForeColour;
            lblDateTime.ForeColor = cFrmBackColour;
            lblDateTime.AutoSize = true;
            if (!bJustRedrawing)
                lblDateTime.Font = new Font(sFontName, 10.0f);
            string sDateTime = "";
            sDateTime += DateTime.Now.DayOfWeek.ToString();
            sDateTime += " ";
            sDateTime += DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
            sDateTime += ", " + DateTime.Now.Hour + ":";
            if (DateTime.Now.Minute < 10)
                sDateTime += "0";
            sDateTime += DateTime.Now.Minute;
            lblDateTime.Text = sDateTime;
            lblDateTime.Top = this.Height - lblDateTime.Height - 50;
            lblDateTime.Tag = "INVERTED";
            this.Controls.Add(lblDateTime);

            lblTillID = new Label();
            lblTillID.Text = "Till = " + tillEngine.TillName;
            lblTillID.BackColor = cFrmForeColour;
            lblTillID.ForeColor = cFrmBackColour;
            lblTillID.Top = lblDateTime.Top;
            lblTillID.Left = lblDateTime.Left + lblDateTime.Width + 5;
            if (!bJustRedrawing)
                lblTillID.Font = new Font(sFontName, 10.0f);
            lblTillID.AutoSize = true;
            lblTillID.Tag = "INVERTED";
            this.Controls.Add(lblTillID);

            lblStaffName = new Label();
            lblStaffName.BackColor = cFrmForeColour;
            lblStaffName.ForeColor = cFrmBackColour;
            lblStaffName.Top = lblDateTime.Top;
            lblStaffName.Left = lblTillID.Left + lblTillID.Width + 5;
            if (!bJustRedrawing)
                lblStaffName.Font = new Font(sFontName, 10.0f);
            lblStaffName.Text = "ID = " + tillEngine.GetCurrentStaffName();
            lblStaffName.AutoSize = true;
            lblStaffName.SizeChanged += new EventHandler(lblStaffName_SizeChanged);
            lblStaffName.Tag = "INVERTED";
            this.Controls.Add(lblStaffName);

            lblTransactionNumber = new Label();
            lblTransactionNumber.Top = lblDateTime.Top;
            lblTransactionNumber.Left = lblStaffName.Left + lblStaffName.Width + 5;
            lblTransactionNumber.AutoSize = true;
            lblTransactionNumber.ForeColor = cFrmBackColour;
            lblTransactionNumber.BackColor = cFrmForeColour;
            lblTransactionNumber.Text = "Transaction = " + (tillEngine.GetNextTransactionNumber()).ToString();
            if (!bJustRedrawing)
                lblTransactionNumber.Font = new Font(sFontName, 10.0f);
            lblTransactionNumber.Tag = "INVERTED";
            this.Controls.Add(lblTransactionNumber);

            lblPrinterStatus = new Label();
            lblPrinterStatus.Top = lblDateTime.Top;
            lblPrinterStatus.Left = lblTransactionNumber.Left + lblTransactionNumber.Width + 5;
            lblPrinterStatus.AutoSize = true;
            if (!bJustRedrawing)
                lblPrinterStatus.Font = new Font(sFontName, 10.0f);
            if (tillEngine.PrinterEnabled)
            {
                lblPrinterStatus.Text = "Printer On";
                lblPrinterStatus.Tag = "INVERTED";
                lblPrinterStatus.BackColor = cFrmForeColour;
                lblPrinterStatus.ForeColor = cFrmBackColour;
            }
            else
            {
                lblPrinterStatus.Text = "Printer Off";
                lblPrinterStatus.Tag = "";
                lblPrinterStatus.ForeColor = cFrmForeColour;
                lblPrinterStatus.BackColor = cFrmBackColour;
            }
            this.Controls.Add(lblPrinterStatus);

            lblLookupInstruction = new Label();
            lblLookupInstruction.Text = "Enter Stock Code:";
            lblLookupInstruction.Top = lblInstruction.Top + lblInstruction.Height;
            lblLookupInstruction.Left = 0;
            lblLookupInstruction.AutoSize = true;
            if (!bJustRedrawing)
                lblLookupInstruction.Font = new Font(sFontName, fStandardFontSize);
            lblLookupInstruction.ForeColor = cFrmForeColour;
            lblLookupInstruction.BackColor = cFrmBackColour;
            lblLookupInstruction.Visible = true;
            this.Controls.Add(lblLookupInstruction);

            tbLookupStockCode = new TextBox();
            tbLookupStockCode.Top = lblLookupInstruction.Top;
            tbLookupStockCode.Left = lblLookupInstruction.Width;
            tbLookupStockCode.BorderStyle = BorderStyle.None;
            tbLookupStockCode.BackColor = cFrmBackColour;
            tbLookupStockCode.ForeColor = cFrmForeColour;
            tbLookupStockCode.Width = 250;
            tbLookupStockCode.Visible = true;
            tbLookupStockCode.Font = lblLookupInstruction.Font;
            tbLookupStockCode.KeyDown += new KeyEventHandler(tbLookupStockCode_KeyDown);
            this.Controls.Add(tbLookupStockCode);

            lblLookupDescription = new Label[7];
            for (int i = 0; i < lblLookupDescription.Length; i++)
            {
                lblLookupDescription[i] = new Label();
                lblLookupDescription[i].Top = ((this.Height / 9) * 3) + (int)(i * (Properties.Settings.Default.fMainScreenFontSize * 2));
                if (!bJustRedrawing)
                    lblLookupDescription[i].Font = new Font(sFontName, fStandardFontSize);
                lblLookupDescription[i].BackColor = cFrmBackColour;
                lblLookupDescription[i].ForeColor = cFrmForeColour;
                lblLookupDescription[i].Left = 20;
                lblLookupDescription[i].AutoSize = true;
                lblLookupDescription[i].Visible = false;
                this.Controls.Add(lblLookupDescription[i]);
            }
            lblLookupDescription[0].Text = "Description: ";
            lblLookupDescription[1].Text = "Price: ";
            lblLookupDescription[2].Text = "Barcode: ";
            lblLookupDescription[3].Text = "Stock Level: ";
            lblLookupDescription[4].Text = "Quantity On Order: ";
            lblLookupDescription[5].Text = "Last Delivery Date: ";
            lblLookupDescription[6].Text = "Enter 'R' to set a Reminder for this item to be ordered.";

            tbItemCode = new TextBox();
            tbItemCode.Top = (this.Height / 9) * 6;
            tbItemCode.Left = lblDisplayDesc[1].Left;
            tbItemCode.Width = 250;
            tbItemCode.MaxLength = 13;
            tbItemCode.BackColor = cFrmBackColour;
            tbItemCode.ForeColor = cFrmForeColour;
            if (!bJustRedrawing)
                tbItemCode.Font = new Font(sFontName, 18.0f);
            tbItemCode.Visible = false;
            tbItemCode.BorderStyle = BorderStyle.Fixed3D;
            tbItemCode.KeyDown += new KeyEventHandler(tbItemCode_KeyDown);
            tbItemCode.KeyUp += new KeyEventHandler(tbItemCode_KeyUp);
            this.Controls.Add(tbItemCode);

            lblNextItemNumber = new Label();
            lblNextItemNumber.Top = tbItemCode.Top;
            lblNextItemNumber.Left = 0;
            lblNextItemNumber.Width = lblDisplayDesc[1].Left;
            lblNextItemNumber.Height = tbItemCode.Height;
            if (!bJustRedrawing)
                lblNextItemNumber.Font = new Font(sFontName, 18.0f);
            lblNextItemNumber.BackColor = cFrmBackColour;
            lblNextItemNumber.ForeColor = cFrmForeColour;
            lblNextItemNumber.TextAlign = ContentAlignment.MiddleCenter;
            lblNextItemNumber.Visible = false;
            lblNextItemNumber.AutoSize = false;
            lblNextItemNumber.TextChanged += new EventHandler(lblNextItemNumber_TextChanged);
            this.Controls.Add(lblNextItemNumber);

            lblCurrentTotal = new Label();
            lblCurrentTotal.Left = lblDisplayDesc[2].Left;
            lblCurrentTotal.BackColor = cFrmBackColour;
            lblCurrentTotal.ForeColor = cFrmForeColour;
            if (!bJustRedrawing)
                lblCurrentTotal.Font = new Font(sFontName, 18.0f, FontStyle.Bold);
            lblCurrentTotal.Width = (this.Width - lblCurrentTotal.Left);
            lblCurrentTotal.Top = tbItemCode.Height + tbItemCode.Top;
            lblCurrentTotal.Height = lblLines[1].Top - lblCurrentTotal.Top;
            lblCurrentTotal.Visible = false;
            lblCurrentTotal.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblCurrentTotal);

            sNonItemDisplayArray = new string[0];

            tbItemDescInput = new TextBox();
            tbItemDescInput.BackColor = cFrmBackColour;
            tbItemDescInput.ForeColor = cFrmForeColour;
            if (!bJustRedrawing)
                tbItemDescInput.Font = new Font(sFontName, 18.0f);
            tbItemDescInput.Location = tbItemCode.Location;
            tbItemDescInput.Size = new Size(lblCurrentTotal.Left - tbItemDescInput.Left - 20, tbItemCode.Height);
            tbItemDescInput.BorderStyle = BorderStyle.None;
            tbItemDescInput.Visible = false;
            tbItemDescInput.KeyDown += new KeyEventHandler(tbItemDescInput_KeyDown);
            this.Controls.Add(tbItemDescInput);

            tbItemPriceInput = new TextBox();
            tbItemPriceInput.BackColor = cFrmBackColour;
            tbItemPriceInput.ForeColor = cFrmForeColour;
            tbItemPriceInput.BorderStyle = BorderStyle.None;
            if (!bJustRedrawing)
                tbItemPriceInput.Font = new Font(sFontName, 18.0f);
            tbItemPriceInput.Visible = false;
            tbItemPriceInput.Location = new Point(lblDisplayDesc[2].Left, tbItemCode.Top);
            tbItemPriceInput.Size = new Size(lblCurrentTotal.Width - 10, tbItemCode.Height);
            tbItemPriceInput.TextAlign = HorizontalAlignment.Right;
            tbItemPriceInput.KeyDown += new KeyEventHandler(tbItemPriceInput_KeyDown);
            this.Controls.Add(tbItemPriceInput);

            tvTransaction = new TransactionView();
            tvTransaction.Top = lblLines[0].Top + lblLines[0].Height;
            tvTransaction.Left = 0;
            tvTransaction.Width = this.Width;
            tvTransaction.Height = tbItemCode.Top - (lblLines[0].Height + lblLines[0].Top) - 10;
            tvTransaction.Show();
            tvTransaction.BackColor = cFrmBackColour;
            tvTransaction.ForeColor = cFrmForeColour;
            tvTransaction.Font = new Font(Properties.Settings.Default.sFontName, Properties.Settings.Default.fMainScreenFontSize);
            tvTransaction.AlignDescription = lblDisplayDesc[1].Left;
            tvTransaction.AlignNumber = lblDisplayDesc[0].Left;
            tvTransaction.AlignPrice = lblDisplayDesc[2].Left;
            tvTransaction.MouseMove +=new MouseEventHandler(frmMain_MouseMove);
            tvTransaction.GotFocus += new EventHandler(tvTransaction_GotFocus);
            this.Controls.Add(tvTransaction);
                        
            tmrMouseMove = new Timer();
            tmrMouseMove.Interval = 1000;
            tmrMouseMove.Tick += new EventHandler(tmrMouseMove_Tick);
            tmrMouseMove.Enabled = true;

            tmrFilesToUpdate = new Timer();
            tmrFilesToUpdate.Interval = 5000;
            tmrFilesToUpdate.Tick += new EventHandler(tmrFilesToUpdate_Tick);
            tmrFilesToUpdate.Enabled = true;

            mlbLoading.Dispose();
            lblBuildNum.Dispose();
            SetFontSizeOnStatusLabels(true);

            // Change the form to login state
            ChangeFormState(FormState.LoginScreen);

            // Now fade the new controls in

            int nR2 = cFrmBackColour.R, nG2 = cFrmBackColour.G, nB2 = cFrmBackColour.B;
            int nDiffR2 = (cFrmForeColour.R - cFrmBackColour.R) / 10;
            int nDiffG2 = (cFrmForeColour.G - cFrmBackColour.G) / 10;
            int nDiffB2 = (cFrmForeColour.B - cFrmBackColour.B) / 10;

            for (int i = 0; i < 10; i++)
            {
                nR2 += nDiffR2;
                nG2 += nDiffG2;
                nB2 += nDiffB2;
                Color cToChangeTo = Color.FromArgb(nR2, nG2, nB2);
                foreach (Control ctrl in this.Controls)
                {
                    // The bottom controls have the background and foreground colours swapped normally
                    if (ctrl.Tag != "INVERTED")
                        ctrl.ForeColor = cToChangeTo;
                    else
                        ctrl.BackColor = cToChangeTo;
                }
                this.Refresh();
                System.Threading.Thread.Sleep(1);
            }
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.Tag != "INVERTED")
                    ctrl.ForeColor = cFrmForeColour;
                else
                    ctrl.BackColor = cFrmForeColour;
            }
            this.Refresh();
        }

        void tbItemCode_KeyUp(object sender, KeyEventArgs e)
        {
            bSubtractHeldDown = false;
        }

        void tmrFilesToUpdate_Tick(object sender, EventArgs e)
        {
            if (tillEngine.AnyFilesToBeProcessed() && (fsCurrentFormState == FormState.LoginScreen || fsCurrentFormState == FormState.Lookup))
            {
                frmProcessingINGNG fProcess = new frmProcessingINGNG();
                tmrFilesToUpdate.Enabled = false;
                fProcess.ShowDialog();
                if (fProcess.bOkToProcess)
                {
                    if (tillEngine.ProcessFilesInINGNG())
                        Application.Restart();
                    tmrFilesToUpdate.Enabled = true;
                }
            }
            tillEngine.ProcessCommands();
        }

        /// <summary>
        /// Occurs when the mouse clicks on the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvTransaction_GotFocus(object sender, EventArgs e)
        {
            if (fsCurrentFormState == FormState.LoginScreen)
                tbInput.Focus();
            else if (fsCurrentFormState == FormState.Transaction)
                tbItemCode.Focus();
            else if (fsCurrentFormState == FormState.Lookup)
                tbLookupStockCode.Focus();
        }

        /// <summary>
        /// Checks whether or not the mouse needs to be hidden, and whether or not the ID input textbox has focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmrMouseMove_Tick(object sender, EventArgs e)
        {
            nTimeSinceLastMouseMove++;
            if (nTimeSinceLastMouseMove >= 1)
                Cursor.Hide();

            if (bFlashTextInputBox)
            {
                if (tbInput.BackColor == Color.White)
                {
                    tbInput.BackColor = Color.Red;
                }
                else
                {
                    tbInput.BackColor = Color.White;
                }
            }
        }

        /// <summary>
        /// If a barcode is being scanned in to the input box, the form automatically goes into lookup mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbInput.Text.Length > 2)
            {
                string sCurrentInput = tbInput.Text;
                ChangeFormState(FormState.Lookup);
                tbLookupStockCode.Text = sCurrentInput;
                tbLookupStockCode.SelectionStart = tbLookupStockCode.Text.Length;
                tbInput.Text = "L";
            }
        }

        /// <summary>
        /// Disposes of all form controls
        /// </summary>
        void DisposeOfAllFormControls()
        {
            do 
            {
                try
                {
                    this.Controls.Remove(this.Controls[0]);
                    this.Controls[0].Dispose();
                }
                catch
                {
                    ;
                }
            } while (this.Controls.Count > 0);
        }

        /// <summary>
        /// The item description key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void tbItemDescInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tillEngine.SetLastItemDescription(tbItemDescInput.Text);
                tbItemPriceInput.Focus();
                if (tbItemPriceInput.Top + tbItemPriceInput.Height >= lblCurrentTotal.Top - 10)
                    lblCurrentTotal.Visible = false;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tillEngine.DeleteLine(tillEngine.GetNumberOfItemsInCurrentTransaction());
                tbItemCode.Visible = false;
                tbItemDescInput.Visible = false;
                tbItemCode.Text = "";
                tbItemCode.Visible = true;
                tbItemCode.Focus();
                tbItemPriceInput.Text = "";
                tbItemPriceInput.Visible = false;
                tillEngine.DeleteLine(tillEngine.GetNumberOfItemsInCurrentTransaction());
            }
        }

        /// <summary>
        /// The price input textbox key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that was pressed</param>
        void tbItemPriceInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                try
                {
                    float fAmountInput = tillEngine.fFixFloatError(TillEngine.TillEngine.fFormattedMoneyString(tbItemPriceInput.Text));
                    if (fAmountInput > 0.0f && (fAmountInput < 10000.0f || (fAmountInput < 100000.0f && (fAmountInput % 1) == 0)))
                    {
                        tbItemPriceInput.Text = TillEngine.TillEngine.fFormattedMoneyString(tbItemPriceInput.Text).ToString();
                        tillEngine.SetLastItemPrice(fAmountInput);
                        lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();
                        tbItemCode.Text = "";
                        tbItemPriceInput.Text = "";
                        tbItemPriceInput.Visible = false;
                        tbItemDescInput.Text = "";
                        tvTransaction.bOkToDraw = true;
                        RedrawTransactions(0);
                        tbItemDescInput.Visible = false;
                        tbItemCode.Visible = true;
                        tbItemCode.Focus();
                        if (!lblCurrentTotal.Visible)
                            lblCurrentTotal.Visible = true;
                    }
                    else // When the input price is incorrect
                    {
                        tbItemPriceInput.Text = "";
                    }
                }
                catch
                {
                    tbItemPriceInput.Text = "";
                }
            }
            else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Delete  || e.KeyCode == Keys.Space) // The user trying to quit
            {
                tbItemCode.Visible = false;
                tbItemDescInput.Visible = false;
                tbItemCode.Text = "";
                tbItemCode.Visible = true;
                tbItemCode.Focus();
                tbItemPriceInput.Text = "";
                tbItemPriceInput.Visible = false;
                tillEngine.DeleteLine(tillEngine.GetNumberOfItemsInCurrentTransaction());
                tvTransaction.bOkToDraw = true;
            }
        }

        // To be used for the shortcut for card payments
        bool bSubtractHeldDown = false;

        /// <summary>
        /// Handles a key down in the next item barcode entry textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that was pressed</param>
        void tbItemCode_KeyDown(object sender, KeyEventArgs e)
        {
            RemoveOtherItemDescription("Discount of");
            if (e.KeyCode == Keys.Multiply || e.KeyCode == Keys.PageUp)
            {
                // Pay by cash
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() == 0)
                {
                    ChangeFormState(FormState.LoginScreen);
                }
                else
                {
                    RemoveOtherItemDescription("Total Due : ");
                    if (tillEngine.GetNumberOfTotalItemsInCurrentTransaction() > 1)
                        AddOtherItemDescription(tillEngine.GetNumberOfTotalItemsInCurrentTransaction().ToString() + " items|0|0|false|descalign");
                    else
                        AddOtherItemDescription("1 item|0|0|false|descalign");
                    AddOtherItemDescription("Total Due : " + FormatMoneyForDisplay(tillEngine.GetTotalAmountInTransaction()) + "|0|15|false|right");
                    tbItemCode.Visible = false;
                    lblNextItemNumber.Visible = false;
                    fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                    fpiGetPayment.Show();
                    fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                    fpiGetPayment.SelectCash();
                    tbItemCode.Visible = false;
                }
            }
            else if ((e.KeyCode == Keys.Subtract || e.KeyCode == Keys.PageDown) && !bSubtractHeldDown)
            {
                // Select payment method
                bSubtractHeldDown = true;
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() == 0)
                    ChangeFormState(FormState.LoginScreen);
                else
                {
                    RemoveOtherItemDescription("Total Due : ");
                    if (tillEngine.GetNumberOfTotalItemsInCurrentTransaction() > 1)
                        AddOtherItemDescription(tillEngine.GetNumberOfTotalItemsInCurrentTransaction().ToString() + " items|0|0|false|descalign");
                    else
                        AddOtherItemDescription("1 item|0|0|false|descalign");
                    AddOtherItemDescription("Total Due : " + FormatMoneyForDisplay(tillEngine.GetTotalAmountInTransaction()) + "|0|0|false|right");
                    tbItemCode.Visible = false;
                    lblNextItemNumber.Visible = false;
                    fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                    fpiGetPayment.Show();
                    fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                    tbItemCode.Visible = false;
                }
            }
            else if (e.KeyCode == Keys.Space)
            {
                // Store transaction into memory
                if (tbItemCode.Text.Length <= 1)
                {
                    tillEngine.StoreTransactionForLater();
                    //AddOtherItemDescription("Transaction stored|0|0|false|centre");
                    //tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray); <-- This line causes AVG to declare the program as a virus!
                    ChangeFormState(FormState.LoginScreen);
                    fsCurrentFormState = FormState.LoginScreen;
                    tvTransaction.NumberToMoveUp = 0;
                    if (tillEngine.CurrentStaffNumber == 0)
                    {
                        tillEngine.DumpTransactionToFile();
                        Application.ExitThread();
                    }
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Barcode entered
                //e.SuppressKeyPress = true;
                tvTransaction.NumberToMoveUp = 0;
                tillEngine.AddItemToTransaction(tbItemCode.Text.ToUpper());
                if (tillEngine.WasItemAddSuccessful())
                {
                    int nCategory = tillEngine.GetItemJustAdded().ItemCategory;
                    if (nCategory == 1 || nCategory == 3 || nCategory == 5 || nCategory == 6)
                    {
                        lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();
                        tbItemCode.Text = "";
                        RedrawTransactions(0);
                    }
                    else if (nCategory == 2 && tillEngine.GetItemJustAdded().Amount == 0.00)
                    {
                        tbItemCode.Visible = false;
                        tbItemDescInput.Text = tillEngine.GetItemJustAdded().Description;
                        tbItemDescInput.Visible = true;
                        tbItemPriceInput.Visible = true;
                        tbItemPriceInput.Focus();
                        tvTransaction.bOkToDraw = false;
                        if (tbItemPriceInput.Top + tbItemPriceInput.Height >= lblCurrentTotal.Top - 10)
                            lblCurrentTotal.Visible = false;
                    }
                    else if (nCategory == 2 && tillEngine.GetItemJustAdded().Amount != 0.00)
                    {
                        lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();
                        tbItemCode.Text = "";
                        RedrawTransactions(0);
                    }
                    else if (nCategory == 4)
                    {
                        tbItemCode.Visible = false;
                        tbItemDescInput.Visible = true;
                        tbItemPriceInput.Visible = true;
                        tbItemDescInput.Focus();
                        tvTransaction.bOkToDraw = false;
                    }
                }
                    // Check if it was an offer barcode
                else if (tillEngine.OfferExists(tbItemCode.Text.ToUpper()))
                {
                    tillEngine.RecordOfferReturned(tbItemCode.Text.ToUpper());
                    AddOtherItemDescription("Offer: " + tillEngine.GetOfferDesc(tbItemCode.Text.ToUpper()) + "|10|10|true|centre|");
                    tbItemCode.Text = "";
                }
                else
                {
                    // The barcode wasn't found, so do a partial barcode search
                    e.SuppressKeyPress = false;
                    if (tbItemCode.Text != "/" && tbItemCode.Text.TrimEnd(' ').Length > 2)
                    {
                        fsfiPartialBCode = new frmSearchForItemV2(ref tillEngine);
                        fsfiPartialBCode.Show();
                        fsfiPartialBCode.FormClosing += new FormClosingEventHandler(fsfiPartialBCode_FormClosing);
                        fsfiPartialBCode.CheckForPartialBarcodeFromScanner(tbItemCode.Text);
                    }
                }
            }
            else if (e.KeyCode == Keys.Oemtilde)
            {
                // Multiply, actually the apostrophe key on a UK Keyboard Layout!
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() != 0) // If there is an item in the transaction
                {
                    Point pStartPosition = new Point(this.Left, this.Top + lblLines[1].Top + 20);
                    fiGetMulOrDisc = new frmInput(frmInput.FormType.MultiplicationAmount, pStartPosition, new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), new string[0]);
                    fiGetMulOrDisc.Show();
                    fiGetMulOrDisc.FormClosing += new FormClosingEventHandler(fiGetMulOrDisc_FormClosing);
                    tbItemCode.Visible = false;
                    lblNextItemNumber.Visible = false;
                }
                else
                    e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Oem5)
            {
                // Open till drawer
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() == 0)
                {
                    tbInput.Text = "";
                    ChangeFormState(FormState.LoginScreen);
                    fpiPassword = new frmPasswordInput(tillEngine.GetPasswords(0), "Please enter the password to open the till drawer:", this.Size, "TILL_DRAWER");
                    fpiPassword.ShowDialog();
                    if (fpiPassword.GetResult() == frmPasswordInput.PasswordDialogResult.Correct)
                    {
                        tillEngine.OpenTillDrawer(true);
                    }
                }
                else
                    e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Oem7)
            {
                // Discount (#) on UK Keyboard
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() != 0 && tillEngine.GetItemJustAdded().Quantity > 0)
                {
                    Point pStartPosition = new Point(this.Left, this.Top + lblLines[1].Top + 20);
                    fiGetMulOrDisc = new frmInput(frmInput.FormType.DiscountAmount, pStartPosition, new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), new string[0]);
                    fiGetMulOrDisc.Show();
                    fiGetMulOrDisc.FormClosing += new FormClosingEventHandler(fiGetMulOrDisc_FormClosing);
                    tbItemCode.Visible = false;
                    lblNextItemNumber.Visible = false;
                }
                else
                    e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Escape)
            {
                // Delete an item!
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() > 0)
                {
                    Point pStartPosition = new Point(this.Left, this.Top + lblLines[1].Top + 20);
                    fiGetMulOrDisc = new frmInput(frmInput.FormType.DeleteLineNum, pStartPosition, new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), new string[0]);
                    fiGetMulOrDisc.Show();
                    fiGetMulOrDisc.FormClosing += new FormClosingEventHandler(fiGetMulOrDisc_FormClosing);
                    tbItemCode.Visible = false;
                    lblNextItemNumber.Visible = false;
                }
                else // if there are no items
                {
                    ChangeFormState(FormState.LoginScreen);
                }
            }
            else if (e.KeyCode == Keys.OemQuestion || e.KeyCode == Keys.Divide)
            {
                // Product Lookup
                tbItemCode.Visible = false;
                fsfiLookup = new frmSearchForItemV2(ref tillEngine);
                fsfiLookup.Show();
                fsfiLookup.FormClosing += new FormClosingEventHandler(fsfiLookup_FormClosing);
            }
            else if (e.KeyCode == Keys.Oem8)
            {
                // Move display up!
                e.SuppressKeyPress = true;
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() != 0)
                {
                    if (e.Shift)
                        tvTransaction.NumberToMoveUp -= 1;
                    else
                        tvTransaction.NumberToMoveUp++;
                    tvTransaction.Refresh();
                }
            }
            else if (e.KeyCode == Keys.Oem1)
            {
                // Repeat Line!
                if (tillEngine.GetNumberOfItemsInCurrentTransaction() != 0)
                {
                    tillEngine.RepeatLastItem();
                    RedrawTransactions(0);
                    e.SuppressKeyPress = true;
                }
                else
                    e.SuppressKeyPress = true;
            }
            #region FunctionKeys
            // The function keys have been pressed
            else if (e.KeyCode == Keys.F1)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F1");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF1");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F2");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF2");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F3");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF3");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F4)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F4");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF4");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F5");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF5");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F6)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F6");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF6");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F7)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F7");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF7");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F8)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F8");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF8");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F9)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F9");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF9");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F10)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F10");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF10");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F11");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF11");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.KeyCode == Keys.F12)
            {
                if (!bShiftKeyDown)
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("F12");
                else
                    tbItemCode.Text = tillEngine.sBarcodeFromFunctionKey("SF12");
                SendKeys.Send("{ENTER}");
                bShiftKeyDown = false;
            }
            else if (e.Shift)
                bShiftKeyDown = !bShiftKeyDown;
            #endregion
            else if (e.Control && tillEngine.GetNumberOfItemsInCurrentTransaction() == 0)
            {
                // The control menu (as long as there are no items in the transaction)
                fpiPassword = new frmPasswordInput(tillEngine.GetPasswords(0), "Please enter the menu password", this.Size, "MAIN_MENU");
                fpiPassword.Show();
                fpiPassword.FormClosing += new FormClosingEventHandler(fpiPassword_FormClosing);
            }
            else if (e.Alt && tillEngine.GetNumberOfItemsInCurrentTransaction() == 0)
            {
                // The Administrator menu (as long as there are no items in the transaction)
                Cursor.Show();
                tmrMouseMove.Enabled = false;
                fpiPassword = new frmPasswordInput(tillEngine.GetPasswords(1), "Please enter the Administrator menu password", this.Size, "ADMIN_MENU");
                fpiPassword.ShowDialog();
                if (fpiPassword.GetResult() == frmPasswordInput.PasswordDialogResult.Correct)
                {
                    fMenu = new frmMenu(frmMenu.MenuType.AdminMenu, this.Size, ref tillEngine);
                    fMenu.ShowDialog();
                    ChangeFormState(FormState.LoginScreen);
                    tmrMouseMove.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Incorrect Password");
                    tmrMouseMove.Enabled = true;
                }
            }
            else
            {
                bShiftKeyDown = false;
            }
        }

        /// <summary>
        /// Handles the password form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fpiPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fpiPassword.GetResult() == frmPasswordInput.PasswordDialogResult.Correct) // If the entered password was correct
            {
                string sShowCode = fpiPassword.GetOpeningReason();
                switch (sShowCode)
                {
                    case "MAIN_MENU":
                        // Show the main menu
                        fMenu = new frmMenu(frmMenu.MenuType.MainMenu, this.Size, ref tillEngine);
                        fMenu.Show();
                        fMenu.FormClosing += new FormClosingEventHandler(fMenu_FormClosing);
                        break;
                    case "CASHUP":
                        // Start the cashup procedure
                        fpiPassword.Dispose();
                        tillEngine.ApplyCreditCardDiscs();
                        if (MessageBox.Show("Do you want to print a register report?", "Cashing up", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            tillEngine.PrintRegisterReport();
                        }
                        BackupEngine.FullBackup("Pre_Cash_Up");
                        tillEngine.CashUp();
                        BackupEngine.FullBackup("Post_Cash_Up");
                        if (GTill.Properties.Settings.Default.bUseFloppyCashup)
                        {
                            #region FloppyCashupProcedure
                            if (!GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy)
                                MessageBox.Show("Please insert a Floppy Disc, and then press Enter to put files on");
                            sNonItemDisplayArray = new string[0];
                            AddOtherItemDescription("Starting Copying|0|0|true|centre");
                            tillEngine.SetupNewTransaction();
                            tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                            tvTransaction.Invalidate();
                            tvTransaction.Refresh();
                            string sCashupLoc = Properties.Settings.Default.sFloppyLocation;
                            if (Directory.Exists(Properties.Settings.Default.sFloppyLocation))
                            {
                                AddOtherItemDescription("Directory " + Properties.Settings.Default.sFloppyLocation + "\\ exists|0|0|false|descalign");
                                tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                tvTransaction.Invalidate();
                                tvTransaction.Refresh();
                            }
                            else
                            {
                                AddOtherItemDescription("Directory " + Properties.Settings.Default.sFloppyLocation + "\\ does not exist|0|0|true|descalign");
                                tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                tvTransaction.Invalidate();
                                tvTransaction.Refresh();
                                try
                                {
                                    Directory.CreateDirectory(Properties.Settings.Default.sFloppyLocation);
                                    if (Directory.Exists(Properties.Settings.Default.sFloppyLocation))
                                    {
                                        AddOtherItemDescription("Directory " + Properties.Settings.Default.sFloppyLocation + "\\ created|0|0|false|descalign");
                                        tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                        tvTransaction.Invalidate();
                                        tvTransaction.Refresh();
                                    }
                                }
                                catch
                                {
                                    AddOtherItemDescription("Directory " + Properties.Settings.Default.sFloppyLocation + "\\ could not be created|0|0|false|descalign");
                                    AddOtherItemDescription("Will use C:\\CashupFiles instead|0|0|false|descalign");
                                    Directory.CreateDirectory("C:\\CashupFiles");
                                    tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                    tvTransaction.Invalidate();
                                    tvTransaction.Refresh();
                                    sCashupLoc = "C:\\CashupFiles";
                                }
                            }
                            string[] sFilesInLoc = Directory.GetFiles(Properties.Settings.Default.sOUTGNGDir, "*" + tillEngine.GetDayNumberFromRepData() + ".DBF");
                            for (int i = 0; i < sFilesInLoc.Length; i++)
                            {
                                if (!Directory.Exists(sCashupLoc + "\\OUTGNG"))
                                {
                                    AddOtherItemDescription("Creating OUTGNG Directory|0|0|false|descalign");
                                    tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                    tvTransaction.Invalidate();
                                    tvTransaction.Refresh();
                                    Directory.CreateDirectory(sCashupLoc + "\\OUTGNG");
                                }
                                AddOtherItemDescription("Copying " + sFilesInLoc[i] + " to Floppy Disc|0|0|false|descalign");
                                tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                tvTransaction.Invalidate();
                                tvTransaction.Refresh();
                                File.Copy(sFilesInLoc[i], sCashupLoc + "\\OUTGNG\\" + sFilesInLoc[i].Split('\\')[sFilesInLoc[i].Split('\\').Length - 1], true);
                            }
                            File.Copy(Properties.Settings.Default.sOUTGNGDir + "\\REPDATA.DBF", sCashupLoc + "\\OUTGNG\\REPDATA.DBF", true);
                            if (!Directory.Exists(sCashupLoc + "\\INGNG"))
                            {
                                Directory.CreateDirectory(sCashupLoc + "\\INGNG");
                                AddOtherItemDescription("Created INGNG directory in " + sCashupLoc + "|0|0|false|descalign");
                            }
                            if (sCashupLoc != Properties.Settings.Default.sFloppyLocation)
                            {
                                MessageBox.Show("An error occured finding " + Properties.Settings.Default.sFloppyLocation + ". C:\\CashupFiles was used instead", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            if (!GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy)
                            {
                                MessageBox.Show("Copy to Floppy Disc complete. Now put the disc in the Backoffice computer and collect as usual. Then bring the disc back, and press the Enter key when you're ready to continue");
                                while (!Directory.Exists(sCashupLoc + "\\INGNG"))
                                {
                                    if (MessageBox.Show("Please ensure that the floppy disc is back in the drive, and you have collected in the Backoffice (because the INGNG directory wasn't found). Select OK to try again, or Cancel to abort", "INGNG not found", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                    {
                                        break;
                                    }
                                }
                                if (Directory.Exists(sCashupLoc + "\\INGNG"))
                                {
                                    sFilesInLoc = Directory.GetFiles(sCashupLoc + "\\INGNG");
                                    for (int i = 0; i < sFilesInLoc.Length; i++)
                                    {
                                        AddOtherItemDescription("Copying " + sFilesInLoc[i] + " to INGNG|0|0|false|rightdescalign");
                                        tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
                                        tvTransaction.Invalidate();
                                        tvTransaction.Refresh();
                                        File.Copy(sFilesInLoc[i], Properties.Settings.Default.sINGNGDir + sFilesInLoc[i].Split('\\')[sFilesInLoc[i].Split('\\').Length - 1], true);
                                       
                                    }
                                }
                            }
                            #endregion
                        }

                        /*if (GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy && GTill.Properties.Settings.Default.bAutoSwapBootFiles)
                        {
                            if (MessageBox.Show("Press Enter on the keyboard to continue. Wait until instructed before doing anything else!", "Reboot into DOS", MessageBoxButtons.OKCancel) == DialogResult.OK)
                            {
                                File.SetAttributes("C:\\BOOT.INI", ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                                File.SetAttributes("C:\\BOOT2.INI", ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                                File.Copy("C:\\BOOT2.INI", "C:\\BOOT.INI", true);
                                File.SetAttributes("C:\\BOOT.INI", (FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                                File.SetAttributes("C:\\BOOT2.INI", (FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                                ShutdownCode.RebootComputer();
                            }
                            else
                            {
                                MessageBox.Show("Ok, GTill will now exit");
                                Application.Exit();
                            }
                        }
                            
                        else if (Properties.Settings.Default.bUseMSDosFileTransferProgram)
                        {
                            MessageBox.Show("Now you need to switch off the screen, printer, cash up and go and collect in the Back Office. Press Enter on the keyboard just before switching the monitor off.");
                            System.Diagnostics.Process.Start("MsDosFileTransfer.exe");
                            Application.Exit();
                        }*/
                        else if (GTill.Properties.Settings.Default.bWaitForShutDown)
                        {
                            tmrFilesToUpdate.Enabled = false;
                            frmShutDownWait fsdw = new frmShutDownWait(ref tillEngine);
                            fsdw.ShowDialog();
                        }
                        else if (MessageBox.Show("Cash up complete. Would you like to shut down the computer?", "Finished", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            ShutdownCode.ShutdownComputer();
                        }
                        else
                        {
                            MessageBox.Show("Ok, GTill will now exit");
                            Application.Exit();
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Handles the menu form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Just go back to the login screen
            ChangeFormState(FormState.LoginScreen);
        }

        /// <summary>
        /// The form that checks the faulty entered barcode closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiPartialBCode_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sResult = fsfiPartialBCode.GetItemBarcode();
            if (sResult != "NONE_SELECTED") // If a barcode was selected from the suggestions
            {
                tbItemCode.Text = sResult;
                // TODO: Maybe there's a better way of doing this? Create a procedure to add a barcode?
                tbItemCode.Focus();
                SendKeys.Send("{ENTER}");
            }
            else
            {
                tbItemCode.Text = "";
                tbItemCode.Focus();
            }
        }

        /// <summary>
        /// The lookup form closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiLookup_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sBarcode = fsfiLookup.GetItemBarcode();
            if (sBarcode != "NONE_SELECTED") // An item has been selected
            {
                tbItemCode.Text = sBarcode;
                tbItemCode.Focus();
                SendKeys.Send("{ENTER}");
            }
            else // No item selected
            {
                tbItemCode.Text = "";
                tbItemCode.Focus();
            }
            tbItemCode.Visible = true;
            tbItemCode.Focus();
        }

        /// <summary>
        /// The multiply/discount form closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fiGetMulOrDisc_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sReturned = fiGetMulOrDisc.sGetDataToReturn();
            fiGetMulOrDisc.Dispose();
            bool bNotFinished = false;
            if (sReturned == "CANCELLED" || sReturned == null)
            {
                tbItemCode.Visible = true;
                lblNextItemNumber.Visible = true;
            }
            else
            {
                string[] sCommand = sReturned.Split(',');
                if (sCommand[0] == "PDISCOUNT")
                {
                    float nDiscountPercentage = 100.0f;
                    try
                    {
                        nDiscountPercentage = (float)Convert.ToDecimal(sCommand[1]);
                    }
                    catch
                    {
                        tbItemCode.Visible = true;
                        lblNextItemNumber.Visible = true;
                        tbItemCode.Text = "";
                        tbItemCode.Focus();
                        return;
                    }
                    float fAmountBeforeDiscount = tillEngine.GetItemJustAdded().Amount;
                    if (TillEngine.TillEngine.FixFloatError(fAmountBeforeDiscount * (1 - (nDiscountPercentage / 100))) >= 0.01f || tillEngine.GetItemJustAdded().Quantity <= 0)
                    {
                        tillEngine.DiscountPercentageFromLastItem(nDiscountPercentage);
                    }
                    else
                    {
                        float fMinDiscPercent = 99.0f;
                        while (TillEngine.TillEngine.FixFloatError(fAmountBeforeDiscount * (1 - (fMinDiscPercent / 100))) < 0.01f)
                            fMinDiscPercent -= 1.0f;
                        AddOtherItemDescription("Discount of item " + tillEngine.GetNumberOfItemsInCurrentTransaction().ToString() + " too high. Try " + fMinDiscPercent.ToString() + "% or lower|10|10|true|centre");
                    }
                }
                else if (sCommand[0] == "ADISCOUNT")
                {
                    float fAmountToDiscount = 0.0f;
                    try
                    {
                        fAmountToDiscount = TillEngine.TillEngine.fFormattedMoneyString(sCommand[1]);
                    }
                    catch
                    {
                        tbItemCode.Visible = true;
                        lblNextItemNumber.Visible = true;
                        tbItemCode.Text = "";
                        tbItemCode.Focus();
                        return;
                    }
                    if (fAmountToDiscount >= tillEngine.GetItemJustAdded().Amount && tillEngine.GetItemJustAdded().Quantity > 0 )
                    {
                        AddOtherItemDescription("Discount of item " + tillEngine.GetNumberOfItemsInCurrentTransaction().ToString() + " too high. Try " + FormatMoneyForDisplay(tillEngine.GetItemJustAdded().Amount - 0.01f) + " or lower|10|10|true|centre");
                    }
                    else
                    {
                        fAmountToDiscount = TillEngine.TillEngine.FixFloatError(fAmountToDiscount);
                    tillEngine.DiscountFixedAmountFromLastItem(fAmountToDiscount);
                    }
                }
                else if (sCommand[0] == "SDISCOUNT")
                {
                    float fNewAmount = 0.0f;
                    try
                    {
                        fNewAmount = TillEngine.TillEngine.fFormattedMoneyString(sCommand[1]);
                    }
                    catch
                    {
                        tbItemCode.Visible = true;
                        lblNextItemNumber.Visible = true;
                        tbItemCode.Text = "";
                        tbItemCode.Focus();
                        return;
                    }
                    if (fNewAmount >= (0.01f * tillEngine.GetItemJustAdded().Quantity))
                    {
                        float fCurrentAmount = tillEngine.GetItemJustAdded().Amount;
                        float fToDiscount = tillEngine.fFixFloatError(fCurrentAmount - fNewAmount);
                        tillEngine.DiscountFixedAmountFromLastItem(fToDiscount);
                    }
                    else
                        AddOtherItemDescription("Discount of item was too low!|10|10|true|centre");

                }
                else if (sCommand[0] == "SWAPITEM")
                {
                    tillEngine.SwapItemWithLast(Convert.ToInt32(sCommand[1]) - 1);
                    Point pStartPosition = new Point(this.Left, this.Top + lblLines[1].Top + 20);
                    
                    fiGetMulOrDisc = new frmInput(frmInput.FormType.DiscountAmount, pStartPosition, new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), new string[0]);
                    fiGetMulOrDisc.Show();
                    fiGetMulOrDisc.FormClosing += new FormClosingEventHandler(fiGetMulOrDisc_FormClosing);
                    bNotFinished = true;
                    
                }
                else if (sCommand[0] == "MULTIPLY")
                {
                    try
                    {
                        if (!fiGetMulOrDisc.bSetQuantity)
                        {
                            int nToMultiplyBy = Convert.ToInt32(sCommand[1]);
                            //if (nToMultiplyBy >= 2)
                            tillEngine.MultiplyLastItemQuantity(nToMultiplyBy);
                        }
                        else
                        {
                            int nToMultiplyBy = Convert.ToInt32(sCommand[1]);
                            if (nToMultiplyBy >= 1)
                                tillEngine.SetLastItemQuantity(nToMultiplyBy);
                        }
                    }
                    catch
                    {
                        ;
                    }
                }
                else if (sCommand[0] == "DELETELINE")
                {
                    try
                    {
                        if (sCommand[1].ToUpper() != "A")
                        {
                            int nToDelete = Convert.ToInt32(sCommand[1]);
                            if (nToDelete <= tillEngine.GetNumberOfItemsInCurrentTransaction() && nToDelete > 0)
                                tillEngine.DeleteLine(nToDelete);
                        }
                        else
                        {
                            while (tillEngine.GetNumberOfItemsInCurrentTransaction() > 0)
                                tillEngine.DeleteLine(1);
                        }
                    }
                    catch
                    {
                        ;
                    }
                    tbItemCode.Visible = true;
                    tbItemCode.Focus();
                    lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();
                    lblNextItemNumber.Visible = true;
                }
            }
            RedrawTransactions(0);
            if (!bNotFinished)
            {
                tbItemCode.Visible = true;
                tbItemCode.Focus();
                lblNextItemNumber.Visible = true;
            }
            
        }

        Timer tmrSinceEndOfTransaction;

        /// <summary>
        /// The payment selection form closed handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fpiGetPayment_FormClosed(object sender, FormClosedEventArgs e)
        {
            float fAmountPaid = fpiGetPayment.GetAmount();
            fAmountPaid = tillEngine.fFixFloatError(fAmountPaid);
            string sPaymentType = fpiGetPayment.GetPaymentMethod();
            // Take off previous messages
            RemoveOtherItemDescription("Discount not allowed");
            RemoveOtherItemDescription("Change not");
            RemoveOtherItemDescription("Change from");
            if (!sPaymentType.StartsWith("NULL") && !sPaymentType.Contains("DISCOUNT") && sPaymentType != "CHRG")
            {
                if (!tillEngine.GetAllPaidFor())
                    tillEngine.AddPayment(sPaymentType, fAmountPaid);
                else
                    tillEngine.AddPayment(sPaymentType, TillEngine.TillEngine.FixFloatError(tillEngine.GetAmountStillDue()), TillEngine.TillEngine.FixFloatError(fAmountPaid - tillEngine.GetAmountStillDue()));
                AddOtherItemDescription("Payment by " + fpiGetPayment.GetPaymentDescription(sPaymentType) + " : " + FormatMoneyForDisplay(fAmountPaid) + "|0|0|false|right");
                RemoveOtherItemDescription("Still Due :");
                if (tillEngine.GetAllPaidFor())
                {
                    if (sPaymentType == "CASH")
                        AddOtherItemDescription("Change Due : " + FormatMoneyForDisplay(tillEngine.GetChangeDue()) + "|15|0|true|right");
                    tillEngine.SaveTransaction();
                    BackupEngine.PartBackup();
                    ChangeFormState(FormState.LoginScreen);
                    nSecsSinceEndOfTransaction = 0;
                    tmrSinceEndOfTransaction = new Timer();
                    tmrSinceEndOfTransaction.Interval = 1000;
                    tmrSinceEndOfTransaction.Tick += new EventHandler(tmrSinceEndOfTransaction_Tick);
                    tmrSinceEndOfTransaction.Start();
                }
                else
                {
                    AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                    fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                    fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                    fpiGetPayment.Show();
                }
            }
            else if (sPaymentType == "NULL_WPM")
            {
                AddOtherItemDescription("Change not available, try again|0|0|true|centre"); AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                fpiGetPayment.Show();
            }
            else if (sPaymentType == "NULL_CFC")
            {
                AddOtherItemDescription("Change from cheques is disabled|0|0|true|centre");
                AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                fpiGetPayment.Show();
            }
            else if (sPaymentType == "PDISCOUNT")
            {
                float fAmountBeforeDiscount = tillEngine.GetAmountStillDue();
                if (TillEngine.TillEngine.FixFloatError(fAmountBeforeDiscount * (1 - ((float)(Convert.ToInt32(Math.Round(fAmountPaid, 2))) / 100))) >= (0.01f * tillEngine.GetNumberOfTotalItemsInCurrentTransaction()))
                {
                    tillEngine.DiscountPercentageFromWholeTransaction((float)Convert.ToDecimal(Math.Round(fAmountPaid, 2)));
                    AddOtherItemDescription(Convert.ToDecimal(Math.Round(fAmountPaid, 2)).ToString() + "% Transaction Discount|0|0|false|rightdescalign");
                }
                else
                {
                    float nDiscPercent = 99.0f;
                    while (TillEngine.TillEngine.FixFloatError(fAmountBeforeDiscount * (1 - (nDiscPercent / 100))) < (0.01f * tillEngine.GetNumberOfTotalItemsInCurrentTransaction())
                        && nDiscPercent >= 1)
                        nDiscPercent -= 1.0f;
                    nDiscPercent = TillEngine.TillEngine.FixFloatError(nDiscPercent);
                    AddOtherItemDescription("Discount not allowed. Try " + nDiscPercent.ToString() + "% or lower|10|10|true|centre");
                }
                RemoveOtherItemDescription("Total Due :");
                RemoveOtherItemDescription("Still Due :");
                AddOtherItemDescription("Total Due : " + FormatMoneyForDisplay(tillEngine.GetTotalAmountInTransaction()) + "|0|15|false|right");
                AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                fpiGetPayment.Show();

            }
            else if (sPaymentType == "ADISCOUNT")
            {
                if (tillEngine.fFixFloatError(tillEngine.fFixFloatError(tillEngine.GetAmountStillDue() - fAmountPaid)) < tillEngine.fFixFloatError(0.01f * tillEngine.GetNumberOfTotalItemsInCurrentTransaction()))
                {
                    AddOtherItemDescription("Discount not allowed. Try " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue() - tillEngine.fFixFloatError(0.01f * tillEngine.GetNumberOfTotalItemsInCurrentTransaction())) + " or lower|10|10|true|centre");
                }
                else
                {
                    AddOtherItemDescription(FormatMoneyForDisplay(fAmountPaid) + " Transaction Discount|0|0|false|rightdescalign");
                    tillEngine.DiscountFixedAmountFromWholeTransaction(fAmountPaid);
                }
                RemoveOtherItemDescription("Total Due :");
                RemoveOtherItemDescription("Still Due :");
                AddOtherItemDescription("Total Due : " + FormatMoneyForDisplay(tillEngine.GetTotalAmountInTransaction()) + "|0|15|false|right");
                AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                fpiGetPayment.Show();
            }
            else if (sPaymentType == "SDISCOUNT")
            {
                float fAmountToDiscount = tillEngine.fFixFloatError(tillEngine.GetAmountStillDue() - fAmountPaid);
                if (fAmountPaid >= (0.01f * tillEngine.GetNumberOfTotalItemsInCurrentTransaction()))
                {
                    tillEngine.DiscountFixedAmountFromWholeTransaction(fAmountToDiscount);
                }
                else
                {
                    AddOtherItemDescription("Discount of item was too low!|10|10|true|centre");
                }
                RemoveOtherItemDescription("Total Due :");
                RemoveOtherItemDescription("Still Due :");
                AddOtherItemDescription("Total Due : " + FormatMoneyForDisplay(tillEngine.GetTotalAmountInTransaction()) + "|0|15|false|right");
                AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                fpiGetPayment = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                fpiGetPayment.FormClosed += new FormClosedEventHandler(fpiGetPayment_FormClosed);
                fpiGetPayment.Show();
            
            }
            else if (sPaymentType == "CHRG")
            {
                fasChargeToAcc = new frmAccSel(ref tillEngine);
                fasChargeToAcc.FormClosed += new FormClosedEventHandler(fasChargeToAcc_FormClosing);
                fasChargeToAcc.Show();
            }
            else
            {
                //sNonItemDisplayArray = new string[0];
                ClearNonItemDisplayArray();
                tbItemCode.Visible = true;
                tbItemCode.Text = "";
                tbItemCode.Focus();
                lblNextItemNumber.Visible = true;
                fpiGetPayment.Dispose();
                tillEngine.ClearPaymentMethods();
            }
            RedrawTransactions(0);
        }

        int nSecsSinceEndOfTransaction = 0;
        void tmrSinceEndOfTransaction_Tick(object sender, EventArgs e)
        {
            nSecsSinceEndOfTransaction++;
            
        }

        /// <summary>
        /// The account to charge form closing handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fasChargeToAcc_FormClosing(object sender, FormClosedEventArgs e)
        {
            string sAccountToCharge = fasChargeToAcc.sGetAccountCode();
            fasChargeToAcc.Dispose();

            if (sAccountToCharge == "NONE_SELECTED")
            {
                //sNonItemDisplayArray = new string[0];
                ClearNonItemDisplayArray();
                tbItemCode.Visible = true;
                tbItemCode.Text = "";
                tbItemCode.Focus();
                lblNextItemNumber.Visible = true;
                fpiGetPayment.Dispose();
                tillEngine.ClearPaymentMethods();
                RedrawTransactions(0);
            }
            else
            {
                string sAccountNameChargedTo = tillEngine.sGetAccountDetailsFromCode(sAccountToCharge)[2].TrimEnd(' ');
                AddOtherItemDescription("Charged to Account (" + sAccountNameChargedTo + ")|0|0|false|right");
                tillEngine.AddPayment("CHRG," + sAccountToCharge, tillEngine.GetAmountStillDue());
                tillEngine.SaveTransaction();
                BackupEngine.PartBackup();
                ChangeFormState(FormState.LoginScreen);
            }
        }

        /// <summary>
        /// If the next line number is less than 10, then a 0 is added before the number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lblNextItemNumber_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Label current = (Label)sender;
                int nNum = Convert.ToInt32(current.Text);
                if (nNum < 10 && current.Text.Length < 2)
                    current.Text = "0" + current.Text;
            }
            catch
            {
                ;
            }
        }

        /// <summary>
        /// The handler for when a key is pressed in the look up textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbLookupStockCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ChangeFormState(FormState.LoginScreen);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                for (int i = 0; i < lblLookupDescription.Length; i++)
                {
                    lblLookupDescription[i].Visible = true;
                }
                string[] sItemDetails = tillEngine.GetItemDetailsForLookup(tbLookupStockCode.Text.ToUpper());
                if (sItemDetails != null)
                {
                    sLookupBarcode = tbLookupStockCode.Text.ToUpper();
                    lblLookupDescription[0].Text += sItemDetails[0];
                    lblLookupDescription[1].Text += tillEngine.CurrencySymbol.ToString() + sItemDetails[1];
                    lblLookupDescription[2].Text += tbLookupStockCode.Text.ToUpper();
                    lblLookupDescription[3].Text += sItemDetails[2];
                    lblLookupDescription[4].Text += sItemDetails[3];
                    if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory != 1)
                    {
                        lblLookupDescription[6].Text = "";
                    }
                    if ((float)Convert.ToDecimal(sItemDetails[3]) > 0)
                    {
                        lblLookupDescription[5].Text = "Due Delivery Date: ";
                        lblLookupDescription[6].Text = "";
                        sLookupBarcode = "";
                    }
                    else if (tillEngine.GetItemAsItemClass(tbLookupStockCode.Text.ToUpper()).ItemCategory == 3)
                    {
                        lblLookupDescription[6].Text = "This item has been discontinued.";
                    }
                    else if (tillEngine.GetItemAsItemClass(tbLookupStockCode.Text.ToUpper()).ItemCategory == 5)
                    {
                        lblLookupDescription[6].Text = "This item is similar to " + tillEngine.GetItemAsItemClass(tbLookupStockCode.Text.ToUpper()).ParentBarcode + ", which may be in stock. Enter E to view the item.";
                    }
                    lblLookupDescription[5].Text += sItemDetails[4];
                    ChangeFormState(FormState.LoginScreen);
                }
                else if (tbLookupStockCode.Text.Length == 0)
                {
                    lblLookupDescription[0].Text = "No barcode was entered. Enter 'L' to try again.";
                    ChangeFormState(FormState.LoginScreen);
                }
                else if (tbLookupStockCode.Text.Length >= 3)
                {
                    fsfiLookupLookup = new frmSearchForItemV2(ref tillEngine);
                    fsfiLookupLookup.Show();
                    fsfiLookupLookup.FormClosing += new FormClosingEventHandler(fsfiLookupLookup_PartialBCode_Closing);
                    fsfiLookupLookup.CheckForPartialBarcodeFromScanner(tbLookupStockCode.Text.ToUpper());
                }
                else
                {
                    lblLookupDescription[0].Text = tbLookupStockCode.Text.ToUpper() + " isn't a recognised barcode.";
                    lblLookupDescription[1].Text = "Please enter 3 or more characters to search for a barcode.";
                    ChangeFormState(FormState.LoginScreen);
                }
            }
            else if (e.KeyCode == Keys.OemQuestion || e.KeyCode == Keys.Divide)
            {
                // Lookup!
                tbLookupStockCode.Visible = false;
                fsfiLookupLookup = new frmSearchForItemV2(ref tillEngine);
                fsfiLookupLookup.Show();
                fsfiLookupLookup.FormClosing += new FormClosingEventHandler(fsfiLookupLookup_FormClosing);
            }
        }

        /// <summary>
        /// When the barcode can't be found, a partial barcode search is done. This handles the result from the partial barcode search.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiLookupLookup_PartialBCode_Closing(object sender, FormClosingEventArgs e)
        {
            string sResultFromDialog = fsfiLookupLookup.GetItemBarcode();
            if (sResultFromDialog == "NONE_SELECTED")
            {
                tbLookupStockCode.Text = "";
                lblLookupDescription[0].Text += " No item with the barcode " + fsfiLookupLookup.OriginalBarcode + " was found.";
                lblLookupDescription[6].Text = "";
                ChangeFormState(FormState.LoginScreen);
            }
            else
            {
                tbLookupStockCode.Text = sResultFromDialog;
                SendKeys.Send("{ENTER}");
            }
        }

        /// <summary>
        /// Handler for the lookup textbox's category/description/partial barcode lookup form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiLookupLookup_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sResponse = fsfiLookupLookup.GetItemBarcode();
            if (sResponse != "NONE_SELECTED")
            {
                tbLookupStockCode.Visible = true;
                tbLookupStockCode.Text = sResponse;
                tbLookupStockCode.Focus();
                SendKeys.Send("{ENTER}");
            }
            else
            {
                tbLookupStockCode.Clear();
                ChangeFormState(FormState.LoginScreen);
            }
        }

        /// <summary>
        /// Handles the staff name size change (when the text changes basically)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lblStaffName_SizeChanged(object sender, EventArgs e)
        {
            if (lblTransactionNumber != null)
            {
                lblTransactionNumber.Left = lblStaffName.Left + lblStaffName.Width + 5;
                lblPrinterStatus.Left = lblTransactionNumber.Left + lblTransactionNumber.Width + 5;
            }
        }

        /// <summary>
        /// Updates the time at the bottom of the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmrUpdatelblDateTime_Tick(object sender, EventArgs e)
        {
            string sDateTime = "";
            sDateTime += DateTime.Now.DayOfWeek.ToString();
            sDateTime += " ";
            sDateTime += DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
            sDateTime += ", " + DateTime.Now.Hour + ":";
            if (DateTime.Now.Minute < 10)
                sDateTime += "0";
            sDateTime += DateTime.Now.Minute;
            lblDateTime.Text = sDateTime;
            SetFontSizeOnStatusLabels(false);
        }

        Timer tmrCountDownChangeDisplayer;

        void LoginStaffMember(int nStaffNum)
        {
            int nCurrentUser = nStaffNum;
            if (nCurrentUser >= -1)
            {
                tillEngine.CurrentStaffNumber = nCurrentUser;
                lblStaffName.Text = "ID = " + tillEngine.GetCurrentStaffName();
                if (tillEngine.GetCurrentStaffName() == "")
                {
                    tillEngine.CurrentStaffNumber = -2;
                    tillEngine.GetCurrentStaffName();
                }
                else
                {
                    ChangeFormState(FormState.Transaction);
                    RedrawTransactions(0);
                }
            }
            else
            {
                tillEngine.CurrentStaffNumber = -2;
                tillEngine.GetCurrentStaffName();
            }
        }

        /// <summary>
        /// Occurs when the user presses a key in the 'Insert Identity Number' textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (fsCurrentFormState == FormState.LoginScreen)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Check how long since the last transaction. If < 10 seconds then display the amount of change due
                    if (tmrSinceEndOfTransaction != null)
                        tmrSinceEndOfTransaction.Enabled = false;
                    if (nSecsSinceEndOfTransaction < 10 && tillEngine.GetChangeDue() != 0.0f)
                    {
                        nSecsSinceEndOfTransaction = 11;
                        tmrCountDownChangeDisplayer = new Timer();
                        tmrCountDownChangeDisplayer.Interval = 100;
                        tmrCountDownChangeDisplayer.Tick += new EventHandler(tmrCountDownChangeDisplayer_Tick);
                        nSecondsChangeDueDisplayed = 0;
                        fChangeDueFromLastTransaction = tillEngine.GetChangeDue();
                        sTargetStaffForChangeDue = tillEngine.GetCurrentStaffName();
                        tmrCountDownChangeDisplayer.Enabled = true;
                    }
                    try
                    {
                        LoginStaffMember(Convert.ToInt32(tbInput.Text));
                    }
                    catch
                    {
                        if (tbInput.Text.ToUpper() == "L")
                        {
                            // Do a lookup
                            ChangeFormState(FormState.Lookup);
                        }
                        else if (tbInput.Text.ToUpper() == "C")
                        {
                            // Change payment method
                            try
                            {
                                float fAmountOfLastTransaction = tillEngine.GetTotalAmountInTransaction();
                                if (fAmountOfLastTransaction > 0.0f)
                                {
                                    fpiChangePaymentMethod = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetTotalAmountInTransaction(), tillEngine.bAllowChangeFromCheques(), false, false);
                                    fpiChangePaymentMethod.FormClosing += new FormClosingEventHandler(fpiChangePaymentMethod_FormClosing);
                                    fpiChangePaymentMethod.Show();
                                }
                                else
                                    throw new NotSupportedException();
                            }
                            catch
                            {
                                MessageBox.Show("Could not change payment method");
                                ChangeFormState(FormState.LoginScreen);
                            }
                        }
                        else if (tbInput.Text.ToUpper() == "O")
                        {
                            // Add to order suggestions
                            /*if (sLookupBarcode != "")
                            {
                                if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 1)
                                {
                                    tillEngine.AddOrderSuggestion(sLookupBarcode);
                                    MessageBox.Show("Item will be shown for ordering next time an order is placed.");
                                    sLookupBarcode = "";
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 5)
                                {
                                    if (MessageBox.Show("Item can't be ordered as it is a child. Suggest the parent instead?", "Child Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        tillEngine.AddItemToTransaction(tillEngine.GetItemAsItemClass(sLookupBarcode).ParentBarcode);
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 3)
                                {
                                    MessageBox.Show("This item has been discontinued, and so it can't be ordered.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 2)
                                {
                                    MessageBox.Show("This item isn't an orderable item, as it's a department item.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 6)
                                {
                                    MessageBox.Show("This is a comission item, so it can't be ordered.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 4)
                                {
                                    MessageBox.Show("This is a multi-item item, please suggest individual items.");
                                }
                            }
                            else
                            {*/
                                frmOfferSelect fos = new frmOfferSelect(ref tillEngine);
                                fos.ShowDialog();
                                if (fos.SelectedOffer != "NONE_SELECTED")
                                {
                                    tillEngine.PrintOffer(fos.SelectedOffer);
                                    tillEngine.PrintReceiptHeader();
                                    tillEngine.EmptyPrinterBuffer();
                                }
                            //}
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text.ToUpper() == "R")
                        {
                            if (sLookupBarcode != "")
                            {
                                if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 1)
                                {
                                    tillEngine.AddOrderSuggestion(sLookupBarcode);
                                    MessageBox.Show("Item will be shown for ordering next time an order is placed.");
                                    sLookupBarcode = "";
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 5)
                                {
                                    if (MessageBox.Show("Item can't be ordered as it is a child. Suggest the parent instead?", "Child Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        tillEngine.AddItemToTransaction(tillEngine.GetItemAsItemClass(sLookupBarcode).ParentBarcode);
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 3)
                                {
                                    MessageBox.Show("This item has been discontinued, and so it can't be ordered.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 2)
                                {
                                    MessageBox.Show("This item isn't an orderable item, as it's a department item.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 6)
                                {
                                    MessageBox.Show("This is a comission item, so it can't be ordered.");
                                }
                                else if (tillEngine.GetItemAsItemClass(sLookupBarcode).ItemCategory == 4)
                                {
                                    MessageBox.Show("This is a multi-item item, please suggest individual items.");
                                }
                            }
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text == "=")
                        {
                            // Cash up
                            fpiPassword = new frmPasswordInput(tillEngine.GetPasswords(0), "Enter the password to cash up, or press ESC to cancel", this.Size, "CASHUP");
                            fpiPassword.Show();
                            fpiPassword.FormClosing += new FormClosingEventHandler(fpiPassword_FormClosing);
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text.ToUpper() == "P")
                        {
                            // Reprint a receipt
                            tillEngine.ReprintLastReceipt();
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text.ToUpper() == "A")
                        {
                            MessageBox.Show("Customer information gathering has been temporarily disabled");
                            /*frmEmailInput fei = new frmEmailInput(ref tillEngine);
                            fei.ShowDialog();*/
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text.ToUpper() == "D")
                        {
                            ChangeFormState(FormState.Lookup);
                            fsfiLookupLookup = new frmSearchForItemV2(ref tillEngine);
                            fsfiLookupLookup.Show();
                            /*
                            fsfiLookupLookup.DrawForm(frmSearchForItem.FormState.CategoryLookup);
                            fsfiLookupLookup.DrawForm(frmSearchForItem.FormState.BarcodeSearch);
                            fsfiLookupLookup.DrawForm(frmSearchForItem.FormState.DescSearch);
                             */
                            fsfiLookupLookup.FormClosing += new FormClosingEventHandler(fsfiLookupLookup_FormClosing);

                        }
                        else if (tbInput.Text.ToUpper() == "S" && GTill.Properties.Settings.Default.bAllowStats)
                        {
                            tbInput.Text = "";
                            MessageBox.Show("Highest value transaction: " + Statistics.StaffWithBiggestTransaction(ref tillEngine) + "\n\nSold the most (by value): " + Statistics.StaffThatHasSoldTheMost(ref tillEngine) + "\n\nSold the most (by quantity): " + Statistics.StaffThatHasSoldHighestQuantityOfItem(ref tillEngine), "Today's Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (tbInput.Text.ToUpper() == "M")
                        {
                            fpiPassword = new frmPasswordInput(tillEngine.GetPasswords(0), "Enter the password to see the money in the till, or press ESC to cancel", this.Size, "MONEYINTILL");
                            fpiPassword.ShowDialog();
                            if (fpiPassword.GetResult() == frmPasswordInput.PasswordDialogResult.Correct)
                            {
                                frmMoneyInTill fMinTill = new frmMoneyInTill(ref tillEngine, this.Location, this.Size);
                                fMinTill.ShowDialog();
                            }
                            tbInput.Text = "";
                        }
                        else if (tbInput.Text.ToUpper() == "Q" && GTill.Properties.Settings.Default.bAllowStats)
                        {
                            frmStats fStat = new frmStats(ref tillEngine, this.Size, this.Location, frmStats.ChartType.Pie);
                            fStat.ShowDialog();
                            tbInput.Text = "";
                            fStat.Dispose();
                        }
                        else if (tbInput.Text.ToUpper() == "W" && GTill.Properties.Settings.Default.bAllowStats)
                        {
                            frmStats fStat = new frmStats(ref tillEngine, this.Size, this.Location, frmStats.ChartType.CumulativeSales);
                            fStat.ShowDialog();
                            tbInput.Text = "";
                            fStat.Dispose();
                        }
                        /*
                    else if (tbInput.Text.ToUpper() == "E" && GTill.Properties.Settings.Default.bAllowStats)
                    {
                        frmStats fStat = new frmStats(ref tillEngine, this.Size, this.Location, frmStats.ChartType.AverageHourly);
                        fStat.ShowDialog();
                        tbInput.Text = "";
                        fStat.Dispose();
                    }
                    else if (tbInput.Text.ToUpper() == "R" && GTill.Properties.Settings.Default.bAllowStats)
                    {
                        frmStats fStat = new frmStats(ref tillEngine, this.Size, this.Location, frmStats.ChartType.WeeklyCumulative);
                        fStat.ShowDialog();
                        tbInput.Text = "";
                        fStat.Dispose();
                    }
                         */
                        else if (tbInput.Text.ToUpper() == "E")
                        {
                            if (sLookupBarcode != "")
                            {
                                lblLookupDescription[0].Text = "Description: ";
                                lblLookupDescription[1].Text = "Price: ";
                                lblLookupDescription[2].Text = "Barcode: ";
                                lblLookupDescription[3].Text = "Stock Level: ";
                                lblLookupDescription[4].Text = "Quantity On Order: ";
                                lblLookupDescription[5].Text = "Last Delivery Date: ";
                                lblLookupDescription[6].Text = "Enter 'R' to set a Reminder for this item to be ordered.";
                                tbLookupStockCode.Visible = true;
                                tbLookupStockCode.Focus();
                                tbLookupStockCode.Text = tillEngine.GetItemAsItemClass(sLookupBarcode).ParentBarcode;
                                SendKeys.Send("{ENTER}");
                            }
                        }
                        else if (tbInput.Text == "-1")
                        {
                            // Cough
                            tbInput.Text = "";
                            tmrMouseMove.Enabled = false;
                            EasterEggs.SnakeGame sGame = new EasterEggs.SnakeGame(tvTransaction.Size, new Point(this.Left + tvTransaction.Left, this.Top + tvTransaction.Top));
                            sGame.ShowDialog();
                            tmrMouseMove.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Error logging in! Check ID number is correct");
                            ChangeFormState(FormState.LoginScreen);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Add && e.Control)
                {
                    e.SuppressKeyPress = true;
                    float fHelpCurrentFontSize = lblHelp.Font.Size;
                    fHelpCurrentFontSize += 1.0f;
                    lblHelp.Font = new Font(sFontName, fHelpCurrentFontSize);
                    lblHelp.AutoSize = true;
                    if (lblHelp.Width < this.Width && (lblLookupInstruction.Top + lblLookupInstruction.Height < lblHelp.Top))
                    {
                        lblHelp.Font = new Font(sFontName, fHelpCurrentFontSize - 1.0f);
                        foreach (Control c in this.Controls)
                        {
                            try
                            {
                                float fCurrentFontSize = c.Font.Size;
                                c.Font = new Font(sFontName, fCurrentFontSize + 1.0f);
                            }
                            catch
                            {
                                ;
                            }
                        }
                        try
                        {
                            RedrawTransactions(0);
                        }
                        catch
                        {
                            ;
                        }
                        finally
                        {
                            RePositionObjectsAfterResize();
                            Properties.Settings.Default.fMainScreenFontSize = fHelpCurrentFontSize;
                            Properties.Settings.Default.Save();
                        }
                        tbInput.BringToFront();
                        tbInput.Text = "";
                        tbInput.Focus();
                    }
                    else
                    {
                        lblHelp.Font = new Font(sFontName, fHelpCurrentFontSize - 1.0f);
                    }
                }
                else if (e.KeyCode == Keys.Subtract && e.Control)
                {
                    e.SuppressKeyPress = true;
                    float fCurrentFontSize = lblTitle.Font.Size;
                    fCurrentFontSize -= 2.0f;
                    foreach (Control c in this.Controls)
                    {
                        try
                        {
                            c.Font = new Font(sFontName, fCurrentFontSize);
                        }
                        catch
                        {
                            ;
                        }
                    }
                    try
                    {
                        RedrawTransactions(0);
                    }
                    catch
                    {
                        ;
                    }
                    finally
                    {
                        tbInput.Text = "";
                        RePositionObjectsAfterResize();
                        Properties.Settings.Default.fMainScreenFontSize = fCurrentFontSize;
                        Properties.Settings.Default.Save();
                        float fFontSize = Properties.Settings.Default.fMainScreenFontSize;
                        for (int i = 0; i < lblLines.Length; i++)
                        {
                            while (lblLines[i].Width < this.Width)
                                lblLines[i].Text += "_";
                        }
                    }
                }
                else if (e.KeyCode == Keys.F10)
                {
                    if (this.FormBorderStyle == FormBorderStyle.None)
                        this.FormBorderStyle = FormBorderStyle.Sizable;
                    else
                        this.FormBorderStyle = FormBorderStyle.None;
                }
                else if ((char)e.KeyCode.ToString()[e.KeyCode.ToString().Length - 1] >= 48 && (char)e.KeyCode.ToString()[e.KeyCode.ToString().Length - 1] <= 57 && e.Shift)
                {
                    LoginStaffMember(Convert.ToInt32(e.KeyCode.ToString()[e.KeyCode.ToString().Length - 1].ToString()));
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                    tbItemCode.Text = sLookupBarcode;
                    SendKeys.Send("{ENTER}");
                }
            }
        }

        int nSecondsChangeDueDisplayed = 0;
        float fChangeDueFromLastTransaction = 0;
        string sTargetStaffForChangeDue = "Unknown";
        void tmrCountDownChangeDisplayer_Tick(object sender, EventArgs e)
        {
            nSecondsChangeDueDisplayed++;
            lblTitle.Text = sTargetStaffForChangeDue + ", the change due from your last transaction was: " + FormatMoneyForDisplay(fChangeDueFromLastTransaction);
            lblTitle.ForeColor = Color.Orange;
            if (nSecondsChangeDueDisplayed >= 300)
            {
                lblTitle.Text = tillEngine.ShopName;
                lblTitle.ForeColor = cFrmForeColour;
                tmrCountDownChangeDisplayer.Enabled = false;
            }
        }

        /// <summary>
        /// Occurs when the change payment method dialog closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        bool bChangePaymentStarted = false;
        
        void fpiChangePaymentMethod_FormClosing(object sender, FormClosingEventArgs e)
        {
            float fAmountPaid = fpiChangePaymentMethod.GetAmount();
            fAmountPaid = tillEngine.fFixFloatError(fAmountPaid);
            string sPaymentType = fpiChangePaymentMethod.GetPaymentMethod();
            if (!sPaymentType.StartsWith("NULL") && !sPaymentType.Contains("DISCOUNT") && sPaymentType != "CHRG")
            {
                if (!bChangePaymentStarted)
                {
                    RemoveOtherItemDescription("Discount not allowed");
                    RemoveOtherItemDescription("Change not");
                    RemoveOtherItemDescription("Change Due");
                    RemoveOtherItemDescription("Change from");
                    RemoveOtherItemDescription("Payment by");
                    RemoveOtherItemDescription("Still Due");
                    RemoveOtherItemDescription("Changed payment");
                    tillEngine.ClearPaymentMethods();
                    bChangePaymentStarted = true;
                }
                if (!tillEngine.GetAllPaidFor())
                    tillEngine.AddPayment(sPaymentType, fAmountPaid);
                else
                    tillEngine.AddPayment(sPaymentType, TillEngine.TillEngine.FixFloatError(tillEngine.GetAmountStillDue()));
                AddOtherItemDescription("Changed payment to " + fpiChangePaymentMethod.GetPaymentDescription(sPaymentType) + " : " + FormatMoneyForDisplay(fAmountPaid) + "|0|0|false|right");
                RemoveOtherItemDescription("Still Due :");
                if (tillEngine.GetAllPaidFor())
                {
                    tillEngine.RemoveTransactionFromDatabases(Convert.ToInt32(tillEngine.GetListOfTransactionNumbers()[tillEngine.GetListOfTransactionNumbers().Length - 1]));
                    if (sPaymentType == "CASH")
                        AddOtherItemDescription("Change Due : " + FormatMoneyForDisplay(tillEngine.GetChangeDue()) + "|15|0|true|right");
                    tillEngine.SaveTransaction();
                    BackupEngine.PartBackup();
                    ChangeFormState(FormState.LoginScreen);
                    bChangePaymentStarted = false;
                }
                else
                {
                    AddOtherItemDescription("Still Due : " + FormatMoneyForDisplay(tillEngine.GetAmountStillDue()) + "|0|0|true|right");
                    fpiChangePaymentMethod = new frmPaymentInput(new Point(this.Left, this.Top + lblLines[1].Top + 20), new Size(this.Width, lblDateTime.Top - lblLines[1].Top - 20), tillEngine.GetCreditCards(), tillEngine.GetAmountStillDue(), tillEngine.bAllowChangeFromCheques(), true, true);
                    fpiChangePaymentMethod.FormClosing += new FormClosingEventHandler(fpiChangePaymentMethod_FormClosing);
                    fpiChangePaymentMethod.Show();
                }
            }
            else if (sPaymentType == "NULL_CFC")
            {
                MessageBox.Show("Change from cheques is disabled");
            }
            else if (sPaymentType == "CHRG")
            {
                MessageBox.Show("Cannot change to Charge to account, sorry.");
            }
            else
            {
                tbInput.Text = "";
            }
            RedrawTransactions(0);
        }

        /// <summary>
        /// Clears the transaction area (where transaction details are stored)
        /// </summary>
        void ClearTransactionArea()
        {
            for (int i = 0; i < lblLookupDescription.Length; i++)
            {
                lblLookupDescription[i].Visible = false;
            }
            lblLookupDescription[0].Text = "Description: ";
            lblLookupDescription[1].Text = "Price: ";
            lblLookupDescription[2].Text = "Barcode: ";
            lblLookupDescription[3].Text = "Stock Level: ";
            lblLookupDescription[4].Text = "Quantity On Order: ";
            lblLookupDescription[5].Text = "Last Delivery Date: ";
            lblLookupDescription[6].Text = "Enter 'R' to set a Reminder for this item to be ordered.";

            tbItemCode.Text = "";
        }

        /// <summary>
        /// Formats a float into money string, so 1.2 become 1.20, and 1.234 becomes 1.23
        /// </summary>
        /// <param name="fAmount">The amount to convert</param>
        /// <returns>The formatted string</returns>
        string FormatMoneyForDisplay(float fAmount)
        {
            string[] sError = fAmount.ToString().Split('.');
            if (sError.Length == 2)
            {
                if (sError[1].Length > 2)
                    fAmount = tillEngine.fFixFloatError(fAmount);
            }
            string[] sSplitUp = fAmount.ToString().Split('.');
            if (sSplitUp.Length == 1)
            {
                string[] temp = new string[2];
                temp[0] = sSplitUp[0];
                temp[1] = "00";
                sSplitUp = temp;
            }
            while (sSplitUp[1].Length < 2)
                sSplitUp[1] += "0";

            string toReturn = sSplitUp[0] + "." + sSplitUp[1];

            return toReturn;
        }

        /// <summary>
        /// Redraws the current transaction onto the screen
        /// </summary>
        /// <param name="nOfItemsToMissOffBottom">The number of items to miss off the bottom</param>
        void RedrawTransactions(int nOfItemsToMissOffBottom)
        {
            ClearTransactionArea();
            lblNextItemNumber.Text = (tillEngine.GetNumberOfItemsInCurrentTransaction() + 1).ToString();             
            Label lblTemp = new Label();
            this.Controls.Add(lblTemp);
            float fFontSize = 0.1f;
            lblTemp.Font = new Font(sFontName, fFontSize);
            lblTemp.AutoSize = true;
            lblTemp.Text = "99999.99";

            while (lblTemp.Width < (this.Width - lblDisplayDesc[2].Left))
            {
                fFontSize += 1.0f;
                lblTemp.Font = new Font(sFontName, fFontSize);
            }

            if (sNonItemDisplayArray.Length == 0 || (sNonItemDisplayArray.Length == 1 && sNonItemDisplayArray[0].StartsWith("Offer:")))
            {
                lblCurrentTotal.Visible = true;
                lblCurrentTotal.Text = "Current Total : " + FormatMoneyForDisplay(tillEngine.fFixFloatError(tillEngine.GetTotalAmountInTransaction()));
                lblCurrentTotal.Font = new Font(sFontName, fFontSize);
                lblCurrentTotal.AutoSize = true;
                lblCurrentTotal.Left = this.Width - lblCurrentTotal.Width;
                lblCurrentTotal.TextAlign = ContentAlignment.MiddleRight;
            }
            else
            {
                lblCurrentTotal.Visible = false;
            }

            lblNextItemNumber.Font = new Font(sFontName, fFontSize);
            tbItemCode.Font = new Font(sFontName, fFontSize);
            lblNextItemNumber.Height = tbItemCode.Height;
            tbItemDescInput.Font = new Font(sFontName, fFontSize);
            tbItemPriceInput.Font = new Font(sFontName, fFontSize);

            tvTransaction.Font = new Font(Properties.Settings.Default.sFontName, fFontSize);
            tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
            tvTransaction.Invalidate();
        }

        /// <summary>
        /// Adds a non-item to the display area, such as "Total Due: 1.23"
        /// </summary>
        /// <param name="sToAdd">The string to add, format: stringToAdd|TopSpace|BottomSpace|BooleanHighlightItem|Alignment(descalign/centre/rightdescalign/right/left),</param>
        void AddOtherItemDescription(string sToAdd)
        {
            string[] sTemp = sNonItemDisplayArray;
            sNonItemDisplayArray = new string[sTemp.Length + 1];
            for (int i = 0; i < sTemp.Length; i++)
            {
                sNonItemDisplayArray[i] = sTemp[i];
            }
            sNonItemDisplayArray[sTemp.Length] = sToAdd;
            RedrawTransactions(0);
        }

        /// <summary>
        /// Removes a non-item (also called otherItem) from the transaction display area
        /// </summary>
        /// <param name="sStartsWith">What the string starts with</param>
        void RemoveOtherItemDescription(string sStartsWith)
        {
            tvTransaction.bOkToDraw = false;
            bool bExists = false;
            int nToDelete = 0;
            for (int i = 0; i < sNonItemDisplayArray.Length; i++)
            {
                if (sNonItemDisplayArray[i].StartsWith(sStartsWith))
                {
                    bExists = true;
                    sNonItemDisplayArray[i] = "TO_DELETE";
                    nToDelete++;
                }
            }

            if (bExists)
            {
                string[] sTemp = sNonItemDisplayArray;
                sNonItemDisplayArray = new string[sTemp.Length - nToDelete];
                int nDisplacement = 0;
                for (int i = 0; i < sTemp.Length; i++)
                {
                    if (sTemp[i] != "TO_DELETE")
                        sNonItemDisplayArray[i - nDisplacement] = sTemp[i];
                    else
                        nDisplacement++;
                }
            }
            tvTransaction.bOkToDraw = true;
            tvTransaction.UpdateTransaction(ref tillEngine, sNonItemDisplayArray);
        }

        private void ClearNonItemDisplayArray()
        {
            string sOffer = "";
            for (int i = 0; i < sNonItemDisplayArray.Length; i++)
            {
                if (sNonItemDisplayArray[i].StartsWith("Offer:"))
                    sOffer = sNonItemDisplayArray[i];
            }
            sNonItemDisplayArray = new string[0];
            if (sOffer != "")
            {
                AddOtherItemDescription(sOffer);
            }
        }

        /// <summary>
        /// Sets the size on the status labels (date, time, id etc) at the bottom of the screen
        /// </summary>
        /// <param name="editTop"></param>
        void ReSortOutStatusLabels(bool editTop)
        {
            if (editTop)
            {
                lblDateTime.Top = this.Height - lblDateTime.Height - 5;
                if (this.FormBorderStyle != FormBorderStyle.None)
                    lblDateTime.Top -= 25;
                lblTillID.Top = lblDateTime.Top;
                lblStaffName.Top = lblDateTime.Top;
                lblTransactionNumber.Top = lblDateTime.Top;
                lblPrinterStatus.Top = lblDateTime.Top;
                lblHelp.Top = lblDateTime.Top - lblHelp.Height - 5;
            }

            lblDateTime.Left = 5;
            lblTillID.Left = lblDateTime.Left + lblDateTime.Width + 5;
            lblStaffName.Left = lblTillID.Left + lblTillID.Width + 5;
            lblTransactionNumber.Left = lblStaffName.Left + lblStaffName.Width + 5;
            lblPrinterStatus.Left = lblTransactionNumber.Left + lblTransactionNumber.Width + 5;
        }

        /// <summary>
        /// Works out the font size based on the text of the bottom labels
        /// </summary>
        /// <param name="editTop">Whether or not to edit the top position of the labels</param>
        void SetFontSizeOnStatusLabels(bool editTop)
        {
            float fFontSize = CalculateFontSizeForBottomLabels();
            lblDateTime.Font = new Font(sFontName, fFontSize);
            lblTillID.Font = new Font(sFontName, fFontSize);
            lblStaffName.Font = new Font(sFontName, fFontSize);
            lblPrinterStatus.Font = new Font(sFontName, fFontSize);
            lblTransactionNumber.Font = new Font(sFontName, fFontSize);
            if (tillEngine.PrinterEnabled)
            {
                lblPrinterStatus.Text = "Printer On";
                lblPrinterStatus.Tag = "INVERTED";
            }
            else
            {
                lblPrinterStatus.Text = "Printer Off";
                lblPrinterStatus.Tag = "";
            }
            lblTransactionNumber.Font = new Font(sFontName, fFontSize);
             
            ReSortOutStatusLabels(editTop);
        }

        /// <summary>
        /// Works out the maximum size for the transaction area font
        /// </summary>
        /// <returns>The maximum font size for the transaction area</returns>
        float WorkOutMaxListItemSize()
        {
            Label temp = new Label();
            this.Controls.Add(temp);
            if (lblDisplayDesc[1] == null)
                return 0.0f;


            float fFont = 1.0f;

            temp.Font = new Font(sFontName, fFont);
            temp.AutoSize = true;
            temp.Text = "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
            int nMaxWidth = lblDisplayDesc[2].Left - lblDisplayDesc[1].Left - 10;
            int nSecondMaxWidth = lblDisplayDesc[1].Left - lblDisplayDesc[0].Left - 5;

            while (temp.Width < nMaxWidth)
            {
                fFont += 0.2f;
                temp.Font = new Font(sFontName, fFont);

                temp.Text = "000";
                if (temp.Width > nSecondMaxWidth)
                    break;
                else
                    temp.Text = "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
            }
            this.Controls.Remove(temp);
            temp.Dispose();
            fFont -= 0.2f;

            lblNextItemNumber.Font = new Font(sFontName, fFont);
            tbItemCode.Font = new Font(sFontName, fFont);
            tbItemDescInput.Font = new Font(sFontName, fFont);
            tbItemPriceInput.Font = new Font(sFontName, fFont);
            
            return fFont;
        }

        /// <summary>
        /// Relocates objects if the form is resized
        /// </summary>
        void RePositionObjectsAfterResize()
        {
            lblTitle.AutoSize = true;
            int nTitleAutoSizeHeight = lblTitle.Height;
            lblTitle.AutoSize = false;
            lblTitle.Left = 0;
            lblTitle.Top = 0;
            lblTitle.Width = this.Width;
            lblTitle.Height = nTitleAutoSizeHeight;

            lblNextItemNumber.AutoSize = true;
            int nNextItemAutoSizeHeight = lblNextItemNumber.Height;
            lblNextItemNumber.AutoSize = false;
            lblNextItemNumber.Left = 0;
            lblNextItemNumber.Width = tbItemCode.Left;
            lblNextItemNumber.Height = nNextItemAutoSizeHeight;

            lblLines[0].Top = lblDisplayDesc[0].Top + lblDisplayDesc[0].Height;

            lblInstruction.Top = lblLines[1].Top + lblLines[1].Height;
            tbInput.Top = lblInstruction.Top;
            tbInput.Left = lblInstruction.Left + lblInstruction.Width;
            tbInput.Text = "";
            SetFontSizeOnStatusLabels(true);

            lblLookupInstruction.Top = lblInstruction.Top + lblInstruction.Height;
            tbLookupStockCode.Top = lblLookupInstruction.Top;
            tbLookupStockCode.Left = lblLookupInstruction.Left + lblLookupInstruction.Width;

            for (int i = 0; i < lblLookupDescription.Length; i++)
            {
                lblLookupDescription[i].Top = ((this.Height / 9) * 3) + (i * lblLookupDescription[i].Height);
            }

            lblHelp.Top = lblDateTime.Top - lblHelp.Height - 5;
            tbInput.Focus();
        }

        /// <summary>
        /// Works out the font size for the bottom labels based on their text contents
        /// </summary>
        /// <returns>The font size to use on the bottom labels</returns>
        float CalculateFontSizeForBottomLabels()
        {
            float fFontSize = 1.0f;
            Graphics g = this.CreateGraphics();
            while (g.MeasureString(lblDateTime.Text, new Font(sFontName, fFontSize)).Width
                + g.MeasureString(lblTillID.Text, new Font(sFontName, fFontSize)).Width
                + g.MeasureString(lblStaffName.Text, new Font(sFontName, fFontSize)).Width
                + g.MeasureString(lblPrinterStatus.Text, new Font(sFontName, fFontSize)).Width
                + g.MeasureString(lblTransactionNumber.Text, new Font(sFontName, fFontSize)).Width + 30 < (float)this.Width)
            {
                fFontSize += 0.5f;
            }
            fFontSize -= 1.0f;
            fFontSize = tillEngine.fFixFloatError(fFontSize);
            return fFontSize;
        }

    }
}