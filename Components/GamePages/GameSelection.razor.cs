using Microsoft.AspNetCore.Components;

namespace _1z10.Components.GamePages;

public partial class GameSelection : ComponentBase
{
    private string _modeVisibility = "display:block;";
    private string _playersVisibility = "display:none;";
#if DEBUG
    private int _rangeValue = 3;
    private readonly int _defaultRangeValue = 3;
#else
    private int _rangeValue = 10;
    private readonlu int _defaultRangeValue = 10;
#endif

    private void HandleSelectionChange(string SelectionToVisualize)
    {
        if (SelectionToVisualize == "selection__mode")
        {
            _modeVisibility = "display:block;";
            _playersVisibility = "display:none;";
        }
        else if (SelectionToVisualize == "selection__players")
        {
            _modeVisibility = "display:none;";
            _playersVisibility = "display:block;";
        }
        else if (SelectionToVisualize == "game_players")
        {
            NavigationManager.NavigateTo("/game/players");
        }
        else
        {
            throw new Exception("Invalid selection");
        }
    }

    private void SelectGameMode(string mode)
    {
        if (mode == "tournament")
        {
            GameServiceRef.SetIsTournamentMode(true);
        }
        else if (mode == "moderated")
        {
            GameServiceRef.SetIsTournamentMode(false);
        }
        else
        {
            throw new Exception("Invalid game mode");
        }
        HandleSelectionChange("selection__players");
    }

    private void SelectPlayersCount()
    {
        GameServiceRef.SetStartingPlayersCount(_rangeValue);
        HandleSelectionChange("game_players");
    }

    private void ReturnToGameModeSelection()
    {
        HandleSelectionChange("selection__mode");
    }
}