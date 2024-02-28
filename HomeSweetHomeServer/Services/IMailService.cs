using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface for mail operations
    public interface IMailService
    {
        Task SendMailAsync(EMailModel mail);
    }
}
