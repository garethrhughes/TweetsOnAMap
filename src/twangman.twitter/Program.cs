namespace twangman.twitter
{
    using System;
    using TweetSharp;
    using System.Collections.Generic;

    class Program
    {
        private static readonly List<TwitterStatus> _allTweets = new List<TwitterStatus>();
        private static readonly List<TwitterStatus> _feedTweets = new List<TwitterStatus>();

        static void Main(string[] args)
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

            Console.ReadLine();
            service.CancelStreaming();
        }

        private static void SaveTweet(TwitterService service, TwitterStreamArtifact tweets)
        {
            var status = service.Deserialize<TwitterStatus>(tweets);
            if (status.User != null)
            {
                if (status.User.Id == 1346769720)
                    _feedTweets.Add(status);
                else 
                    _allTweets.Add(status);

                Console.WriteLine(status.Text);
            }
        }
    }
}
