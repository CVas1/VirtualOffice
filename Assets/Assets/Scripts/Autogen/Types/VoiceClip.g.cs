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
    public sealed partial class VoiceClip
    {
        [DataMember(Name = "sender")]
        public SpacetimeDB.Identity Sender;
        [DataMember(Name = "room_id")]
        public uint RoomId;
        [DataMember(Name = "timestamp")]
        public ulong Timestamp;
        [DataMember(Name = "audio_data")]
        public System.Collections.Generic.List<byte> AudioData;

        public VoiceClip(
            SpacetimeDB.Identity Sender,
            uint RoomId,
            ulong Timestamp,
            System.Collections.Generic.List<byte> AudioData
        )
        {
            this.Sender = Sender;
            this.RoomId = RoomId;
            this.Timestamp = Timestamp;
            this.AudioData = AudioData;
        }

        public VoiceClip()
        {
            this.AudioData = new();
        }
    }
}
