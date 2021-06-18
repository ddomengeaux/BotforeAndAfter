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
    public class ComplimentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly LiteDatabase _database;
        private readonly SheetsService _sheetsService;
        private IList<Compliment> _compliments = new List<Compliment>();

        public ComplimentService(IServiceProvider services)
        {
            _config = services.GetService<IConfiguration>();
            _database = services.GetService<LiteDatabase>();
            _logger = services.GetService<ILogger>();
            _sheetsService = Configuration.SetupGoogleSheets(_config, _logger).Result;
        }

        public async Task<int> UpdateDataSourceAsync()
        {
            if (_sheetsService == null || string.IsNullOrEmpty(_config[Keys.SHEETS_COMPLIMENT_SHEET_ID]) || string.IsNullOrEmpty(_config[Keys.SHEETS_COMPLIMENT_SHEET_RANGE]))
                return 0;

            var response = await _sheetsService.Spreadsheets.Values.Get(_config[Keys.SHEETS_COMPLIMENT_SHEET_ID], _config[Keys.SHEETS_COMPLIMENT_SHEET_RANGE]).ExecuteAsync();

            _compliments = response.Values.Where(x =>
                !string.IsNullOrEmpty(x[0].ToString())).Select(x =>
                new Compliment()
                {
                    Text = x[0].ToString()
                }).ToList();

            return _compliments.Count();
        }

        public async Task<string> GetComplimentAsync()
        {
            if (!_compliments.Any())
                await UpdateDataSourceAsync();

            return _compliments[new Random().Next(0, _compliments.Count())].Text;
        }
    }
}
