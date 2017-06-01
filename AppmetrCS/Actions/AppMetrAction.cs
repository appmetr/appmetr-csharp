﻿using System.Linq;

namespace AppmetrCS.Actions
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public abstract class AppMetrAction
    {
        [DataMember(Name = "action")]
        private String _action;

        [DataMember(Name = "timestamp")]
        private Int64 _timestamp = Utils.GetNowUnixTimestamp();

        [DataMember(Name = "properties")]
        private IDictionary<String, Object> _properties = new Dictionary<String, Object>();

        [DataMember(Name = "userId")]
        private String _userId;

        protected AppMetrAction()
        {
        }

        protected AppMetrAction(String action)
        {
            _action = action;
        }

        public String Action
        {
            get => _action;
            set => _action = value;
        }

        public Int64 Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }

        public IDictionary<String, Object> Properties
        {
            get => _properties;
            set => _properties = value;
        }

        public String UserId
        {
            get => _userId;
            set => _userId = value;
        }

        //http://codeblog.jonskeet.uk/2011/04/05/of-memory-and-strings/
        public virtual Int32 CalcApproximateSize()
        {
            var size = 40 + (40 * _properties.Count); //40 - Map size and 40 - each entry overhead

            size += GetStringLength(_action);
            size += GetStringLength(Convert.ToString(_timestamp));
            size += GetStringLength(_userId);

            foreach (var pair in _properties) {
                size += GetStringLength(pair.Key);
                size += GetStringLength(pair.Value != null ? Convert.ToString(pair.Value) : null);   //toString because sending this object via json
            }

            return 8 + size + 8; //8 - object header
        }

        protected Int32 GetStringLength(String str)
        {
            return str == null ? 0 : str.Length * 2 + 26;    //24 - String object size, 16 - char[]
        }
        
        public override String ToString()
        {
            return $"{GetType().Name}{{action={Action}, timestamp={Timestamp}, userId={UserId}, " +
                   $"properties{"{" + String.Join(",", Properties.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}"}}}";
        }
    }
}
