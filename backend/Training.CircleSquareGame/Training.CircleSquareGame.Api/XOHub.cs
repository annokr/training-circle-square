using Microsoft.AspNetCore.SignalR;
using System.Reflection;

namespace Training.CircleSquareGame.Api;

public class XOHub : Hub
{
    private static Dictionary<char, int> winCount = new Dictionary<char, int>()
    {
        {'X', 0 }, {'O', 0}, {'D', 0}
    };

    private static Dictionary<string, char> fields = new Dictionary<string, char>()
    {
        {"a1", '-'}, {"a2", '-'}, {"a3", '-'},
        {"b1", '-'}, {"b2", '-'}, {"b3", '-'},
        {"c1", '-'}, {"c2", '-'}, {"c3", '-'}
    };
    private static char currentPlayer = 'X';
    private string[,] winningPatterns = new string[,] {
    { "a1", "a2", "a3" }, { "b1", "b2", "b3" }, { "c1", "c2", "c3" },
    { "a1", "b1", "c1" }, { "a2", "b2", "c2" }, { "a3", "b3", "c3" },
    { "a1", "b2", "c3" }, { "a3", "b2", "c1" }
    };

    public async Task SetField(string fieldId)
    {
        if (fields[fieldId] == '-')
        {
            fields[fieldId] = currentPlayer;
            if (currentPlayer == 'X')
                currentPlayer = 'O';
            else
                currentPlayer = 'X';
            await Clients.Caller.SendAsync("UpdateField", fieldId, fields[fieldId]);
            await Clients.All.SendAsync("UpdateTurn", currentPlayer);
            char result = await CheckForWin();
            if (result != '-')
            {
                await Clients.All.SendAsync("GameOver", result);
            }
        }
    }

    public async Task GetField(string fieldId)
    {
        await Clients.Caller.SendAsync("UpdateField", fieldId, fields[fieldId]);
    }

    public async Task GetStandings()
    {
        await Clients.Caller.SendAsync("SendStandings", winCount.First().Value, winCount['D'], winCount.Last().Value);
    }

    private async Task<char> CheckForWin()
    {
        for (int i = 0; i < 8; i++)
        {
            if (fields[winningPatterns[i, 0]] == fields[winningPatterns[i, 1]] && fields[winningPatterns[i, 1]] == fields[winningPatterns[i, 2]])
            {
                if (fields[winningPatterns[i, 0]] != '-')
                {
                    var winner = fields[winningPatterns[i, 0]];
                    winCount[winner] = winCount[winner] + 1;
                    await Clients.All.SendAsync("NewStandings", winner, winCount[winner]);
                    return fields[winningPatterns[i, 0]];
                }
            }
        }
        if (!fields.Values.Any(f => f == '-'))
        {
            var winner = 'D';
            winCount[winner] = winCount[winner] + 1;
            await Clients.All.SendAsync("NewStandings", winner, winCount[winner]);
            return winner;
        }
        return '-';
    }

    public void NewGame()
    {
        foreach (var key in fields.Keys.ToList())
            fields[key] = '-';

        currentPlayer = 'X';

        Clients.All.SendAsync("UpdateFields", fields);
        Clients.All.SendAsync("UpdateTurn", currentPlayer);
        Clients.All.SendAsync("SendStandings", winCount.First().Value, winCount['D'], winCount.Last().Value);
    }
}