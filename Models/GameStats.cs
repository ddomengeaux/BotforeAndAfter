using System;
using System.Collections.Generic;

namespace BotforeAndAfters.Models
{
    public class GameStats
    {
        public int Total { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }

        public List<UserStats> Users { get; set; }
    }

    public class UserStats
    {
        public string User { get; set; }
        public int Total { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
    }
}
