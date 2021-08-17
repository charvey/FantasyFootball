﻿using ClickyDraft;
using FantasyFootball.Draft.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FantasyFootball.Draft.ClickyDraft
{
    public class ClickyDraftDraft : IDraft
    {
        private readonly PlayerIdConverter playerIdConverter;
        private readonly ClickyDraftService clickyDraftService;
        private readonly int leagueId;
        private readonly int leagueInstanceId;

        public ClickyDraftDraft(int leagueId, int leagueInstanceId)
        {
            clickyDraftService = new ClickyDraftService();
            playerIdConverter = new PlayerIdConverter();
            this.leagueId = leagueId;
            this.leagueInstanceId = leagueInstanceId;
        }

        public IReadOnlyList<DraftParticipant> Participants
        {
            get
            {
                var league = clickyDraftService.League(leagueId, leagueInstanceId);

                return league.FantasyTeams.Select(ft => new DraftParticipant
                (
                    Id: ft.Id.ToString(),
                    Owner: league.LeagueUsers.Single(lu => lu.Id == ft.LeagueUserId).DisplayName,
                    Name: ft.TeamName,
                    Order: ft.DraftPosition
                )).ToList();
            }
        }

        public IReadOnlyList<Player> AllPlayers
        {
            get
            {
                var players = clickyDraftService.DraftablePlayers(leagueId, leagueInstanceId);

                return players.Select(p => new Player
                (
                    Id: playerIdConverter.Convert(p.Id),
                    Name: HttpUtility.HtmlDecode((p.FirstName + " " + p.LastName).Trim()),
                    Positions: p.Positions,
                    Team: p.TeamFullName
                )).ToList();
            }
        }

        public IReadOnlyList<Player> PickedPlayers => throw new NotImplementedException();

        public IReadOnlyList<Player> UnpickedPlayers => throw new NotImplementedException();

        public DraftParticipant ParticipantByPlayer(Player player)
        {
            var pick = clickyDraftService.Picks(leagueId, leagueInstanceId).SingleOrDefault(p => playerIdConverter.Convert(p.DraftablePlayer.Id) == player.Id);

            if (pick == null)
                return null;

            return Participants.Single(p => p.Id == pick.FantasyTeamId.ToString());
        }

        public Player Pick(DraftParticipant t, int r)
        {
            throw new NotImplementedException();
        }

        public void Pick(DraftParticipant t, int r, Player p)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t)
        {
            var picks = clickyDraftService.Picks(leagueId, leagueInstanceId).Where(p => p.FantasyTeamId.ToString() == t.Id);

            return picks.Select(pi => AllPlayers.Single(pl => pl.Id == playerIdConverter.Convert(pi.DraftablePlayer.Id))).ToList();
        }
    }
}