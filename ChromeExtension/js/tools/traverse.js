function selectedWeek(){
	var statType=document.getElementById("statselect").value;
	var week = parseInt(statType.substr(statType.lastIndexOf("_")+1));
	return week;
}

function updateFilter(selectElementId,filter){
	var selectElement=document.querySelector("#"+selectElementId);
	selectElement.value=filter||selectElement.value;
}

function navigateTo(statusFilter,positionFilter,statsFilter){
	updateFilter("statusselect",statusFilter);
	updateFilter("posselect",positionFilter);
	updateFilter("statselect",statsFilter);
	document.querySelector("input[value=Filter]").click();
}

function onFilter(statusFilter,positionFilter,statsFilter){
	return document.querySelector("#statusselect").value==statusFilter
	&& document.querySelector("#posselect").value==positionFilter
	&& document.querySelector("#statselect").value==statsFilter;
}

function scrapeFilter(statusFilter,positionFilter,statsFilter,callback){
	var nextLink=document.querySelector(".pagingnavlist .last a");
	var isLastPage=nextLink==null;
	
	if(!onFilter(statusFilter,positionFilter,statsFilter)){
		navigateTo(statusFilter,positionFilter,statsFilter);
	}
	else if(isLastPage){
		callback();
		return;
	} else {
		nextLink.click();
	}

	setTimeout(function(){scrapeFilter(statusFilter,positionFilter,statsFilter,callback);},5000);
}

function traverse(s,e){
	if(s<1||17<s) throw("Wrong start week");
	if(e<1||17<e) throw("Wrong end week");
	
	function scrapeNextWeek(w){
		if(w>e) return;

		var statusFilter=document.querySelector("#statusselect").value;
		var positionFilter=document.querySelector("#posselect").value;
		var statFilter=(w<getWeek()) ? "S_W_"+w	: "S_PW_"+w;

		scrapeFilter(statusFilter,positionFilter,statFilter,function(){
			scrapeNextWeek(w+1);
		});
	}

	scrapeNextWeek(s);
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
					M:0, T:0,
					Team:t[i],Position:POSITIONS[j],Week:w
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
		return c[m];
	} else {
		return null;
	}
};