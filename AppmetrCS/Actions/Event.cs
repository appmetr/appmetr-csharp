namespace AppmetrCS.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion
    
    [DataContract]
    public class Event : AppMetrAction
    {
        private const String Action = "trackEvent";

        [DataMember(Name = "event")]
        private String _event;

        protected Event()
        {
        }

        public Event(String eventName) : base(Action)
        {
            _event = eventName;
        }

        public String GetEvent()
        {
            return _event;
        }

        public override Int32 CalcApproximateSize()
        {
            return base.CalcApproximateSize() + GetStringLength(_event);
        }
          
        public override String ToString()
        {
            return $"{base.ToString()},event={GetEvent()}";
        }
    }
}
