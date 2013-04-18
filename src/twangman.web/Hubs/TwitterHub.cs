namespace twangman.web.Hubs
{
  using System;
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

    private int _userCount;

    private TwitterTicker(IHubConnectionContext clients)
    {
      Clients = clients;
    }

    protected IHubConnectionContext Clients { get; set; }

    public static TwitterTicker Instance
    {
      get { return _instance.Value; }
    }

    public int UserCount
    {
      get { return _userCount; }
    }

    public void DecreaseUsers()
    {
      _userCount -= 1;
      Clients.All.updateUserCount(_userCount);
    }

    public void IncreaseUsers()
    {
      _userCount += 1;
      Clients.All.updateUserCount(_userCount);
    }
  }
}