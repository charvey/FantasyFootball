chrome.extension.onRequest.addListener(function(newData) {
	var currentWeek = getWeek();

	if(
		(newData.W<currentWeek && newData.T=="Projected") ||
		(currentWeek<=newData.W && newData.T=="Actual")
		){
		return;
	}
	
	loadData(function(data){
		var stats = newData.S;

		for(var i=0; i<stats.length; i++){
			var p=stats[i];
			
			if(data.Players[p.Id]==null){
				data.Players[p.Id]=initPlayer();
				data.Players[p.Id].Id=p.Id;
				data.LastModified=new Date().toLocaleString();
			}
		
			var player = data.Players[p.Id];

			player.Name = p.Name;
			player.Team = p.Team;
			player.Position = p.Position;

			var oldValue=null;
			if(newData.T=="Actual"){
				oldValue=player.Stats[0][newData.W-1]
				player.Stats[0][newData.W-1]=p.StatValue;	
			} else if(newData.T=="Projected"){
				oldValue=player.Stats[currentWeek][newData.W-currentWeek];
				player.Stats[currentWeek][newData.W-currentWeek]=p.StatValue;
			}	

			if(oldValue!=p.StatValue)
				data.LastModified=new Date().toLocaleString();
		}

		saveData(data);
	});
});
