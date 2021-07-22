using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSocket.SocketManager
{
    public class SocketMiddelWare
    {
        private readonly RequestDelegate _next;
        private SocketHandler Handler { get; set; }

        public SocketMiddelWare(RequestDelegate next, SocketHandler handler)
        {
            _next = next;
            Handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await Handler.OnConnected(socket);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                     await Handler.Receive(socket, result, buffer);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Handler.OnDisconeccted(socket);
                }
            });

            await _next(context);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> messagHandler)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                messagHandler.Invoke(result, buffer);
            }
        }
    }
}