using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nexplorer.Domain.Dtos;
using StackExchange.Redis;

namespace Nexplorer.Web.Hubs
{
    public class TransactionHub : Hub
    {
        public TransactionHub()
        {
        }
    }
}
