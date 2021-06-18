using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotforeAndAfters.Extensions;
using BotforeAndAfters.Models;
using Google.Apis.Sheets.v4;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace BotforeAndAfters.Services
{
    public class GameService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly LiteDatabase _database;
        private readonly SheetsService _sheetsService;
        private IList<BeforeAndAfter> _beforeAndAfters = new List<BeforeAndAfter>();
        private BeforeAndAfterGame _currentGame;
        private int _roundTimer;
        private ILiteCollection<BeforeAndAfterGame> _games;
        private ILiteCollection<BeforeAndAfterGame> Games => _games ??= _database.GetCollection<BeforeAndAfterGame>();

        public GameService(IServiceProvider services)
        {
            _config = services.GetService<IConfiguration>();
            _database = services.GetService<LiteDatabase>();
            _logger = services.GetService<ILogger>();
            _sheetsService = Configuration.SetupGoogleSheets(_config, _logger).Result;
        }

        public ulong? Id => _currentGame?.Id;

        public string Question => _currentGame?.Question.Plot;
        public string Answer => _currentGame?.Question.Answer;
        public string Movies => _currentGame?.Question.Movies;
        public string Episode => _currentGame?.Question.Episode;
        public TimeSpan GuessedIn => _currentGame.GuessedIn;

        public int TimesPlayed => Games.Query().Where(x => x.Question.Answer == _currentGame.Question.Answer).Count();
        public bool WasWon => _currentGame?.WonBy > 0;
        public int TimesWon => Games.Query().Where(x => x.Question.Answer == _currentGame.Question.Answer && x.WonBy > 0).Count();

        public bool IsActive =>
            !((DateTimeOffset.Now - _currentGame.StartedOn).TotalMinutes >
                _roundTimer || _currentGame.WonBy > 0);

        public TimeSpan TimeRemaining =>
            TimeSpan.FromMinutes(_roundTimer) - (DateTimeOffset.Now - _currentGame.StartedOn);

        public int? Guesses => _currentGame?.Guesses;

        public async Task<int> UpdateDataSourceAsync()
        {
            if (_sheetsService == null || string.IsNullOrEmpty(_config[Keys.SHEETS_SHEET_ID]) || string.IsNullOrEmpty(_config[Keys.SHEETS_SHEET_RANGE]))
                return 0;

            var response = await _sheetsService.Spreadsheets.Values.Get(_config[Keys.SHEETS_SHEET_ID], _config[Keys.SHEETS_SHEET_RANGE]).ExecuteAsync();
            _beforeAndAfters = response.Values.Where(x =>
                !string.IsNullOrEmpty(x[0].ToString()) || !string.IsNullOrEmpty(x[1].ToString())).Select(x =>
                new BeforeAndAfter()
                {
                    Plot = x[1].ToString(),
                    Answer = x[0].ToString(),
                    Movies = x[2].ToString(),
                    Episode = x[3].ToString()
                }).ToList();

            return _beforeAndAfters.Count();
        }

        public async Task StartRoundAsync(ulong id, ulong user, int roundTimer = 3)
        {
            if (!_beforeAndAfters.Any())
                await UpdateDataSourceAsync();  

            _roundTimer = roundTimer;
            _currentGame = new BeforeAndAfterGame(id, user, PickQuestion(), _roundTimer);

            Games.Insert(_currentGame);
        }

        public async Task<bool> CheckAnswerAsync(ulong user, string guess)
        {
            if (_currentGame == null || !IsActive)
                return false;

            var result = _currentGame.CheckAnswer(user, guess);
            Games.Update(_currentGame);

            return result;
        }

        private BeforeAndAfter PickQuestion()
        {
            return _beforeAndAfters[new Random().Next(_beforeAndAfters.Count - 1)];
            // while (true)
            // {
            //     var item = _beforeAndAfters[new Random().Next(_beforeAndAfters.Count - 1)];
            //     var a = Games.Query().Where(x => x.Question.Answer == item.Answer).OrderByDescending(x => x.StartedOn)
            //         .FirstOrDefault();
            //     
            //     if (a == null)
            //         return item;
            //     
            //     Debug.WriteLine("choosing a new question");
            // }
        }

        public Tuple<bool, TimeSpan> CheckForCooldown(ulong user)
        {
            if (int.Parse(_config[Keys.COOLDOWN_TIMER]) == 0)
                return new Tuple<bool, TimeSpan>(false, TimeSpan.Zero);

            var latest = Games.Query().Where(x => x.StartedBy == user).OrderByDescending(x => x.StartedOn)
                .FirstOrDefault();

            if (latest == default(BeforeAndAfterGame) || latest == null)
                return new Tuple<bool, TimeSpan>(false, TimeSpan.Zero);

            return new Tuple<bool, TimeSpan>((DateTime.Now - latest.StartedOn).TotalMinutes <=
                                             int.Parse(_config[Keys.COOLDOWN_TIMER]),
                latest.StartedOn.AddMinutes(
                     int.Parse(_config[Keys.COOLDOWN_TIMER])) - DateTime.Now);
        }

        public async Task<GameStats> GetStats()
        {
            if (!_beforeAndAfters.Any())
                await UpdateDataSourceAsync();

            var users = new List<UserStats>();

            //var group = Games.Query().GroupBy("StartedBy").

            return new GameStats()
            {
                Total = _beforeAndAfters.Count(),
                Played = Games.Query().Count(),
                Won = Games.Query().Where(x => x.WonBy != 0).Count()
            };
        }
    }
}
