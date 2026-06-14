using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Pokretanje GalaxyPPG Klijenta ===");

            Console.Write("Unesite broj učesnika (npr. 01, 02.. ili P01, P24): ");
            string input = Console.ReadLine().Trim().ToUpper();

            string participant = input;
            if (!participant.StartsWith("P"))
            {
                participant = "P" + input.PadLeft(2, '0');
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string rootDir = Directory.GetParent(baseDir).Parent.Parent.FullName;
            string filePath = Path.Combine(rootDir, "Dataset", participant, "PolarH10", "ECG.csv");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[GREŠKA] Fajl nije pronađen! Tražena putanja: {filePath}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\n✓ Započinjem proces za učesnika: {participant}");
            Console.WriteLine($"✓ Fajl pronađen: {filePath}");

            ChannelFactory<IEcgService> factory = new ChannelFactory<IEcgService>("EcgEndpoint");
            IEcgService proxy = factory.CreateChannel();

            try
            {
                SessionMeta meta = new SessionMeta
                {
                    ParticipantId = participant,
                    DeviceId = "PolarH10",
                    SampleRateHz = 130
                };

                proxy.StartSession(meta);
                Console.WriteLine("\n[WCF] Sesija uspešno otvorena.");

                CsvParser parser = new CsvParser();
                List<EcgSample> samples = parser.ParseEcgCsv(filePath, participant);
                Console.WriteLine($"[CSV] Učitano {samples.Count} validnih zapisa. Slanje...");

                int okCount = 0;
                int rejectedCount = 0;

                int total = samples.Count;

                foreach (EcgSample s in samples)
                {
                    try
                    {
                        proxy.PushSample(s);
                        okCount++;
                    }
                    catch (FaultException<ValidationFault> vf)
                    {
                        rejectedCount++;
                    }
                    catch (FaultException<DataFormatFault> df)
                    {
                        rejectedCount++;
                    }

                    int current = okCount + rejectedCount;
                    if (current % 10000 == 0)
                    {
                        Console.WriteLine($"[Progres] {current}/{total} ({(current * 100 / total)}%)");
                    }
                }

                proxy.EndSession();
                Console.WriteLine($"\n[Rezultat] Prihvaćeno: {okCount}, Odbijeno na serveru: {rejectedCount}");
                Console.WriteLine("[WCF] Prenos završen. Sesija zatvorena.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Kritična greška]: {ex.Message}");
            }
            finally
            {
                if (factory.State == CommunicationState.Opened)
                {
                    factory.Close();
                }
                else
                {
                    factory.Abort();
                }
            }

            Console.WriteLine("Pritisnite Enter za izlaz...");
            Console.ReadLine();
        }
    }
}