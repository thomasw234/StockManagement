using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    /// <summary>
    /// Holds details such as the daily qty sold etc.
    /// </summary>
    public class ItemStatistics
    {
        private decimal _averageCost;
        private decimal _averageSales;
        private decimal _onOrderQuantity;
        private string _lastReceviedDate;

        private decimal _quantitySoldDaily;
        private decimal _grossSalesDaily;
        private decimal _netSalesDaily;
        private decimal _costOfGoodsSoldDaily;

        private decimal _quantitySoldWeekly;
        private decimal _grossSalesWeekly;
        private decimal _netSalesWeekly;
        private decimal _costOfGoodsSoldWeekly;

        private decimal _quantitySoldMonthly;
        private decimal _grossSalesMonthly;
        private decimal _netSalesMonthly;
        private decimal _costOfGoodsSoldMonthly;

        private decimal _quantitySoldYearly;
        private decimal _grossSalesYearly;
        private decimal _netSalesYearly;
        private decimal _costOfGoodsSoldYearly;

        private decimal _openingStockLevel;
        private decimal _deliveredQuantity;
        private decimal _deliveredCost;

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

        public decimal AverageSales
        {
            get
            {
                return _averageSales;
            }
            set
            {

                _averageSales = value;
            }
        }

        public decimal QuantityOnOrder
        {
            get
            {
                return _onOrderQuantity;
            }
            set
            {
                _onOrderQuantity = value;
            }
        }

        public string LastReceivedAsString
        {
            get
            {
                return _lastReceviedDate;
            }
            set
            {
                _lastReceviedDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the last received date as a DateTime object
        /// Returns DateTime of date 0/0/0000 if not possible
        /// </summary>
        public DateTime LastReceivedAsDate
        {
            get
            {
                try {
                    DateTime toReturn = new DateTime(Convert.ToInt32(_lastReceviedDate[0].ToString() + _lastReceviedDate[1].ToString()),
                                                    Convert.ToInt32(_lastReceviedDate[2].ToString() + _lastReceviedDate[3].ToString()),
                                                    Convert.ToInt32(_lastReceviedDate[3].ToString() + _lastReceviedDate[4].ToString()) + 2000);
                    return toReturn;
                
                    }
                catch {
                    return new DateTime(0,0,0);
                }
            }
            set
            {
                if (value.Day < 10)
                    _lastReceviedDate = "0";
                _lastReceviedDate += value.Day.ToString();

                if (value.Month < 10)
                    _lastReceviedDate += "0";
                _lastReceviedDate += value.Month.ToString();

                _lastReceviedDate += value.Year.ToString()[2].ToString() + value.Year.ToString()[3].ToString();
            }
        }

        public decimal QuantitySoldDaily
        {
            get
            {
                return _quantitySoldDaily;
            }
            set
            {
                _quantitySoldDaily = value;
            }
        }

        public decimal GrossSalesDaily
        {
            get
            {
                return _grossSalesDaily;
            }
            set
            {
                _grossSalesDaily = value;
            }
        }

        public decimal NetSalesDaily
        {
            get
            {
                return _netSalesDaily;
            }
            set
            {
                _netSalesDaily = value;
            }
        }

        public decimal CostOfGoodsDaily
        {
            get
            {
                return _costOfGoodsSoldDaily;
            }
            set
            {
                _costOfGoodsSoldDaily = value;
            }
        }

        public decimal QuantitySoldWeekly
        {
            get
            {
                return _quantitySoldWeekly;
            }
            set
            {
                _quantitySoldWeekly = value;
            }
        }

        public decimal GrossSalesWeekly
        {
            get
            {
                return _grossSalesWeekly;
            }
            set
            {
                _grossSalesWeekly = value;
            }
        }

        public decimal NetSalesWeekly
        {
            get
            {
                return _netSalesWeekly;
            }
            set
            {
                _netSalesWeekly = value;
            }
        }

        public decimal CostOfGoodsWeekly
        {
            get
            {
                return _costOfGoodsSoldWeekly;
            }
            set
            {
                _costOfGoodsSoldWeekly = value;
            }
        }

        public decimal QuantitySoldMonthly
        {
            get
            {
                return _quantitySoldMonthly;
            }
            set
            {
                _quantitySoldMonthly = value;
            }
        }

        public decimal GrossSalesMonthly
        {
            get
            {
                return _grossSalesMonthly;
            }
            set
            {
                _grossSalesMonthly = value;
            }
        }

        public decimal NetSalesMonthly
        {
            get
            {
                return _netSalesMonthly;
            }
            set
            {
                _netSalesMonthly = value;
            }
        }

        public decimal CostOfGoodsMonthly
        {
            get
            {
                return _costOfGoodsSoldMonthly;
            }
            set
            {
                _costOfGoodsSoldMonthly = value;
            }
        }

        public decimal QuantitySoldYearly
        {
            get
            {
                return _quantitySoldYearly;
            }
            set
            {
                _quantitySoldYearly = value;
            }
        }

        public decimal GrossSalesYearly
        {
            get
            {
                return _grossSalesYearly;
            }
            set
            {
                _grossSalesYearly = value;
            }
        }

        public decimal NetSalesYearly
        {
            get
            {
                return _netSalesYearly;
            }
            set
            {
                _netSalesYearly = value;
            }
        }

        public decimal CostOfGoodsYearly
        {
            get
            {
                return _costOfGoodsSoldYearly;
            }
            set
            {
                _costOfGoodsSoldYearly = value;
            }
        }

        public decimal OpeningStockLevel
        {
            get
            {
                return _openingStockLevel;
            }
            set
            {
                _openingStockLevel = value;
            }
        }

        public decimal DeliveredQuantity
        {
            get
            {
                return _deliveredQuantity;
            }
            set
            {
                _deliveredQuantity = value;
            }
        }

        public decimal DeliveredCost
        {
            get
            {
                return _deliveredCost;
            }
            set
            {
                _deliveredCost = value;
            }
        }
    }
}
