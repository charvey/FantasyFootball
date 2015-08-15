function selectedWeek(){
	var statType=document.getElementById("statselect").value;
	var week = parseInt(statType.substr(statType.lastIndexOf("_")+1));
	return week;
}


function traverse(s,e){
	if(s<1||17<s) throw("Wrong start week");
	if(e<1||17<e) throw("Wrong end week");
	if(s>e) return;

	var c=getWeek();
	var navTo=function(w){
		var s="S_";
		if(w<c) s+="W";
		else s+="PW";

		document.querySelector("#statselect").value=s+"_"+w;
		document.querySelector("input[value=Filter]").click();
	}

	var lastPage=function(){
		var next=document.querySelector(".pagingnavlist .last a");
		return next==null;
	};
	/*
	var _doneWeek=null;
	var doneWeek=function(w){
		if(_doneWeek==null){
			_doneWeek=[];
			for(var i=0;i<=17;i++) _doneWeek[i]=false;
		}

		loadData(function(data){
			if(w<1||17<w) throw("Wrong week");

			for(var i in data.Players){
				var x=null;
				if(w<getWeek()){
					x=data.Players[i].Stats[0][w-1];
				} else {
					x=data.Players[i].Stats[getWeek()][w-getWeek()];
				}
				if(x==null){
					_doneWeek[w]=false;
					return;
				}
			}
			_doneWeek[w]=true;
		});

		return _doneWeek[w];
	};
	*/
	function loop(w){
		if(selectedWeek()!=w){
			navTo(w);
			console.log("Move to week "+w);
		} else if(lastPage()/* || doneWeek(w)*/){
			w++;
			console.log("Will move to week "+w);
		} else {
			var next=document.querySelector(".pagingnavlist .last a");
			next.click();
			console.log("Continue in week "+w);
		}

		if(w>e) return;
		setTimeout(function(){
			loop(w);
		},(Math.random()*5+7.5)*1000);
	}

	loop(s);
}


function nextTraverse(data){
	var POSITIONS=["Off","QB","WR","RB","TE","W/R","W/R/T","K","DEF"];
	var POS_INDEX={};
	for(var i=0; i<POSITIONS.length; i++){POS_INDEX[POSITIONS[i]]=i;}

	var t=["*"].concat(TEAMS);
	var c={};
	for(var i=0; i<t.length; i++){
		var keyA=t[i]+"-";
		for(var j=0; j<POSITIONS.length; j++){
			var keyB=keyA+POSITIONS[j]+"-";
			for(var w=1; w<=17; w++){
				var keyC=keyB+w;
				c[keyC]={
					M:0, T:0
				};
			}
		}
	}

	var positions=[];
	for(var i=0; i<POSITIONS.length; i++){
		positions[POS_INDEX[POSITIONS[i]]]=false;
	}

	for(var i in data.Players){
		var player=data.Players[i];
		var d=getWholeWeek(player);

		var p=positions.slice(0);
		var l=player.Position.split('/');

		for(var j=0; j<l.length; j++){
			p[POS_INDEX[l[j]]]=true;
			if(l[j]=="QB" || l[j]=="WR" || l[j]=="RB" || l[j]=="TE"){
				p[POS_INDEX["Off"]]=true;
				if(l[j]!="QB"){
					p[POS_INDEX["W/R/T"]]=true;
					if(l[j]!="TE"){
						p[POS_INDEX["W/R"]]=true;
					}
				}
			}
		}

		{
			var keyA=player.Team+"-";
			var keyB="*"+"-";
			for(var k=0; k<p.length; k++){
				if(p[k]){
					var keyC=keyA+POSITIONS[k]+"-";
					var keyD=keyB+POSITIONS[k]+"-";
					for(var w=1; w<=17; w++){
						if(d[w-1]==null){
							c[keyC+w].M++;
							c[keyD+w].M++;
						}
						c[keyC+w].T++;
						c[keyD+w].T++;
					}
				}
			}
		}
	}

	var m=null;
	for(var i in c){
		if(m==null || (c[m].M/c[m].T)<(c[i].M/c[i].T)){
			m=i;
		}
	}

	if(c[m].M>0){
		return m;
	} else {
		return null;
	}
};