﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ModernListBox
{
    /// <summary>
    /// Initially designed as a listbox, now used to show which database is being loaded when the software first starts
    /// </summary>
    public partial class ModernListBox : UserControl
    {
        /// <summary>
        /// A label for each of the items in the list
        /// </summary>
        Label[] lblItem;
        /// <summary>
        /// The currently 'selected' (shows in a bigger font than other list items) item
        /// </summary>
        int nSelectedIndex = 0;
        /// <summary>
        /// The usual height of an item
        /// </summary>
        int nStandardHeight = 0;
        /// <summary>
        /// The minimum font size and maximum font size
        /// </summary>
        float fMax, fMin;
        /// <summary>
        /// Whether or not the control should move smoothly between list items. Disabled normally for speed
        /// </summary>
        bool bSmoothMoveEnabled = true;

        /// <summary>
        /// Initialises the control
        /// </summary>
        /// <param name="fMaxFontSize">The maximum font size to use</param>
        /// <param name="fStandardFontSize">The normal font size (for non selected items)</param>
        /// <param name="bSmoothMove">Whether or not to enable smooth moves between items</param>
        public ModernListBox(float fMaxFontSize, float fStandardFontSize, bool bSmoothMove)
        {
            this.DoubleBuffered = true;
            fMax = fMaxFontSize;
            fMin = fStandardFontSize;
            lblItem = new Label[0];
            bSmoothMoveEnabled = bSmoothMove;
        }

        /// <summary>
        /// Captures up and down keys and moves the list up or down
        /// </summary>
        /// <param name="m">The keyboard message</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds an item to the list
        /// </summary>
        /// <param name="sDisplayText">The text to display for that item</param>
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
            lblItem[lblItem.Length - 1].Font = new Font("Franklin Gothic Medium", fMin);
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
                lblItem[0].Font = new Font("Franklin Gothic Medium", 25.0f);
        }

        /// <summary>
        /// Redraws the list
        /// </summary>
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

        /// <summary>
        /// Moves the list down by one
        /// </summary>
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

        /// <summary>
        /// Moves the list up by one
        /// </summary>
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

        /// <summary>
        /// Smoothly moves between two items
        /// </summary>
        /// <param name="nCurrentItem">The currently selected item</param>
        /// <param name="nTargetItem">The item to move to</param>
        void SmoothMoveBetweenTwoItems(int nCurrentItem, int nTargetItem)
        {
            if (bSmoothMoveEnabled)
            {
                while (lblItem[nCurrentItem].Font.Size > fMin)
                {
                    lblItem[nCurrentItem].Font = new Font("Franklin Gothic Medium", lblItem[nCurrentItem].Font.Size - 1.0f);
                    lblItem[nTargetItem].Font = new Font("Franklin Gothic Medium", lblItem[nTargetItem].Font.Size + 1.0f);
                    RedrawItems();
                    this.Refresh();
                }
            }
            else
            {
                lblItem[nCurrentItem].Font = new Font("Franklin Gothic Medium", fMin);
                lblItem[nTargetItem].Font = new Font("Franklin Gothic Medium", fMax);
                RedrawItems();
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the forecolour of the control
        /// </summary>
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