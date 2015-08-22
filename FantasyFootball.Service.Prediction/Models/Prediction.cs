﻿using System;

namespace FantasyFootball.Service.Prediction.Models
{
    public class Prediction
    {
        public Guid Id { get; set; }
        public double Value { get; set; }
        public int Week { get; set; }
        public string PlayerId { get; set; }
        public string Model { get; set; }
        public string Run { get; set; }
    }
}
