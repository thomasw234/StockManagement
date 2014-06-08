using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfCommissioners : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbCode;
        CListBox lbName;
        public string Commissioner = "$NONE";
        bool bShowingOnlyOwed = false;
        /*public bool OnlyShowOwedCommissioners
        {
            get
            {
                return bShowingOnlyOwed;
            }
            set
            {
                lbCode.Items.Clear();
                lbName.Items.Clear();
                string[] sCodes = sEngine.GetListOfCommissioners();
                for (int i = 0; i < sCodes.Length; i++)
                {
                    if ((sEngine.GetCommissionerAmountOwed(sCodes[i]) != 0 && value) || !value)
                    {
                        lbCode.Items.Add(sCodes[i]);
                        lbName.Items.Add(sEngine.GetCommissionerName(sCodes[i]));
                    }
                }

                if (lbCode.Items.Count > 0)
                    lbCode.SelectedIndex = 0;
            }
        }*/

        public frmListOfCommissioners(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;

            this.Size = new Size(280, 300);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            AddMessage("INST", "Press Insert to add or edit a commissioner.", new Point(10, 10));


            lbCode = new CListBox();
            lbCode.Location = new Point(10, BelowLastControl);
            lbCode.Size = new Size(60, this.Height - 65);
            lbCode.BorderStyle = BorderStyle.None;
            lbCode.KeyDown += new KeyEventHandler(lbKeyDown);
            lbCode.SelectedIndexChanged += new EventHandler(lbSelChanged);
            this.Controls.Add(lbCode);

            lbName = new CListBox();
            lbName.Location = new Point(lbCode.Left + lbCode.Width, lbCode.Top);
            lbName.Size = new Size(200, this.Height - 65);
            lbName.BorderStyle = BorderStyle.None;
            lbName.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbName.SelectedIndexChanged +=new EventHandler(lbSelChanged);
            this.Controls.Add(lbName);

            LoadCommissioners();

            this.Text = "Select a Commissioner";
        }

        private class Commissioners : IComparable
        {
            private string code;
            private string name;

            public string Code
            {
                get
                {
                    return code;
                }
                set
                {
                    code = value;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    name = value;
                }
            }

            public Commissioners(string code, string name)
            {
                this.code = code;
                this.name = name;
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                return String.Compare(code, ((Commissioners)obj).Code);
            }

            #endregion
        }

        private void LoadCommissioners()
        {
            lbCode.Items.Clear();
            lbName.Items.Clear();
            string[] sCodes = sEngine.GetListOfCommissioners();
            Commissioners[] comms = new Commissioners[sCodes.Length];

            for (int i = 0; i < sCodes.Length; i++)
            {
                comms[i] = new Commissioners(sCodes[i], sEngine.GetCommissionerName(sCodes[i]));
            }

            Array.Sort(comms);

            for (int i = 0; i < comms.Length; i++)
            {
                lbCode.Items.Add(comms[i].Code);
                lbName.Items.Add(comms[i].Name);
            }

            if (lbCode.Items.Count > 0)
                lbCode.SelectedIndex = 0;
        }


        void lbSelChanged(object sender, EventArgs e)
        {
            lbCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbName.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && lbCode.SelectedIndex >= 0)
            {
                Commissioner = lbCode.Items[((ListBox)sender).SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Insert)
            {
                frmAddCommPerson facp = new frmAddCommPerson(ref sEngine);
                facp.ShowDialog();

                lbCode.Items.Clear();
                lbName.Items.Clear();
                string[] sCodes = sEngine.GetListOfCommissioners();
                for (int i = 0; i < sCodes.Length; i++)
                {
                    lbCode.Items.Add(sCodes[i]);
                    lbName.Items.Add(sEngine.GetCommissionerName(sCodes[i]));
                }

                if (lbCode.Items.Count > 0)
                    lbCode.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                // The user wants to delete the commissioner

                // First, check that there are no items in COMMITEM which use this commissioner

                if (!sEngine.CheckIfCommissionerIsUsed(lbCode.Items[lbCode.SelectedIndex].ToString()) || e.Control)
                {
                    sEngine.DeleteCommissioner(lbCode.Items[lbCode.SelectedIndex].ToString());
                    MessageBox.Show("Commissioner Deleted");
                    LoadCommissioners();
                }
                else
                {
                    MessageBox.Show("You can't delete this commissioner, as they are in use on a product. Use Ctrl+Shift+Delete if you are certain that it doesn't matter");
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
