using System;
using System.Collections.Generic;
using System.Text;

namespace AbnLookup
{
    public class Business
    {
        // Length = 11 characters
        public string Abn { get; internal set; }
        // Max length = 200 characters
        public string Name { get; internal set; }
        // Length = 3 characters
        public string State { get; internal set; }
        // Max length = 12 characters
        public string Postcode { get; internal set; }

        // Range 0-100
        /// <summary>Each match is awarded a score based on how closely it matches the search criteria.</summary>
        public int Score { get; internal set; }
    }
}
