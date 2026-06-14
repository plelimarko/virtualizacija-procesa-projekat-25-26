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
        private long _lastTimestamp = -1; // Za proveru monotonog rasta


        private double ecgMin = double.Parse(ConfigurationManager.AppSettings["EcgMinMicroV"]);
        private double ecgMax = double.Parse(ConfigurationManager.AppSettings["EcgMaxMicroV"]);
        private double hrMin = double.Parse(ConfigurationManager.AppSettings["HrMinBpm"]);
        private double hrMax = double.Parse(ConfigurationManager.AppSettings["HrMaxBpm"]);

        public void StartSession(SessionMeta meta)
        {
            Console.WriteLine($"[SESIJA] Početak za učesnika: {meta.ParticipantId}");
            string fileName = $"{meta.ParticipantId}_ECG_Received.csv";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            fileWriter = new FileWriter(path);
        }

        public void PushSample(EcgSample sample)
        {
            // 1. Validacija: Monoton rast vremena
            if (sample.TimestampMs <= _lastTimestamp)
            {
                throw new FaultException<ValidationFault>(new ValidationFault
                {
                    Message = "Timestamp mora biti veći od prethodnog!",
                    Parametar = "TimestampMs"
                });
            }
            _lastTimestamp = sample.TimestampMs;

            // 2. Validacija: Opseg EKG [-5000, 5000]
            if (sample.EcgMicroV.HasValue && (sample.EcgMicroV < ecgMin || sample.EcgMicroV > ecgMax))
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault
                {
                    Message = "EKG vrednost van dozvoljenog opsega!",
                    Details = "EcgMicroV"
                });
            }

            // 3. Validacija: Opseg pulsa [30, 220]
            if (sample.HeartRate.HasValue && (sample.HeartRate < hrMin || sample.HeartRate > hrMax))
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault
                {
                    Message = "Puls van realnog opsega!",
                    Details = "HeartRate"
                });
            }

            // Ako je sve u redu, pišemo na disk
            string line = $"{sample.TimestampMs},{sample.EcgMicroV},{sample.HeartRate},{sample.RowIndex}";
            fileWriter.WriteLine(line);
        }

        public void EndSession()
        {
            Console.WriteLine("[SESIJA] Kraj prenosa.");
            Dispose();
        }

        public void Dispose()
        {
            if (fileWriter != null)
            {
                fileWriter.Dispose();
                fileWriter = null;
                Console.WriteLine("[DEBUG] FileWriter resursi su uspešno oslobođeni.");
            }
        }
    }
}
