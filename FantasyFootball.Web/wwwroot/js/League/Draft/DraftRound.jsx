
var DraftPick = React.createClass({
    render:function(){
        return (
            <td>Player Name</td>
        );
    }
});

var DraftRound = React.createClass({
    getInitialState:function(){
        return {round:0,picks:new [0]};
    },
    render: function () {
        var cells=this.state.picks.map(function(pick){
            return (
                <DraftPick pick={pick} />
            );
        });
        return (
            <tr><td>{round}</td>{cells}</tr>
        );
    }
});

var DraftBoard = React.createClass({
    getInitialState:function(){
        return {teams:[],rounds:[]};
    },
    render: function () {
        var teamHeaders=this.state.teams.map(function(team){
            return (
                <th>{team}</th>
            );
        });
        var body=this.state.rounds.map(function(round){
            return (
                <DraftRound round={round} />
            );
        });
        return (
            <thead><tr><th/>{teamHeaders}</tr></thead>
            <tbody>{body}</tbody>
        );
    }
});