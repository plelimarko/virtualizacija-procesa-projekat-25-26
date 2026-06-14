using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class EcgService : IEcgService, IDisposable
    {
        private FileWriter fileWriter;
        private FileWriter rejectWriter;
        private long _lastTimestamp = -1;
        private bool disposed = false;

        private double ecgMin = double.Parse(ConfigurationManager.AppSettings["EcgMinMicroV"]);
        private double ecgMax = double.Parse(ConfigurationManager.AppSettings["EcgMaxMicroV"]);
        private double hrMin = double.Parse(ConfigurationManager.AppSettings["HrMinBpm"]);
        private double hrMax = double.Parse(ConfigurationManager.AppSettings["HrMaxBpm"]);

        public void StartSession(SessionMeta meta)
        {
            Console.WriteLine($"[SESIJA] Početak za učesnika: {meta.ParticipantId}");

            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Data", meta.ParticipantId);
            Directory.CreateDirectory(basePath);

            fileWriter = new FileWriter(Path.Combine(basePath, "session.csv"));
            rejectWriter = new FileWriter(Path.Combine(basePath, "rejects.csv"));
        }

        public void PushSample(EcgSample sample)
        {
            if (sample.TimestampMs <= _lastTimestamp)
            {
                rejectWriter.WriteLine(
                    $"{sample.RowIndex},Timestamp nije rastuci,{sample.TimestampMs}");
                throw new FaultException<ValidationFault>(new ValidationFault
                {
                    Message = "Timestamp mora biti veći od prethodnog!",
                    Parametar = "TimestampMs"
                }, new FaultReason("Timestamp mora biti veći od prethodnog!"));
            }
            _lastTimestamp = sample.TimestampMs;

            if (sample.EcgMicroV.HasValue && (sample.EcgMicroV < ecgMin || sample.EcgMicroV > ecgMax))
            {
                rejectWriter.WriteLine(
                    $"{sample.RowIndex},EcgMicroV van opsega,{sample.TimestampMs},{sample.EcgMicroV}");
                throw new FaultException<DataFormatFault>(new DataFormatFault
                {
                    Message = "EKG vrednost van dozvoljenog opsega!",
                    Details = "EcgMicroV"
                }, new FaultReason("EKG vrednost van dozvoljenog opsega!"));
            }

            if (sample.HeartRate.HasValue && (sample.HeartRate < hrMin || sample.HeartRate > hrMax))
            {
                rejectWriter.WriteLine(
                    $"{sample.RowIndex},HeartRate van opsega,{sample.TimestampMs},{sample.HeartRate}");
                throw new FaultException<DataFormatFault>(new DataFormatFault
                {
                    Message = "Puls van realnog opsega!",
                    Details = "HeartRate"
                }, new FaultReason("Puls van realnog opsega!"));
            }

            string line = $"{sample.TimestampMs},{sample.EcgMicroV},{sample.HeartRate}," +
                           $"{sample.IBI_ms},{sample.AccX},{sample.AccY},{sample.AccZ},{sample.RowIndex}";
            fileWriter.WriteLine(line);
        }

        public void EndSession()
        {
            Console.WriteLine("[SESIJA] Kraj prenosa.");
            Dispose();
        }

        ~EcgService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (fileWriter != null)
                    {
                        fileWriter.Dispose();
                        fileWriter = null;
                    }
                    if (rejectWriter != null)
                    {
                        rejectWriter.Dispose();
                        rejectWriter = null;
                    }
                    Console.WriteLine("[DEBUG] Resursi uspešno oslobođeni.");
                }
                disposed = true;
            }
        }
    }
}
