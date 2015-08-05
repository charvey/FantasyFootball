using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace FantasyFootball.Terminal
{
    public class Program
    {
        public void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            
            var x = new FantasySportsService();
            foreach(var game in x.Games)
            {
                Console.WriteLine(game.name);
                foreach(var player in x.Players(game.game_key))
                {
                    Console.WriteLine(player);
                }
            }
            //Console.WriteLine(x.Players());

            Console.Read();
        }
    }
}
