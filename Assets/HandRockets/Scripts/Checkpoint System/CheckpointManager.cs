using System.Globalization;
using UnityEngine;

namespace HandRockets.Scripts.Checkpoint_System
{
    public class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint[] checkpoints;

        private int _completedCheckpoints = 0;


        private void Awake()
        {
            Debug.Log($"Checkpoint Manager: Level Started at: {Time.time.ToString(CultureInfo.InvariantCulture)}");
            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i].Index = i;
                checkpoints[i].onCompleted += OnCheckpointComplete;
            }
        }

        private void OnCheckpointComplete(int idx)
        {
            _completedCheckpoints++;
            Debug.Log(
                $"Checkpoint Manager: Checkpoint {idx.ToString()} completed at: {Time.time.ToString(CultureInfo.InvariantCulture)}");

            if (_completedCheckpoints == checkpoints.Length)
            {
                Debug.Log($"Checkpoint Manager: Level Ended. Bye :)");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}