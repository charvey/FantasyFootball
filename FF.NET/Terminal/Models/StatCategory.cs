using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models
{
    class StatCategory
    {
        public int Id;
        public string Name;
        public string Position;

        private static Func<IReadOnlyDictionary<string, string>, string> NotImplemented = p => "Not Implemented";

        private static Dictionary<string, Func<IReadOnlyDictionary<string, string>, int>> IntermediateMaps = new Dictionary<string, Func<IReadOnlyDictionary<string, string>, int>>
        {
            {"Defensive Points Allowed",p=>p["Total Points Allowed"].ToOrDefault<int>()-p["Offensive Points Allowed"].ToOrDefault<int>()}
        };

        public static Dictionary<string, Func<IReadOnlyDictionary<string, string>, string>> Maps = new Dictionary<string, Func<IReadOnlyDictionary<string, string>, string>>
        {
            {"Targets",NotImplemented},
            {"Receptions",NotImplemented},
            {"Rushing Attempts",NotImplemented},
            {"Passing Yards",p=>p["Passing Yards"]},
            {"Passing Touchdowns",p=>p["Passing Touchdowns"]},
            {"Interceptions",p=>p["Interceptions"]},
            {"Rushing Yards",p=>p["Rushing Yards"]},
            {"Rushing Touchdowns",p=>p["Rushing Touchdowns"]},
            {"Reception Yards",p=>p["Reception Yards"]},
            {"Reception Touchdowns",p=>p["Reception Touchdowns"]},
            {"Return Touchdowns",NotImplemented},
            {"2-Point Conversions",p=>p["2-Point Conversions"]},
            {"Fumbles Lost",NotImplemented},
            {"40+ Yard Completions",NotImplemented},
            {"40+ Yard Passing Touchdowns",NotImplemented},
            {"40+ Yard Run",NotImplemented},
            {"40+ Yard Rushing Touchdowns",NotImplemented},
            {"40+ Yard Receptions",NotImplemented},
            {"40+ Yard Reception Touchdowns",NotImplemented},
            {"Offensive Fumble Return TD",NotImplemented},
            {"Field Goals 0-19 Yards",p=>p["Field Goals Made 0-19 Yards"]},
            {"Field Goals 20-29 Yards",p=>p["Field Goals Made 20-29 Yards"]},
            {"Field Goals 30-39 Yards",p=>p["Field Goals Made 30-39 Yards"]},
            {"Field Goals 40-49 Yards",p=>p["Field Goals Made 40-49 Yards"]},
            {"Field Goals 50+ Yards",p=>p["Field Goals Made 50+ Yards"]},
            {"Field Goals Missed 0-19 Yards",p=>p["Field Goals Missed 0-19 Yards"]},
            {"Point After Attempt Made",p=>p["Point After Attempt Made"]},
            {"Point After Attempt Missed",p=>p["Point After Attempt Missed"]},
            {"Points Allowed",NotImplemented},
            {"Sack",NotImplemented},
            {"Interception",NotImplemented},
            {"Fumble Recovery",NotImplemented},
            {"Touchdown",NotImplemented},
            {"Safety",p=>p["Safety"]},
            {"Block Kick",p=>p["Block Kick"]},
            {"Kickoff and Punt Return Touchdowns",NotImplemented},
            {"Points Allowed 0 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+(pa==0?1:0);}},
            {"Points Allowed 1-6 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((1<=pa&&pa<=6)?1:0);}},
            {"Points Allowed 7-13 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((7<=pa&&pa<=13)?1:0);}},
            {"Points Allowed 14-20 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((14<=pa&&pa<=20)?1:0);}},
            {"Points Allowed 21-27 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((21<=pa&&pa<=27)?1:0);}},
            {"Points Allowed 28-34 points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((28<=pa&&pa<=34)?1:0);}},
            {"Points Allowed 35+ points",p=>{int pa=IntermediateMaps["Defensive Points Allowed"](p);return ""+((35<=pa)?1:0);}}
        };
    }
}
