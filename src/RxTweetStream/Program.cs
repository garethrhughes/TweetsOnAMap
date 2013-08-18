using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace RxTweetStream
{
    class Program
    {
        private static string[] _users;
        private static int[] _postcodes;
        private static string[] _parties;
        private static readonly Random _rng = new Random();

        static void Main()
        {
            _users = Enumerable.Range(0, 10).Select(i => "@user" + i).ToArray();
            _postcodes = new[] {2000, 2001, 2002};
            _parties = new[] {"Labor", "Liberal", "National", "Greens", "One Nation", "Independent"};

            var tweetStream = Observable.Interval(TimeSpan.FromSeconds(1)).Select(_ => CreateTweet());

            var tweetsByPostcode = tweetStream.GroupBy(t => t.Postcode);

            var votesForPostcodes = 
                tweetsByPostcode
                    .Select(tweetsForPostcode =>
                        tweetsForPostcode.Scan(
                            new PostcodeTweetDetails(tweetsForPostcode.Key),
                            (ptd, t) => ptd.AddTweet(t)))
                    .Merge();

            var subscription = votesForPostcodes.Subscribe(Console.WriteLine);

            Console.WriteLine("Press enter to unsubscribe");
            Console.ReadLine();
            subscription.Dispose();

            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
        }

        private static TweetDetails CreateTweet()
        {
            return new TweetDetails
            {
                User = _users[_rng.Next(_users.Length)],
                Postcode = _postcodes[_rng.Next(_postcodes.Length)],
                Party = _parties[_rng.Next(_parties.Length)]
            };
        }
    }

    internal class PostcodeTweetDetails
    {
        public PostcodeTweetDetails(int postcode)
        {
            Postcode = postcode;
            VotesByParty = new Dictionary<string, int>();
        }

        public int Postcode { get; set; }
        public int TotalVotes { get; set; }
        public IDictionary<string, int> VotesByParty { get; private set; }

        public PostcodeTweetDetails AddTweet(TweetDetails t)
        {
            TotalVotes++;

            if (!VotesByParty.ContainsKey(t.Party))
                VotesByParty[t.Party] = 1;
            else
                VotesByParty[t.Party]++;

            return this;
        }

        public override string ToString()
        {
            var votesByParty = VotesByParty.Select(kvp => string.Format("{0}: {1}", kvp.Key, kvp.Value));
            return string.Format("{0} total votes {1} - {2}", Postcode, TotalVotes, string.Join(", ", votesByParty));
        }
    }

    internal class TweetDetails
    {
        public string User { get; set; }
        public int Postcode { get; set; }
        public string Party { get; set; }

        public override string ToString()
        {
            return string.Format("{0} votes {1} in {2}", User, Party, Postcode);
        }
    }
}
