using System;
using System.Collections.Generic;

namespace FantasyFootball.Service.Prediction.Models
{
    public class Model
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
    }
}
