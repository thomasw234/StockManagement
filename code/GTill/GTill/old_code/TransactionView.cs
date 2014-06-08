using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TillEngine;
using System.Drawing;

namespace GTill
{
    class TransactionView : UserControl
    {

        TillEngine.TillEngine tEngine;
        string[] sNonItemDisplayArray;
        int nFontHeight;

        public int AlignNumber;
        public int AlignDescription;
        public int AlignPrice;
        private int NumberToScrollUp;
        public int NumberToMoveUp
        {
            get
            {
                return NumberToScrollUp;
            }
            set
            {
                if (value < tEngine.GetNumberOfItemsInCurrentTransaction() && value >= 0)
                {
                    NumberToScrollUp = value;
                }
            }
        }

        /// <summary>
        /// Initialise TransactionView Control
        /// </summary>
        public TransactionView()
        {
            this.Paint += new PaintEventHandler(TransactionView_Paint);
            this.FontChanged +=new EventHandler(TransactionView_FontChanged);
            nFontHeight = 0;
        }

        /// <summary>
        /// Update the label height
        /// </summary>
        /// <param name="sender">The object that called this procedure</param>
        /// <param name="e">Event arguments, not used here</param>
        void TransactionView_FontChanged(object sender, EventArgs e)
        {
            // Work out the font height
            nFontHeight = Convert.ToInt32(this.CreateGraphics().MeasureString("ABC", this.Font, this.Width, StringFormat.GenericTypographic).Height);
        }

        void TransactionView_Paint(object sender, PaintEventArgs e)
        {
            if (sNonItemDisplayArray != null && tEngine != null)
            {
                Graphics g = e.Graphics;
                int nCurrentTop = this.Height - nFontHeight;
                string[] sDisplayText;
                int nTopSpace = 0, nBottomSpace = 0, nTextLeft = 0, nTextWidth = 0;
                bool bInvertColours = false;
                string sAlign = "", sTextToDraw = "";
                for (int i = sNonItemDisplayArray.Length - 1; i >= 0; i -= 1)
                {
                    sDisplayText = sNonItemDisplayArray[i].Split('|');
                    nTopSpace = Convert.ToInt32(sDisplayText[1]);
                    nBottomSpace = Convert.ToInt32(sDisplayText[2]);
                    nCurrentTop -= nBottomSpace;
                    bInvertColours = Convert.ToBoolean(sDisplayText[3]);
                    if (bInvertColours)
                    {
                        g.FillRectangle(new SolidBrush(this.ForeColor), new Rectangle(0, nCurrentTop, this.Width, nFontHeight));
                    }
                    sAlign = sDisplayText[4];
                    sTextToDraw = sDisplayText[0];
                    nTextWidth = WorkOutTextWidth(sTextToDraw);
                    switch (sAlign)
                    {
                        case "centre":
                            nTextLeft = (this.Width / 2) - (nTextWidth / 2);
                            break;
                        case "left":
                            nTextLeft = 0;
                            break;
                        case "right":
                            nTextLeft = this.Width - nTextWidth;
                            break;
                        case "descalign":
                            nTextLeft = AlignDescription;
                            break;
                        case "rightdescalign":
                            nTextLeft = AlignPrice - nTextWidth;
                            break;
                    }
                    if (!bInvertColours)
                    {
                        g.DrawString(sTextToDraw, this.Font, new SolidBrush(this.ForeColor), new PointF((float)nTextLeft, (float)nCurrentTop));
                    }
                    else
                    {
                        g.DrawString(sTextToDraw, this.Font, new SolidBrush(this.BackColor), new PointF((float)nTextLeft, (float)nCurrentTop));
                    }
                    nCurrentTop -= nTopSpace;
                    nCurrentTop -= nFontHeight;
                }

                // Now draw the items
                string[,] sItemsToDisplay = tEngine.GetItemsToDisplay();
                int nTotalNumberOfItems = Convert.ToInt32(sItemsToDisplay[0, 0]), nQuantityOfItem, nTempTop, nQuantity;
                float fGrossPriceMultiplied, fNetPrice, fGross, fNet, fDiscountAmount, fGrossAmnt, fNetAmnt, fDAmount;
                string sItemNumToDisplay = "00";
                bool bItemDiscount = false, bItemMultipleQuantity = false;
                for (int i = nTotalNumberOfItems - NumberToScrollUp; i >= 1 && nCurrentTop > 0; i -= 1)
                {
                    fGrossPriceMultiplied = TillEngine.TillEngine.FixFloatError((float)Convert.ToDecimal(sItemsToDisplay[i, 1]));
                    fNetPrice = TillEngine.TillEngine.FixFloatError((float)Convert.ToDecimal(sItemsToDisplay[i, 2]) * Convert.ToInt32(sItemsToDisplay[i, 3]));
                    if (fGrossPriceMultiplied < fNetPrice) // Some discount has been applied, so show this
                    {
                        fGross = (float)Convert.ToDecimal(sItemsToDisplay[i, 2]);
                        fNet = (float)Convert.ToDecimal(sItemsToDisplay[i, 1]);
                        nQuantityOfItem = Convert.ToInt32(sItemsToDisplay[i, 3]);
                        fDiscountAmount = ((fGross * nQuantityOfItem) - (fNet));
                        fDiscountAmount = tEngine.fFixFloatError(fDiscountAmount);
                        nTextWidth = WorkOutTextWidth("Discounted by " + TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(fDiscountAmount)));
                        nTextLeft = AlignPrice - nTextWidth;
                        g.DrawString("Discounted by " + TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(fDiscountAmount)), this.Font, new SolidBrush(this.ForeColor), new PointF((float)nTextLeft, (float)nCurrentTop));
                        nCurrentTop -= nFontHeight;
                        bItemDiscount = true;
                    }
                    else
                        bItemDiscount = false;

                    if (Math.Abs(Convert.ToInt32(sItemsToDisplay[i, 3])) != 1) // The quantity if more than one
                    {
                        nQuantityOfItem = Convert.ToInt32(sItemsToDisplay[i, 3]);
                        fGross = (float)Convert.ToDecimal(sItemsToDisplay[i, 2]);
                        fNet = (float)Convert.ToDecimal(sItemsToDisplay[i, 1]);
                        fGross = tEngine.fFixFloatError(fGross);
                        nTextWidth = WorkOutTextWidth(nQuantityOfItem.ToString() + " at " + TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(fGross)));
                        nTextLeft = AlignPrice - nTextWidth;
                        g.DrawString(nQuantityOfItem.ToString() + " at " + TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(fGross)), this.Font, new SolidBrush(this.ForeColor), new PointF((float)nTextLeft, (float)nCurrentTop));
                        nCurrentTop -= nFontHeight;
                        bItemMultipleQuantity = true;
                    }
                    else
                        bItemMultipleQuantity = false;

