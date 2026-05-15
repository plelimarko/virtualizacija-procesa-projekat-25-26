using Common;
using Common1;
using System;
using System.IO;
using System.ServiceModel;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Pokretanje GalaxyPPG Klijenta ===");

            // 1. Dinamički unos učesnika
            Console.Write("Unesite broj učesnika (npr. 01, 02.. ili P01, P24): ");
            string input = Console.ReadLine().Trim().ToUpper();

            string participant = input;
            if (!participant.StartsWith("P"))
            {
                // Ako korisnik unese samo "1", formatiramo ga u "P01"
                participant = "P" + input.PadLeft(2, '0');
            }

            // 2. Dinamičko kreiranje putanje
            // Klijent se pokreće iz ...\Client\bin\Debug
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Idemo 3 nivoa gore do glavnog foldera rešenja
            string rootDir = Directory.GetParent(baseDir).Parent.Parent.FullName;
            // Sklapamo punu putanju
            string filePath = Path.Combine(rootDir, "Dataset", participant, "PolarH10", "ECG.csv");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[GREŠKA] Fajl nije pronađen! Tražena putanja: {filePath}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\n✓ Započinjem proces za učesnika: {participant}");
            Console.WriteLine($"✓ Fajl pronađen: {filePath}");

            // 3. Pokretanje WCF komunikacije (IDisposable klijent)
            ChannelFactory<IEcgService> factory = new ChannelFactory<IEcgService>("EcgEndpoint");
            IEcgService proxy = factory.CreateChannel();

            try
            {
                var meta = new SessionMeta
                {
                    ParticipantId = participant,
                    DeviceId = "Polar H10",
                    SampleRateHz = 130
                };

                proxy.StartSession(meta);
                Console.WriteLine("\n[WCF] Sesija uspešno otvorena.");

                CsvParser parser = new CsvParser();
                var samples = parser.ParseEcgCsv(filePath, participant);

                Console.WriteLine($"[CSV] Učitano {samples.Count} validnih zapisa u memoriju. Slanje...");

                foreach (var s in samples)
                {
                    try
                    {
                        proxy.PushSample(s);
                    }
                    catch (FaultException<ValidationFault> vf)
                    {
                        Console.WriteLine($"[Validacija] Red {s.RowIndex}: {vf.Detail.Message} (Polje: {vf.Detail.Parametar})");
                    }
                    catch (FaultException<DataFormatFault> df)
                    {
                        Console.WriteLine($"[Format] Red {s.RowIndex}: {df.Detail.Message} (Polje: {df.Detail.Details})");
                    }
                }

                proxy.EndSession();
                Console.WriteLine("\n[WCF] Prenos završen. Sesija zatvorena.");
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