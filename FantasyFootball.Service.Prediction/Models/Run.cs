using System;
using System.Collections.Generic;

namespace FantasyFootball.Service.Prediction.Models
{
    public class Run
    {
        public Guid Id { get; set; }
        public virtual Model Model { get; set; }
        public virtual ICollection<Prediction> Predictions { get; set; }
    }
}
