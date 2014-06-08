using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmOfferSelect : Form
    {
        TillEngine.TillEngine tEngine;
        public string SelectedOffer = "NONE_SELECTED";
        string[] sOfferCodes = new string[0];
        ListBox lbOffers;

        public frmOfferSelect(ref TillEngine.TillEngine te)
        {
            tEngine = te;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.Size = new Size(640, 480);

            lbOffers = new ListBox();
            lbOffers.Location = new Point(15, 15);
            lbOffers.Size = new Size(this.Width - 30, this.Height - 30);
            lbOffers.Font = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
            lbOffers.BorderStyle = BorderStyle.None;
            lbOffers.KeyDown += new KeyEventHandler(lbOffers_KeyDown);
            this.Controls.Add(lbOffers);

            AddOffers();

        }

        void lbOffers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectedOffer = sOfferCodes[lbOffers.SelectedIndex];
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void AddOffers()
        {
            sOfferCodes = tEngine.GetListOfOfferCodes();
            for (int i = 0; i < sOfferCodes.Length; i++)
            {
                lbOffers.Items.Add(tEngine.GetOfferDesc(sOfferCodes[i]));
            }
            if (lbOffers.Items.Count > 0)
            {
                lbOffers.SelectedIndex = 0;
            }
        }
    }
}
