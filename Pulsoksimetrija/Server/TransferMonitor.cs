using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TransferStartedEventArgs : EventArgs
    {
        public string ParticipantId { get; }
        public string DeviceId { get; }

        public TransferStartedEventArgs(string participantId, string deviceId)
        {
            ParticipantId = participantId;
            DeviceId = deviceId;
        }
    }

    public class SampleReceivedEventArgs : EventArgs
    {
        public EcgSample Sample { get; }

        public SampleReceivedEventArgs(EcgSample sample)
        {
            Sample = sample;
        }
    }

    public class BatchReceivedEventArgs : EventArgs
    {
        public int BatchNumber { get; }
        public int BatchSize { get; }
        public int Accepted { get; }
        public int Rejected { get; }

        public BatchReceivedEventArgs(int batchNumber, int batchSize, int accepted, int rejected)
        {
            BatchNumber = batchNumber;
            BatchSize = batchSize;
            Accepted = accepted;
            Rejected = rejected;
        }
    }

    public class WarningEventArgs : EventArgs
    {
        public WarningType Type { get; }
        public string Message { get; }
        public int RowIndex { get; }

        public WarningEventArgs(WarningType type, string message, int rowIndex)
        {
            Type = type;
            Message = message;
            RowIndex = rowIndex;
        }
    }

    public class TransferCompletedEventArgs : EventArgs
    {
        public string ParticipantId { get; }
        public int TotalAccepted { get; }
        public int TotalRejected { get; }

        public TransferCompletedEventArgs(string participantId, int totalAccepted, int totalRejected)
        {
            ParticipantId = participantId;
            TotalAccepted = totalAccepted;
            TotalRejected = totalRejected;
        }
    }

    public class TransferMonitor
    {
        public delegate void TransferStartedHandler(object sender, TransferStartedEventArgs e);
        public delegate void SampleReceivedHandler(object sender, SampleReceivedEventArgs e);
        public delegate void BatchReceivedHandler(object sender, BatchReceivedEventArgs e);
        public delegate void WarningRaisedHandler(object sender, WarningEventArgs e);
        public delegate void TransferCompletedHandler(object sender, TransferCompletedEventArgs e);

        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event BatchReceivedHandler OnBatchReceived;
        public event WarningRaisedHandler OnWarningRaised;
        public event TransferCompletedHandler OnTransferCompleted;

        public void RaiseTransferStarted(object sender, TransferStartedEventArgs e)
        {
            if (OnTransferStarted != null)
                OnTransferStarted(sender, e);
        }

        public void RaiseSampleReceived(object sender, SampleReceivedEventArgs e)
        {
            if (OnSampleReceived != null)
                OnSampleReceived(sender, e);
        }

        public void RaiseBatchReceived(object sender, BatchReceivedEventArgs e)
        {
            if (OnBatchReceived != null)
                OnBatchReceived(sender, e);
        }

        public void RaiseWarning(object sender, WarningEventArgs e)
        {
            if (OnWarningRaised != null)
                OnWarningRaised(sender, e);
        }

        public void RaiseTransferCompleted(object sender, TransferCompletedEventArgs e)
        {
            if (OnTransferCompleted != null)
                OnTransferCompleted(sender, e);
        }
    }
}
