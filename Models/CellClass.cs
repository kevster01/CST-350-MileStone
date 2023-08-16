using System;


namespace CST350_Milestone1.Models
{
    public class CellClass
    {
        public int Id { get; set; }
        public int CellState { get; set; } // two states visited and bomb/live
        public int CellPic { get; set; } // set for the picture
        public bool flagged { get; set; } // check if this cell is flagged
        public int LiveNeighbors { get; set; }
    }
}
