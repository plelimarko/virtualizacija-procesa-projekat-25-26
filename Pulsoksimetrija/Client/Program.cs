using Common;
using Common1;
using System;
using System.ServiceModel;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Task 1: Klijent bira učesnika (fiksirano za test, kasnije petlja)
            string participant = "P01";
            string filePath = "ECG.csv"; // Putanja do tvog fajla

            // Task 4: IDisposable nad WCF proxy-jem
            ChannelFactory<IEcgService> factory = new ChannelFactory<IEcgService>("EcgEndpoint");
            IEcgService proxy = factory.CreateChannel();

            try
            {
                // Task 1: StartSession sa Meta podacima
                var meta = new SessionMeta
                {
                    ParticipantId = participant,
                    DeviceId = "Polar H10",
                    SampleRateHz = 130
                };

                proxy.StartSession(meta);
                Console.WriteLine("Sesija otvorena...");

                CsvParser parser = new CsvParser();
                var samples = parser.ParseEcgCsv(filePath, participant);

                foreach (var s in samples)
                {
                    try
                    {
                        proxy.PushSample(s);
                    }
                    catch (FaultException<ValidationFault> ex)
                    {
                        Console.WriteLine($"Validaciona greška: {ex.Detail.Message}");
                    }
                    catch (FaultException<DataFormatFault> ex)
                    {
                        Console.WriteLine($"Format greška: {ex.Detail.Message}");
                    }
                }

                proxy.EndSession();
                ((IClientChannel)proxy).Close();
                Console.WriteLine("Prenos završen uspešno.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Došlo je do prekida: {ex.Message}");
                ((IClientChannel)proxy).Abort(); // Task 4: Pravilno zatvaranje pri prekidu
            }
            finally
            {
                factory.Close();
            }

            Console.ReadLine();
        }
    }
}