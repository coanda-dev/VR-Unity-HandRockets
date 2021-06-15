using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Core.HandRocketLocomotion
{
    public class HandPropulsionProvider : LocomotionProvider
    {
        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read Propulsion data from the left hand controller. Must be a Value Button Control.")]
        InputActionProperty m_LeftHandPropulsionAction;

        /// <summary>
        /// The Input System Action that will be used to read Propulsion data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="ButtonControl"/> Control.
        /// </summary>
        public InputActionProperty LeftHandPropulsionAction
        {
            get => m_LeftHandPropulsionAction;
            set => SetInputActionProperty(ref m_LeftHandPropulsionAction, value);
        }

        [SerializeField]
        [Tooltip(
            "The Input System Action that will be used to read Propulsion data from the right hand controller. Must be a Value Button Control.")]
        InputActionProperty m_RightHandPropulsionAction;

        /// <summary>
        /// The Input System Action that will be used to read Propulsion data from the right hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="ButtonControl"/> Control.
        /// </summary>
        public InputActionProperty RightHandPropulsionAction
        {
            get => m_RightHandPropulsionAction;
            set => SetInputActionProperty(ref m_RightHandPropulsionAction, value);
        }

        [SerializeField]
        [Tooltip(
            "The magnitude of the force applied in the desired direction.")]
        private float forceMagnitude = 10.0f;

        private Rigidbody _xrRigRigidbody;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            // Actions
            m_LeftHandPropulsionAction.EnableDirectAction();
            m_RightHandPropulsionAction.EnableDirectAction();

            // Physics
            _xrRigRigidbody = system.xrRig.GetComponent<Rigidbody>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_LeftHandPropulsionAction.DisableDirectAction();
            m_RightHandPropulsionAction.DisableDirectAction();
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        private void Update()
        {
            GetHandForwardVector(MotionControllerHand.LeftHand, out Vector3 forwardVector);
            Debug.Log($"Left hand forward vector: {forwardVector.ToString()}");
            //
            // GetHandForwardVector(MotionControllerHand.RightHand, out forwardVector);
            // Debug.Log($"Right hand forward vector: {forwardVector.ToString()}");

            // var targetFlightDirection = ComputeTargetFlightDirection();
            //
            // Debug.Log($"Remote Controlled Obj. Target Direction: {targetFlightDirection.ToString("F4")}");
            //
            // // Apply a force to the player rig in the direction the hands indicate
            // Vector3 force = targetFlightDirection.normalized * forceMagnitude;
            // _xrRigRigidbody.AddForce(force);
        }

        private enum MotionControllerHand
        {
            LeftHand,
            RightHand,
        }

        /// <summary>
        /// Grab the forward vector of the specified motion controller.
        /// </summary>
        /// <param name="hand">The requested motion controller.</param>
        /// <param name="forwardVector">Output parameter to write the forward vector in.</param>
        void GetHandForwardVector(MotionControllerHand hand, out Vector3 forwardVector)
        {
            InputDevice inputDevice = new InputDevice();
            Quaternion rotation = Quaternion.identity;

            switch (hand)
            {
                case MotionControllerHand.LeftHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                    break;
                case MotionControllerHand.RightHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                    break;
                default:
                    Assert.IsTrue(false, $"Exhausted options in {nameof(GetHandForwardVector)} for hands.");
                    break;
            }

            if (inputDevice.isValid)
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out rotation);

            forwardVector = rotation * Vector3.forward;
        }

        private Vector3 ComputeTargetFlightDirection()
        {
            bool leftHandGripped = (m_LeftHandPropulsionAction.action?.ReadValue<float>() ?? 0.0f) > .0f;
            bool rightHandGripped = (m_RightHandPropulsionAction.action?.ReadValue<float>() ?? 0.0f) > .0f;

            var targetFlightDirection = Vector3.zero;

            if (leftHandGripped)
                targetFlightDirection += ComputeDesiredDirection(MotionControllerHand.LeftHand);

            if (rightHandGripped)
                targetFlightDirection += ComputeDesiredDirection(MotionControllerHand.RightHand);

            targetFlightDirection = -targetFlightDirection;

            return targetFlightDirection;
        }


        private Vector3 ComputeDesiredDirection(MotionControllerHand hand)
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return Vector3.zero;

            var rigTransform = xrRig.rig.transform;
            var rigUp = rigTransform.up;

            Vector3 directionInWorldSpace = Vector3.zero;

            // switch (hand)
            // {
            //     case MotionControllerHand.LeftHand:
            //         directionInWorldSpace += _leftHandController.transform.right;
            //         break;
            //     case MotionControllerHand.RightHand:
            //         directionInWorldSpace -= _rightHandController.transform.right;
            //         break;
            //     default:
            //         // ReSharper disable once HeapView.BoxingAllocation
            //         Assert.IsTrue(false, $"{nameof(hand)}={hand} outside expected range.");
            //         break;
            // }


            return directionInWorldSpace;
        }
    }
}