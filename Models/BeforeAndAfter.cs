using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LiteDB;

namespace BotforeAndAfters.Models
{
    public class BeforeAndAfter
    {
        public string Plot { get; set; }
        public string Answer { get; set; }
        public string Movies { get; set; }
        public string Episode { get; set; }
    }

    public class BeforeAndAfterGame
    {
        private readonly Regex _regex = new Regex("[^a-zA-Z0-9]");
        private readonly Regex _trimRegex = new Regex("(?i)^the +");

        private readonly List<string> _replacements = new List<string>() {
            "(a stretch I know but I still went for it)",
            "(blade and blade-runner)",
            "(alternatively Tetsuo, the Iron Man)",
            "(hard to say)",
            "[a Natalie Portmanteau*]"};

        public BeforeAndAfterGame()
        {

        }

        public BeforeAndAfterGame(ulong id, ulong startedBy, BeforeAndAfter beforeAndAfter, int roundTimer)
        {
            Id = id;
            StartedBy = startedBy;
            StartedOn = DateTimeOffset.Now;
            Question = beforeAndAfter;
            RoundTimer = roundTimer;
        }

        public ulong Id { get; set; }
        public ulong StartedBy { get; set; }
        public DateTimeOffset StartedOn { get; set; }
        public int RoundTimer { get; set; }
        public BeforeAndAfter Question { get; }
        public ulong? WonBy { get; set; }
        public int Guesses { get; set; }
        public TimeSpan GuessedIn { get; set; }

        [BsonIgnore]
        public bool CheckAnswer(ulong user, string guess)
        {
            Guesses++;

            var answer = Question.Answer;

            foreach (var replace in _replacements)
            {
                answer = answer.Replace(replace, "");
            }

            var g = _trimRegex.Replace(guess, "");
            var g2 = _regex.Replace(g, "");

            var a = _trimRegex.Replace(answer, "");
            var a2 = _regex.Replace(a, "");

            if (!string.Equals(g2, a2,
                StringComparison.CurrentCultureIgnoreCase))
                return false;

            GuessedIn = StartedOn - DateTimeOffset.Now;
            WonBy = user;

            return true;
        }

        [BsonIgnore]
        public bool IsActive =>
            !((DateTimeOffset.Now - StartedOn).TotalMinutes >
                3 || WonBy > 0);

        [BsonIgnore]
        public TimeSpan TimeRemaining =>
            TimeSpan.FromMinutes(3) - (DateTimeOffset.Now - StartedOn);
    }
}