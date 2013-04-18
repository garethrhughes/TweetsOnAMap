using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using twangman.web.App_Start;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(TwitterClient), "Start")]

namespace twangman.web.App_Start
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using TweetSharp;

    using twangman.twitter;
    using twangman.web.Hubs;

    public class TwitterClient
    {
        private static readonly List<TwitterStatus> _allTweets = new List<TwitterStatus>();

        private static Task twitterTask;

        public static void Start ()
        {
            twitterTask = new Task(FakeMain);
            twitterTask.Start();
        }

        public static void Main()
        {
            var service = new TwitterService(Authentication.ConsumerKey, Authentication.ConsumerSecret);
            service.AuthenticateWith(Authentication.AccessToken, Authentication.AccessTokenSecret);

            _allTweets.AddRange(GetHistoricalTweets(service));

            service.StreamUser((tweets, response) =>
            {
                SaveTweet(service, tweets);
            });

        }

        public static void FakeMain()
        {
            var twitterStatus = new TwitterStatus();
            var rand = new Random();
            

            for (int i = 0; i < 1000000; i++ )
            {
                int nextPostcode = rand.Next(2000, 2100);
                var randomRating = rand.Next(0, 10);
                twitterStatus.Text = string.Format("{0} {1}/10 Testing", nextPostcode, randomRating);
                ProcessText(twitterStatus);
                twitterTask.Wait(500);
            }
        }

        private static void SaveTweet(TwitterService service, TwitterStreamArtifact tweets)
        {
            var status = service.Deserialize<TwitterStatus>(tweets);
            if (status.User != null)
            {
                ProcessText(status);
            }
        }

        private static void ProcessText(TwitterStatus status)
        {
            _allTweets.Add(status);
            var match = Regex.Match(status.Text, @"^([0-9]{3,4}) ([0-9]{1,2})\/10");
            if (match.Success)
            {
                var code = int.Parse(match.Groups[1].Value);
                var rating = int.Parse(match.Groups[2].Value);
                if (rating > 10) rating = 10;
                if (rating < 0) rating = 0;
                var size = 20000;

                var postcode = PostcodeLoader.Postcodes.FirstOrDefault(x => x.Code == code);
                if(postcode != null)
                    TwitterTicker.Instance.SendPostcode(code, size, rating, postcode.Latitude, postcode.Longitude);
            }
        }

        private static List<TwitterStatus> GetHistoricalTweets(TwitterService service)
        {
            var list = new List<TwitterStatus>();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            foreach (var tweet in tweets)
            {
                //Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
                list.Add(tweet);
            }
            list.Reverse();
            return list;
        }
    }
}