using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class DraftViewModel
    {
        public Team[] Teams { get; set; }
        public int Rounds { get; set; }
    }
}