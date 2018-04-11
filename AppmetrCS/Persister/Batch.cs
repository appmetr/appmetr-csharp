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
    [KnownType(typeof(TrackIdentify))]
    [KnownType(typeof(TrackState))]
    public class Batch
    {
        [DataMember(Name = "batchId")]
        public readonly Int64 BatchId;

        [DataMember(Name = "batch")]
        public readonly List<AppMetrAction> Actions;

        private Batch()
        {
        }

        public Batch(Int64 batchId, IEnumerable<AppMetrAction> actionList)
        {
            BatchId = batchId;
            Actions = new List<AppMetrAction>();

            if (actionList != null)
            {
                Actions.AddRange(actionList);
            }
        }

        public override String ToString()
        {
            return $"Batch{{batchId={BatchId}, events={String.Join(",", Actions.Select(v => v.ToString()).ToArray())}}}";
        }
    }
}
