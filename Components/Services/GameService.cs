using System.Numerics;

public class GameService
{
    public struct Player
    {
        public string firstName;
        public string lastName;
        public int age;
        public int lives;
        public int score;
    }

    private List<Player> _players = new List<Player>();
    private int _alivePlayers;

    private bool SubstractLife(int index, int currentLives)
    {
        if (_players[index].lives != currentLives) return false;
        var player = _players[index];
        player.lives--;
        if (player.lives == 0)
        {
            _alivePlayers--;
        }
        _players[index] = player;
        return true;
    }

    public void AddPlayer(string firstName, string lastName, int age)
    {
        _players.Add(new Player
        {
            firstName = firstName,
            lastName = lastName,
            age = age,
            lives = 3,
            score = 0
        });
        _alivePlayers++;
    }

    public bool HandleEliminationsEnd()
    {
        if (_alivePlayers != 3) return false;
        for (int i = _players.Count; i >= 0; i--)
        {
            if (_players[i].lives == 0)
            {
                _players.RemoveAt(i);
            }
            else
            {
                var player = _players[i];
                player.score = player.lives;
                player.lives = 3;
                _players[i] = player;
            }
        }
        return true;
    }

    public List<Tuple<string, string>> GetPlayerNames()
    {
        List<Tuple<string, string>> names = new List<Tuple<string, string>>();
        foreach (var player in _players)
        {
            names.Add(new Tuple<string, string>(player.firstName, player.lastName));
        }
        return names;
    }

    public bool HandleAnswerFromDB(int playerIndex, int currentLives, string answer, bool eliminations = true)
    {
        /*TODO Checking answers from db*/
        if (answer == "wrong")
            return SubstractLife(playerIndex, currentLives);
        if (!eliminations)
        {
            var player = _players[playerIndex];
            player.score += 10;
            _players[playerIndex] = player;
        }
        return true;
    }

    public bool HandleAnswerFromUI(int playerIndex, int currentLives, bool answer, bool eliminations = true)
    {
        if (!answer)
            return SubstractLife(playerIndex, currentLives);
        if (!eliminations)
        {
            var player = _players[playerIndex];
            player.score += 10;
            _players[playerIndex] = player;
        }
        return true;
    }

    public int GetScore(int playerIndex)
    {
        return _players[playerIndex].score;
    }

    public int GetLives(int playerIndex)
    {
        return _players[playerIndex].lives;
    }

    public int GetPlayersCount()
    {
        return _players.Count;
    }

    public void ResetVars()
    {
        _players.Clear();
        _alivePlayers = 0;
    }

    public void HandleFinalEnd()
    {
        Player bestPlayer = new Player();
        foreach (var player in _players)
        {
            if (player.score > bestPlayer.score)
            {
                bestPlayer = player;
            }
        }
        /*TODO Send data to SQL*/
        ResetVars();
    }
}