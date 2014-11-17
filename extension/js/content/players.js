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
		
		var nameTag=c[1].querySelector("a.Nowrap");
		var id=getId(nameTag.href);
		var n=nameTag.innerText;
		
		var pointTag=c[4];
		var s=null;
		if(pointTag.innerText.trim()=="–"){
			s=0;
		} else {
			s=parseFloat(pointTag.innerText);
		}

		stats.push({
			Id:id,
			Name:n,
			Team:t,
			Position:p,
			StatValue:s
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

function addAutoTraverseButton(){
	addButton("Auto Traverse",function(){
		loadData(function(data){
			var r=nextTraverse(data);
			alert(r);
		});
	});
}

addTraverseButton();
addAutoTraverseButton();