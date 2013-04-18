using twangman.web.App_Start;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (PostcodeLoader), "Start")]

namespace twangman.web.App_Start
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Web.Hosting;
  using CsvHelper;

  public class PostcodeLoader
  {
    public static IEnumerable<Postcode> Postcodes { get; set; }
    public static void Start()
    {
      var path = HostingEnvironment.MapPath("~/postcode.csv");
      var csv = new CsvReader(new StreamReader(path));
      Postcodes = csv.GetRecords<Postcode>().ToList();
    }
  }

  public class Postcode
  {
    public int Code { get; set; }
    public string Area { get; set; }
    public string State { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }
}