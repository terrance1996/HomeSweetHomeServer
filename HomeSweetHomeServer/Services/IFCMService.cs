using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about Firebase Cloud Message operations
    public interface IFCMService
    {
        Task SendFCMAsync(FCMModel fcmMessage);
    }
}
