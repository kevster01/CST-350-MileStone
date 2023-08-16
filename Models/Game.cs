using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CST350_Milestone1.Models
{
    [Serializable()]
    public class Game
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Difficulty { get; set; }
        public int Result { get; set; }
        public int Time { get; set; }
        public int FlagsRemaining { get; set; }
        public int Status { get; set; }
        public Board State { get; set; }
    }
}
