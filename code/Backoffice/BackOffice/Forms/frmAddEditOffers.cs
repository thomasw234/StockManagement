using System;
using System.Collections.Generic;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace BackOffice
{
    class frmAddEditOffers : ScalableForm
    {
        StockEngine sEngine;
        ListBox lbOfferCode;
        ListBox lbOfferDesc;
        ListBox lbOfferPrinted;
        ListBox lbOfferReturned;
        Button btnAddOffer;

        public frmAddEditOffers(ref StockEngine sEngine)
        {
            this.sEngine = sEngine;
            this.AllowScaling = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Size = new Size(825, 550);
            this.Text = "Add/Edit Offers";
            SetupForm();
            LoadOffers();
        }

        private void SetupForm()
        {
            AddMessage("INST2", "An Offer is a barcode that can be printed on the end of a receipt. The number of offers printed and returned is recorded and shown here.", new Point(10, 10));
            AddMessage("INST1", "Press Insert to add an offer, or enter to edit the selected offer", new Point(10, BelowLastControl));

            AddMessage("OFFERCODE", "Offer Code", new Point(10, BelowLastControl));
            AddMessage("OFFERDESC", "Offer Description", new Point(210, MessageLabel("OFFERCODE").Top));
            AddMessage("OFFERPRNT", "Number Printed", new Point(510, MessageLabel("OFFERCODE").Top));
            AddMessage("OFFERRTRN", "Number Returned", new Point(660, MessageLabel("OFFERCODE").Top));

            lbOfferCode = new ListBox();
            lbOfferCode.Location = new System.Drawing.Point(10, BelowLastControl);
            lbOfferCode.Size = new System.Drawing.Size(200, 500);
            lbOfferCode.BorderStyle = BorderStyle.None;
            lbOfferCode.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbOfferCode.KeyDown += new KeyEventHandler(lbKeyDown);
            lbOfferCode.MouseDown += new MouseEventHandler(lbMouseDown);
            this.Controls.Add(lbOfferCode);

            lbOfferDesc = new ListBox();
            lbOfferDesc.Location = new Point(MessageLabel("OFFERDESC").Left, lbOfferCode.Top);
            lbOfferDesc.Size = new Size(300, 500);
            lbOfferDesc.BorderStyle = BorderStyle.None;
            lbOfferDesc.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbOfferDesc.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbOfferDesc.MouseDown +=new MouseEventHandler(lbMouseDown);
            this.Controls.Add(lbOfferDesc);

            lbOfferPrinted = new ListBox();
            lbOfferPrinted.Location = new Point(MessageLabel("OFFERPRNT").Left, lbOfferCode.Top);
            lbOfferPrinted.Size = new Size(150, 500);
            lbOfferPrinted.BorderStyle = BorderStyle.None;
            lbOfferPrinted.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbOfferPrinted.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbOfferPrinted.MouseDown +=new MouseEventHandler(lbMouseDown);
            this.Controls.Add(lbOfferPrinted);

            lbOfferReturned = new ListBox();
            lbOfferReturned.Location = new Point(MessageLabel("OFFERRTRN").Left, lbOfferCode.Top);
            lbOfferReturned.Size = new Size(150, 500);
            lbOfferReturned.BorderStyle = BorderStyle.None;
            lbOfferReturned.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbOfferReturned.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbOfferReturned.MouseDown +=new MouseEventHandler(lbMouseDown);
            this.Controls.Add(lbOfferReturned);

            btnAddOffer = new Button();
            btnAddOffer.Text = "New Offer";
            btnAddOffer.Location = new Point(730, 30);
            btnAddOffer.AutoSize = true;
            this.Controls.Add(btnAddOffer);
            btnAddOffer.Click += new EventHandler(btnAddOffer_Click);

            this.FormClosing += new FormClosingEventHandler(frmAddEditOffers_FormClosing);
        }

        void btnAddOffer_Click(object sender, EventArgs e)
        {
            AddOffer();
        }

        void lbMouseDown(object sender, MouseEventArgs e)
        {
            lbOfferCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferPrinted.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferReturned.SelectedIndex = ((ListBox)sender).SelectedIndex;
            if (lbOfferCode.SelectedIndex != -1)
            {
                EditOffer(lbOfferCode.Items[((ListBox)sender).SelectedIndex].ToString());
            }
        }

        void frmAddEditOffers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Upload changes to all tills?", "Upload?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                sEngine.CopyWaitingFilesToTills();
            }
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Insert)
            {
                AddOffer();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                EditOffer(lbOfferCode.Items[lbOfferCode.SelectedIndex].ToString());
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbOfferCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferPrinted.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOfferReturned.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        private void LoadOffers()
        {
            lbOfferCode.Items.Clear();
            lbOfferDesc.Items.Clear();
            lbOfferPrinted.Items.Clear();
            lbOfferReturned.Items.Clear();

            string[] sOfferNums = sEngine.GetListOfOfferNumbers();
            for (int i = 0; i < sOfferNums.Length; i++)
            {
                string[] sOfferDetails = sEngine.GetDetailsOfOffer(sOfferNums[i]);
                lbOfferCode.Items.Add(sOfferDetails[0]);
                lbOfferDesc.Items.Add(sOfferDetails[1]);
                lbOfferPrinted.Items.Add(sOfferDetails[4]);
                lbOfferReturned.Items.Add(sOfferDetails[5]);
            }

            if (lbOfferCode.Items.Count > 0)
            {
                lbOfferCode.SelectedIndex = 0;
            }
        }

        private void AddOffer()
        {
            frmSingleInputBox fsiGetCode = new frmSingleInputBox("Enter an 8 digit numerical code for the offer F6 to auto-generate one", ref sEngine);
            fsiGetCode.tbResponse.KeyDown += new KeyEventHandler(tbResponse_KeyDown);
            fsiGetCode.ShowDialog();
            if (fsiGetCode.Response != "$NONE")
            {
                frmSingleInputBox fsiGetDesc = new frmSingleInputBox("Enter a description of the offer [30 characters maximum]", ref sEngine);
                fsiGetDesc.ShowDialog();
                if (fsiGetDesc.Response != "$NONE")
                {
                    frmOffersReceptDesigner ford = new frmOffersReceptDesigner(fsiGetCode.tbResponse.Text, ref sEngine);
                    ford.ShowDialog();
                    sEngine.CreateAnOffer(fsiGetCode.Response, fsiGetDesc.Response, "", fsiGetCode.Response + ".txt");
                }
            }

            LoadOffers();

            sEngine.GenerateOffersForAllTills();
        }

        void tbResponse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6)
            {
                ((TextBox)sender).Text = sEngine.GetNextAutoBarcode();
            }
            else if (e.KeyCode == Keys.F4)
            {
                AddMultiBarcodeItem ambi = new AddMultiBarcodeItem(ref sEngine);
                ((TextBox)sender).Text = ambi.Barcode;
            }
        }

        private void EditOffer(string sBarcode)
        {
            frmSingleInputBox fsiGetDesc = new frmSingleInputBox("Enter a description of the offer [30 characters maximum]", ref sEngine);
            fsiGetDesc.tbResponse.Text = sEngine.GetDetailsOfOffer(sBarcode)[1];
            fsiGetDesc.ShowDialog();
            if (fsiGetDesc.Response != "$NONE")
            {
                frmOffersReceptDesigner ford = new frmOffersReceptDesigner(sBarcode, ref sEngine);
                ford.ShowDialog();
                sEngine.CreateAnOffer(sBarcode, fsiGetDesc.Response, "", sBarcode + ".txt");

            }

            LoadOffers();

            sEngine.GenerateOffersForAllTills();
        }


    }
}
