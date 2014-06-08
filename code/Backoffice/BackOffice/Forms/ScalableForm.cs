using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms.WormaldForms
{
    class CTextBox : TextBox
    {
        public int MaxCharCount = 0;
        public bool ShowCharsLeft = false;
        public int OriginalWidth = 0;
    }
    class CListBox : ListBox
    {
        // Taken from http://social.msdn.microsoft.com/forums/en-US/Vsexpressvb/thread/bfd2efc8-7ca5-4694-adc2-a202619e0680/
        private bool mShowScroll;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (!mShowScroll) cp.Style &= ~0x200000;  // Turn off WS_VSCROLL
                return cp;
            }
        }
        [DefaultValue(true)]
        public bool ShowScrollbar
        {
            get { return mShowScroll; }
            set
            {
                if (value == mShowScroll) return;
                mShowScroll = value;
                if (this.Handle != IntPtr.Zero) RecreateHandle();
            }
        }
    }
    class ScalableForm : Form
    {
        Size sOriginalSize;
        Size sLastSize;
        public InputBox[] ibArray;
        public LabelMessage[] lmArray;
        public bool AllowScaling = true;
        public bool SurroundListBoxes = false;
        public static string TitleAddition = "";
        public static Color BackGroundColour;

        public Size OriginalSize
        {
            get
            {
                return sOriginalSize;
            }
            set
            {
                sOriginalSize = value;
            }
        }

        public ScalableForm()
        {
            sOriginalSize = new Size(1024, 768);
            this.Size = OriginalSize;
            sLastSize = sOriginalSize;
            //this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Franklin Gothic Medium", 10.0f);
            //this.BackColor = Color.Black;
            //this.ForeColor = Color.Yellow;
            ibArray = new InputBox[0];
            lmArray = new LabelMessage[0];
            this.TextChanged += new EventHandler(ScalableForm_TextChanged);
            if (BackGroundColour != null)
                this.BackColor = BackGroundColour;


        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (SurroundListBoxes)
            {
                this.SurroundListBoxGroups();
            }
        }

        void ScalableForm_TextChanged(object sender, EventArgs e)
        {
            if (this.Text != "" && TitleAddition != "" && !this.Text.Contains(TitleAddition))
            {
                this.Text += " - " + TitleAddition;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.Tag = e.Control.Location;
            e.Control.KeyDown += new KeyEventHandler(Control_KeyDown);
            e.Control.GotFocus += new EventHandler(Control_GotFocus);
            if (e.Control.GetType().Name == "CListBox")
            {
                e.Control.Click += new EventHandler(Control_Click);
                //e.Control.BackColor = this.BackColor;
                //e.Control.ForeColor = this.ForeColor;
            }
            else if (e.Control.GetType().Name == "Label")
            {
                MenuItem mCopy = new MenuItem("Copy");
                ContextMenu cMenu = new ContextMenu();
                cMenu.MenuItems.Add(mCopy);
                mCopy.Click +=new EventHandler(mCopy_Click);
                e.Control.ContextMenu = cMenu;
                e.Control.MouseDown += new MouseEventHandler(LabelMouseDown);
                e.Control.MouseUp += new MouseEventHandler(LabelMouseUp);
            }
        }

        void Control_Click(object sender, EventArgs e)
        {
            ((ListBox)sender).Focus();
            SendKeys.Send("{ENTER}");
        }

        void LabelMouseUp(object sender, MouseEventArgs e)
        {
            if (((Label)sender).BackColor != this.BackColor)
            {
                ((Label)sender).BackColor = this.BackColor;
                ((Label)sender).ForeColor = this.ForeColor;
            }
        }

        string sLabelText = "";
        void LabelMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                sLabelText = ((Label)sender).Text;
                ((Label)sender).BackColor = Color.DarkGray;
                ((Label)sender).ForeColor = Color.White;
            }
        }

        void mCopy_Click(object sender, EventArgs e)
        {
            if (sLabelText == null)
                sLabelText = "";
            Clipboard.SetText(sLabelText);
        }

        void Control_GotFocus(object sender, EventArgs e)
        {
            if (sender.GetType().Name == "CTextBox")
            {
                if (((TextBox)sender).Text.Length != 0)
                {
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }

        void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (sender.GetType().Name == "CTextBox"))
            {
                int nCurrentTabIndex = ((Control)sender).TabIndex;
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i].TabIndex == nCurrentTabIndex + 1)
                    {
                        this.Controls[i].Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.Up && (sender.GetType().Name == "CTextBox"))
            {
                int nCurrentTabIndex = ((Control)sender).TabIndex;
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i].TabIndex == nCurrentTabIndex - 1)
                    {
                        this.Controls[i].Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.Down && (sender.GetType().Name == "CTextBox"))
            {
                int nCurrentTabIndex = ((Control)sender).TabIndex;
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i].TabIndex == nCurrentTabIndex + 1)
                    {
                        this.Controls[i].Focus();
                    }
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnResize(e);
            if (AllowScaling)
            {
                foreach (Control c in this.Controls)
                {
                    c.Left = Convert.ToInt32(((float)this.Width / (float)sOriginalSize.Width) * (float)((Point)c.Tag).X);
                    c.Top = Convert.ToInt32(((float)this.Height / (float)sOriginalSize.Height) * (float)((Point)c.Tag).Y);
                }

                sLastSize = this.Size;
            }
        }

        public void AddInputControl(string sTag, string sInstruction, Point pLocation, int nWidth)
        {
            AddInputControl(sTag, sInstruction, pLocation, nWidth, "");
            /*
            Array.Resize<InputBox>(ref ibArray, ibArray.Length + 1);

            ibArray[ibArray.Length - 1].lblInstruction = new Label();
            ibArray[ibArray.Length - 1].lblInstruction.Text = sInstruction;
            ibArray[ibArray.Length - 1].lblInstruction.Location = pLocation;
            ibArray[ibArray.Length - 1].lblInstruction.AutoSize = true;
            this.Controls.Add(ibArray[ibArray.Length - 1].lblInstruction);

            ibArray[ibArray.Length - 1].tbInput = new CTextBox();
            ibArray[ibArray.Length - 1].tbInput.Location = pLocation;
            ibArray[ibArray.Length - 1].tbInput.GotFocus += new EventHandler(tbInput_GotFocus);
            ibArray[ibArray.Length - 1].tbInput.AccessibleDescription = "NULL";
            ibArray[ibArray.Length - 1].tbInput.Left += ibArray[ibArray.Length - 1].lblInstruction.Width + 20;
            ibArray[ibArray.Length - 1].tbInput.Width = nWidth - ibArray[ibArray.Length - 1].lblInstruction.Width - 20;
            ibArray[ibArray.Length - 1].tbInput.OriginalWidth = ibArray[ibArray.Length - 1].tbInput.Width;
            if (this.Controls.Count >= 2)
                ibArray[ibArray.Length - 1].tbInput.TabIndex = this.Controls[this.Controls.Count - 2].TabIndex + 1;
            else
                ibArray[ibArray.Length - 1].tbInput.TabIndex = 0;
            this.Controls.Add(ibArray[ibArray.Length - 1].tbInput);

            ibArray[ibArray.Length - 1].sTag = sTag;
            ibArray[ibArray.Length - 1].tbInput.KeyDown += new KeyEventHandler(tbInputKeyDown);
            ibArray[ibArray.Length - 1].tbInput.KeyUp += new KeyEventHandler(tbInputKeyup); */
        }

        void tbInputKeyup(object sender, KeyEventArgs e)
        {
            CTextBox tbSender = (CTextBox)sender;
            if (tbSender.MaxCharCount != 0)
            {
                string sLeft = "[" + tbSender.Text.Length + " / " + tbSender.MaxCharCount + "]";
                int nPos = 0;
                for (nPos = 0; nPos < MessageLabel(tbSender.AccessibleDescription).Text.Length; nPos++)
                {
                    if (MessageLabel(tbSender.AccessibleDescription).Text[nPos] == '[')
                        break;
                }
                string sNewText = "";
                if (nPos < MessageLabel(tbSender.AccessibleDescription).Text.Length)
                    sNewText = MessageLabel(tbSender.AccessibleDescription).Text.Remove(nPos) + sLeft;
                else
                    sNewText = MessageLabel(tbSender.AccessibleDescription).Text + " " + sLeft;
                MessageLabel(tbSender.AccessibleDescription).Text = sNewText;
            }
            if (Convert.ToInt32(tbSender.CreateGraphics().MeasureString(tbSender.Text, tbSender.Font).Width) > tbSender.Width)
            {
                tbSender.Width = Convert.ToInt32(tbSender.CreateGraphics().MeasureString(tbSender.Text, tbSender.Font).Width);
                int nCurrentCursorPos = tbSender.SelectionStart;
                tbSender.SelectionStart = 0;
                tbSender.SelectionStart = nCurrentCursorPos;
            }
            else if (tbSender.OriginalWidth < tbSender.Width && tbSender.Width > Convert.ToInt32(tbSender.CreateGraphics().MeasureString(tbSender.Text, tbSender.Font).Width))
            {
                if (Convert.ToInt32(tbSender.CreateGraphics().MeasureString(tbSender.Text, tbSender.Font).Width) < tbSender.OriginalWidth)
                    tbSender.Width = tbSender.OriginalWidth;
                else
                    tbSender.Width = Convert.ToInt32(tbSender.CreateGraphics().MeasureString(tbSender.Text, tbSender.Font).Width);
            }
            MessageLabel(tbSender.AccessibleDescription).Location = new Point(tbSender.Left + tbSender.Width + 10, tbSender.Top);
        }

        void tbInputKeyDown(object sender, KeyEventArgs e)
        {
            CTextBox tbSender = (CTextBox)sender;
            /*
            if (tbSender.AccessibleDescription.Contains("£"))
            {
                if (e.KeyCode == Keys.Enter)
                {
                    try
                    {
                        tbSender.Text = FormatMoneyForDisplay(tbSender.Text);
                    }
                    catch
                    {
                        ;
                    }
                }
            }*/
            if (tbSender.MaxCharCount != 0 && tbSender.Text.Length == tbSender.MaxCharCount && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back && e.KeyCode != Keys.Escape && e.KeyCode != Keys.Enter && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                e.SuppressKeyPress = true;
        }
        public void AddInputControl(string sTag, string sInstruction, Point pLocation, int nWidth, string sHelpMessage)
        {
            Array.Resize<InputBox>(ref ibArray, ibArray.Length + 1);

            ibArray[ibArray.Length - 1].lblInstruction = new Label();
            ibArray[ibArray.Length - 1].lblInstruction.Text = sInstruction;
            ibArray[ibArray.Length - 1].lblInstruction.Location = pLocation;
            ibArray[ibArray.Length - 1].lblInstruction.AutoSize = true;
            this.Controls.Add(ibArray[ibArray.Length - 1].lblInstruction);

            ibArray[ibArray.Length - 1].tbInput = new CTextBox();
            ibArray[ibArray.Length - 1].tbInput.Location = pLocation;
            ibArray[ibArray.Length - 1].tbInput.Left += ibArray[ibArray.Length - 1].lblInstruction.Width + 20;
            ibArray[ibArray.Length - 1].tbInput.AccessibleDescription = sTag;
            ibArray[ibArray.Length - 1].tbInput.Width = nWidth - ibArray[ibArray.Length - 1].lblInstruction.Width - 20;
            ibArray[ibArray.Length - 1].tbInput.OriginalWidth = ibArray[ibArray.Length - 1].tbInput.Width;
            ibArray[ibArray.Length - 1].tbInput.GotFocus += new EventHandler(tbInput_GotFocus);
            ibArray[ibArray.Length - 1].tbInput.LostFocus += new EventHandler(tbInput_LostFocus);
            if (this.Controls.Count >= 2)
                ibArray[ibArray.Length - 1].tbInput.TabIndex = this.Controls[this.Controls.Count - 2].TabIndex + 1;
            else
                ibArray[ibArray.Length - 1].tbInput.TabIndex = 0;
            this.Controls.Add(ibArray[ibArray.Length - 1].tbInput);

            ibArray[ibArray.Length - 1].sTag = sTag;
            ibArray[ibArray.Length - 1].sHelpMessage = sHelpMessage;
            ibArray[ibArray.Length - 1].tbInput.KeyPress += new KeyPressEventHandler(tbInput_KeyPress);
            ibArray[ibArray.Length - 1].tbInput.KeyDown += new KeyEventHandler(tbInputKeyDown);
            ibArray[ibArray.Length - 1].tbInput.KeyUp +=new KeyEventHandler(tbInputKeyup);
        }

        void tbInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                e.Handled = true;
        }

        void tbInput_LostFocus(object sender, EventArgs e)
        {
            string sTagToLookFor = ((CTextBox)sender).AccessibleDescription;
            RemoveMessage(sTagToLookFor + "_HELPMSG");
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (ibArray[i].sTag == sTagToLookFor)
                {
                    if (ibArray[i].sAutoFill != null && ibArray[i].tbInput.Text.Length == 0)
                    {
                        ibArray[i].tbInput.Text = ibArray[i].sAutoFill;
                    }
                }
            }
        }

        void tbInput_GotFocus(object sender, EventArgs e)
        {
            CTextBox tSender = ((CTextBox)sender);
            string sTagToLookFor = (tSender).AccessibleDescription;
            if (sTagToLookFor != "NULL")
            {
                for (int i = 0; i < ibArray.Length; i++)
                {
                    if (ibArray[i].sTag == sTagToLookFor)
                    {
                        AddMessage(sTagToLookFor + "_HELPMSG", ibArray[i].sHelpMessage, new Point(tSender.Left + tSender.Width + 10, tSender.Top));
                    }
                }
            }
            ((CTextBox)sender).SelectionStart = ((CTextBox)sender).Text.Length;
        }

        public void AddMessage(string sTag, string sMessage, Point pLocation)
        {
            Array.Resize<LabelMessage>(ref lmArray, lmArray.Length + 1);

            lmArray[lmArray.Length - 1].lblMessage = new Label();
            lmArray[lmArray.Length - 1].lblMessage.Text = sMessage;
            lmArray[lmArray.Length - 1].lblMessage.Location = pLocation;
            lmArray[lmArray.Length - 1].lblMessage.AutoSize = true;
            this.Controls.Add(lmArray[lmArray.Length - 1].lblMessage);
            lmArray[lmArray.Length - 1].sTag = sTag;
        }

        public void RemoveMessage(string sTag)
        {
            int nToRemove = -1;
            for (int i = 0; i < lmArray.Length; i++)
            {
                if (lmArray[i].sTag == sTag)
                {
                    // Remove this element
                    nToRemove = i;
                }
            }
            if (nToRemove != -1)
            {
                for (int i = nToRemove + 1; i < lmArray.Length; i++)
                {
                    lmArray[i - 1] = lmArray[i];
                }
                this.Controls.Remove(lmArray[lmArray.Length - 1].lblMessage);
                lmArray[lmArray.Length - 1].lblMessage.Dispose();
                Array.Resize<LabelMessage>(ref lmArray, lmArray.Length - 1);
            }
        }

        public int BelowLastControl
        {
            get
            {
                if (this.Controls.Count >= 1)
                {
                    return this.Controls[this.Controls.Count - 1].Top + this.Controls[this.Controls.Count - 1].Height + 11;
                }
                else
                    return 0;
            }
        }

        public CTextBox InputTextBox(string sTag)
        {
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (ibArray[i].sTag == sTag)
                    return ibArray[i].tbInput;
            }
            throw new NotImplementedException("InputBox tag not found");
        }
        public Label InputTextBoxAssociatedLabel(string sTag)
        {
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (ibArray[i].sTag == sTag)
                    return ibArray[i].lblInstruction;
            }
            throw new NotImplementedException("InputBox tag not found");
        }

        public Label MessageLabel(string sTag)
        {
            for (int i = 0; i < lmArray.Length; i++)
            {
                if (lmArray[i].sTag == sTag + "_HELPMSG")
                    return lmArray[i].lblMessage;
                else if (lmArray[i].sTag == sTag)
                    return lmArray[i].lblMessage;
            }
            return null;
        }

        public void AddAutoFill(string sTag, string sAutoFill)
        {
            bool bFound = false;
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (ibArray[i].sTag == sTag)
                {
                    ibArray[i].sAutoFill = sAutoFill;
                    bFound = true;
                }
            }
            if (!bFound)
                throw new NotImplementedException("InputBox tag not found");
        }

        public static string FormatMoneyForDisplay(decimal dAmount)
        {
            dAmount = Math.Round(dAmount, 2, MidpointRounding.AwayFromZero);
            string[] sSplitUp = dAmount.ToString().Split('.');
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
        public static string FormatMoneyForDisplay(string sAmount)
        {
            try
            {
                decimal dAmount = Convert.ToDecimal(sAmount);
                dAmount = Math.Round(dAmount, 2, MidpointRounding.AwayFromZero);
                string[] sSplitUp = dAmount.ToString().Split('.');
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
            catch
            {

                throw new NotSupportedException("Is " + sAmount + " a number?!");
            }
        }

        public string[] FillInBlanks(string[] sRecord)
        {
            for (int i = 0; i < sRecord.Length; i++)
            {
                if (sRecord[i] == "")
                    sRecord[i] = "0.00";
            }
            return sRecord;
        }

        public void RemoveInputTextBox(string sCode)
        {
            for (int i = 0; i < ibArray.Length; i++)
            {
                if (ibArray[i].sTag == sCode)
                {
                    // One to remove
                    this.Controls.Remove(ibArray[i].lblInstruction);
                    this.Controls.Remove(ibArray[i].tbInput);
                    RemoveMessage(ibArray[i].sTag + "_HELPMSG");
                    for (int x = i; x < ibArray.Length - 1; x++)
                    {
                        ibArray[x] = ibArray[x - 1];
                    }
                    Array.Resize<InputBox>(ref ibArray, ibArray.Length - 1);
                    break;
                }
            }
        }

        public void AlignInputTextBoxes()
        {
            int nLeftMost = 0;
            for (int i = 0; i < ibArray.Length; i++)
            {

                if (ibArray[i].tbInput.Left > nLeftMost)
                    nLeftMost = ibArray[i].tbInput.Left;
            }
            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.Width -= (nLeftMost - ibArray[i].tbInput.Left);
                ibArray[i].tbInput.Left = nLeftMost;
            }
        }

        /// <summary>
        /// Surrounds adjacent list boxes with a black line, similar to BorderStyle.Flat or whatever
        /// </summary>
        public void SurroundListBoxGroups()
        {
            foreach (Control c in this.Controls)
            {
                if (c.GetType().Name.EndsWith("ListBox"))
                {
                    if (((CListBox)c).BorderStyle == BorderStyle.None)
                    {
                        this.CreateGraphics().DrawRectangle(new Pen(Color.Black), new Rectangle(((CListBox)c).Left - 1, ((CListBox)c).Top - 1,
                                                                                                ((CListBox)c).Width + 1, ((CListBox)c).Height + 2));
                        this.CreateGraphics().DrawRectangle(new Pen(Color.White), new Rectangle(((CListBox)c).Left, ((CListBox)c).Top,
                                                                                                ((CListBox)c).Width - 1, ((CListBox)c).Height));
                    }
                }
            }
        }
    }

    struct InputBox
    {
        public CTextBox tbInput;
        public Label lblInstruction;
        public string sTag;
        public string sHelpMessage;
        public string sAutoFill;
    }

    struct LabelMessage
    {
        public Label lblMessage;
        public string sTag;
    }
}
