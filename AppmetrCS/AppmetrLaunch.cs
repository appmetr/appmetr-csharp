using System;
using System.Collections.Generic;
using System.Diagnostics;
using AppmetrCS.Actions;

namespace AppmetrCS
{
    public class AppmetrLaunch
    {
        public const String Deploy = "d21d509b-12ec-407d-98fe-957effc31f2d";
        public const String Url = /*"http://localhost:8080/api";*/ "http://api.hz.eu.stage.appmetr.pix.team/api";

        public static void Main(String[] args)
        {
            var appMetr = new AppMetr(Url, Deploy, "tester", "/tmp/appmetr");

//            var action = new TrackEvent("Lev");
//            action.Properties = new Dictionary<String, Object>
//            {
//                {"player", "super"},
//                {"level", 500}
//            };
//
//            Console.WriteLine(action.ToString());
//            Console.WriteLine(NewtonsoftSerializer.Instance.Serialize(action));
//            
//            appMetr.Track(action);
            
            CreateActions().ForEach(action => appMetr.Track(action));
            
            appMetr.Flush();
            appMetr.Upload();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            }
        }

        public static List<AppMetrAction> CreateActions()
        {
            var trackEvent = new TrackEvent("Hello")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my event"},
                    {"int", 11},
                    {"double", 1.99}
                }
            };
            var attachProperties = new AttachProperties
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my props"},
                    {"int", 22},
                    {"double", 2.99}
                }
            };
            var trackLevel = new TrackLevel(5)
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my level"},
                    {"int", 33},
                    {"double", 3.99}
                }
            };
            var trackSession = new TrackSession(4);
            trackSession.Properties.Add("string", "my session");
            trackSession.Properties.Add("int", 44);
            trackSession.Properties.Add("double", 4.99);
            var trackPayment = new TrackPayment("order 1", "transaction 1", "processor 1", "USD", "100", "RUB", "600")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my payment"},
                    {"int", 55},
                    {"double", 5.99}
                }
            };
            var trackIdentify = new TrackIdentify("tester")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my identifier"},
                    {"int", 66},
                    {"double", 6.99}
                }
            };
            var trackState = new TrackState()
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my state"},
                    {"int", 77},
                    {"double", 7.99}
                },
                State = new Dictionary<String, Object>
                {
                    {"string", "new state"},
                    {"int", 7},
                    {"double", 77.99}
                }
            };

            var actions = new List<AppMetrAction>();
            actions.Add(trackEvent);
            actions.Add(attachProperties);
            actions.Add(trackLevel);
            actions.Add(trackSession);
            actions.Add(trackPayment);
            actions.Add(trackIdentify);
            actions.Add(trackState);

            return actions;
        }
    }
}
