using System;

namespace Network
{
    // ReSharper disable InconsistentNaming
    
    public interface IMessage {}
    public interface IClientMessage : IMessage {}

    public static class MessageTypes
    {
        // Client to server
        public const string ClientHello = "hello";
        public const string ClientRoll = "roll";
        public const string ClientAcceptRoll = "accept_roll";
        public const string ClientDeclineRoll = "decline_roll";
        
        // Server to client
        public const string ServerHello = "server_hello";
        public const string ServerGoldUpdated = "gold_updated";
        public const string ServerRollFailed = "roll_failed";
        public const string ServerRollSuccess = "roll_success";
        public const string ServerRollDecided = "roll_decided";
    }

    namespace Game
    {
        [Serializable]
        public struct RolledItem
        {
        }
        
        [Serializable]
        public struct Player
        {
        }
    }

    namespace Messages
    {
        [Serializable]
        public struct MessageBase
        {
            public string type;
        }
        
        [Serializable]
        public class ClientHello : IClientMessage
        {
            public string type = MessageTypes.ClientHello;
            public string username;
            public string token;
        }
        
        [Serializable]
        public class ClientRoll : IClientMessage
        {
            public string type = MessageTypes.ClientRoll;
            public string merchant;
        }
        
        [Serializable]
        public class ClientAcceptRoll : IClientMessage
        {
            public string type = MessageTypes.ClientAcceptRoll;
        }
        
        [Serializable]
        public class ClientDeclineRoll : IClientMessage
        {
            public string type = MessageTypes.ClientDeclineRoll;
        }
        
        [Serializable]
        public struct ServerHello : IMessage
        {
            public Game.Player player;
        }
        
        [Serializable]
        public struct ServerGoldUpdated : IMessage
        {
            public int old_gold;
            public int new_gold;
            public float next_update_time;
        }
        
        [Serializable]
        public struct ServerRollFailed : IMessage
        {
            public string error;
        }
        
        [Serializable]
        public struct ServerRollSuccess : IMessage
        {
            public Game.RolledItem rolled_item;
            public Game.Player player;
        }
        
        [Serializable]
        public struct ServerRollDecided : IMessage
        {
            public Game.Player player;
        }
    }
}