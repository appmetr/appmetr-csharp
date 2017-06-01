using System.Linq;

namespace AppmetrCS.Persister
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Actions;

    #endregion

    [DataContract]
    [KnownType(typeof(AttachProperties))]
    [KnownType(typeof(TrackEvent))]
    [KnownType(typeof(TrackLevel))]
    [KnownType(typeof(TrackPayment))]
    [KnownType(typeof(TrackSession))]
    public class Batch
    {
        [DataMember(Name = "batchId")]
        public readonly Int32 BatchId;

        [DataMember(Name = "batch")]
        public readonly List<AppMetrAction> Actions;

        [DataMember(Name = "serverId")]
        public readonly String ServerId;

        private Batch()
        {
        }

        public Batch(String serverId, Int32 batchId, IEnumerable<AppMetrAction> actionList)
        {
            ServerId = serverId;
            BatchId = batchId;
            Actions = new List<AppMetrAction>(actionList);
        }

        public override String ToString()
        {
            return $"Batch{{batchId={BatchId}, serverId={ServerId}, events={String.Join(",", Actions.Select(v => v.ToString()).ToArray())}}}";
        }
    }
}
