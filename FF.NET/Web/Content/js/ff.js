var ffApp=angular.module('ffApp',[]);

ffApp.controller('dataComparison', function($scope){
	$scope.Columns=[
		{Name:"Name"},
		{Name:"Total"}
	];
	var cats=["Passing Yards","Passing Touchdowns","Interceptions","Rushing Yards","Rushing Touchdowns","Reception Yards","Reception Touchdowns","Return Touchdowns","2-Point Conversions","Fumbles Lost","40+ Yard Completions","40+ Yard Passing Touchdowns","40+ Yard Rushing Attempts","40+ Yard Rushing Touchdowns","40+ Yard Receptions","40+ Yard Reception Touchdowns","Offensive Fumble Return TD"];
	for(var i=0; i<cats.length; i++){
		$scope.Columns.push({Name:cats[i]});
	}
	
	$scope.Data=[
		{Name:"Peyton Manning", Cells:[]},
		{Name:"Brett Favre",Cells:[]}
	];
	
	for(var i=0;i<$scope.Data.length; i++){
		var d=$scope.Data[i];
		d.Cells.push({Value:d.Name});
		for(var j=0; j<cats.length+1; j++){
			d.Cells.push({Value:'',Graph:true});
		}
	}
});

ffApp.directive('histogram', function(){
	return {
		link: function($scope, element, attrs){
			var pm=[284/25,223/25,404/25,274/25,210/25,284/25,313/25,290/25,237/25,235/25,198/25,260/25,186/25,298/25,276/25,163/25,227/25,273/25,367/25,440/25,187/25,334/25,281/25,268/25,288/25,302/25,210/25,294/25,209/25,339/25,132/25,206/25,283/25,194/25,231/25,421/25,196/25,241/25,335/25,201/25,199/25,253/25,262/25,370/25,310/25,173/25,325/25,228/25,195/25,191/25,211/25,289/25,272/25,224/25,284/25,304/25,214/25,327/25,319/25,252/25,229/25,190/25,297/25,277/25,365/25,146/25,137/25,211/25,173/25,216/25,314/25,386/25,293/25,269/25,266/25,347/25,401/25,229/25,278/25,228/25,290/25,146/25,220/25,377/25,304/25,237/25,256/25,254/25,393/25,220/25,198/25,368/25,472/25,268/25,320/25,211/25,236/25,425/25,298/25,249/25,383/25,6/25,458/25,238/25,254/25,122/25,228/25,264/25,255/25,191/25,237/25,321/25,297/25,365/25,245/25,187/25,324/25,336/25,116/25,5/25,290/25,276/25,400/25,219/25,217/25,166/25,342/25,345/25,326/25,236/25,254/25,183/25,351/25,313/25,282/25,205/25,282/25,268/25,170/25,349/25,247/25,288/25,312/25,273/25,193/25,253/25,259/25,255/25,225/25,328/25,163/25,272/25,288/25,249/25,276/25,311/25,95/25,402/25,257/25,311/25,216/25,247/25,271/25,229/25,223/25,254/25,240/25,320/25,255/25,125/25,277/25,318/25,364/25,95/25,310/25,301/25,303/25,379/25,353/25,309/25,235/25,347/25,318/25,327/25,299/25,244/25,270/25,220/25,308/25,192/25,95/25,246/25,377/25,333/25,433/25,255/25,325/25,352/25,244/25,307/25,268/25,294/25,185/25,396/25,285/25,365/25,319/25,229/25,179/25,264/25,225/25,253/25,241/25,330/25,338/25,337/25,309/25,305/25,291/25,301/25,270/25,285/25,242/25,310/25,204/25,339/25,304/25,290/25,462/25,307/25,374/25,327/25,414/25,295/25,386/25,354/25,330/25,323/25,150/25,403/25,397/25,289/25,400/25,266/25,230/25,400/25];
			histo(pm, element[0]);
		}
	};
});

ffApp.controller('draftBoard', ['$scope','$http', function ($scope, $http) {
    $http.get('/ff/api/draft/1/detail').success(function (data) {
        $scope.Teams = data.Teams;
        $scope.Rounds=[];
        for (var i = 1; i <= data.Rounds; i++) {
            $scope.Rounds.push(i);
        }
    });
}]);

ffApp.directive('draftposition', ['$http',function ($http) {
    return {
        link: function ($scope, element, attrs) {
            $http.get('/ff/api/draft/get?teamid=' + attrs.dpTeam + '&round=' + attrs.dpRound).success(function (data) {
                if (data != null) {
                    element.val(data.Name);
                }
            });           

            var engine = new Bloodhound({
                prefetch: '/ff/api/players/index',
                datumTokenizer: function (d) {
                    return Bloodhound.tokenizers.whitespace(d.Name);
                },
                queryTokenizer: Bloodhound.tokenizers.whitespace
            });

            engine.initialize();

            element.typeahead(null, {
                displayKey: 'Name',
                
                source: engine.ttAdapter()
            });

            element.on('typeahead:selected', function (e, o) {
                $http.get('/ff/api/draft/set?teamid=' + attrs.dpTeam + '&round=' + attrs.dpRound + '&playerid=' + o.Id);
            });
        }
    };
}]);