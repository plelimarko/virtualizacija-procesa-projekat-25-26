using Common;
using System;
using System.Collections.Generic;
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

        public void StartSession(SessionMeta meta)
        {
            Console.WriteLine($"[START] Sesija za: {meta.ParticipantId}");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{meta.ParticipantId}_data.csv");
            fileWriter = new FileWriter(path);
        }

        public void PushSample(EcgSample sample)
        {
            fileWriter.WriteLine($"{sample.TimestampMs},{sample.HeartRate},{sample.EcgMicroV}");
        }

        public void EndSession() => Dispose();

        public void Dispose() => fileWriter?.Dispose();
    }
}
