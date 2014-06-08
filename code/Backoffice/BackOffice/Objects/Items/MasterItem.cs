using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    public abstract class MasterItem
    {
        private string _barcode;
        private Description _description;
        private decimal _RRP;
        private decimal _lastCost;
        private decimal _averageCost;
        private VATRate _VATRate;
        private ItemStatistics _stats;
        private int _itemType;
        private string _lastSold;
        private string _penultimatelySold;

        /// <summary>
        /// Gets but can not set the barcode
        /// </summary>
        public string Barcode
        {
            get
            {
                return _barcode;
            }
        }

        /// <summary>
        /// Can get and set the description of the item
        /// </summary>
        public Description Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        /// <summary>
        /// Gets and sets the RRP of the item
        /// </summary>
        public decimal RRP
        {
            get
            {
                return _RRP;
            }
            set
            {
                _RRP = value;
            }
        }

        /// <summary>
        /// Gets and sets the last cost of this item
        /// </summary>
        public decimal LastCost
        {
            get
            {
                return _lastCost;
            }
            set
            {
                _lastCost = value;
            }
        }

        /// <summary>
        /// Gets and sets the average cost of this item
        /// </summary>
        public decimal AverageCost
        {
            get
            {
                return _averageCost;
            }
            set
            {
                _averageCost = value;
            }
        }

        /// <summary>
        /// Gets and sets the VAT Rate of this item
        /// </summary>
        public VATRate VATRate
        {
            get
            {
                return _VATRate;
            }
            set
            {
                _VATRate = value;
            }
        }

        public ItemStatistics Statistics
        {
            get
            {
                return _stats;
            }
            set
            {
                _stats = value;
            }
        }

        public int ItemType
        {
            get
            {
                return _itemType;
            }
            set
            {
                if (value > 0 && value < 7)
                    _itemType = value;
                else
                    throw new ArgumentOutOfRangeException("Item was an invalid type: " + value.ToString() + ". Barcode : " + _barcode);
            }
        }

        /// <summary>
        /// Gets a StockSta array of strings which represent a record
        /// </summary>
        /// <param name="sEngine">The stockengine which data is pulled from?</param>
        /// <returns></returns>
        string[] toStockStaRecord(StockEngine sEngine) { return new string[0]; }

        /// <summary>
        /// Gets a MainStock array of string which represent a record
        /// </summary>
        /// <param name="sEngine">The stockengine which data is pulled from</param>
        /// <returns></returns>
        string[] toMainstockRecord(StockEngine sEngine) { return new string[0]; }
    }
}
