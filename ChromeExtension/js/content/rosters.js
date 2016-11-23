
var NUM_TEAMS=12;
var NUM_PLAYERS=15;

function getRosters(){
	var players=document.querySelectorAll("a.name");

	//if(players.length!=(NUM_PLAYERS*NUM_TEAMS))
		//throw "Wrong number of players";

	var results=new Array(NUM_TEAMS);
	var rosters=[];

var offset=0;

	for(var i=0; i<NUM_TEAMS; i++){
		results[i]=new Array(NUM_PLAYERS);
		for(var j=0; j<NUM_PLAYERS; j++){

			if(i==9&&j==NUM_PLAYERS-2){offset=-1;continue;}
try{
			var id=getId(players[NUM_PLAYERS*i+j+offset].href);

			results[i][j]=id;
			rosters.push({Id:id,Team:i+1});
			}
			catch(er){
				console.log(er);				
				console.log(i,j);
				console.log(players);
				console.log(NUM_PLAYERS*i+j);
				console.log(players[NUM_PLAYERS*i+j]);
			}
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