using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeSweetHomeServer.Models
{
    //Email content
    public class EMailModel
    {
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string ToAddress { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
