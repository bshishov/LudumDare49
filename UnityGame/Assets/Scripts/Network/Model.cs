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
        public const string ServerError = "error";
        public const string ServerGoldUpdated = "gold_updated";
        
        public const string ServerRollSuccess = "roll_success";
        public const string ServerRollDecided = "roll_decided";
    }

    namespace Game
    {
        [Serializable]
        public struct ItemData
        {
            public string id;
            public int power;
            public string slot;
            public string rarity;
        }
        
        [Serializable]
        public struct RolledItem
        {
            public ItemData item;
            public string quality;
            public float obtained_at;
            public string merchant;
            public int total_power;
        }
        
        [Serializable]
        public struct Player
        {
            public string username;
            public int gold;
            public float last_gold_update_time;
            public RolledItem[] items;
            public RolledItem? current_undecided_roll_item;
        }

        [Serializable]
        public struct DivisionPlayer
        {
            public string username;
            public int rank;
            public int power;
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
        public struct ServerError : IMessage
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
            public bool accepted;
        }
    }
}