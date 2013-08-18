namespace twangman.web.Hubs
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Microsoft.AspNet.SignalR;
  using Microsoft.AspNet.SignalR.Hubs;

  using TweetSharp;

  using twangman.web.App_Start;

  [HubName("twitterTicker")]
  public class TwitterHub : Hub
  {
    private readonly TwitterTicker _twitterTicker;

    public TwitterHub() : this(TwitterTicker.Instance)
    {
    }

    public TwitterHub(TwitterTicker stockTicker)
    {
      _twitterTicker = stockTicker;
    }

    public int GetUserCount()
    {
      return _twitterTicker.UserCount;
    }

    public string GetPostcodeInfo(string postcode)
    {
        return _twitterTicker.DisplayInfo(postcode);
    }

    public override Task OnConnected()
    {
      _twitterTicker.IncreaseUsers(Context.ConnectionId);

      return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
    }

    public override Task OnDisconnected()
    {
      _twitterTicker.DecreaseUsers(Context.ConnectionId);

      return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
    }
  }

  public class TwitterTicker
  {
    private static readonly Lazy<TwitterTicker> _instance = new Lazy<TwitterTicker>(
      () => new TwitterTicker(GlobalHost.ConnectionManager.GetHubContext<TwitterHub>().Clients));

    public static IList<TweetData> AllTweets { get; set; }

    private TwitterTicker(IHubConnectionContext clients)
    {
      ClientIDs = new List<string>();
      AllTweets = new List<TweetData>();
      Clients = clients;
    }

    protected IHubConnectionContext Clients { get; set; }

    protected IList<string> ClientIDs { get; set; }

    public static TwitterTicker Instance
    {
      get { return _instance.Value; }
    }

    public int UserCount
    {
      get { return ClientIDs.Count; }
    }

    public void DecreaseUsers(string id)
    {
      if(ClientIDs.Any(x => x == id))
        ClientIDs.Remove(id);

      Clients.All.updateUserCount(UserCount);
    }

    public void IncreaseUsers(string id)
    {
      if (!ClientIDs.Any(x => x == id))
        ClientIDs.Add(id);

      Clients.All.updateUserCount(UserCount);
    }

    public void SendPostcode(TweetDetails status)
    {
        var postcode = PostcodeLoader.Postcodes.FirstOrDefault(x => x.Code == int.Parse(status.Postcode));

        if (postcode != null)
        {
            Clients.All.displayPostcode(
                postcode,
                1,
                1,
                postcode.Latitude,
                postcode.Longitude,
                status.Status.Text,
                status.Status.User.ScreenName,
                status.Status.User.ProfileImageUrl,
                AllTweets.Count);
        }
    }

      public void SendElectorateUpdate(ElectorateSummary summary)
      {
          var postcode = PostcodeLoader.Postcodes.FirstOrDefault(x => x.Code == int.Parse(summary.Postcode));

          if (postcode != null)
          {
              Clients.All.displayElectorateUpdate(postcode, summary.TotalVotes, summary.VotesByParty);
          }
      }

      public void SendAccountTweet(string text, string screenName, string profileImageUrl)
      {
          Clients.All.displayAccountTweet(text, screenName, profileImageUrl);
      }

      public string DisplayInfo(string code)
      {
          var i = int.Parse(code);
          var postcode =  PostcodeLoader.Postcodes.FirstOrDefault(x => x.Code == i);
          return string.Format("{0}, {1}<br />Total Tweets: {2}, Average Rating: {3}", postcode.Area, code, AllTweets.Count(x => x.Code == i), AllTweets.Where(x => x.Code == i).Average(x => x.Rating).ToString("0,0.00"));
      }
  }
}