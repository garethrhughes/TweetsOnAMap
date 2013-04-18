namespace twangman.twitter
{
    using System;
    using TweetSharp;
    using System.Collections.Generic;

    class Program
    {
        private static readonly List<TwitterStatus> _allTweets = new List<TwitterStatus>();

        static void Main(string[] args)
        {
            var service = new TwitterService(Authentication.ConsumerKey, Authentication.ConsumerSecret);
            service.AuthenticateWith(Authentication.AccessToken, Authentication.AccessTokenSecret);

            _allTweets.AddRange(GetHistoricalTweets(service));

            //service.StreamFilter(
            //    (tweets, response) =>
            //        {
            //            if (tweets != null)
            //            {
            //                SaveTweet(service, tweets);
            //            }
            //        });
            service.StreamUser((tweets, response) =>
                {
                  if (tweets != null)
                  {
                    SaveTweet(service, tweets);
                  }
                });

            Console.ReadLine();
            service.CancelStreaming();
        }

        private static void SaveTweet(TwitterService service, TwitterStreamArtifact tweets)
        {
            var status = service.Deserialize<TwitterStatus>(tweets);
            if (status.User != null)
            {
                _allTweets.Add(status);
                Console.WriteLine(status.Text);
            }
        }

        private static List<TwitterStatus> GetHistoricalTweets(TwitterService service)
        {
            var list = new List<TwitterStatus>();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            foreach (var tweet in tweets)
            {
                Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
                list.Add(tweet);
            }
            list.Reverse();
            return list;
        }
    }
}
