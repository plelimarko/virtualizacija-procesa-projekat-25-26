using Common;
using System;
using System.Configuration;

namespace Server
{
    public class AnalyticsEngine
    {
        private TransferMonitor monitor;

        // Faza 4: ECG spike
        private double? lastEcgValue = null;
        private double ecgSpikeThreshold = double.Parse(
            ConfigurationManager.AppSettings["EcgSpikeThresholdMicroV"]);

        // Faza 4: HR opseg
        private double hrMin = double.Parse(
            ConfigurationManager.AppSettings["HrMinBpm"]);
        private double hrMax = double.Parse(
            ConfigurationManager.AppSettings["HrMaxBpm"]);

        // Faza 5: ACC pokret
        private double accMotionThreshold = double.Parse(
            ConfigurationManager.AppSettings["AccMotionThreshold"]);

        // Faza 5: IBI tekuci prosek
        private double ibiSum = 0;
        private int ibiCount = 0;

        public AnalyticsEngine(TransferMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void ProcessSample(object sender, EcgSample sample)
        {
            CheckEcgSpike(sender, sample);
            CheckHeartRate(sender, sample);
            CheckMotion(sender, sample);
            CheckIbi(sender, sample);
        }

        private void CheckEcgSpike(object sender, EcgSample sample)
        {
            if (!sample.EcgMicroV.HasValue)
                return;

            if (lastEcgValue.HasValue)
            {
                double delta = sample.EcgMicroV.Value - lastEcgValue.Value;

                if (Math.Abs(delta) > ecgSpikeThreshold)
                {
                    string smer = delta > 0 ? "rast" : "pad";
                    monitor.RaiseWarning(sender, new WarningEventArgs(
                        WarningType.EcgSpike,
                        $"Nagli {smer} EKG amplitude: delta={delta:F1} uV",
                        sample.RowIndex));
                }
            }

            lastEcgValue = sample.EcgMicroV.Value;
        }

        private void CheckHeartRate(object sender, EcgSample sample)
        {
            if (!sample.HeartRate.HasValue)
                return;

            if (sample.HeartRate.Value < hrMin || sample.HeartRate.Value > hrMax)
            {
                monitor.RaiseWarning(sender, new WarningEventArgs(
                    WarningType.HrOutOfRange,
                    $"HR van opsega: {sample.HeartRate.Value} bpm (dozvoljeno {hrMin}-{hrMax})",
                    sample.RowIndex));
            }
        }

        private void CheckMotion(object sender, EcgSample sample)
        {
            if (!sample.AccX.HasValue || !sample.AccY.HasValue || !sample.AccZ.HasValue)
                return;

            double aNorm = Math.Sqrt(
                sample.AccX.Value * sample.AccX.Value +
                sample.AccY.Value * sample.AccY.Value +
                sample.AccZ.Value * sample.AccZ.Value);

            if (aNorm > accMotionThreshold)
            {
                monitor.RaiseWarning(sender, new WarningEventArgs(
                    WarningType.ExcessiveMotion,
                    $"Prekomerno kretanje: Anorm={aNorm:F2} (prag={accMotionThreshold})",
                    sample.RowIndex));
            }
        }

        private void CheckIbi(object sender, EcgSample sample)
        {
            if (!sample.IBI_ms.HasValue)
                return;

            double ibiValue = sample.IBI_ms.Value;

            if (ibiCount > 0)
            {
                double ibiMean = ibiSum / ibiCount;

                if (ibiValue < 0.75 * ibiMean || ibiValue > 1.25 * ibiMean)
                {
                    monitor.RaiseWarning(sender, new WarningEventArgs(
                        WarningType.IbiOutOfBand,
                        $"IBI odstupanje: {ibiValue:F1} ms (prosek={ibiMean:F1} ms, opseg={0.75 * ibiMean:F1}-{1.25 * ibiMean:F1})",
                        sample.RowIndex));
                }
            }

            ibiSum += ibiValue;
            ibiCount++;
        }
    }
}
