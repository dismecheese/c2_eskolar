using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace c2_eskolar.Hubs
{
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            // Clients are identified by their authenticated user id (IUserIdProvider)
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
