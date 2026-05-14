using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class EcgSample
    {
        [DataMember] public long TimestampMs { get; set; }
        [DataMember] public double? EcgMicroV { get; set; }
        [DataMember] public double? HeartRate { get; set; }
        [DataMember] public double? IBI_ms { get; set; }
        [DataMember] public double? AccX { get; set; }
        [DataMember] public double? AccY { get; set; }
        [DataMember] public double? AccZ { get; set; }
        [DataMember] public string ParticipantId { get; set; }
        [DataMember] public int RowIndex { get; set; }
    }
}