using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace _1z10.Components.GamePages;

public partial class GameFinal : ComponentBase
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
    private bool _isSubmitted = true;
    private bool _previousAnswer = true;
    private int _questionCount = 60;

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
                Id = i,
                Name = playerNames[i].Item1,
                Lives = new bool[] { true, true, true },
                Points = GameServiceRef.GetScore(i)
            });
            //_questionCategory = "Przyroda";
            //_questionText = "Ile nóg ma pająk?";
            _questionCategory = "Początek  gry";
            _questionText = "Wybierz pierwszego gracz";
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
            if (GameServiceRef.HandleAnswerFromDB(_currentPlayer, Players[_currentPlayer].LivesCount, _answerInput1, false))
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
        if (!_isTournamentMode && _previousAnswer && _isSubmitted) return;
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted)
        {
            GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, true, false, false);
            Players[_currentPlayer].AddLife();
        }
        if (!_isTournamentMode && !_isSubmitted) GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, true, true, false);
        _isSubmitted = true;
        _previousAnswer = true;
        Players[_currentPlayer].Points = GameServiceRef.GetScore(_currentPlayer);
    }

    public void WrongAnswer()
    {
        // TODO : Substract life from the current player
        if (!_isTournamentMode && !_previousAnswer && _isSubmitted) return;
        if (!_isTournamentMode && _isSubmitted) GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, false, true, false);
        else if (!_isTournamentMode) GameServiceRef.HandleAnswerFromUI(_currentPlayer, Players[_currentPlayer].LivesCount, false, false, false);
        Players[_currentPlayer].SubstractLife();
        _isSubmitted = true;
        _previousAnswer = false;
        Players[_currentPlayer].Points = GameServiceRef.GetScore(_currentPlayer);
    }

    public void SelectPlayer(int player)
    {
        if (!_isSubmitted || Players[player].LivesCount <= 0 || player > Players.Count())
            return;
        _currentPlayer = player;
        NextQuestion();
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
        _questionCount--;
        if (_questionCount == 0)
        {
            //EndGame()
        }
    }
}