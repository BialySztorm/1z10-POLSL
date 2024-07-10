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
    private int _currentPlayer = 0;

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
            //_questionCategory = "Przyroda";
            //_questionText = "Ile n�g ma paj�k?";
            _questionCategory = GameServiceRef.GetCurrentQuestionType();
            _questionText = GameServiceRef.GetCurrentQuestion();
        }
    }

    public void AnswerQuestion()
    {
        string _answer = GameServiceRef.GetCurrentQuestionAnswer();
        //_answer = "8";
        if (_isTournamentMode)
        {
            if (_isSubmitted)
            {
                return;
            }
            _answerOutput1 = _answer;
            if (GameServiceRef.HandleAnswerFromDB(_currentPlayer, Players[_currentPlayer].LivesCount, _answerInput1))
            {
                CorrectAnswer();
            }
            else
            {
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
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted) 
        {
            GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, true, true);
            Players[0].AddLife();
        }
        if (! _isTournamentMode && !_isSubmitted) GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, true);
        _isSubmitted = true;
        _previousAnswer = true;
    }

    public void WrongAnswer()
    {
        // TODO : Substract life from the current player
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted) return;
        if(!_isTournamentMode) GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, false);
        Players[0].SubstractLife();
        _isSubmitted = true;
        _previousAnswer = false;
    }

    public void NextQuestion()
    {
        GameServiceRef.NextQuestion();
        if (_isTournamentMode)
        {
            _answerInput1 = "";
            _answerOutput1 = "";
            _isSubmitted = false;
        }
        else
        {
            _answerInput2 = "display:none";
            _answerOutput2 = "";
            _isSubmitted = false;
            _previousAnswer = false;
        }
        _questionText = GameServiceRef.GetCurrentQuestion();
        _questionCategory = GameServiceRef.GetCurrentQuestionType();
    }
}