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
        private static IList<TweetData> AllTweets { get; set; } 

        private static Task twitterTask;

        public static void Start ()
        {
            twitterTask = new Task(Main);
            AllTweets = new List<TweetData>();
            twitterTask.Start();
        }

        public static void Main()
        {
            var service = new TwitterService(Authentication.ConsumerKey, Authentication.ConsumerSecret);
            service.AuthenticateWith(Authentication.AccessToken, Authentication.AccessTokenSecret);
            

            service.StreamUser((tweets, response) =>
            {
              if (tweets != null)
              {
                SaveTweet(service, tweets);
              }
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

            var accountStatus = new TwitterStatus
            {
                User = new TwitterUser
                    {
                        ProfileImageUrl = "https://si0.twimg.com/sticky/default_profile_images/default_profile_2_normal.png",
                        ScreenName = "TweetsOnAMap"
                    },
                Text = "Something"
            };

            var rand = new Random();

            for (int i = 0; i < 1000000; i++ )
            {
                int nextPostcode = rand.Next(2000, 2100);
                var randomRating = rand.Next(0, 10);
                twitterStatus.Text = string.Format("{0} {1}/10 Testing", nextPostcode, randomRating);
                ProcessPostcodeTweet(twitterStatus);
                ProcessAccountTweet(accountStatus);
                twitterTask.Wait(5000);
            }
        }

        private static void SaveTweet(TwitterService service, TwitterStreamArtifact tweets)
        {
            var status = service.Deserialize<TwitterStatus>(tweets);
            if (status.User != null)
            {
                if (status.User.ScreenName == "TweetsOnAMap") 
                    ProcessAccountTweet(status);
                else
                    ProcessPostcodeTweet(status);
            }   
        }

        private static void ProcessAccountTweet(TwitterStatus status)
        {
            TwitterTicker.Instance.SendAccountTweet(status.Text, status.User.ScreenName, status.User.ProfileImageUrl);
        }

        private static void ProcessPostcodeTweet(TwitterStatus status)
        {
            var match = Regex.Match(status.Text, @"@tweetsonamap ([0-9]{3,4}) ([0-9]{1,2})\/10 ([^$]+)", RegexOptions.IgnoreCase);
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
                    AllTweets.Add(new TweetData { Code = code, Rating = rating, Tweet = status.Text });

                    var size = AllTweets.Count(x => x.Code == code);
                    var averageRating = AllTweets.Where(x => x.Code == code).Average(x => x.Rating);

                    TwitterTicker.Instance.SendPostcode(
                        code, size, averageRating, postcode.Latitude, postcode.Longitude, text, status.User.ScreenName, status.User.ProfileImageUrl, AllTweets.Count());
                }
            }
        }
    }

    internal class TweetData
    {
        public int Code { get; set; }

        public int Rating { get; set; }

        public string Tweet { get; set; }
    }
}