document.addEventListener('DOMContentLoaded', function () {
	loadData(function(data){
		var w=getWeek();

		var s="";
		for(var r in data.Players){
			var p=data.Players[r];

			s+= p.Id+","+p.Name+","+p.Team+","+p.Position;

			for(var i=1; i<w; i++){
				s+=","+p.Stats[0][i-1];
			}
			for(var i=w; i<=17; i++){
				s+=","+p.Stats[w][i-w];
			}

			s+="\n";
		}

		var div=document.querySelector("#data");
		div.innerText=s;

		saveData(data);
	});
});


