using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember] public string ParticipantId { get; set; }
        [DataMember] public string DeviceId { get; set; }
        [DataMember] public double SampleRateHz { get; set; }
        [DataMember] public long TimestampOffsetMs { get; set; }
    }
}