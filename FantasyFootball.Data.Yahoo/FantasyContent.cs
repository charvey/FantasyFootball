using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasyContentWrapper
    {
        public FantasyContent fantasy_content;
    }

    public class FantasyContent
    {
        public Game[] game;
        public Player[] player;
    }
}
