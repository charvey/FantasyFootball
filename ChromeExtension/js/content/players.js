function getPlayerName(row){
	var cells=row.getElementsByTagName("td");
	var nameTag=cells[1].querySelector("a.Nowrap");
	return nameTag.innerText;
}

function getPlayerId(row){
	var cells=row.getElementsByTagName("td");		
	var nameTag=cells[1].querySelector("a.Nowrap");
	return getId(nameTag.href);
}

function getStatValue(row){
	var cells=row.getElementsByTagName("td");	
	var pointTag=cells[6];
	if(pointTag.innerText.trim()=="–"){
		return 0;
	} else {
		return parseFloat(pointTag.innerText);
	}
}

function getStats(){
	var t=document.getElementById("players-table");
	var b=t.getElementsByTagName("tbody")[0];
	var r=b.getElementsByTagName("tr");

	var stats=[];
	for(var i=0; i<r.length; i++){
		var c=r[i].getElementsByTagName("td");

		var ptText=c[1].querySelector(".ysf-player-name span").innerText;
		var t=ptText.split('-')[0].trim();
		var p=ptText.split('-')[1].trim().replace(',','/');

		stats.push({
			Id:getPlayerId(r[i]),
			Name:getPlayerName(r[i]),
			Team:t,
			Position:p,
			StatValue:getStatValue(r[i])
		});
	}
	return stats;
}

var url="";
var miss=0;
function run(){
	if(document.location.search!=url){
		var statType=document.getElementById("statselect").value;
		var type="";
		var week=0;

		if(statType.indexOf("S_PW")!=-1){
			type="Projected";
		} else if(statType.indexOf("S_W")!=-1){
			type="Actual";
		}
		var week = parseInt(statType.substr(statType.lastIndexOf("_")+1));

		if((type=="Projected" || type=="Actual") && (1<=week && week<=17)){
			var stat = getStats();

			chrome.extension.sendRequest({
				T:type,
				W:week,
				S:stat
			});
			//console.log(type,week,stat);
		}

		url=document.location.search;
		miss=0;
	} else {
		miss++;
	}
	
	if(miss<1000){
		setTimeout(run,250);
	} else {
		console.log("Timed out");
	}
}

run();

function addButton(text,onclick){
	var nav=document.querySelector("#sitenavsub");
	var li=document.createElement("li");
	li.className="Navitem";
	var a=document.createElement("a");
	a.innerText=text;
	a.className="Navtarget";
	a.onclick=onclick;
	li.appendChild(a);
	nav.appendChild(li);
}

function addTraverseButton(){
	var traverseClick=function(){
		var s=parseInt(prompt("Start week",getWeek()));
		var e=parseInt(prompt("End week",17));
		traverse(s,e);
	}

	addButton("Traverse",traverseClick);
}

function addGetRecommnedationButton(){
	addButton("Get Recommendation",function(){
		loadData(function(data){
			var r=nextTraverse(data);
			if(r==null){
				alert("Done");
			} else {
				var recommendation=r.Team+"-"+r.Position+"-"+r.Week;
				alert(recommendation);
			}
		});
	});
}

function addAutoTraverseButton(){
	addButton("Auto Traverse",function(){
		function selectStatusFilter(team){
			switch (team) {
				case "*": return "ALL";
				case "Ari": return "ET_22";
				case "Atl": return "ET_1";
				case "Bal": return "ET_33";
				case "Buf": return "ET_2";
				case "Car": return "ET_29";
				case "Chi": return "ET_3";
				case "Cin": return "ET_4";
				case "Cle": return "ET_5";
				case "Dal": return "ET_6";
				case "Den": return "ET_7";
				case "Det": return "ET_8";
				case "GB": return "ET_9";
				case "Hou": return "ET_34";
				case "Ind": return "ET_11";
				case "Jax": return "ET_30";
				case "KC": return "ET_12";
				case "LA": return "ET_14";
				case "Mia": return "ET_15";
				case "Min": return "ET_16";
				case "NYG": return "ET_19";
				case "NYJ": return "ET_20";
				case "NE": return "ET_17";
				case "NO": return "ET_18";
				case "Oak": return "ET_13";
				case "Phi": return "ET_21";
				case "Pit": return "ET_23";
				case "SD": return "ET_24";
				case "SF": return "ET_25";
				case "Sea": return "ET_26";
				case "TB": return "ET_27";
				case "Ten": return "ET_10";
				case "Was": return "ET_28";
				default: throw "Team not found "+team;
			}
		}

		function selectPositionFilter(position){
			switch (position) {
				case "DEF": return "DEF";
				case "K": return "K";
				case "Off": return "O";
				case "QB": return "QB";
				case "RB": return "RB";
				case "TE": return "TE";
				case "WR": return "WR";
				default: throw "Position not found "+position;
			}
		}

		function selectStatFilter(week){
			return (week<getWeek()) ? "S_W_"+week	: "S_PW_"+week;
		}

		function scrapeRecommendation(){
			loadData(function(data){
				var r=nextTraverse(data);

				if(r==null){
					alert("Done");
					return;
				} else {
					console.log("Scraping "+r.Team+"-"+r.Position+"-"+r.Week);
					scrapeFilter(selectStatusFilter(r.Team),selectPositionFilter(r.Position),selectStatFilter(r.Week),scrapeRecommendation);
				}				
			});
		}

		scrapeRecommendation();
	});
}

addTraverseButton();
addGetRecommnedationButton();
addAutoTraverseButton();