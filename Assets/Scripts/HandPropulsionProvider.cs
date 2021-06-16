using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Core.HandRocketLocomotion
{
    public class HandPropulsionProvider : LocomotionProvider
    {
        [SerializeField]
        [Tooltip(
            "The magnitude of the force applied in the desired direction.")]
        private float forceSize = 10.0f;

        private Rigidbody _xrRigRigidbody;

        private void Start()
        {
            // Physics
            _xrRigRigidbody = system.xrRig.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 thrustForce = ComputeThrustForce();
            _xrRigRigidbody.AddForce(thrustForce * forceSize);
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
        /// <param name="palmFacingDirection">Output parameter to write the forward vector in.</param>
        void GetPalmFacingDirection(MotionControllerHand hand, out Vector3 palmFacingDirection)
        {
            InputDevice inputDevice = new InputDevice();
            Quaternion rotation = Quaternion.identity;
            palmFacingDirection = Vector3.one;

            switch (hand)
            {
                case MotionControllerHand.LeftHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                    palmFacingDirection = Vector3.right;
                    break;
                case MotionControllerHand.RightHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                    palmFacingDirection = Vector3.left;
                    break;
                default:
                    Assert.IsTrue(false, $"Exhausted options in {nameof(GetPalmFacingDirection)} for hands.");
                    break;
            }

            if (inputDevice.isValid)
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out rotation);

            palmFacingDirection = rotation * palmFacingDirection;
        }

        float GetGripValue(MotionControllerHand hand)
        {
            InputDevice inputDevice = new InputDevice();
            float value = 0.0f;

            switch (hand)
            {
                case MotionControllerHand.LeftHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                    break;
                case MotionControllerHand.RightHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                    break;
            }

            if (inputDevice.isValid)
                inputDevice.TryGetFeatureValue(CommonUsages.grip, out value);

            return value;
        }

        Vector3 ComputeThrustForce()
        {
            Vector3 thrustForce = Vector3.zero;

            float leftGripPressure = GetGripValue(MotionControllerHand.LeftHand);
            if (leftGripPressure >= 0.1)
            {
                GetPalmFacingDirection(MotionControllerHand.LeftHand, out Vector3 palmFacingDirection);
                thrustForce += -palmFacingDirection * leftGripPressure;
            }


            float rightGripPressure = GetGripValue(MotionControllerHand.RightHand);
            if (rightGripPressure >= 0.1)
            {
                GetPalmFacingDirection(MotionControllerHand.RightHand, out Vector3 palmFacingDirection);
                thrustForce += -palmFacingDirection * rightGripPressure;
            }

            return thrustForce;
        }
    }
}