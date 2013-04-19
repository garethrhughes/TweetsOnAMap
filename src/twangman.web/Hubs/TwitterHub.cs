namespace twangman.web.Hubs
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNet.SignalR;
  using Microsoft.AspNet.SignalR.Hubs;

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

    public override Task OnConnected()
    {
      _twitterTicker.IncreaseUsers();

      return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
    }

    public override Task OnDisconnected()
    {
      _twitterTicker.DecreaseUsers();

      return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
    }
  }

  public class TwitterTicker
  {
    private static readonly Lazy<TwitterTicker> _instance = new Lazy<TwitterTicker>(
      () => new TwitterTicker(GlobalHost.ConnectionManager.GetHubContext<TwitterHub>().Clients));

    
    private TwitterTicker(IHubConnectionContext clients)
    {
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
      if(!ClientIDs.Any(x => x == id))
        ClientIDs.Add(id);

      Clients.All.updateUserCount(UserCount);
    }

    public void IncreaseUsers(string id)
    {
      if (ClientIDs.Any(x => x == id))
        ClientIDs.Remove(id);

      Clients.All.updateUserCount(UserCount);
    }

      public void SendPostcode(int postcode, int size, double rating, double latitude, double longitude, string text, string screenName, string profileImageUrl)
      {
          Clients.All.displayPostcode(postcode, size, rating, latitude, longitude, text, screenName, profileImageUrl);
      }
  }
}