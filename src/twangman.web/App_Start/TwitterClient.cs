using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Web;

using twangman.web.App_Start;
using TweetSharp;

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
        
        private static string TweetsOnAMapScreenName = "TweetsOnAMap";
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

            var statusStream = Observable.Create<TwitterStatus>(observer =>
            {
                service.StreamUser((tweets, response) => observer.OnNext(service.Deserialize<TwitterStatus>(tweets)));

                return Disposable.Empty;
            });

            var votes = statusStream
                            .Where(t => t.User != null)
                            .Select(t =>
                            {
                                var match = Regex.Match(t.Text, @"@tweetsonamap ([0-9]{3,4}) ([A-Z]{3})", RegexOptions.IgnoreCase);
                                return new TweetDetails
                                {
                                    User = t.User.ScreenName,
                                    Postcode = match.Groups[1].Value,
                                    Party = match.Groups[2].Value,
                                    Status = t
                                };
                            });

            votes.Subscribe(v => TwitterTicker.Instance.SendPostcode(v));

            //var tweetsByPostcode = votes.GroupBy(t => t.Postcode);

            //var votesForPostcodes =
            //    tweetsByPostcode
            //        .Select(tweetsForPostcode =>
            //            tweetsForPostcode.Scan(
            //                new PostcodeTweetDetails(tweetsForPostcode.Key),
            //                (ptd, t) => ptd.AddTweet(t)))
            //        .Merge();

            //service.StreamUser((tweets, response) =>
            //{
            //  if (tweets != null)
            //  {
            //    SaveTweet(service, tweets);
            //  }
            //});

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
                        ScreenName = TweetsOnAMapScreenName
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
                if (status.User.ScreenName == TweetsOnAMapScreenName) 
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
            //TwitterTicker.Instance.SendPostcode(status);
        }
    }

    public class TweetData
    {
        public int Code { get; set; }

        public int Rating { get; set; }

        public string Tweet { get; set; }

        public string Party { get; set; }
    }
}

internal class PostcodeTweetDetails
{
    public PostcodeTweetDetails(string postcode)
    {
        Postcode = postcode;
        VotesByParty = new Dictionary<string, int>();
    }

    public string Postcode { get; set; }
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

public class TweetDetails
{
    public string User { get; set; }
    public string Postcode { get; set; }
    public string Party { get; set; }
    public TwitterStatus Status { get; set; }

    public override string ToString()
    {
        return string.Format("{0} votes {1} in {2}", User, Party, Postcode);
    }
}