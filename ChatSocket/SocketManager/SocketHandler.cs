using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSocket.SocketManager
{
    public abstract class SocketHandler
    {
        public ConnectionManager ConnectionManager { get; set; }

        protected SocketHandler(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            await Task.Run(() => ConnectionManager.AddSocket(socket));
        }

        public virtual async Task OnDisconeccted(WebSocket socket)
        {
            await ConnectionManager.RemoveSocketAsync(ConnectionManager.GetId(socket));
        }

        public virtual async Task SendMessage(WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.Open)
                return;

            await socket.SendAsync(
                    buffer: new ArraySegment<byte>(
                        array: Encoding.UTF8.GetBytes(message),
                        offset: 0,
                        count: message.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
        }

        public virtual async Task SendMessage(string id, string message)
        {
            await SendMessage(ConnectionManager.GetSocketById(id), message);
        }

        public virtual async Task SendMessageToAll(string message)
        {
            foreach (var con in ConnectionManager.GetAllConnections())
            {
                await SendMessage(con.Value, message);
            }
        }

        public abstract Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}