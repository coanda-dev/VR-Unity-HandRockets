using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace HorseRiding.Scripts
{
    [AddComponentMenu("XR/Locomotion/Equitation Provider (Action-based)")]
    public class ActionBasedEquitationProvider : LocomotionProvider
    {
        // Measurements
        [SerializeField, Range(50, 1000)] private int _numberOfMeasurements = 100;
        [SerializeField, Range(5, 20)] private int _neighbourhood = 5;
        [SerializeField, Range(1, 50)] private float _speedMultiplier = 1.0f;

        private float[] _zPositionsBuffer;

        private int _insertIdx;

        private Rigidbody _rigidbody;
        private float _speed;

        // Inputs 
        [SerializeField] private InputActionProperty _hmdPosition;

        public InputActionProperty HmdPosition
        {
            get => _hmdPosition;
            set => SetInputActionProperty(ref _hmdPosition, value);
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        #region Lifecycle Methods

        private void Start()
        {
            _insertIdx = 0;
            _zPositionsBuffer = new float[_numberOfMeasurements];
            _rigidbody = system.xrRig.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_insertIdx < _zPositionsBuffer.Length)
            {
                Vector3 measurement = _hmdPosition.action?.ReadValue<Vector3>() ?? Vector3.zero;
                _zPositionsBuffer[_insertIdx++] = measurement.y;
            }
        }

        private void Update()
        {
            // 1. compute speed
            if (_insertIdx >= _zPositionsBuffer.Length * .9f)
            {
                int extremumCount = 0;
                List<float> zPositionsList = _zPositionsBuffer.ToList();
                List<float> amplitudes = new List<float>();
                for (int i = _neighbourhood; i < _insertIdx - _neighbourhood; i++)
                {
                    List<float> _neighbours = zPositionsList.GetRange(i - _neighbourhood, _neighbourhood * 2 + 1);
                    _neighbours.RemoveAt(_neighbourhood);
                    if (zPositionsList[i] <= _neighbours.Min() || zPositionsList[i] >= _neighbours.Max())
                        extremumCount++;
                    amplitudes.Add(_neighbours.Max() - _neighbours.Min());
                }

                _speed = extremumCount  * amplitudes.Average() * _speedMultiplier;
                _insertIdx = 0;
            }

            // 2. apply velocity to the XR Rig
            // if (CanBeginLocomotion() && BeginLocomotion())
            // {

            Vector3 velocity = system.xrRig.transform.forward * _speed;
            Debug.Log(
                $"Time: {Time.time.ToString()} - Forward: {system.xrRig.transform.forward.ToString()} - InsertIdx: {_insertIdx.ToString()} - Speed: {_speed.ToString()} - Velocity: {velocity.ToString()}");
            // Debug.Log($"Force vector applied at time {Time.time.ToString()}: {velocity.ToString()}");
            Vector3 v = new Vector3(0, 0, 10.0f);
            _rigidbody.AddForce(velocity);
            // EndLocomotion();
            // }
        }

        #endregion
    }
}