using System;
using UnityEngine;
using UnityEngine.VFX;

namespace HandRockets.Scripts.Checkpoint_System
{
    public class Checkpoint : MonoBehaviour
    {
        public bool StopOnComplete = false;
        private VisualEffect _visualEffect;

        private int _index;

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public Action<int> onCompleted;

        private bool b_IsCompleted = false;

        private void Awake()
        {
            _visualEffect = GetComponentInChildren<VisualEffect>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (b_IsCompleted) return;

            onCompleted(_index);
            b_IsCompleted = true;
            _visualEffect.SetVector4("SmokeColor", new Color(114, 200, 147, 255));
            if (StopOnComplete)
                _visualEffect.Stop();
        }
    }
}