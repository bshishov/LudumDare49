using System;
using System.Collections.Generic;
using System.Linq;
using Network.Messages;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace Network
{
    [RequireComponent(typeof(WebsocketConnection))]
    public class Connection : Singleton<Connection>
    {
        private static readonly Dictionary<string, Type> TypeToClass= new Dictionary<string, Type>
        {
            // Client
            {MessageTypes.ClientHello, typeof(ClientHello)},
            {MessageTypes.ClientRoll, typeof(ClientRoll)},
            {MessageTypes.ClientAcceptRoll, typeof(ClientAcceptRoll)},
            {MessageTypes.ClientDeclineRoll, typeof(ClientDeclineRoll)},
            {MessageTypes.ClientDivisionInfoRequest, typeof(ClientDivisionInfoRequest)},
            
            // Server
            {MessageTypes.ServerHello, typeof(ServerHello)},
            {MessageTypes.ServerError, typeof(ServerError)},
            {MessageTypes.ServerGoldUpdated, typeof(ServerGoldUpdated)},
            {MessageTypes.ServerRollSuccess, typeof(ServerRollSuccess)},
            {MessageTypes.ServerRollDecided, typeof(ServerRollDecided)},
            {MessageTypes.ServerDivisionInfo, typeof(ServerDivisionInfo)},
        };

        private static readonly Dictionary<Type, string> ClassToType =
            TypeToClass.ToDictionary(x => x.Value, x => x.Key);

        private IClientConnection _connection;

        // Events
        public readonly TypedEvent<IMessage> MessageReceived = new TypedEvent<IMessage>();
        public bool IsConnected => _connection.IsConnected;
        public event Action Connected;
        public event Action Disconnected;

        private void Start()
        {
            _connection = GetComponent<IClientConnection>();
            _connection.Disconnected += () => { Disconnected?.Invoke(); };
            _connection.MessageReceived += OnMessageReceived;
            _connection.Connected += () => { Connected?.Invoke(); };

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
        }

        public void Connect(string host)
        {
            if (_connection.IsConnected)
                _connection.Close();
            _connection.Connect(host);
        }

        private void OnMessageReceived(string messageRaw)
        {
            Debug.Log(messageRaw);
            var msg = JsonConvert.DeserializeObject<MessageBase>(messageRaw);

            if (TypeToClass.TryGetValue(msg.type, out var messageClass))
            {
                var payload = JsonConvert.DeserializeObject(messageRaw, messageClass);
                MessageReceived.Invoke(messageClass, payload as IMessage);
            }
            else
            {
                Debug.LogWarningFormat("No such message type: {0}", msg.type);
            }
        }

        public void Send<T>(T message)
            where T : IClientMessage
        {
            if (_connection != null && _connection.IsConnected)
            {
                var serialized = JsonConvert.SerializeObject(message);
                Debug.Log(serialized);
                _connection.Send(serialized);
            }
        }

        public void Close()
        {
            _connection?.Close();
        }

        private void OnDestroy()
        {
            Close();
        }
    }
}