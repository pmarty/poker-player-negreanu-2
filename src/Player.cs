using System.Collections.Generic;

namespace Nancy.Simple
{
    public class Player
    {
        public int id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string version { get; set; }
        public int stack { get; set; }
        public int bet { get; set; }
        public List<Card> hole_cards { get; set; }
    }
}