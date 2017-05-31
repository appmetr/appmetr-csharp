using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
