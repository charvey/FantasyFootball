using FantasyPros.Projections;
using HtmlAgilityPack;
using System.Diagnostics;

namespace FantasyPros
{
    internal static class ProjectionPageParser
    {
        public static DateTime ParseTime(HtmlDocument document)
        {
            var time = document.DocumentNode.Descendants("time").Single();
            return DateTime.Parse(time.GetAttributeValue("datetime", ""));
        }

        public static IEnumerable<Tuple<FantasyProsPlayerId, Projection>> ParseProjections(HtmlDocument document)
        {
            var table = document.GetElementbyId("data");
            var headers = table.Element("thead").Elements("tr").Select(tr => tr.Elements("th").Select(n => n.InnerText).ToArray()).ToArray();

            var title = document.DocumentNode.Element("html").Element("head").Element("title").InnerText;
            title = title.Split('|')[0].Trim();
            var forPos = title.Substring(title.LastIndexOf(" for ") + 5);

            var rows = table.Element("tbody").Elements("tr");
            foreach (var row in rows)
            {
                var idClass = row.GetClasses().Single();
                var id = int.Parse(idClass.Replace("mpb-player-", ""));

                var cells = row.Elements("td").Select(n => n.InnerText).ToArray();

                if (forPos == "Defense & Special Teams")
                {
                    yield return Tuple.Create(
                        new FantasyProsPlayerId(id),
                        new DstProjection
                        {
                            Sacks = float.Parse(cells[1]),
                            Interceptions = float.Parse(cells[2]),
                            FumbleRecovery = float.Parse(cells[3]),
                            ForcedFumble = float.Parse(cells[4]),
                            Touchdowns = float.Parse(cells[5]),
                            //Assist______ = float.Parse(cells[]),
                            Safeties = float.Parse(cells[6]),
                            PointsAgaints = float.Parse(cells[7]),
                            YardsAgainst = float.Parse(cells[8]),
                            FantasyPoints = float.Parse(cells[9])
                        } as Projection
                    );
                }
                else if (forPos == "Running Backs")
                {
                    yield return Tuple.Create(
                        new FantasyProsPlayerId(id),
                        new RbProjection
                        {
                            RushingAttempts = float.Parse(cells[1]),
                            RushingYards = float.Parse(cells[2]),
                            RushingTouchdowns = float.Parse(cells[3]),
                            Receptions = float.Parse(cells[4]),
                            ReceivingYards = float.Parse(cells[5]),
                            ReceivingTouchdowns = float.Parse(cells[6]),
                            Fumbles = float.Parse(cells[7]),
                            FantasyPoints = float.Parse(cells[8])
                        } as Projection
                    );
                }
                else if (forPos == "Tight Ends")
                {
                    yield return Tuple.Create(
                        new FantasyProsPlayerId(id),
                        new TeProjection
                        {
                            Receptions = float.Parse(cells[1]),
                            ReceivingYards = float.Parse(cells[2]),
                            ReceivingTouchdowns = float.Parse(cells[3]),
                            Fumbles = float.Parse(cells[4]),
                            FantasyPoints = float.Parse(cells[5])
                        } as Projection
                    );
                }
                else if (forPos == "Wide Receivers")
                {
                    yield return Tuple.Create(
                        new FantasyProsPlayerId(id),
                        new WrProjection
                        {
                            Receptions = float.Parse(cells[1]),
                            ReceivingYards = float.Parse(cells[2]),
                            ReceivingTouchdowns = float.Parse(cells[3]),
                            RushingAttempts = float.Parse(cells[4]),
                            RushingYards = float.Parse(cells[5]),
                            RushingTouchdowns = float.Parse(cells[6]),
                            Fumbles = float.Parse(cells[7]),
                            FantasyPoints = float.Parse(cells[8])
                        } as Projection
                    );
                }
                else if (forPos == "Quarterbacks")
                {
                    yield return Tuple.Create(
                        new FantasyProsPlayerId(id),
                        new QbProjection
                        {
                            PassingAttempts = float.Parse(cells[1]),
                            PassingCompletions = float.Parse(cells[2]),
                            PassingYards = float.Parse(cells[3]),
                            PassingTouchdowns = float.Parse(cells[4]),
                            Interceptions = float.Parse(cells[5]),
                            RushingAttempts = float.Parse(cells[6]),
                            RushingYards = float.Parse(cells[7]),
                            RushingTouchdowns = float.Parse(cells[8]),
                            Fumbles = float.Parse(cells[9]),
                            FantasyPoints = float.Parse(cells[10])
                        } as Projection
                    );
                }
                else if (forPos == "Kickers")
                {
                    //TODO
                    continue;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        internal static IEnumerable<FantasyProsPlayer> ParsePlayers(HtmlDocument document)
        {
            var table = document.GetElementbyId("data");
            var rows = table.Element("tbody").Elements("tr");
            foreach (var row in rows)
            {
                var idClass = row.GetClasses().Single();
                var id = int.Parse(idClass.Replace("mpb-player-", ""));
                var nameNode = row.Elements("td").First().Elements("a").First();
                Debug.Assert(nameNode.HasClass("player-name"));
                var name = nameNode.InnerText;
                if (!string.IsNullOrWhiteSpace(name))
                    yield return new FantasyProsPlayer(new FantasyProsPlayerId(id), name);
            }
        }
    }
}
