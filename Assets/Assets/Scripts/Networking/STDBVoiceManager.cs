using System.Linq;
using Assets.Scripts.VoiceRecorder;
using SpacetimeDB.Types;

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

            // Only get new voice clips after join timestamp
            string sql = $"SELECT * FROM voice_clip WHERE room_id = {roomId} AND timestamp > {timestamp}";
            currentVoiceSub = conn.SubscriptionBuilder().Subscribe(new[] { sql });
        }

        private void OnVoiceClipInsert(EventContext ctx, VoiceClip clip)
        {
            if (clip.Sender.Equals(STDBBackendManager.LocalIdentity)) return;

            if (STDBBackendManager.Instance.playerManager.Players.TryGetValue(clip.Sender, out PlayerController player))
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
            conn.Reducers.SendVoice(audioData.ToList());
        }
    }
}