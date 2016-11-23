function addIds(){
	var names = document.querySelectorAll("a.name");
	for(var i=0;i<names.length;i++){
		var url=names[i].href;
		url=url.substr(url.lastIndexOf("/")+1);
		names[i].innerText+=" - "+url;
	}
}

function missingData(w){
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
				console.log(true, i);
				return;
			}
		}

		console.log(false);
	});
}