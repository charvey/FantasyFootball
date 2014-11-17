
var NUM_TEAMS=12;
var NUM_PLAYERS=15;

function getRosters(){
	var players=document.querySelectorAll("a.name");

	var results=new Array(NUM_TEAMS);

	for(var i=0; i<NUM_TEAMS; i++){
		results[i]=new Array(NUM_PLAYERS);
		for(var j=0; j<NUM_PLAYERS; j++){
			results[i][j]=getId(players[NUM_PLAYERS*i+j].href);
		}
	}

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

console.log(getRosters());
printRosters(getRosters());