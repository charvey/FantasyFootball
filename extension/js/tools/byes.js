function fillByes(){
	var byes = {
		Ari:4,Cin:4,Cle:4,Den:4,Sea:4,StL:4,
		Mia:5,Oak:5,
		KC:6,NO:6,
		Phi:7,TB:7,
		NYG:8,SF:8,
		Atl:9,Buf:9,Chi:9,Det:9,GB:9,Ten:9,
		Hou:10,Ind:10,Min:10,NE:10,SD:10,Was:10,
		Bal:11,Dal:11,Jax:11,NYJ:11,
		Car:12,Pit:12
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