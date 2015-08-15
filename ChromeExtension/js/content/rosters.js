
var NUM_TEAMS=12;
var NUM_PLAYERS=15;

function getRosters(){
	var players=document.querySelectorAll("a.name");

	var results=new Array(NUM_TEAMS);
	var rosters=[];

	for(var i=0; i<NUM_TEAMS; i++){
		results[i]=new Array(NUM_PLAYERS);
		for(var j=0; j<NUM_PLAYERS; j++){
			var id=getId(players[NUM_PLAYERS*i+j].href);
			results[i][j]=id;
			rosters.push({Id:id,Team:i+1});
		}
	}

	loadData(function(data){
		console.log(rosters);
		data.Rosters=rosters;
		saveData(data);
	});

	return results;
}

function printRosters(roster){
	var s="";
	for(var t=1; t<=NUM_TEAMS; t++){
		for(var p=0; p<NUM_PLAYERS; p++){
			s+=roster[t-1][p]+","+t+"\n";
		}
	}
	console.log(s);
}

getRosters();
//printRosters(getRosters());