                    // Show the item number
                    sItemNumToDisplay = i.ToString();
                    if (i < 10)
                        sItemNumToDisplay = "0" + sItemNumToDisplay;
                    nTextLeft = (AlignDescription / 2) -(WorkOutTextWidth(sItemNumToDisplay) / 2);
                    g.DrawString(sItemNumToDisplay, this.Font, new SolidBrush(this.ForeColor), new PointF((float)nTextLeft, (float)nCurrentTop));

                    // Show the item description
                    g.DrawString(sItemsToDisplay[i, 0], this.Font, new SolidBrush(this.ForeColor), new PointF((float)AlignDescription, (float)nCurrentTop));

                    // Show the item price
                    nTempTop = nCurrentTop;
                    if (bItemMultipleQuantity)
                        nTempTop += nFontHeight;
                    if (bItemDiscount)
                        nTempTop += nFontHeight;
                    fGrossAmnt = (float)Convert.ToDecimal(sItemsToDisplay[i, 2]);
                    fNetAmnt = (float)Convert.ToDecimal(sItemsToDisplay[i, 1]);
                    nQuantity = Convert.ToInt32(sItemsToDisplay[i, 3]);
                    fDAmount = ((fGrossAmnt * nQuantity) - (fNetAmnt));
                    nTextLeft = this.Width - WorkOutTextWidth(TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(((float)Convert.ToDouble(sItemsToDisplay[i, 2]) * nQuantity) - fDAmount)));
                    g.DrawString(TillEngine.TillEngine.FormatMoneyForDisplay(tEngine.fFixFloatError(((float)Convert.ToDouble(sItemsToDisplay[i, 2]) * nQuantity) - fDAmount)), this.Font, new SolidBrush(this.ForeColor), new PointF((float)nTextLeft, (float)nTempTop));
                    nCurrentTop -= nFontHeight;
                }
            }
        
        }

        public void UpdateTransaction(ref TillEngine.TillEngine te, string[] sNonItems)
        {
            tEngine = te;
            sNonItemDisplayArray = sNonItems;
        }

        private int WorkOutTextWidth(string sText)
        {
            return Convert.ToInt32(this.CreateGraphics().MeasureString(sText, this.Font, this.Width, StringFormat.GenericTypographic).Width) + 20;
        }
    }
}
