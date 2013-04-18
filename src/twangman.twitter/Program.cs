namespace twangman.twitter
{
    using System;

    using TweetSharp;

    using System.Collections.Generic;

    class Program
    {

        private static List<TwitterStatus> _allTweets = new List<TwitterStatus>();

        static void Main(string[] args)
        {
            var service = new TwitterService(Authentication.ConsumerKey, Authentication.ConsumerSecret);
            service.AuthenticateWith(Authentication.AccessToken, Authentication.AccessTokenSecret);

            _allTweets.AddRange(GetHistoricalTweets(service));

            service.StreamUser(
                (tweets, response) =>
                    {
                        {
                            var status = service.Deserialize<TwitterStatus>(tweets);
                            Program._allTweets.Add(status);
                            Console.WriteLine(status.Text);
                        }
                    });

            Console.ReadLine();
            service.CancelStreaming();
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
            return list;
        }
    }
}
 