
var DraftPick = React.createClass({
    render:function(){
        return (
            <td>Player Name</td>
        );
    }
});

var DraftRound = React.createClass({
    getInitialState:function(){
        return {round:0,picks:new [12]};
    },
    render: function () {
        var cells=this.state.picks.map(function(pick){
            return (
                <DraftPick pick={pick} />
            );
        });
        return (
            <tr>
                <td>{this.state.round}</td>
                {cells}
            </tr>
        );
    }
});