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


        private static IList<TweetData> Tweets { get; set; } 

        private static Task twitterTask;

        public static void Start ()
        {
            twitterTask = new Task(Main);
            Tweets = new List<TweetData>();
            twitterTask.Start();
        }

        public static void Main()
        {
            var service = new TwitterService(Authentication.ConsumerKey, Authentication.ConsumerSecret);
            service.AuthenticateWith(Authentication.AccessToken, Authentication.AccessTokenSecret);
            

            //_allTweets.AddRange(GetHistoricalTweets(service));

            service.StreamFilter((tweets, response) =>
            {
                SaveTweet(service, tweets);
            });

        }

        public static void FakeMain()
        {
            var twitterStatus = new TwitterStatus
                {
                    User =
                        new TwitterUser
                            {
                                ProfileImageUrl =
                                    "https://si0.twimg.com/sticky/default_profile_images/default_profile_2_normal.png",
                                ScreenName = "Susan"
                            }
                };

            var rand = new Random();

            for (int i = 0; i < 1000000; i++ )
            {
                int nextPostcode = rand.Next(2000, 2100);
                var randomRating = rand.Next(0, 10);
                twitterStatus.Text = string.Format("{0} {1}/10 Testing", nextPostcode, randomRating);
                ProcessText(twitterStatus);
                twitterTask.Wait(2000);
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
            var match = Regex.Match(status.Text, @"^([0-9]{3,4}) ([0-9]{1,2})\/10 ([^$]+)");
            if (match.Success)
            {
                var code = int.Parse(match.Groups[1].Value);
                var rating = int.Parse(match.Groups[2].Value);
                var text = match.Groups[3].Value;
                if (rating > 10) rating = 10;
                if (rating < 0) rating = 0;

                var postcode = PostcodeLoader.Postcodes.FirstOrDefault(x => x.Code == code);
                if(postcode != null)
                {
                    Tweets.Add(new TweetData { Code = code, Rating = rating, Tweet = status.Text });

                    var size = Tweets.Count(x => x.Code == code);
                    var averageRating = Tweets.Where(x => x.Code == code).Average(x => x.Rating);

                    TwitterTicker.Instance.SendPostcode(
                        code, size, averageRating, postcode.Latitude, postcode.Longitude, text, status.User.ScreenName, status.User.ProfileImageUrl);
                }
            }
        }

        private static List<TwitterStatus> GetHistoricalTweets(TwitterService service)
        {
            var list = new List<TwitterStatus>();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            foreach (var tweet in tweets)
            {
                list.Add(tweet);
            }
            list.Reverse();
            return list;
        }
    }

    internal class TweetData
    {
        public int Code { get; set; }

        public int Rating { get; set; }

        public string Tweet { get; set; }
    }
}