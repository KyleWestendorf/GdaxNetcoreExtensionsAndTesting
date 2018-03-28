using System.Collections.Generic;

namespace WpfApp1
{
    class Coin
    {
        public string sequence { get; set; }
        public List<List<object>> bids { get; set; }
        public List<List<object>> asks { get; set; }
    }
}