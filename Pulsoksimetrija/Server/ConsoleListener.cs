using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ConsoleListener
    {
        public ConsoleListener(TransferMonitor monitor)
        {
            monitor.OnTransferStarted += OnTransferStarted;
            monitor.OnSampleReceived += OnSampleReceived;
            monitor.OnBatchReceived += OnBatchReceived;
            monitor.OnWarningRaised += OnWarningRaised;
            monitor.OnTransferCompleted += OnTransferCompleted;
        }

        private void OnTransferStarted(object sender, TransferStartedEventArgs e)
        {
            Console.WriteLine($"[EVENT] Transfer započet za {e.ParticipantId} ({e.DeviceId})");
        }

        private void OnSampleReceived(object sender, SampleReceivedEventArgs e)
        {
            // Ne ispisujemo svaki uzorak da ne usporimo
        }

        private void OnBatchReceived(object sender, BatchReceivedEventArgs e)
        {
            Console.WriteLine($"[EVENT] Blok #{e.BatchNumber} primljen ({e.Accepted} ok, {e.Rejected} odbijeno)");
        }

        private void OnWarningRaised(object sender, WarningEventArgs e)
        {
            Console.WriteLine($"[WARNING] {e.Type} - Red {e.RowIndex}: {e.Message}");
        }

        private void OnTransferCompleted(object sender, TransferCompletedEventArgs e)
        {
            Console.WriteLine($"[EVENT] Transfer završen za {e.ParticipantId}. Ukupno: {e.TotalAccepted} prihvaćeno, {e.TotalRejected} odbijeno");
        }
    }
}
