using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace _1z10.Components.Services
{
    internal class GameService
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
        private List<Tuple<int, string, string, string>> _questions = new List<Tuple<int, string, string, string>>();
        private int _currentQuestionIndex = 0;
        private MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();
        private readonly IConfiguration _configuration;

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

        public GameService(IConfiguration configuration)
        {
            string connectionString = $"server={configuration["Database:server"]};" + $"port={configuration["Database:port"]};" + $"uid={configuration["Database:username"]};" + $"pwd={configuration["Database:password"]};" + $"Database={configuration["Database:database"]}";
            conn.ConnectionString = connectionString;
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
            if (answer != _questions[_currentQuestionIndex].Item3)
                return SubstractLife(playerIndex, currentLives);
            if (!eliminations)
            {
                var player = _players[playerIndex];
                player.score += 10;
                _players[playerIndex] = player;
            }
            _currentQuestionIndex++;
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
            _currentQuestionIndex++;
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

        public void ResetToDefaults()
        {
            _players.Clear();
            _alivePlayers = 0;
            _currentQuestionIndex = 0;
        }

        public void HandleFinalEnd()
        {
            Player bestPlayer = new();
            foreach (var player in _players)
            {
                if (player.score > bestPlayer.score)
                {
                    bestPlayer = player;
                }
            }
            /*TODO Send data to SQL*/
            ResetToDefaults();
        }

        public void GetQuestionsFromDB()
        {
            if (!_questions.Any())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Questions.idQuestion, Questions.question, Questions.answer, QuestionsTypes.questionType FROM Questions INNER JOIN QuestionsTypes ON Questions.type = QuestionsTypes.idQuestionType;";

                    MySqlCommand cmd = new(query, conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _questions.Add(new Tuple<int, string, string, string>(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                        }
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
                finally
                {
                    conn.Close();
                }
            }
            Random rand = new();
            _questions = _questions.OrderBy(x => rand.Next()).ToList();
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
            string data = "";
            GetQuestionsFromDB();
            foreach (var question in _questions)
            {
                data += question.ToString() + "\n";
            }

            return data;
        }
    }
}