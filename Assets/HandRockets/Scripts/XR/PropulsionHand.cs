using HandRockets.Scripts.XR.Locomotion;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace HandRockets.Scripts.XR
{
    [RequireComponent(typeof(Animator))]
    public class PropulsionHand : MonoBehaviour
    {
        #region Fields

        // Animation
        private Animator _animator;
        private static readonly int Grip = Animator.StringToHash("Grip");

        // Visual Effect
        private VisualEffect _vfx;
        private bool _isEffectPlaying;

        [SerializeField]
        private HandPropulsionProvider.PropulsionType _propulsionType = HandPropulsionProvider.PropulsionType.Palm;

        // Gauge 
        private float _currentGrip;
        [SerializeField] private float _gripSpeed = 1.0f;

        #endregion

        #region Properties

        private float _targetGrip;

        public float TargetGrip
        {
            get => _targetGrip;
            set => _targetGrip = value;
        }

        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Propulsion data. Must be a Value float Control.")]
        InputActionProperty _propulsionGaugeAction;

        /// <summary>
        /// The Input System Action that will be used to read Propulsion data. Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/> Control.
        /// </summary>
        public InputActionProperty PropulsionGaugeAction
        {
            get => _propulsionGaugeAction;
            set => SetInputActionProperty(ref _propulsionGaugeAction, value);
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        #endregion

        #region Lifecycle Functions

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _vfx = GetComponentInChildren<VisualEffect>();
            if (_propulsionType == HandPropulsionProvider.PropulsionType.Backwards)
            {
                _vfx.transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
        }

        private void OnEnable()
        {
            _propulsionGaugeAction.EnableDirectAction();
        }

        private void OnDisable()
        {
            _propulsionGaugeAction.DisableDirectAction();
        }

        // Update is called once per frame
        void Update()
        {
            // Grab the current gauge press
            _targetGrip = _propulsionGaugeAction.action?.ReadValue<float>() ?? 0.0f;
            // Interpolate for smooth animation
            _currentGrip = Mathf.MoveTowards(_currentGrip, _targetGrip, Time.deltaTime * _gripSpeed);

            // UpdateControllerInputValues();

            // Update animation
            _animator.SetFloat(Grip, _currentGrip);

            UpdateVisualEffect();
        }

        #endregion

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
    }
}