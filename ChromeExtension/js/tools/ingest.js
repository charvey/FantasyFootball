function ingest(){
	saveData(null);

	loadData(function(data){
		for(var i=0;i<newPlayers.length;i++){
			data.Players[newPlayers[i][0]]=initPlayer();
			data.Players[newPlayers[i][0]].Id=newPlayers[i][0];

			var player=data.Players[newPlayers[i][0]];

			for(var j=1;j<=17;j++){
				player.Stats[1][j-1]=newPlayers[i][j];
			}
		}

		saveData(data);
		console.log("Done");
	});
}