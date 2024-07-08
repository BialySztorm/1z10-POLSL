using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace _1z10.Components.GamePages;

public partial class GamePLayers : ComponentBase
{
    private string _cancelBtn = "";
    private string _previousBtn = "display: none;";
    private string _nextBtn = "";
    private string _submitBtn = "display: none;";

    // reset the values for release
    private string _firstName = "Ania";

    private string _lastName = "Zaradna";
    private string _description = "";
    private int _age = 0;
    private int _avatar = 1;
    private int _current = 1;
    private int _total = 0;

    private async Task CallJavaScriptFunction(string type, string message)
    {
        await JSRuntime.InvokeVoidAsync("createToast", type, message);
    }

    private void Next()
    {
        if (_total > 0)
        {
            if (string.IsNullOrEmpty(_firstName) || string.IsNullOrEmpty(_lastName))
            {
                if (string.IsNullOrEmpty(_firstName) && !string.IsNullOrEmpty(_lastName))
                    CallJavaScriptFunction("danger", "Missing first name!");
                else if (string.IsNullOrEmpty(_lastName) && !string.IsNullOrEmpty(_firstName))
                    CallJavaScriptFunction("danger", "Missing last name!");
                else
                    CallJavaScriptFunction("danger", "Missing first and last name!");
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
                _firstName = "Jan";
                _lastName = "Stary";
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