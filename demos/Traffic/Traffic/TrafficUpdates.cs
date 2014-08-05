using System;
using PubNubMessaging.Core;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace Traffic
{
    public class TrafficUpdates
    {
        public event TrafficUpdateHandler TrafficUpdate; 
        public delegate void TrafficUpdateHandler(string message, TrafficMessage trafficMessage);

        public event AddressUpdateHandler AddressUpdate; 
        public delegate void AddressUpdateHandler(string address, string lat, string lng, bool fromAddress, string polylinePoints, TrafficDialogViewController.GetLocationActions locActions);

        public event PostToChannelHandler PostToChannel; 
        public delegate void PostToChannelHandler(string channel, TrafficMessage trafficMessage);

        public TrafficUpdates ()
        {
            GoogleApiKey = "<YOUR GOOGLE API KEY>";
            pn = new Pubnub ("demo", "demo", "demo", "", false);
        }
        Pubnub pn;
        bool exitFlag = false;
        int iLength = 0;
        int iCount = 0;
        Thread thdAutoUpdate;
        private static int seedCounter = new Random().Next();

        string GoogleApiKey {
            get;
            set;
        }
            
        public string Destination {
            get;
            set;
        }

        public string Origin {
            get;
            set;
        }

        public string DestinationStreet {
            get;
            set;
        }

        public string OriginStreet {
            get;
            set;
        }

        double historyFetchTime = -6d * 60;

        void ParseDetailedHistoryResponse (string history, string address)
        {
            TrafficUpdate (address, TrafficMessage.None);
            object response = DeserializeToObject (history);
            IList<object> fields = response as IList<object>;
            if (fields [0] != null) {
                bool trafficEntryNotFound = true;
                var myObjectArray = (from item in fields
                    select item as object).ToArray ();
                IList<object> enumerable = myObjectArray [0] as IList<object>;
                if ((enumerable != null) && (enumerable.Count > 0)) {
                    Console.WriteLine (string.Format ("=> Traffic:{0}", enumerable [(enumerable.Count - 1)]));
                    if (enumerable [(enumerable.Count - 1)].ToString().Contains("Blocked")) {
                        TrafficUpdate (string.Format ("=> Traffic:{0}", enumerable [(enumerable.Count - 1)]), TrafficMessage.Blocked);
                    } else if (enumerable [(enumerable.Count - 1)].ToString().Contains("Heavy")) {
                        TrafficUpdate (string.Format ("=> Traffic:{0}", enumerable [(enumerable.Count - 1)]), TrafficMessage.Heavy);
                    } else if (enumerable [(enumerable.Count - 1)].ToString().Contains("Normal")) {
                        TrafficUpdate (string.Format ("=> Traffic:{0}", enumerable [(enumerable.Count - 1)]), TrafficMessage.Normal);
                    } else if (enumerable [(enumerable.Count - 1)].ToString().Contains("Low")) {
                        TrafficUpdate (string.Format ("=> Traffic:{0}", enumerable [(enumerable.Count - 1)]), TrafficMessage.Low);
                    }

                    long l2;
                    if (long.TryParse (myObjectArray [2].ToString (), out l2)) {
                        DateTime dt = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime (l2);
                        dt = dt.ToLocalTime ();
                        TimeSpan ts = DateTime.Now.Subtract (dt);
                        string display = string.Format ("{0:####} mins", ts.TotalMinutes);
                        if (ts.TotalMinutes < 2) {
                            display = string.Format ("1 min", ts.TotalMinutes);
                        } else if ((ts.TotalMinutes > 60) && (ts.TotalMinutes < 120)) {
                            display = string.Format ("{0:####} hour", ts.TotalHours);
                        } else if (ts.TotalMinutes > 120) {
                            display = string.Format ("{0:####} hours", ts.TotalHours);
                        }
                        Console.WriteLine (string.Format ("=> last updated :{0} ago", display));
                        TrafficUpdate (string.Format ("=> last updated :{0} ago", display), TrafficMessage.None);
                    }
                    trafficEntryNotFound = false;
                }
                if (trafficEntryNotFound) {
                    //get last history
                    Console.WriteLine (string.Format ("=> Traffic: No Details Found"));
                    TrafficUpdate (string.Format ("=> Traffic: No Details Found"), TrafficMessage.None);
                }
                Console.WriteLine ();
            }
            iCount++;
            if (iCount == iLength) {
                Console.WriteLine (string.Format ("* Destination:{0}", Destination));
                TrafficUpdate (string.Format ("* Destination:{0}", Destination), TrafficMessage.None);
                Console.WriteLine ();
            }
        }

        void FetchLocatioNameAndPubnubHistory (double lat, double lng, bool fetchHistory, long pubnubTime, long pubnubTimeM15)
        {
            string url = string.Format ("https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=true&key={2}", lat, lng, GoogleApiKey);
            string channelname = string.Format ("{0}:{1}", lat, lng);
            string json = MakeCallAndGetJsonResponseSync (url);
            JArray locjson = ParseJObject (json, "results");

            if ((locjson != null) && (locjson.Count() > 0)) {
                Console.Write (string.Format ("* [{0}:{1}], {2}", lat, lng, locjson[0]["formatted_address"]));
                string address = string.Format ("* [{0}:{1}], {2}", lat, lng, locjson [0] ["formatted_address"]);
                if (fetchHistory) {
                    pn.DetailedHistory<string> (channelname, pubnubTime, pubnubTimeM15, 10, true, //pn.DetailedHistory <string> (channelname, 10,
                        history => {
                            ParseDetailedHistoryResponse (history, address);
                        }, DisplayErrorMessage);
                }
            }
            Console.WriteLine ("");
        }

        void FetchLocatioNameAndPubnubHistory (string polylinePoints, double lat, double lng, bool fetchHistory, long pubnubTime, long pubnubTimeM15)
        {
            string url = string.Format ("https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=true&key={2}", lat, lng, GoogleApiKey);
            string json = MakeCallAndGetJsonResponseSync (url);
            JArray locjson = ParseJObject (json, "results");

            if ((locjson != null) && (locjson.Count() > 0)) {
                Console.Write (string.Format ("* [{0}:{1}], {2}, {3}", lat, lng, locjson[0]["formatted_address"], polylinePoints));
                string address = string.Format ("* [{0}:{1}], {2}", lat, lng, locjson [0] ["formatted_address"]);
                string channelname = CreateAddress (json);
                if (fetchHistory) {
                    pn.DetailedHistory<string> (channelname, pubnubTime, pubnubTimeM15, 10, true, //pn.DetailedHistory <string> (channelname, 10,
                        history => {
                            ParseDetailedHistoryResponse (history, address);
                        }, DisplayErrorMessage);
                }
            }
            Console.WriteLine ("");
        }

        void ParseRoutes (JArray result, long pubnubTime, long pubnubTimeM15)
        {
            Console.WriteLine (string.Format ("* Origin:{0}", OriginStreet));
            TrafficUpdate(string.Format ("* Origin:{0}", OriginStreet), TrafficMessage.None);
            try {
                var steps = result[0].SelectToken("legs")[0].SelectToken("steps");
                if((steps!= null) && (steps.Count() > 0)){
                    iLength = steps.Count();
                    iCount = 0;

                    foreach (JToken step in steps) {
                        FetchLocatioNameAndPubnubHistory (Encrypt(step["polyline"]["points"].ToString()), (double)step["end_location"]["lat"], (double)step["end_location"]["lng"], true, pubnubTime, pubnubTimeM15);
                    }
                }

            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }
        }

        public void ParseJsonResponse (string jsonString, string time)
        {
            JArray result = ParseJObject (jsonString, "routes");

            long pubnubTime;
            long pubnubTimeM15 = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (DateTime.UtcNow.AddMinutes (historyFetchTime));
            if (long.TryParse (time, out pubnubTime)) {
                DateTime pubnubDateTime = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime (pubnubTime);
                DateTime pubnubDateTimeM15 = pubnubDateTime.AddMinutes (historyFetchTime);
                pubnubTimeM15 = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (pubnubDateTimeM15);
                Console.WriteLine ("Parsing route");
                TrafficUpdate ("Parsing route", TrafficMessage.None);
                ParseRoutes (result, pubnubTime, pubnubTimeM15);
            }
        }

        public string MakeCallAndGetJsonResponseSync (string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);

            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
            Stream receiveStream = response.GetResponseStream ();

            StreamReader readStream = new StreamReader (receiveStream, Encoding.UTF8);

            string resp = readStream.ReadToEnd ();
            response.Close ();
            readStream.Close ();
            return resp;
        }

        public void RunSearch(string location, bool isLatLng, bool isOrigin){
            if (String.IsNullOrWhiteSpace (Origin)) {
                Origin = "Downtown, san francisco";
                Console.WriteLine (string.Format ("Using default Origin"));
            }
            if (String.IsNullOrWhiteSpace (Destination)) {
                Destination = "725 Folsom • San Francisco, CA 94107";
                Console.WriteLine (string.Format ("Using default Destination"));
            }

            if (isLatLng) {
                UpdateToNearestStreet (location, isOrigin, TrafficMessage.None);
            } else {
                OriginStreet = Origin;
                DestinationStreet = Destination;

                GetDirections ();
            }
        }

        public void GetDirections(){
            if ((String.IsNullOrWhiteSpace (DestinationStreet)) || (String.IsNullOrWhiteSpace (OriginStreet))) {
                Console.WriteLine ("Missing origin or destination");
            } else {
                Console.WriteLine ("Loading...");
                TrafficUpdate ("Loading...", TrafficMessage.None);
                StringBuilder sbUrl = new StringBuilder ();
                sbUrl.Append ("https://maps.googleapis.com/maps/api/directions/json?");
                sbUrl.Append ("origin=");
                sbUrl.Append (OriginStreet);
                sbUrl.Append ("&");
                sbUrl.Append ("destination=");
                sbUrl.Append (DestinationStreet);
                sbUrl.Append ("&sensor=false&");
                sbUrl.Append ("key=");
                sbUrl.Append (GoogleApiKey);

                MakeCallAndGetJsonResponse (sbUrl.ToString (), true, ParseJsonResponse);
                Console.WriteLine ();
            }
        }

        public bool ParseValidateLocationResponse (string jsonString, bool fromAddress, TrafficDialogViewController.GetLocationActions locActions){

            try {
                JArray locjson = ParseJObject (jsonString, "results");

                if ((locjson != null) && (locjson.Count() > 0)) {
                    Console.Write (string.Format ("Location from Coordinates * {0}{1}{2}", locjson[0]["formatted_address"], locjson[0]["geometry"]["location"]["lat"], locjson[0]["geometry"]["location"]["lng"]));
                    AddressUpdate(locjson[0]["formatted_address"].ToString(), locjson[0]["geometry"]["location"]["lat"].ToString(), locjson[0]["geometry"]["location"]["lng"].ToString(), fromAddress, "",  locActions);
                }
            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }       
            return true;
        }

        public void ValidateLocation (string location, TrafficDialogViewController.GetLocationActions locActions){

            Console.WriteLine ("Validating locations...");
            string url = string.Format ("https://maps.googleapis.com/maps/api/geocode/json?latlng={0}&sensor=true&key={1}", location, GoogleApiKey);
            MakeCallAndGetJsonResponse (url, false, 
                (string jsonString, string time) => {

                    if(ParseValidateLocationResponse(jsonString, false, locActions) ){
                    }
                }
            );

            Console.WriteLine ("");
        }

        string CreateAddress(string jsonString){
            try {
                JArray locjson = ParseJObject (jsonString, "results");

                if ((locjson != null) && (locjson.Count() > 0)) {
                    Console.Write (string.Format ("Location from Coordinates * {0}{1}{2}", locjson[0]["formatted_address"], locjson[0]["geometry"]["location"]["lat"], locjson[0]["geometry"]["location"]["lng"]));
                    StringBuilder sbAddress = new StringBuilder();
                    if((locjson[0] != null) && (locjson[0]["address_components"]!=null)){


                        foreach (var obj in locjson[0]["address_components"]){
                            Console.WriteLine("long_name:"+obj["long_name"]);
                            Console.WriteLine("short_name:"+obj["short_name"]);
                            Console.WriteLine("types:"+obj["types"]);
                            if(obj["types"][0].ToString().Equals("route")){
                                sbAddress.Append(obj["short_name"]);
                                sbAddress.Append("_");
                            }
                            if(obj["types"][0].ToString().Equals("locality")){
                                sbAddress.Append(obj["short_name"]);
                                sbAddress.Append("_");
                            }
                            if(obj["types"][0].ToString().Equals("administrative_area_level_1")){
                                sbAddress.Append(obj["short_name"]);
                                sbAddress.Append("_");
                            }
                            if(obj["types"][0].ToString().Equals("country")){
                                sbAddress.Append(obj["short_name"]);
                                sbAddress.Append("_");
                            }
                            if(obj["types"][0].ToString().Equals("postal_code")){
                                sbAddress.Append(obj["short_name"]);
                                sbAddress.Append("_");
                            }

                        }
                    }

                    return Encrypt(sbAddress.ToString());
                }
            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }       
            return "";
        }

        public bool ParseNearestStreetResponse (string jsonString, bool isOrigin){

            try {
                JArray locjson = ParseJObject (jsonString, "results");

                if ((locjson != null) && (locjson.Count() > 0)) {
                    Console.Write (string.Format ("Location from Coordinates * {0}", locjson[0]["formatted_address"]));
                    if (isOrigin) {
                        OriginStreet = locjson[0]["formatted_address"].ToString();
                        DestinationStreet = Destination;
                    } else {
                        DestinationStreet = locjson[0]["formatted_address"].ToString();
                        OriginStreet = Origin;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
                if (isOrigin) {
                    OriginStreet = Origin;
                } else {
                    DestinationStreet = Destination;
                }
            }       
            return true;
        }

        public void ParseNextStreetJsonResponse (string jsonString, string time)
        {
            JArray result = ParseJObject (jsonString, "routes");

            try {
                if ((result != null) && (result.Count() > 0)) {
                    var steps = result[0].SelectToken("legs")[0].SelectToken("start_location");
                    var steppolyline = result[0].SelectToken("legs")[0]["steps"][0].SelectToken("polyline");
                    var address = result[0].SelectToken("legs")[0].SelectToken("start_address");
                    if(steps!= null && steppolyline != null){
                        Console.Write (string.Format ("Location from Coordinates * {0}{1}{2}", address, steps["lat"], steps["lng"]));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }       
          
        }

        public void UpdateToNearestStreetFromAddress (string location, string nextStreet){

            Console.WriteLine ("Validating locations...");
            if ((String.IsNullOrWhiteSpace (location)) || (String.IsNullOrWhiteSpace (nextStreet))) {
                Console.WriteLine ("Missing location or next street");
            } else {
                Console.WriteLine ("Loading...");
                StringBuilder sbUrl = new StringBuilder ();
                sbUrl.Append ("https://maps.googleapis.com/maps/api/directions/json?");
                sbUrl.Append ("origin=");
                sbUrl.Append (location);
                sbUrl.Append ("&");
                sbUrl.Append ("destination=");
                sbUrl.Append (nextStreet);
                sbUrl.Append ("&sensor=false&");
                sbUrl.Append ("key=");
                sbUrl.Append (GoogleApiKey);

                MakeCallAndGetJsonResponse (sbUrl.ToString (), true, ParseNextStreetJsonResponse);
                Console.WriteLine ();
            }

        }

        public void GetStreetAddress(string location, string nextStreet, TrafficMessage trafficMessage){
            Console.WriteLine ("Validating locations...");
            if ((String.IsNullOrWhiteSpace (location)) || (String.IsNullOrWhiteSpace (nextStreet))) {
                Console.WriteLine ("Missing location or next street");
            } else {
                Console.WriteLine ("Loading...");
                StringBuilder sbUrl = new StringBuilder ();

                UpdateToNearestStreet (location, trafficMessage);
                UpdateToNearestStreet (nextStreet, trafficMessage);
            }

        }

        public void GetStreetAddress(string location, TrafficMessage trafficMessage){
            Console.WriteLine ("Validating location");
            if (String.IsNullOrWhiteSpace (location)) {
                Console.WriteLine ("Missing location");
                AddressUpdate ("", "", "", false, "", TrafficDialogViewController.GetLocationActions.None);
            } else {
                Console.WriteLine ("Loading...");
                StringBuilder sbUrl = new StringBuilder ();

                UpdateToNearestStreet (location, trafficMessage);
            }

        }

        public void UpdateToNearestStreet (string location, TrafficMessage trafficMessage){
            string url = string.Format ("https://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=true&key={1}", location, GoogleApiKey);
            MakeCallAndGetJsonResponse (url, false, 
                (string jsonString, string time) => {
                    string channel = CreateAddress(jsonString);
                    if(!string.IsNullOrWhiteSpace(channel)){
                        PostToChannel(channel, trafficMessage);
                    }
                }
            );

            Console.WriteLine ("");
        }

        public void UpdateToNearestStreet (string location, bool isOrigin, TrafficMessage trafficMessage){

            Console.WriteLine ("Validating locations...");
            string url = string.Format ("https://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=true&key={1}", location, GoogleApiKey);
            MakeCallAndGetJsonResponse (url, false, 
                (string jsonString, string time) => {
                    string channel = CreateAddress(jsonString);
                    if(!string.IsNullOrWhiteSpace(channel)){
                        PostToChannel(channel, trafficMessage);
                    }
                }
            );

            Console.WriteLine ("");
        }

        public void MakeCallAndGetJsonResponse (string url, bool getTime, Action<string, string> callback)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);

            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            try {
                // Make request with the following inline Asynchronous callback
                request.BeginGetResponse (new AsyncCallback ((asynchronousResult) => {
                    HttpWebRequest asyncWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                    HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse (asynchronousResult);
                    using (StreamReader streamReader = new StreamReader (asyncWebResponse.GetResponseStream ())) {
                        // Deserialize the result
                        string jsonString = streamReader.ReadToEnd ();
                        if(getTime){
                            pn.Time<string> (time => {
                                object response = DeserializeToObject (time);

                                IList<object> fields = response as IList<object>;

                                callback (jsonString, fields[0].ToString());
                            }, DisplayErrorMessage);
                        } else {
                            callback (jsonString, "");
                        }
                    }
                }), request

                );
            } catch (System.Exception ex) {
                Console.WriteLine ("ResponseJsonException:" + ex.ToString ());
            }
        }

        string GetRandom (string[] arr, int total)
        {
            int seed = Interlocked.Increment(ref seedCounter);
            Random rnd = new Random (seed);

            int pos = rnd.Next (0, total);
            return arr [pos]; 
        }

        static void DisplayErrorMessage (PubnubClientError result)
        {
            Console.WriteLine ();
            Console.WriteLine (result.Description);
            Console.WriteLine ();
        }

        void PublishAutoMessages ()
        {
            StringBuilder sbUrl = new StringBuilder ();
            sbUrl.Append ("https://maps.googleapis.com/maps/api/directions/json?");
            sbUrl.Append ("origin=");
            sbUrl.Append (OriginStreet);
            sbUrl.Append ("&");
            sbUrl.Append ("destination=");
            sbUrl.Append (DestinationStreet);
            sbUrl.Append ("&sensor=false&");
            sbUrl.Append ("key=");
            sbUrl.Append (GoogleApiKey);

            MakeCallAndGetJsonResponse (sbUrl.ToString (), false, ParseJsonResponseForRandomPublish);
        }

        public void ParseJsonResponseForNearestStreet (string jsonString, string time){
            JArray result = ParseJObject (jsonString, "routes");

            try {
                var steps = result[0].SelectToken("legs")[0].SelectToken("steps");
                if((steps!= null) && (steps.Count() > 0)){
                    iLength = steps.Count();
                    iCount = 0;

                    foreach (JToken step in steps) {
                        Console.WriteLine(string.Format ("{0}:{1}", (double)step["end_location"]["lat"], (double)step["end_location"]["lng"]));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }       
        }

        public enum TrafficMessage{
            Blocked,
            Heavy,
            Normal,
            Low,
            None
        }

        public void PublishMessage (string channel, TrafficMessage message)
        {
            pn.Publish (channel, message.ToString(), resultPublish => {
                Console.WriteLine (string.Format ("Message {0} to channel {1} published.", message, channel));
            }, DisplayErrorMessage);
        }
    
        public string Encrypt (string str){
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] preHash = System.Text.Encoding.UTF32.GetBytes(str);
            byte[] hash = sha.ComputeHash(preHash);
            return System.Convert.ToBase64String(hash);
        }

        public void ParseJsonResponseForRandomPublish (string jsonString, string time)
        {
            ArrayList channels = new ArrayList ();
            string[] messages = { "blocked", "heavy", "normal", "low" };
            Console.WriteLine ("Publishing random message to random channel within Origin and Destination path)");  

            JArray result = ParseJObject (jsonString, "routes");

            try {
                var steps = result[0].SelectToken("legs")[0].SelectToken("steps");
                if((steps!= null) && (steps.Count() > 0)){
                    iLength = steps.Count();
                    iCount = 0;

                    foreach (JToken step in steps) {
                        channels.Add(Encrypt(step["polyline"]["points"].ToString()));
                    }
                }

                string channel = GetRandom ((string[])channels.ToArray(typeof(string)), channels.Count);
                string message = GetRandom (messages, messages.Count());
                pn.Publish (channel, message, resultPublish => {
                    Console.WriteLine (string.Format ("Message {0} to channel {1} published.", message, channel));
                }, DisplayErrorMessage);

            } catch (Exception ex) {
                Console.WriteLine ("Error" + ex.ToString ());
            }       
        }

        public JArray ParseJObject (string jsonString, string token){
            JObject o = JObject.Parse(jsonString);
            JArray arr = (JArray) o.SelectToken(token);
            return arr;
        }

        public JToken ParseJArray (string jsonString){
            var array = JArray.Parse(jsonString);
            return array;
        }

        public object DeserializeToObject (string jsonString)
        {
            object output = JsonConvert.DeserializeObject<object> (jsonString);
            if (output.GetType ().ToString () == "Newtonsoft.Json.Linq.JArray") {
                JArray jarrayResult = output as JArray;
                List<object> objectContainer = jarrayResult.ToObject<List<object>> ();
                if (objectContainer != null && objectContainer.Count > 0) {
                    for (int index = 0; index < objectContainer.Count; index++) {
                        if (objectContainer [index].GetType ().ToString () == "Newtonsoft.Json.Linq.JArray") {
                            JArray internalItem = objectContainer [index] as JArray;
                            objectContainer [index] = internalItem.Select (item => (object)item).ToArray ();
                        }
                    }
                    output = objectContainer;
                } 
            } 
            return output;
        }
    }
}

