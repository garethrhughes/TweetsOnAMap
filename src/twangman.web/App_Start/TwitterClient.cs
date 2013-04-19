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
        
        private static string ScreenName = "TweetsOnAMap";
        private static Task twitterTask;


        public static void Start ()
        {
            twitterTask = new Task(Main);
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
                        ScreenName = ScreenName
                    },
                Text = "Something"
            };

            var rand = new Random();

            for (int i = 0; i < 1000000; i++ )
            {
                int nextPostcode = rand.Next(2000, 2100);
                var randomRating = rand.Next(0, 10);
                twitterStatus.Text = string.Format("@tweetsonamap {0} {1}/10 Testing", nextPostcode, randomRating);
                ProcessPostcodeTweet(twitterStatus);
                
                if(i % 10 == 0)
                    ProcessAccountTweet(accountStatus);

                twitterTask.Wait(2000);
            }
        }

        private static void SaveTweet(TwitterService service, TwitterStreamArtifact tweets)
        {
            var status = service.Deserialize<TwitterStatus>(tweets);
            if (status.User != null)
            {
                if (status.User.ScreenName == ScreenName) 
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
            
            TwitterTicker.Instance.SendPostcode(status);
        }
    }

    public class TweetData
    {
        public int Code { get; set; }

        public int Rating { get; set; }

        public string Tweet { get; set; }
    }
}