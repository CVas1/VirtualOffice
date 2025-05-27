using System.Linq;
using Assets.Scripts.VoiceRecorder;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBVoiceManager
    {
        private DbConnection conn;
        private SubscriptionHandle currentVoiceSub;

        public void Init(DbConnection connection)
        {
            conn = connection;
            conn.Db.VoiceClip.OnInsert += OnVoiceClipInsert;
            conn.Db.VoiceClip.OnUpdate += OnVoiceClipUpdate;
        }

        public void SubscribeToVoice(uint roomId, ulong timestamp)
        {
            // Unsubscribe from old subscription if active
            if (currentVoiceSub != null && currentVoiceSub.IsActive)
                currentVoiceSub.Unsubscribe();

            // Only get new voice clips after join timestamp and exclude our own clips
            string sql = $"SELECT * FROM voice_clip WHERE room_id = {roomId} AND timestamp > {timestamp} AND sender_user_id != {STDBBackendManager.LocalUserId}";
            currentVoiceSub = conn.SubscriptionBuilder().Subscribe(new[] { sql });
        }

        private void OnVoiceClipInsert(EventContext ctx, VoiceClip clip)
        {
            // Skip our own voice clips
            if (clip.SenderUserId == STDBBackendManager.LocalUserId) return;

            if (STDBBackendManager.Instance.playerManager.Players.TryGetValue(clip.SenderUserId, out PlayerController player))
            {
                VoiceChatPlayer.Instance.EnqueueAudio(clip.AudioData.ToArray(), player.PlayerId);
            }
        }

        private void OnVoiceClipUpdate(EventContext ctx, VoiceClip clipOld, VoiceClip clipNew)
        {
            OnVoiceClipInsert(ctx, clipNew);
        }

        public void SendVoiceClip(byte[] audioData)
        {
            if (!STDBBackendManager.IsAuthenticated())
            {
                Debug.LogError("Must be logged in to send voice clips");
                return;
            }
            conn.Reducers.SendVoice(audioData.ToList());
        }
    }
}