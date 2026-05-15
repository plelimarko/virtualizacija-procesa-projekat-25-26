using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ServiceHost je WCF objekat koji 'oživljava' tvoj servis
            using (ServiceHost host = new ServiceHost(typeof(EcgService)))
            {
                try
                {
                    host.Open();
                    Console.WriteLine("========================================");
                    Console.WriteLine("   GALAXY PPG SERVER JE POKRENUT");
                    Console.WriteLine("   Adresa: net.tcp://localhost:4000/EcgService");
                    Console.WriteLine("========================================");
                    Console.WriteLine("Pritisni [Enter] da ugasiš server...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri pokretanju: {ex.Message}");
                    Console.ReadLine();
                }
            }
        }
    }
}
