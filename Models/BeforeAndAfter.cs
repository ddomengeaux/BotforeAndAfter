using System;
using System.Text.RegularExpressions;

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
        public ulong WonBy { get; set; }
        public int Guesses { get; set; }
        public TimeSpan GuessedIn { get; set; }

        public bool CheckAnswer(ulong user, string guess)
        {
            Guesses++;
            var answer = string.Equals(_regex.Replace(guess, ""), _regex.Replace(Question.Answer, ""),
                StringComparison.CurrentCultureIgnoreCase);

            if (!answer)
                return false;

            GuessedIn = StartedOn - DateTimeOffset.Now;
            WonBy = user;

            return true;
        }
    }
}