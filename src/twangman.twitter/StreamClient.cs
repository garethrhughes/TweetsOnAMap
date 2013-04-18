using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Json;

namespace TwitterExperiments
{
    public sealed class StreamClient : IDisposable
    {

        ManualResetEventSlim _stopEvent = new ManualResetEventSlim(false);
        ManualResetEventSlim _stopedEvent = new ManualResetEventSlim(true);

        public void Stop()
        {
            _stopEvent.Set();
        }

        public void Connect(string terms, string user, string password, Action<status> OnStatus)
        {
            _stopEvent.Reset();
            _stopedEvent.Reset();

            int wait = 0;

            while (!_stopEvent.IsSet)
            {
                Thread.Sleep(wait);

                try
                {
                    //Connect

                    var request = (HttpWebRequest)WebRequest.Create("https://stream.twitter.com/1/statuses/filter.json");

                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.Credentials = new NetworkCredential(user, password);

                    //stream api specs
                    request.Timeout = -1;
                    request.Method = "POST";

                    string parameters = string.Format(@"track={0}&delimited=length", terms);
                    byte[] byteArray = Encoding.Default.GetBytes(parameters);
                    request.ContentType = @"application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;
                    using (var streamOut = request.GetRequestStream())
                    {
                        streamOut.Write(byteArray, 0, byteArray.Length);
                    }

                    using (var webResponse = (HttpWebResponse)request.GetResponse())
                    {
                        System.Console.WriteLine("Response content encoding :" + webResponse.ContentEncoding);
                        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        using (var responseStream = new StreamReader(webResponse.GetResponseStream(), encode))
                        {
                            while (!_stopEvent.IsSet)
                            {
                                ParseStream(responseStream, OnStatus);
                                wait = 250;
                            }
                        }
                    }

                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);

                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        //-- From Twitter Docs --
                        //When a HTTP error (> 200) is returned, back off exponentially.
                        //Perhaps start with a 10 second wait, double on each subsequent failure,
                        //and finally cap the wait at 240 seconds.
                        //Exponential Backoff
                        if (wait < 10000)
                        {
                            wait = 10000;
                        }
                        else
                        {
                            if (wait < 240000)
                            {
                                wait = wait * 2;
                            }
                        }
                    }
                    else
                    {
                        //-- From Twitter Docs --
                        //When a network error (TCP/IP level) is encountered, back off linearly.
                        //Perhaps start at 250 milliseconds and cap at 16 seconds.
                        //Linear Backoff
                        if (wait < 16000)
                        {
                            wait += 250;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " " + ex.Message);
                }

            }

            _stopedEvent.Set();
        }

        DataContractJsonSerializer _statusJson = new DataContractJsonSerializer(typeof(status));

        private void ParseStream(StreamReader responseStream, Action<status> OnStatus)
        {
            status status = new status();

            var lenghtString = responseStream.ReadLine();
            if (lenghtString.Trim().Length == 0)
            {
                //skip keep alive newline
                return;
            }
            var lenght = int.Parse(lenghtString);
            var buffer = new char[lenght];
            responseStream.Read(buffer, 0, lenght);
            string str = new string(buffer);

            var bytes = Encoding.UTF8.GetBytes(buffer);
            using (var memStream = new MemoryStream(bytes))
            {
                status = (status)_statusJson.ReadObject(memStream);
            }

            if (status.user == null)
            {
                Console.Write("Unknown object: " + str);
                return;
            }

            try
            {
                OnStatus(status);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " " + ex.ToString());
            }

        }

        public void Dispose()
        {
            Stop();
            _stopedEvent.Wait();
            _stopedEvent.Dispose();
            _stopEvent.Dispose();
        }
    }

    public class status
    {
        public object user { get; set; }
    }
}