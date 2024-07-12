using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace _1z10.Components.Services;

internal class GameService
{
    public struct Player : IComparable<Player>
    {
        public string firstName;
        public string lastName;
        public int age;
        public int lives;
        public int score;

        public int CompareTo(Player other)
        {
            // Jeœli pierwszy gracz ma wiêcej ni¿ 0 ¿yæ, a drugi 0, pierwszy jest lepszy
            if (this.lives > 0 && other.lives == 0)
            {
                return -1; // this jest lepszy ni¿ other
            }
            // Jeœli drugi gracz ma wiêcej ni¿ 0 ¿yæ, a pierwszy 0, drugi jest lepszy
            else if (this.lives == 0 && other.lives > 0)
            {
                return 1; // other jest lepszy ni¿ this
            }
            // Jeœli obaj gracze maj¹ ¿yæ wiêksze lub równe 0, porównaj score
            else
            {
                // Gracz z wiêkszym score jest lepszy
                return other.score.CompareTo(this.score);
            }
        }
    }

    private List<Player> _players = new List<Player>();
    private int _alivePlayers;
    private List<Tuple<int, string, string, string>> _questions = new List<Tuple<int, string, string, string>>();
    private int _currentQuestionIndex = 0;
    private readonly MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();
    private readonly IConfiguration _configuration;
    private bool _isTournamentMode;
    private int _playersCount;

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

    private bool AddLife(int index, int currentLives)
    {
        if (_players[index].lives != currentLives) return false;
        var player = _players[index];
        player.lives++;
        if (player.lives == 1)
        {
            _alivePlayers++;
        }
        _players[index] = player;
        return true;
    }

    public GameService(IConfiguration configuration)
    {
        string connectionString = $"server={configuration["Database:server"]};" + $"port={configuration["Database:port"]};" + $"uid={configuration["Database:username"]};" + $"pwd={configuration["Database:password"]};" + $"Database={configuration["Database:database"]}";
        conn.ConnectionString = connectionString;
    }

    public void SetIsTournamentMode(bool isTournamentMode)
    {
        _isTournamentMode = isTournamentMode;
    }

    public bool GetIsTournamentMode()
    {
        return _isTournamentMode;
    }

    public void SetStartingPlayersCount(int playersCount)
    {
        _playersCount = playersCount;
    }

    public int GetStartingPlayersCount()
    {
        return _playersCount;
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

    public void RemoveLastPlayer()
    {
        _players.RemoveAt(_players.Count - 1);
        _alivePlayers--;
    }

    public bool HandleEliminationsEnd()
    {
        if (_alivePlayers != 3) return false;
        List<Player> finalists = new List<Player>();
        for (int i = 0; i < _playersCount; i++)
        {
            if (_players[i].lives == 0)
                continue;
            var player = _players[i];
            player.score = player.lives;
            player.lives = 3;
            finalists.Add(player);
        }
        _players = finalists;
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
        if (answer != _questions[_currentQuestionIndex].Item3)
            return SubstractLife(playerIndex, currentLives);
        if (!eliminations)
        {
            var player = _players[playerIndex];
            player.score += 10;
            _players[playerIndex] = player;
        }
        return true;
    }

    public bool HandleAnswerFromUI(int playerIndex, int currentLives, bool answer, bool mistaken = false, bool eliminations = true)
    {
        if (!answer)
        {
            if (mistaken && !eliminations)
            {
                var player = _players[playerIndex];
                player.score -= 10;
                _players[playerIndex] = player;
            }
            return SubstractLife(playerIndex, currentLives);
        }
        if (!eliminations)
        {
            var player = _players[playerIndex];
            player.score += 10;
            _players[playerIndex] = player;
        }
        if (mistaken)
            AddLife(playerIndex, currentLives);
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

    public int GetAlivePlayersCount()
    {
        return _alivePlayers;
    }

    public void ResetToDefaults()
    {
        _players.Clear();
        _alivePlayers = 0;
        _currentQuestionIndex = 0;
    }

    public void HandleFinalEnd()
    {
        _players.Sort();
        Player bestPlayer = _players[0];

        /*TODO Send data to SQL*/
        ResetToDefaults();
    }

    public bool GetQuestionsFromDB()
    {
        if (!_questions.Any())
        {
            try
            {
                conn.Open();
                string query = "SELECT question.question_pk, question.question_text, question.answer_text, question_type.question_type_name FROM question INNER JOIN question_type ON question.question_type_pk = question_type.question_type_pk;";

                MySqlCommand cmd = new(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _questions.Add(new Tuple<int, string, string, string>(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        Random rand = new();
        _questions = _questions.OrderBy(x => rand.Next()).ToList();
        return true;
    }

    public bool NextQuestion()
    {
        if (_currentQuestionIndex == _questions.Count - 1)
        {
            return false;
        }
        _currentQuestionIndex++;
        return true;
    }

    public string GetCurrentQuestion()
    {
        return _questions[_currentQuestionIndex].Item2;
    }

    public string GetCurrentQuestionAnswer()
    {
        return _questions[_currentQuestionIndex].Item3;
    }

    public string GetCurrentQuestionType()
    {
        return _questions[_currentQuestionIndex].Item4;
    }

    public string TestSql()
    {
        string data = "Test data: <br />";
        if (GetQuestionsFromDB())
        {
            foreach (var question in _questions)
            {
                data += question.ToString() + "<br />";
            }
        }

        return data;
    }
}