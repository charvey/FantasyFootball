
console.log("common.js");

function getWeek(){
	var tuesdayBeforeFirstGame=new Date(2016,8,6);
	var rightNow=new Date();
	var millisecondsSinceSeasonStart=rightNow-tuesdayBeforeFirstGame;
	var weeksSinceSeasonStart=Math.ceil(millisecondsSinceSeasonStart/(1000*60*60*24*7));
	return Math.max(1,Math.min(17,weeksSinceSeasonStart));
}

function percentError(a,p){
	return 100*(a-p)/((a+p)/2);
}

String.prototype.trim=function(){return this.replace(/(?:(?:^|\n)\s+|\s+(?:$|\n))/g,'').replace(/\s+/g,' ');};

function clearChildren(node){while(node.hasChildNodes()) node.removeChild(node.lastChild);}

function getId(url){
	return url.substr(url.lastIndexOf("/")+1);
}

function loadData(callback){
	chrome.storage.local.get("data",function(r){
				data=r.data;
				if(data==null){
					data={
						Players:{}
					};
				}
				sanitize(data);
				callback(data);
	});
}
function saveData(data){
	chrome.storage.local.set({"data":data});
}

function sanitize(data){
	// Sanitize Players
	{
		var d=[];
		for(var i in data.Players){
			var good=false;
			for(var j=0;j<17;j++){
				var x=data.Players[i].Stats[0][j];
				if(x!=null && x!=0){
					good=true;
					break;
				}
			}
			if(good) continue;
			for(var j=1; j<=17; j++){
				for(var k=j; k<=17; k++){
					var x=data.Players[i].Stats[j][k-j];
					if(x!=null && x!=0){
						good=true;
						break;
					}
				}
				if(good) break;
			}
			if(!good) d.push(i);
		}
		var r=[28543];//"6913","6359","7286"];
		for(var i=0;i<d.length;i++){
			if(r.every(function(e){return e!=d[i];})){
				delete data.Players[d[i]];
			}
		}
	}
}

function initPlayer(){
	var stats = new Array(18);

	stats[0]=[];
	for(var i=1;i<=17;i++){
		stats[0].push(null);
		stats[i]=[];
		for(var j=i;j<=17;j++){
			stats[i].push(null);
		}
	}

	return {
			Id: "",
			Name: "",
			Team: "",
			Position: "",
			Stats: stats
	};
}

function getActuals(player, w){
	w=w||getWeek();

	return player.Stats[0].slice(0, w-1);
}

function getActual(player, w){
	if(w<1 || 17<w) throw("Week must be between 1 and 17");
	if(w>=getWeek()) throw("Actual not known yet.");

	return getActuals(player)[w-1];
}

function getProjections(player, w){
	w=w||getWeek();

	return player.Stats[w];
}

function getWholeWeek(player, w){
	w=w||getWeek();

	var a=getActuals(player, w);
	var p=getProjections(player, w);

	return a.concat(p);
}

var TEAMS=["Ari", "Atl", "Bal", "Buf", "Car", "Chi", "Cin", "Cle", "Dal", "Den", "Det", "GB", "Hou", "Ind", "Jax", "KC", "LA", "Mia", "Min", "NE", "NO", "NYG", "NYJ", "Oak", "Phi", "Pit", "SD", "SF", "Sea", "TB", "Ten", "Was"];