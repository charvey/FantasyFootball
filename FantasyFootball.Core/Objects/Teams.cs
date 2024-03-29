﻿namespace FantasyFootball.Core.Objects
{
    public static class Teams
    {
        public static Team From(FantasyFootball.Data.Yahoo.Models.Team team)
        {
            return new Team
            {
                Id = team.team_id,
                Owner = team.managers.Single().nickname,
                Name = team.name
            };
        }
    }
}
