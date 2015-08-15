function write(s){
	var div=document.createElement("div");
	div.innerText=s;
	document.body.appendChild(div);
}

document.addEventListener('DOMContentLoaded', function () {
	var links = document.querySelectorAll("a");

	for(var i=0; i<links.length; i++){
		var link=links[i];

		link.onclick=function(element){
			chrome.tabs.create({
				url: element.target.href
			});

			return false;
		}
	}
	
	loadData(function(data){
		write(Object.keys(data.Players).length+" Players");
		write(""+data.LastModified);
	});
});

