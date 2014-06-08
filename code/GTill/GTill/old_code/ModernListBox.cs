using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ModernListBox
{
    public partial class ModernListBox : UserControl
    {
        Label[] lblItem;
        int nSelectedIndex = 0;
        int nStandardHeight = 0;
        float fMax, fMin;
        bool bSmoothMoveEnabled = true;

        public ModernListBox(float fMaxFontSize, float fStandardFontSize, bool bSmoothMove)
        {
            this.DoubleBuffered = true;
            fMax = fMaxFontSize;
            fMin = fStandardFontSize;
            lblItem = new Label[0];
            bSmoothMoveEnabled = bSmoothMove;
        }

        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            switch (m.WParam.ToInt32())
            {
                case 40:
                    MoveDown();
                    break;
                case 38:
                    MoveUp();
                    break;
            }
            return base.ProcessKeyEventArgs(ref m);
        }

        public void AddItem(string sDisplayText)
        {
            Label[] lblTemp = lblItem;
            lblItem = new Label[lblTemp.Length + 1];
            for (int i = 0; i < lblTemp.Length; i++)
            {
                lblItem[i] = lblTemp[i];
            }
            lblItem[lblItem.Length - 1] = new Label();
            lblItem[lblItem.Length - 1].Text = sDisplayText;
            lblItem[lblItem.Length - 1].Font = new Font(GTill.Properties.Settings.Default.sFontName, fMin);
            lblItem[lblItem.Length - 1].BackColor = this.BackColor;
            lblItem[lblItem.Length - 1].ForeColor = this.ForeColor;
            if (lblItem.Length == 1)
                lblItem[0].Top = 1;
            else
                lblItem[lblItem.Length - 1].Top = lblItem[lblItem.Length - 2].Top + lblItem[lblItem.Length - 2].Height;
            lblItem[lblItem.Length - 1].Left = 1;
            lblItem[lblItem.Length - 1].AutoSize = true;
            int nHeight = lblItem[lblItem.Length - 1].Height;
            nStandardHeight = nHeight;
            lblItem[lblItem.Length - 1].AutoSize = false;
            lblItem[lblItem.Length - 1].Height = nHeight;
            lblItem[lblItem.Length - 1].Width = this.Width - 2;
            lblItem[lblItem.Length - 1].TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblItem[lblItem.Length - 1]);
            if (lblItem.Length == 1)
                lblItem[0].Font = new Font(GTill.Properties.Settings.Default.sFontName, 25.0f);
        }

        void RedrawItems()
        {
            for (int i = 0; i < lblItem.Length; i++)
            {
                if (lblItem[i].Font != this.Font)
                {
                    lblItem[i].AutoSize = true;
                    int nHeight = lblItem[i].Height;
                    lblItem[i].AutoSize = false;
                    lblItem[i].Width = this.Width - 2;
                    lblItem[i].Height = nHeight;
                }
                if (i != 0)
                    lblItem[i].Top = lblItem[i - 1].Top + lblItem[i - 1].Height;
                else
                    lblItem[0].Top = 1;
            }
        }

        public void MoveDown()
        {
            if (nSelectedIndex < lblItem.Length - 1)
            {
                nSelectedIndex++;
                SmoothMoveBetweenTwoItems(nSelectedIndex - 1, nSelectedIndex);
            }
            else
            {
                nSelectedIndex = 0;
                SmoothMoveBetweenTwoItems(lblItem.Length - 1, nSelectedIndex);
            }
        }

        public void MoveUp()
        {
            if (nSelectedIndex > 0)
            {
                nSelectedIndex -= 1;
                SmoothMoveBetweenTwoItems(nSelectedIndex + 1, nSelectedIndex);
            }
            else
            {
                nSelectedIndex = lblItem.Length - 1;
                SmoothMoveBetweenTwoItems(0, nSelectedIndex);
            }
        }

        void SmoothMoveBetweenTwoItems(int nCurrentItem, int nTargetItem)
        {
            if (bSmoothMoveEnabled)
            {
                while (lblItem[nCurrentItem].Font.Size > fMin)
                {
                    lblItem[nCurrentItem].Font = new Font(GTill.Properties.Settings.Default.sFontName, lblItem[nCurrentItem].Font.Size - 1.0f);
                    lblItem[nTargetItem].Font = new Font(GTill.Properties.Settings.Default.sFontName, lblItem[nTargetItem].Font.Size + 1.0f);
                    RedrawItems();
                    this.Refresh();
                }
            }
            else
            {
                lblItem[nCurrentItem].Font = new Font(GTill.Properties.Settings.Default.sFontName, fMin);
                lblItem[nTargetItem].Font = new Font(GTill.Properties.Settings.Default.sFontName, fMax);
                RedrawItems();
                this.Refresh();
            }
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                foreach (Label lbl in lblItem)
                    lbl.ForeColor = base.ForeColor;
            }
        }
    }
}
