using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] Dns = { "208.67.222.123", "208.67.220.123" };
            List<String> networkDescriptions = new List<String>();



            CheckDnsRule(Dns, networkDescriptions);

            CheckDnsRule(Dns, networkDescriptions);

            RemoveDnsRule(networkDescriptions);


        }

        public static void FlushDns()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = $"/flushdns",
                CreateNoWindow = true,
                Verb = "runas",
                UseShellExecute = true
            }).WaitForExit();
        }

        //public static void AddDnsRule(List<String> ntwDescriptions)
        //{
        //    string[] Dns = { "208.67.222.123", "208.67.220.123" };

        //    var CurrentInterface = NetworkInterface;
        //    if (CurrentInterface == null)
        //    {
        //        //log null network interface
        //        return;
        //    }

        //    ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //    ManagementObjectCollection objMOC = objMC.GetInstances();
        //    foreach (ManagementObject objMO in objMOC)
        //    {
        //        if ((bool)objMO["IPEnabled"])
        //        {
        //            if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
        //            {
        //                ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
        //                if (objdns != null)
        //                {
        //                    objdns["DNSServerSearchOrder"] = Dns;
        //                    objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);

        //                    String networkDescription = ntwDescriptions.Where(ntwDescr => ntwDescr == CurrentInterface.Description).FirstOrDefault();

        //                    if (networkDescription is null)
        //                        ntwDescriptions.Add(CurrentInterface.Description);
        //                }
        //            }
        //        }
        //    }

        //    FlushDns();
        //}

        public static void CheckDnsRule(string[] Dns, List<String> ntwDescriptions)
        {
            //Dns = new String[] { "208.67.222.123", "208.67.220.123" };
            NetworkObject networkObject = new NetworkObject();

            var CurrentInterface = networkObject.GetActiveNetworkInterface();
            
            if (CurrentInterface == null)
            {
                //log null network interface
                return;
            }

            var userDns = new List<IPAddress>()
            {
                IPAddress.Parse(Dns[0]),
                IPAddress.Parse(Dns[1])
            };

            var asdd = CurrentInterface.GetIPProperties();

            foreach (IPAddress dnsAddr in asdd.DnsAddresses)
            {
                if (userDns.Any(i => i.Equals(dnsAddr)))
                {
                    //se tutti entrano in questo if, allora ok
                }
            }

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            //dns è stato eliminato o modificato -> ripristinare dns scelto
                            objdns["DNSServerSearchOrder"] = Dns;

                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);

                            String networkDescription = ntwDescriptions.Where(ntwDescr => ntwDescr == CurrentInterface.Description).FirstOrDefault();

                            if (networkDescription is null)
                                ntwDescriptions.Add(CurrentInterface.Description);

                            FlushDns();
                        }
                    }
                }
            }
        }

        public static bool DnsChanged { get; set; }

        public static void RemoveDnsRule(List<String> ntwDescriptions)
        {
            NetworkObject networkObject = new NetworkObject();

            var CurrentInterface = networkObject.GetActiveNetworkInterface();

            if (CurrentInterface == null)
            {
                //log null network interface
                return;
            }

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if (ntwDescriptions.Any(ntwDescription => ntwDescription == objMO["Description"].ToString()))
                {
                    ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                    if (objdns != null)
                    {
                        objdns["DNSServerSearchOrder"] = null;
                        objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }
                }
            }

            FlushDns();

        }
    }

    public class NetworkObject
    {
        public NetworkInterface GetActiveNetworkInterface()
        {
            var asd = NetworkInterface.GetAllNetworkInterfaces();
            return asd.FirstOrDefault(
        a => a.OperationalStatus == OperationalStatus.Up && (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
        a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

        }
    }
}
