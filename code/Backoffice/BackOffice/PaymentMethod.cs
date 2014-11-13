using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
    public class PaymentMethod
    {
        /// <summary>
        /// The code of the payment method
        /// </summary>
        string sPMName;
        /// <summary>
        /// The amount paid
        /// </summary>
        decimal fAmount;
        /// <summary>
        /// The amount paid without change given
        /// If £6 is due, the customer may give £10. This variable will hold 10, fAmount will hold 6 
        /// </summary>
        decimal fGross = 0.0m;

        /// <summary>
        /// Sets up the payment method
        /// </summary>
        /// <param name="name">The code of the payment method</param>
        /// <param name="flAmount">The amount paid using this payment method</param>
        public void SetPaymentMethod(string name, decimal flAmount)
        {
            sPMName = name;
            fAmount = flAmount;
        }
        /// <summary>
        /// Sets up the payment method
        /// </summary>
        /// <param name="name">The code of the payment method</param>
        /// <param name="flAmount">The final amount paid</param>
        /// <param name="flGross">The gross amount paid (possibly in excess of the amount due)</param>
        public void SetPaymentMethod(string name, decimal flAmount, decimal flGross)
        {
            sPMName = name;
            fAmount = flAmount;
            fGross = flGross + flAmount;
        }

        /// <summary>
        /// The amount paid using this payment method
        /// </summary>
        public decimal Amount
        {
            get
            {
                return fAmount;
            }
        }

        /// <summary>
        /// The total amount received on this payment method, including excess
        /// </summary>
        public decimal Excess
        {
            get
            {
                return fGross;
            }
        }

        /// <summary>
        /// Gets the payment code
        /// </summary>
        public string PMType
        {
            get
            {
                return sPMName;
            }
        }
    }
}
