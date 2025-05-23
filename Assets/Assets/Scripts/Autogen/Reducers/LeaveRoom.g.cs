// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase
    {
        public delegate void LeaveRoomHandler(ReducerEventContext ctx);
        public event LeaveRoomHandler? OnLeaveRoom;

        public void LeaveRoom()
        {
            conn.InternalCallReducer(new Reducer.LeaveRoom(), this.SetCallReducerFlags.LeaveRoomFlags);
        }

        public bool InvokeLeaveRoom(ReducerEventContext ctx, Reducer.LeaveRoom args)
        {
            if (OnLeaveRoom == null) return false;
            OnLeaveRoom(
                ctx
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class LeaveRoom : Reducer, IReducerArgs
        {
            string IReducerArgs.ReducerName => "LeaveRoom";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags LeaveRoomFlags;
        public void LeaveRoom(CallReducerFlags flags) => LeaveRoomFlags = flags;
    }
}
