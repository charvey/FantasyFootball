document.addEventListener('DOMContentLoaded', function () {
	loadData(function(data){
		var W=getWeek();

		var h="id,Position";
		for(var w=1; w<=W-1; w++){
			h+=",A-"+w;
		}
		for(var w=1; w<=W; w++){
			for(var o=w; o<=17; o++){
				h+=",P-"+w+"-"+o;
			}
		}

		document.querySelector("#title").innerText=h;

		var s="";
		for(var r in data.Players){
			var player=data.Players[r];

			s+= player.Id+","+player.Position;

			var a=getActuals(player);
			for(var i=1; i<W; i++){
				s+=","+a[i-1];
			}
			for(var i=1; i<=W; i++){
				var p=getProjections(player,i);
				for(var j=i; j<=17; j++){
					s+=","+p[j-i];
				}
			}

			s+="\n";
		}

		var div=document.querySelector("#data");
		div.innerText=s;
	});
});


