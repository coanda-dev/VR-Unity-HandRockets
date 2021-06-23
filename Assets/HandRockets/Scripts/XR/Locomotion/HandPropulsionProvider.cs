using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace HandRockets.Scripts.XR.Locomotion
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly move their rig though the air, similarly to
    /// Iron-Man or Superman.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public class HandPropulsionProvider : LocomotionProvider
    {
        #region Fields

        [SerializeField] [Tooltip("Scale the magnitude of the thrust force.")]
        private float _thrustForceScale = 10.0f;

        private Rigidbody _xrRigRigidbody;

        public enum PropulsionType
        {
            Palm,
            Backwards
        }

        [SerializeField] private PropulsionType _propulsionType = PropulsionType.Palm;

        #endregion

        #region Input Control

        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read the orientation of the right hand. Must be a Value Quaternion Control.")]
        InputActionProperty _leftHandRotation;

        /// <summary>
        /// The Input System Action that will be used to read the orientation of the right hand. Must be a <see cref="InputActionType.Value"/> <see cref="QuaternionControl"/> Control.
        /// </summary>
        public InputActionProperty LeftHandRotation
        {
            get => _leftHandRotation;
            set => SetInputActionProperty(ref _leftHandRotation, value);
        }

        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read the orientation of the right hand. Must be a Value Quaternion Control.")]
        InputActionProperty _rightHandRotation;

        /// <summary>
        /// The Input System Action that will be used to read the orientation of the right hand. Must be a <see cref="InputActionType.Value"/> <see cref="QuaternionControl"/> Control.
        /// </summary>
        public InputActionProperty RightHandRotation
        {
            get => _rightHandRotation;
            set => SetInputActionProperty(ref _rightHandRotation, value);
        }


        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read Propulsion data from the left hand. Must be a Value float Control.")]
        InputActionProperty _leftPropulsionGaugeAction;

        /// <summary>
        /// The Input System Action that will be used to read Propulsion data from the left hand. Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/> Control.
        /// </summary>
        public InputActionProperty LeftPropulsionGaugeAction
        {
            get => _leftPropulsionGaugeAction;
            set => SetInputActionProperty(ref _leftPropulsionGaugeAction, value);
        }

        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read Propulsion data from the right hand. Must be a Value float Control.")]
        InputActionProperty _rightPropulsionGaugeAction;

        /// <summary>
        /// The Input System Action that will be used to read Propulsion data from the right hand. Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/> Control.
        /// </summary>
        public InputActionProperty RightPropulsionGaugeAction
        {
            get => _rightPropulsionGaugeAction;
            set => SetInputActionProperty(ref _rightPropulsionGaugeAction, value);
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

        #region Lifecycle Methods

        private void OnEnable()
        {
            _leftHandRotation.EnableDirectAction();
            _rightHandRotation.EnableDirectAction();
            _leftPropulsionGaugeAction.EnableDirectAction();
            _rightPropulsionGaugeAction.EnableDirectAction();
        }

        private void OnDisable()
        {
            _leftHandRotation.DisableDirectAction();
            _rightHandRotation.DisableDirectAction();
            _leftPropulsionGaugeAction.DisableDirectAction();
            _rightPropulsionGaugeAction.DisableDirectAction();
        }

        private void Start()
        {
            _xrRigRigidbody = system.xrRig.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 thrustForceDirection = ComputeThrustForceDirection();
            _xrRigRigidbody.AddForce(thrustForceDirection * _thrustForceScale);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines the thrust force vector by summing up the opposite directions the two palms are facing.
        /// </summary>
        /// <returns></returns>
        Vector3 ComputeThrustForceDirection()
        {
            // Variable to accumulate the vectors in
            Vector3 thrustForceDirection = Vector3.zero;
            Vector3 propulsionDirection = Vector3.back;

            // Compute the left hand palm direction only if the left grip button is pressed
            float leftGripGauge = _leftPropulsionGaugeAction.action?.ReadValue<float>() ?? 0.0f;
            if (leftGripGauge >= 0.1)
            {
                Quaternion leftHandOrientation =
                    _leftHandRotation.action?.ReadValue<Quaternion>() ?? Quaternion.identity;
                if (_propulsionType == PropulsionType.Palm)
                    propulsionDirection = Vector3.right;
                thrustForceDirection += leftHandOrientation * propulsionDirection;
            }

            // Compute the right hand palm direction only if the right grip button is pressed
            float rightGripPressure = _rightPropulsionGaugeAction.action?.ReadValue<float>() ?? 0.0f;
            if (rightGripPressure >= 0.1)
            {
                Quaternion rightHandOrientation =
                    _rightHandRotation.action?.ReadValue<Quaternion>() ?? Quaternion.identity;
                if (_propulsionType == PropulsionType.Palm)
                    propulsionDirection = Vector3.left;
                thrustForceDirection += rightHandOrientation * propulsionDirection;
            }

            return -thrustForceDirection;
        }

        #endregion
    }
}