using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MySql.Data.MySqlClient;

namespace _1z10.Components.Pages;

public partial class Highscores : ComponentBase
{
    public struct Player
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string age { get; set; }
        public string gameDate { get; set; }
        public string score { get; set; }

        public Player(string firstName, string lastName, string age, string gameDate, string score)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.age = age;
            this.gameDate = gameDate;
            this.score = score;
        }
    }

    private List<Player> _players = new List<Player>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("initializeDataTables");
        }
    }

    protected override void OnInitialized()
    {
        MySqlConnection conn = GameServiceRef.GetConnection();
        try
        {
            conn.Open();
            string query = "SELECT player.first_name, player.last_name, player.age, game.date, highscore.highscore_value FROM highscore INNER JOIN player ON highscore.player_pk = player.player_pk INNER JOIN game ON player.game_pk  = game.game_pk;";

            MySqlCommand cmd = new(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _players.Add(new Player(reader.GetString(0), reader.GetString(1), reader.GetInt32(2).ToString(), reader.GetDateTime(3).ToString("yyyy-MM-dd"), reader.GetInt32(4).ToString()));
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
}