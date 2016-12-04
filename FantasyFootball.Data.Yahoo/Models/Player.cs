﻿using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("player")]
    public class Player
    {
        public string player_key;
        public string player_id;
        public PlayerName name;
        public Week[] bye_weeks;
        public string image_url;
        public string editorial_team_abbr;
        public string display_position;
        public Position[] eligible_positions;
        public SelectedPosition selected_position;
        public TransactionData transactioin_data;
    }

    [XmlType("name")]
    public class PlayerName
    {
        public string full;
        public string first;
        public string last;
        public string ascii_first;
        public string ascii_last;
    }

    [XmlType("position")]
    public class Position
    {
        [XmlText]
        public string value;
    }

    [XmlType("week")]
    public class Week
    {
        [XmlText]
        public int value;
    }

    [XmlType("selected_position")]
    public class SelectedPosition
    {
        public string position;
    }

    [XmlType("transaction_data")]
    public class TransactionData
    {
        public string type;
        public string source_type;
        public string source_team_key;
        public string source_team_name;
        public string destination_type;
        public string destination_team_key;
        public string destination_team_name;
    }
}
