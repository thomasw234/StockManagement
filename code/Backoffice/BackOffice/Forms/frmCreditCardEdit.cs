using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmCreditCardEdit : ScalableForm
    {
        StockEngine sEngine;
        TextBox[] tbCards;
        TextBox[] tbDisc;
        int nOfItems = 0;

        public frmCreditCardEdit(ref StockEngine se)
        {
            sEngine = se;
            int nOfCards = sEngine.NumberOfCards;
            nOfItems = nOfCards;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            AllowScaling = false;
            this.Size = new Size(300, 100);
            AddMessage("CARD_NAME", "+ key to add a card,\nShift and Delete to remove\nthe selected card", new Point(10, 10));
            tbCards = new TextBox[nOfCards];
            tbDisc = new TextBox[nOfCards];
            string[] sListOfCards = sEngine.ListOfCards;
            string[] sListOfDiscs = sEngine.GetListOfCardDiscs();
            for (int i = 0; i < nOfCards; i++)
            {
                tbCards[i] = new TextBox();
                tbCards[i].Top = BelowLastControl;
                tbCards[i].Left = 10;
                tbCards[i].Width = 150;
                tbCards[i].KeyDown += new KeyEventHandler(frmCreditCardEdit_KeyDown);
                if (i < sListOfCards.Length)
                    tbCards[i].Text = sListOfCards[i];
                this.Controls.Add(tbCards[i]);
                this.Height += tbCards[i].Height + 10;
                tbCards[i].Tag = i;

                tbDisc[i] = new TextBox();
                tbDisc[i].Top = tbCards[i].Top;
                tbDisc[i].Left = tbCards[i].Left + tbCards[i].Width + 10;
                tbDisc[i].Width = 100;
                tbDisc[i].KeyDown += new KeyEventHandler(tbDiscKeyDown);
                tbDisc[i].Text = sListOfDiscs[i];
                this.Controls.Add(tbDisc[i]);
                tbDisc[i].Tag = i;
            }
            this.Text = "Add / Edit Credit/Debit Cards";
        }

        void tbDiscKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter && Convert.ToInt32(((TextBox)sender).Tag) == nOfItems  -1) || e.KeyCode == Keys.Escape)
            {
                AskAndQuit();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                tbCards[Convert.ToInt32(((TextBox)sender).Tag) + 1].Focus();
            }
        }

        void frmCreditCardEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Insert)
            {
                // Add New Card
                Array.Resize<TextBox>(ref tbCards, tbCards.Length + 1);
                tbCards[tbCards.Length - 1] = new TextBox();
                tbCards[tbCards.Length - 1].Top = BelowLastControl;
                tbCards[tbCards.Length - 1].Left = 10;
                tbCards[tbCards.Length - 1].Width = 150;
                tbCards[tbCards.Length - 1].KeyDown += new KeyEventHandler(frmCreditCardEdit_KeyDown);
                this.Controls.Add(tbCards[tbCards.Length - 1]);
                this.Height += tbCards[tbCards.Length - 1].Height + 10;
                tbCards[tbCards.Length - 1].Tag = tbCards.Length - 1;
                tbCards[tbCards.Length - 1].Focus();

                Array.Resize<TextBox>(ref tbDisc, tbDisc.Length + 1);
                tbDisc[tbCards.Length - 1] = new TextBox();
                tbDisc[tbCards.Length - 1].Top = tbCards[tbCards.Length - 1].Top;
                tbDisc[tbCards.Length - 1].Left = tbCards[tbCards.Length - 1].Left + tbCards[tbCards.Length - 1].Width + 10;
                tbDisc[tbCards.Length - 1].Width = 100;
                tbDisc[tbCards.Length - 1].KeyDown += new KeyEventHandler(tbDiscKeyDown);
                tbDisc[tbCards.Length - 1].Text = "0.00";
                this.Controls.Add(tbDisc[tbCards.Length - 1]);
                tbDisc[tbCards.Length - 1].Tag = tbCards.Length - 1;
                nOfItems++;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                // Delete the selected card
                TextBox tbSender = (TextBox)sender;
                int nCardToRemove = (int)tbSender.Tag;
                for (int i = nCardToRemove; i < nOfItems - 1; i++)
                {
                    tbCards[i].Text = tbCards[i + 1].Text;
                    tbDisc[i].Text = tbDisc[i + 1].Text;
                }
                this.Controls.Remove(tbCards[tbCards.Length - 1]);
                this.Controls.Remove(tbDisc[tbDisc.Length - 1]);
                tbCards[tbCards.Length - 1].Dispose();
                tbDisc[tbDisc.Length - 1].Dispose();
                this.Height -= (tbCards[tbCards.Length - 1].Height + 10);
                Array.Resize<TextBox>(ref tbCards, tbCards.Length - 1);
                Array.Resize<TextBox>(ref tbDisc, tbDisc.Length - 1);
                nOfItems -= 1;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                tbDisc[Convert.ToInt32(((TextBox)sender).Tag)].Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                AskAndQuit();
            }
        }

        void AskAndQuit()
        {
            switch (MessageBox.Show("Would you like to save any changes?", "Save Changes", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    string[] sCardNames = new string[tbCards.Length];
                    string[] sListOfDiscs = new string[tbDisc.Length];
                    for (int i = 0; i < sCardNames.Length; i++)
                    {
                        sCardNames[i] = tbCards[i].Text;
                        sListOfDiscs[i] = tbDisc[i].Text;
                    }
                    sEngine.ListOfCards = sCardNames;
                    sEngine.SetListOfCardDiscs(sListOfDiscs);
                    if (MessageBox.Show("Would you like to upload any changes to all tills now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        sEngine.CopyWaitingFilesToTills();
                    }
                    this.Close();
                    break;
                case DialogResult.No:
                    this.Close();
                    break;
                case DialogResult.Cancel:
                    break;
            }

        }
    }
}
