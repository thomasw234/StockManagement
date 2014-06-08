using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects
{
    public class Description
    {
        // Internal storage of the item's description
        private string item_description = String.Empty;

        /// <summary>
        /// Initialises a new description
        /// </summary>
        /// <param name="description">The description to use</param>
        public Description(string description)
        {
            this.item_description = description;
        }

        /// <summary>
        /// Accessor for item_description
        /// </summary>
        public string _Description
        {
            get
            {
                return item_description;
            }
            set
            {
                // If the length of the description is over 30, just use the first 30 characters
                if (value.Length > 30)
                {
                    item_description = String.Empty;
                    for (int i = 0; i < 30; i++)
                        item_description += value[i];
                }
                else
                {
                    item_description = value;
                }
            }
        }

    }
}
