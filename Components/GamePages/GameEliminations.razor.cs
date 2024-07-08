using Microsoft.AspNetCore.Components;
using System.Diagnostics;
namespace _1z10.Components.GamePages;

public class Player
{
    public string Name { get; set; } = "";
    public bool[] Lives { get; set; } = new bool[3];
    public int Points { get; set; } = 0;
    public bool IsAlive => Lives[0] || Lives[1] || Lives[2];

    public int LivesCount => Lives[2] ? 3 : Lives[1] ? 2 : Lives[0] ? 1 : 0;
    public void SubstractLife()
    {
        if (LivesCount > 0)
        {
            Lives[LivesCount - 1] = false;
        }
    }
    public void AddLife()
    {
        if (LivesCount < 3)
        {
            Lives[LivesCount] = true;
        }
    }
}

public partial class GameEliminations : ComponentBase
{
    private bool _isTournamentMode;
    private int _playersCount;
    private string _answerTournament = "";
    private string _answerModerated = "";
    private string _questionCategory = "";
    private string _questionText = "";
    private string _answerInput1 = "";
    private string _answerInput2 = "display:none";
    private string _answerOutput1 = "";
    private string _answerOutput2 = "";
    private bool _isSubmitted = false;
    private bool _previousAnswer = false;

    public List<Player> Players { get; set; } = new List<Player>();

    protected override void OnInitialized()
    {
        _isTournamentMode = GameServiceRef.GetIsTournamentMode();
        Debug.WriteLine("Tournament mode: " + _isTournamentMode);
        if (_isTournamentMode)
        {
            _answerTournament = "display: block;";
            _answerModerated = "display: none;";
        }
        else
        {
            _answerTournament = "display: none;";
            _answerModerated = "display: block;";
        }

        _playersCount = GameServiceRef.GetStartingPlayersCount();
        List<Tuple<string, string>> playerNames = GameServiceRef.GetPlayerNames();
        for (int i = 0; i < _playersCount; i++)
        {
            Players.Add(new Player
            {
                Name = playerNames[i].Item1,
                Lives = new bool[] { true, true, true },
                Points = i + 1
            });
#if DEBUG
            _questionCategory = "Przyroda";
            _questionText = "Ile nóg ma paj¹k?";
#endif
        }
    }

    public void AnswerQuestion()
    {
        string _answer = "";
#if DEBUG
        _answer = "8";
#endif
        if (_isTournamentMode)
        {
            if (_isSubmitted)
            {
                return;
            }
            if (_answerInput1 == _answer)
            {
                _answerOutput1 = _answer;
                CorrectAnswer();
            }
            else
            {
                _answerOutput1 = _answer;
                WrongAnswer();
            }
        }
        else
        {
            _answerInput2 = "display:flex";
            _answerOutput2 = _answer;
        }
    }

    public void CorrectAnswer()
    {
        // TODO : Add points to the current player

        if (!_isTournamentMode && _previousAnswer && _isSubmitted) return;
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted) Players[0].AddLife();
        _isSubmitted = true;
        _previousAnswer = true;
    }

    public void WrongAnswer()
    {
        // TODO : Substract life from the current player
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted) return;
        Players[0].SubstractLife();
        _isSubmitted = true;
        _previousAnswer = false;
    }
}