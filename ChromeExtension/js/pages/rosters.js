document.addEventListener('DOMContentLoaded', function () {
	loadData(function(data){
		var s="";
		console.log(data.Rosters);
		for(var r in data.Rosters){
			r=data.Rosters[r];
			s+= r.Id+","+r.Team+"\n";
		}

		var div=document.querySelector("#data");
		div.innerText=s;
	});
});


