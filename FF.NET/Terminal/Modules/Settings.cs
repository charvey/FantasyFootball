using Newtonsoft.Json;
using Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Models;

namespace Terminal.Modules
{
    class Settings : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "Yahoo" }; }
        }

        Yahoo yahoo
        {
            get
            {
                return DependencyModules["Yahoo"] as Yahoo;
            }
        }

        public IEnumerable<StatCategory> StatCategories
        {
            get
            {
                DataSet statCategories = DataSet.fromCSV("StatCategories.csv");

                return statCategories.Rows.Select(r => new StatCategory
                {
                    Id = r["Id"].To<int>(),
                    Name = r["Name"],
                    Position = r["Position"]
                });
            }
        }

        protected override void Initialize()
        {
            string response = yahoo.GetCall("http://fantasysports.yahooapis.com/fantasy/v2/league/331.l.114425/settings?format=json");
            var settings = JsonConvert.DeserializeObject<dynamic>(response).fantasy_content.league[1].settings[0];

            ReadStatCategories(settings);
            ReadStatModifiers(settings);
            ReadRosterPositions(settings);
        }

        private void ReadStatCategories(dynamic settings)
        {
            string filename = "StatCategories.csv";
            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                DataSet statCategories = new DataSet();

                var stat_categories = settings.stat_categories;
                foreach (var stat in stat_categories.stats)
                {
                    int row = statCategories.Add();

                    string id = stat.stat.stat_id;
                    string name = stat.stat.name;
                    string pos = stat.stat.position_type;

                    statCategories[row, "Id"] = id;
                    statCategories[row, "Name"] = name;
                    statCategories[row, "Position"] = pos;
                }

                statCategories.toCSV(filename);
            }
        }

        private void ReadStatModifiers(dynamic settings)
        {
            string filename = "StatModifiers.csv";
            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                DataSet statModifiers = new DataSet();

                var stat_modifiers = settings.stat_modifiers;
                foreach (var stat in stat_modifiers.stats)
                {
                    int row = statModifiers.Add();

                    string id = stat.stat.stat_id;
                    string value = stat.stat.value;

                    statModifiers[row, "Id"] = id;
                    statModifiers[row, "Value"] = value;
                }

                statModifiers.toCSV(filename);
            }
        }

        private void ReadRosterPositions(dynamic settings)
        {
            string filename = "RosterPositions.csv";
            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                DataSet rosterPositions = new DataSet();

                var roster_positions = settings.roster_positions;
                foreach (var position in roster_positions)
                {
                    int row = rosterPositions.Add();

                    string pos = position.roster_position.position;
                    string count = position.roster_position.count;

                    rosterPositions[row, "Position"] = pos;
                    rosterPositions[row, "Count"] = count;
                }

                rosterPositions.toCSV(filename);
            }
        }
    }
}
