using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotforeAndAfters.Extensions;
using BotforeAndAfters.Models;
using Google.Apis.Sheets.v4;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Services
{
    public class BeforeAndAftersService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly LiteDatabase _database;
        private readonly SheetsService _sheetsService;
        private IList<BeforeAndAfter> _beforeAndAfters = new List<BeforeAndAfter>();

        public Dictionary<ulong, BeforeAndAfterGame> CurrentGames = new Dictionary<ulong, BeforeAndAfterGame>();

        public BeforeAndAftersService(IServiceProvider services)
        {
            _config = services.GetService<IConfiguration>();
            _database = services.GetService<LiteDatabase>();
            _logger = services.GetService<ILogger>();
            _sheetsService = Configuration.SetupGoogleSheets(_config, _logger).Result;
        }

        public bool CheckAnswer(ulong guildId, ulong user, string guess)
        {
            var res = CurrentGames[guildId].CheckAnswer(user, guess);

            if (res)
                CurrentGames.Remove(guildId);

            return res;
        }

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

        public async Task<BeforeAndAfter> GetBeforeAndAfterAsync()
        {
            if (!_beforeAndAfters.Any())
                await UpdateDataSourceAsync();

            return _beforeAndAfters[new Random().Next(0, _beforeAndAfters.Count())];
        }
    }
}
