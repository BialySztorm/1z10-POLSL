using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace _1z10.Components.GamePages;

public partial class GameFinalists : ComponentBase
{
    private List<Tuple<string, string>> _players = new List<Tuple<string, string>>();
    private List<int> _playersLives = new List<int>();
    private Timer? _timer;

    protected override void OnInitialized()
    {
        if (!GameServiceRef.HandleEliminationsEnd())
            Debug.WriteLine("Error handling eliminations end");
        _players = GameServiceRef.GetPlayerNames();
        for (int i = 0; i < _players.Count; i++)
            _playersLives.Add(GameServiceRef.GetScore(i));

        _timer = new Timer(_ => Skip(), null, 20000, Timeout.Infinite);
    }

    public void Skip()
    {
        InvokeAsync(() =>
        {
            _timer?.Dispose();
            _timer = null;

            NavigationManager.NavigateTo("/game/final");
        });
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}