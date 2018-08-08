using DnsClient;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace DnsHost
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("DnsChecker");

        static void Main()
        {
            /*
             How DNS Works
             https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/dd197552(v=ws.10)#Understanding%20aging%20and%20scavenging
             */
            string dnsIp = "<DNS server Ip Address>";
            string hostOrIp = "<hostname/IP to resolve>";

            //Start 4 threads
            //Contuniously resolve hostname - to check DNS server performance - best to troubleshoot any issue with inhouse DNS server
            for (int i = 0; i < 4; i++)
            {
                new Thread(() => Resolver(dnsIp, hostOrIp)) { Name = $"Thread-{i}" }.Start();
                Thread.Sleep(250);
            }
        }

        static void Resolver(string dnsIp, string hostOrIp)
        {
            try
            {
                int iWaitTime = Convert.ToInt32(ConfigurationManager.AppSettings["WaitTimeInSec"] ?? "5") * 1000;
                Stopwatch sw = new Stopwatch();
                
                var endpoint = new IPEndPoint(IPAddress.Parse(dnsIp), 53);
                LookupClient lookup = null;
                IPHostEntry hostEntry = null;
                while (true)
                {
                    sw.Restart();
                    lookup = new LookupClient(endpoint);
                    hostEntry = lookup.GetHostEntry(hostOrIp);
                    sw.Stop();
                    log.Debug($"{hostEntry.HostName} returned {hostEntry.AddressList[0]} ipaddress from DNS {dnsIp} and took {sw.Elapsed} time.");
                    hostEntry = null;
                    lookup = null;
                    Thread.Sleep(iWaitTime);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        static void MainOld()
        {
            string hostname = "";

            IPHostEntry host;

            host = Dns.GetHostEntry(hostname);

            Console.WriteLine("GetHostEntry({0}) returns:", hostname);

            foreach (IPAddress ip in host.AddressList)
            {
                Console.WriteLine("    {0}", ip);
            }
        }
    }
}
