using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ExcuseService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly LiteDatabase _database;
        private readonly SheetsService _sheetsService;
        private IList<Excuse> _excuses = new List<Excuse>();

        public ExcuseService(IServiceProvider services)
        {
            _config = services.GetService<IConfiguration>();
            _database = services.GetService<LiteDatabase>();
            _logger = services.GetService<ILogger>();
            _sheetsService = Configuration.SetupGoogleSheets(_config, _logger).Result;
        }

        public async Task<int> UpdateDataSourceAsync()
        {
            if (_sheetsService == null || string.IsNullOrEmpty(_config[Keys.SHEETS_COMPLIMENT_SHEET_ID]) || string.IsNullOrEmpty(_config[Keys.SHEETS_EXCUSE_SHEET_RANGE]))
                return 0;

            var response = await _sheetsService.Spreadsheets.Values.Get(_config[Keys.SHEETS_COMPLIMENT_SHEET_ID], _config[Keys.SHEETS_EXCUSE_SHEET_RANGE]).ExecuteAsync();

            _excuses = response.Values.Where(x =>
                !string.IsNullOrEmpty(x[0].ToString())).Select(x =>
                new Excuse()
                {
                    A = x[0].ToString(),
                    B = x[1].ToString(),
                    C = x[2].ToString()
                }).ToList();

            return _excuses.Count();
        }

        public async Task<string> GetExcuseAsync()
        {
            if (!_excuses.Any())
                await UpdateDataSourceAsync();

            return $"{_excuses[new Random().Next(0, _excuses.Count())].A} {_excuses[new Random().Next(0, _excuses.Count())].B} {_excuses[new Random().Next(0, _excuses.Count())].C}";
        }
    }
}
