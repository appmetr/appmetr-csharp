using System;
using System.Runtime.Serialization;

namespace AppmetrCS.Actions
{
    [DataContract]
    public class TrackSession : AppMetrAction
    {
        private const String ACTION = "trackSession";
        
        public const String Duration = "$duration";

        public TrackSession() : base(ACTION)
        {
        }

        public TrackSession(Int64 duration) : base(ACTION)
        {
            Properties.Add(Duration, duration);
        }
    }
}
