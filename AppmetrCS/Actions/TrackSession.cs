using System;
using System.Runtime.Serialization;

namespace AppmetrCS.Actions
{
    [DataContract]
    public class TrackSession : AppMetrAction
    {
        private const String ACTION = "trackSession";

        public TrackSession() : base(ACTION)
        {
        }
    }
}
