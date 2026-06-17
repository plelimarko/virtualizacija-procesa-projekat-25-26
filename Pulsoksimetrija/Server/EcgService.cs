using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class EcgService : IEcgService, IDisposable
    {
        private FileWriter fileWriter;
        private FileWriter rejectWriter;
        private long _lastTimestamp = -1;
        private bool disposed = false;

        private TransferMonitor monitor;
        private ConsoleListener listener;
        private AnalyticsEngine analytics;
        private string participantId;
        private int totalAccepted = 0;
        private int totalRejected = 0;
        private int batchNumber = 0;

        private double ecgMin = double.Parse(ConfigurationManager.AppSettings["EcgMinMicroV"]);
        private double ecgMax = double.Parse(ConfigurationManager.AppSettings["EcgMaxMicroV"]);
        private double hrMin = double.Parse(ConfigurationManager.AppSettings["HrMinBpm"]);
        private double hrMax = double.Parse(ConfigurationManager.AppSettings["HrMaxBpm"]);

        public void StartSession(SessionMeta meta)
        {
            participantId = meta.ParticipantId;

            string datePath = DateTime.Now.ToString("yyyy-MM-dd");
            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", meta.ParticipantId, "PolarH10", datePath);
            Directory.CreateDirectory(basePath);

            fileWriter = new FileWriter(Path.Combine(basePath, "session.csv"));
            rejectWriter = new FileWriter(Path.Combine(basePath, "rejects.csv"));

            monitor = new TransferMonitor();
            listener = new ConsoleListener(monitor);
            analytics = new AnalyticsEngine(monitor);

            monitor.RaiseTransferStarted(this,
                new TransferStartedEventArgs(meta.ParticipantId, meta.DeviceId));
        }

        public void PushSample(EcgSample sample)
        {
            monitor.RaiseSampleReceived(this, new SampleReceivedEventArgs(sample));

            if (sample.TimestampMs <= _lastTimestamp)
            {
                rejectWriter.WriteLine(
                    $"{sample.RowIndex},Timestamp nije rastuci,{sample.TimestampMs}");
                throw new FaultException<ValidationFault>(new ValidationFault
                {
                    Message = "Timestamp mora biti veci od prethodnog!",
                    Parametar = "TimestampMs"
                }, new FaultReason("Timestamp mora biti veci od prethodnog!"));
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
            analytics.ProcessSample(this, sample);
        }

        public string PushBatch(List<EcgSample> batch)
        {
            int accepted = 0;
            int rejected = 0;

            foreach (EcgSample sample in batch)
            {
                monitor.RaiseSampleReceived(this, new SampleReceivedEventArgs(sample));

                if (sample.TimestampMs <= _lastTimestamp)
                {
                    rejectWriter.WriteLine(
                        $"{sample.RowIndex},Timestamp nije rastuci,{sample.TimestampMs}");
                    rejected++;
                    continue;
                }
                _lastTimestamp = sample.TimestampMs;

                if (sample.EcgMicroV.HasValue && (sample.EcgMicroV < ecgMin || sample.EcgMicroV > ecgMax))
                {
                    rejectWriter.WriteLine(
                        $"{sample.RowIndex},EcgMicroV van opsega,{sample.TimestampMs},{sample.EcgMicroV}");
                    rejected++;
                    continue;
                }

                if (sample.HeartRate.HasValue && (sample.HeartRate < hrMin || sample.HeartRate > hrMax))
                {
                    rejectWriter.WriteLine(
                        $"{sample.RowIndex},HeartRate van opsega,{sample.TimestampMs},{sample.HeartRate}");
                    rejected++;
                    continue;
                }

                string line = $"{sample.TimestampMs},{sample.EcgMicroV},{sample.HeartRate}," +
                               $"{sample.IBI_ms},{sample.AccX},{sample.AccY},{sample.AccZ},{sample.RowIndex}";
                fileWriter.WriteLine(line);
                analytics.ProcessSample(this, sample);
                accepted++;
            }

            batchNumber++;
            totalAccepted += accepted;
            totalRejected += rejected;

            monitor.RaiseBatchReceived(this,
                new BatchReceivedEventArgs(batchNumber, batch.Count, accepted, rejected));

            return $"OK:{accepted},REJECTED:{rejected}";
        }

        public void EndSession()
        {
            monitor.RaiseTransferCompleted(this,
                new TransferCompletedEventArgs(participantId, totalAccepted, totalRejected));
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
                }
                disposed = true;
            }
        }
    }
}
