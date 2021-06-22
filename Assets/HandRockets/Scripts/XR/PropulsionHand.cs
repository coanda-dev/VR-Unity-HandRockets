using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using UnityEngine.XR;

namespace HandRockets.Scripts.XR
{
    [RequireComponent(typeof(Animator))]
    public class PropulsionHand : MonoBehaviour
    {
        #region Fields

        private Animator _animator;
        private static readonly int Grip = Animator.StringToHash("Grip");
        private float _currentGrip;
        [SerializeField] private float _gripSpeed = 1.0f;
        private VisualEffect _vfx;
        private bool _isEffectPlaying;

        #endregion

        #region Properties

        private float _targetGrip;

        public float TargetGrip
        {
            get => _targetGrip;
            set => _targetGrip = value;
        }


        /// <summary>
        /// Differentiate between the motion controller hands.
        /// </summary>
        public enum ControllerHand
        {
            LeftHand = 0,
            RightHand = 1,
        }

        [SerializeField] private ControllerHand _motionControllerControllerHand;

        public ControllerHand MotionControllerControllerHand
        {
            get => _motionControllerControllerHand;
            set => _motionControllerControllerHand = value;
        }

        #endregion

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _vfx = GetComponentInChildren<VisualEffect>();
        }

        // Update is called once per frame
        void Update()
        {
            _currentGrip = Mathf.MoveTowards(_currentGrip, _targetGrip, Time.deltaTime * _gripSpeed);
        
            UpdateControllerInputValues();
        
            UpdateAnimation();
            UpdateVisualEffect();
        }

        private void UpdateVisualEffect()
        {
            if (_currentGrip > .1 && !_isEffectPlaying)
            {
                _isEffectPlaying = true;
                _vfx.SendEvent("OnPlay");
            }
        
            if (_currentGrip < .1 && _isEffectPlaying)
            {
                _isEffectPlaying = false;
                _vfx.SendEvent("OnStop");
            }
        
            _vfx.SetFloat("PropulsionStrength", Mathf.Lerp(0.0f, 0.25f, _currentGrip));
        }

        private void UpdateControllerInputValues()
        {
            InputDevice inputDevice = new InputDevice();

            switch (_motionControllerControllerHand)
            {
                case ControllerHand.LeftHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                    break;
                case ControllerHand.RightHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                    break;
                default:
                    Assert.IsTrue(false, $"Exhausted options in {nameof(UpdateControllerInputValues)} for hands.");
                    break;
            }

            if (inputDevice.isValid)
            {
                inputDevice.TryGetFeatureValue(CommonUsages.grip, out var value);
                Debug.Log($"{value.ToString(CultureInfo.InvariantCulture)}");
                _targetGrip = value;
            }
        }

        private void UpdateAnimation()
        {
            _animator.SetFloat(Grip, _currentGrip);
        }
    }
}