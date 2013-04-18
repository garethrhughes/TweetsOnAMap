using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace twitter_hangman
{
    class Program
    {
        private const string _consumerKey = "";
        private static string _consumerSecret = "";
        private static string _accessToken = "";
        private static string _accessTokenSecret = "";

        static void Main(string[] args)
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

           

            
            IAsyncResult result = service.StreamUser(
                (tweets, response) =>
                    {
                        //if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine(response);
                            Console.WriteLine(tweets.RawSource);

                            //foreach (var tweet in tweets)
                            //{
                            //    Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Text);
                            //}
                        }
                    });

            Console.WriteLine(result.AsyncState);

            //var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());
   
            //foreach (var tweet in tweets)
            //{
            //    Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
            //}
            
            Console.ReadLine();
            service.CancelStreaming();
        }
    }
}
 