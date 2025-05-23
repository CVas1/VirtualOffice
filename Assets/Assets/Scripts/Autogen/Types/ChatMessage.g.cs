// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    [SpacetimeDB.Type]
    [DataContract]
    public sealed partial class ChatMessage
    {
        [DataMember(Name = "message_id")]
        public uint MessageId;
        [DataMember(Name = "sender")]
        public SpacetimeDB.Identity Sender;
        [DataMember(Name = "room_id")]
        public uint RoomId;
        [DataMember(Name = "content")]
        public string Content;
        [DataMember(Name = "shout")]
        public bool Shout;
        [DataMember(Name = "timestamp")]
        public ulong Timestamp;

        public ChatMessage(
            uint MessageId,
            SpacetimeDB.Identity Sender,
            uint RoomId,
            string Content,
            bool Shout,
            ulong Timestamp
        )
        {
            this.MessageId = MessageId;
            this.Sender = Sender;
            this.RoomId = RoomId;
            this.Content = Content;
            this.Shout = Shout;
            this.Timestamp = Timestamp;
        }

        public ChatMessage()
        {
            this.Content = "";
        }
    }
}
