using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CST350_Milestone1.Models
{
    public class PlayerStats : IComparable<PlayerStats>
    {
        public string PlayerId { get; set; }
        public string name { get; set; }
        // Time taken to complete game (in seconds)
        public decimal time { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Total { get; set; }
        public DateTime AverageTime { get; set; }
        public DateTime BestTime { get; set; }

        public PlayerStats() { }

        public PlayerStats(string n, decimal t, int w, int l, int total, DateTime avg, DateTime best)
        {
            name = n;
            time = t;
            Wins = w;
            Losses = l;
            Total = total;
            AverageTime = avg;
            BestTime = best;
        }

        public int CompareTo(PlayerStats other)
        {
            return time.CompareTo(time);
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", name, time);
        }
    }
}
