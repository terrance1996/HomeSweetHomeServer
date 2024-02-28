using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Models
{
    //Firebase Cloud Messaging content
    [Serializable]
    [DataContract]
    public class FCMModel
    {
        [DataMember]
        public string to { get; set; }

        [DataMember]
        public string priority { get; set; }

        [DataMember]
        public Dictionary<string, object> notification { get; set; }

        [DataMember]
        public Dictionary<string, object> data { get; set; }
        
        public FCMModel(string deviceId, 
                        Dictionary<string, object> notification = null,
                        string type = "BasicNotification",
                        string priority = "high")
        {
            to = deviceId;
            this.priority = priority;
            this.notification = notification;
            data = new Dictionary<string, object>();

            data.Add("FcmType", type);
        }
    }
}
