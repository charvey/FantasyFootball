function presentData(map){
	loadData(function(data){
		var t=document.createElement("table");
		for(var i in data.Players){
			var player=data.Players[i];

			if(!map.preFilter(player)) continue;

			var v=map.mapPlayer(player);

			if(!map.postFilter(v)) continue;

			var r=document.createElement("tr");
			for(var j=0; j<v.length; j++){
				var c=document.createElement("td");
				c.innerText=v[j];
				r.appendChild(c);
			}
			t.appendChild(r);
		}
		
		var div=document.querySelector("#data");
		clearChildren(div);
		div.appendChild(t);
	});
}

document.addEventListener('DOMContentLoaded', function () {
	bind();

	document.querySelector("#run").onclick=function() {
		bind();
	};
});


var map={
	preFilter:function(player){
		return true;
	},
	mapPlayer:function(player){
		var ret=[player.Id,player.Name];
		return ret;
	},
	postFilter:function(player){
		return true;
	}
};

function bind(){
	console.log(map);

	presentData(map);
}
