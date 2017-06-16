namespace AppmetrCS.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion
    
    [DataContract]
    public class TrackEvent : AppMetrAction
    {
        private const String ACTION = "trackEvent";

        [DataMember(Name = "event")]
        public String Event { get; set; }

        protected TrackEvent() : base(ACTION)
        {
        }

        public TrackEvent(String eventName) : base(ACTION)
        {
            Event = eventName;
        }

        public override Int32 CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(Event);
        }
          
        public override String ToString()
        {
            return $"{base.ToString()},event={Event}";
        }
    }
}
