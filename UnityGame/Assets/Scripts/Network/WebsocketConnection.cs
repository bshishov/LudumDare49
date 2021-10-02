using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public interface IClientConnection
    {
        event Action Connected; 
        event Action Disconnected; 
        event Action<string> MessageReceived;
        bool IsConnected { get; }
        void Connect(string uri);
        void Send(string message);
        void Close();
    }
    
    public class WebsocketConnection : MonoBehaviour, IClientConnection
    {
        private const int BufferSize = 8192;
        private const int MaxMessagesPerFrame = 10;
        private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
        private bool _disconnectEventRequestedFlag;
        private ClientWebSocket _webSocket = new ClientWebSocket();
        private readonly StringBuilder _receiveSb = new StringBuilder();
        private readonly ArraySegment<byte> _buffer = WebSocket.CreateClientBuffer(BufferSize, BufferSize);

        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> MessageReceived;
        public bool IsConnected => _webSocket.State == WebSocketState.Open;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            var messagesProcessed = 0;
            while (_messages.Count > 0)
            {
                if (_messages.TryDequeue(out var message))
                    MessageReceived?.Invoke(message);

                messagesProcessed++;
                if (messagesProcessed > MaxMessagesPerFrame)
                    break;
            }

            if (_disconnectEventRequestedFlag)
            {
                Disconnected?.Invoke();
                _disconnectEventRequestedFlag = false;
            }
        }

        private void OnDestroy()
        {
            Close();
        }

        public async void Connect(string uri)
        {
            Debug.Log($"[WS] Connecting to {uri}");
            try
            {
                _webSocket = new ClientWebSocket();
                _webSocket.Options.SetBuffer(BufferSize, BufferSize);
                
                // ConfigureAwait(true) because we directly call the connected event so we want
                // to capture execution context
                await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None).ConfigureAwait(true);
                Debug.Log("[WS] Connected");
                Connected?.Invoke();

                // ConfigureAwait(false) because we don't care about the context the method will be executed in 
                await ReceiveAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _disconnectEventRequestedFlag = true;
            }
        }

        public void Send(string message)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;
            
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            _webSocket
                .SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None)
                .ContinueWith(task => {}); // Fire and forget
        }

        private async Task ReceiveAsync()
        {
            while (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseSent)
            {
                _receiveSb.Clear();
                WebSocketReceiveResult result;
                
                do
                {
                    result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);
                    _receiveSb.Append(Encoding.UTF8.GetString(_buffer.Array, 0, result.Count));
                } while (!result.EndOfMessage);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    _messages.Enqueue(_receiveSb.ToString());
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // WS received close
                    _disconnectEventRequestedFlag = true;
                }
            }
        }

        public void Close()
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                // Request closure
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
}