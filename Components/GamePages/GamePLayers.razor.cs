using Microsoft.AspNetCore.Components;

namespace _1z10.Components.GamePages;

public partial class GamePLayers : ComponentBase
{
    private string _cancelBtn = "";
    private string _previousBtn = "display: none;";
    private string _nextBtn = "";
    private string _submitBtn = "display: none;";
    private string _popup = "display: none;";
    private string _popupMessage = "";

    // reset the values for release
    private string _firstName = "ss";

    private string _lastName = "ss";
    private string _description = "";
    private int _age = 0;
    private int _avatar = 1;
    private int _current = 1;
    private int _total = 0;

    private void Next()
    {
        if (_total > 0)
        {
            if (string.IsNullOrEmpty(_firstName) || string.IsNullOrEmpty(_lastName))
            {
                _popup = "";
                if (string.IsNullOrEmpty(_firstName))
                    _popupMessage = "First name is required";
                else if (string.IsNullOrEmpty(_lastName))
                    _popupMessage = "Last name is required";
                else
                    _popupMessage = "First name and last name are required";
            }
            else
            {
                GameServiceRef.AddPlayer(_firstName, _lastName, _age);
                _current++;

                if (_current == _total)
                {
                    _nextBtn = "display: none;";
                    _submitBtn = "";
                }
                else if (_current > _total)
                {
                    NavigationManager.NavigateTo("/game/eliminations");
                }
                else if (_current > 1)
                {
                    _cancelBtn = "display: none;";
                    _previousBtn = "";
                }
                // reset the values for release
                _firstName = "dd";
                _lastName = "dd";
                _description = "";
                _age = 0;
            }
        }
        else
        {
            _total = GameServiceRef.GetStartingPlayersCount();
            if (_total > 0)
                Next();
            else
            {
                throw new Exception("Invalid players count");
            }
        }
    }

    private void Previous()
    {
        if (_current > 1)
        {
            _current--;
            GameServiceRef.RemoveLastPlayer();
            if (_current == 1)
            {
                _previousBtn = "display: none;";
                _cancelBtn = "";
            }
            else if (_current < _total)
            {
                _nextBtn = "";
                _submitBtn = "display: none;";
            }
        }
        else
        {
            NavigationManager.NavigateTo("/game/selection");
        }
    }
}