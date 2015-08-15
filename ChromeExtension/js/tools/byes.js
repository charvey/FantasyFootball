function fillByes(){
	var byes = {
		Ne:4,Ten:4,
		Car:5,Mia:5,Min:5,NYJ:5,
		Dal:6,Oak:6,StL:6,TB:6,
		Chi:7,Cin:7,Den:7,GB:7,
		Buf:8,Jax:8,Phi:8,Was:8,
		Ari:9,Bal:9,Det:9,Hou:9,KC:9,Sea:9,
		Atl:10,Ind:10,SD:10,SF:10,
		Cle:11,NO:11,NYG:11,Pit:11
	};

	loadData(function(data){
		var w=getWeek();

		for(var i in data.Players){
			var p=data.Players[i];
			var x=p.Stats[w][byes[p.Team]-w];

			if(x!=null && x!=0) {
				console.log(
					p.Id,
					p.Team,
					byes[p.Team],
					p.Stats[w][byes[p.Team]-w]
				);
			} else {
				p.Stats[w][byes[p.Team]-w]=0;
			}
		}
		saveData(data);
	});
}