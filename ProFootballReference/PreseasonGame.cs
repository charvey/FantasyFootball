using System;

namespace ProFootballReference
{
    public class PreseasonGame
    {
        public int Week { get; internal set; }
        public DateTime Day { get; internal set; }
        public string VisTm { get; internal set; }
        public int VisTmPts { get; internal set; }
        public string HomeTm { get; internal set; }
        public int HomeTmPts { get; internal set; }
    }
}
