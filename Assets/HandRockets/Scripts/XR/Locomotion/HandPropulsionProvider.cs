using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
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
        #region Properties and Fields

        [SerializeField] [Tooltip("Scale the magnitude of the thrust force.")]
        private float forceSize = 10.0f;

        private Rigidbody _xrRigRigidbody;

        #endregion

        private void Start()
        {
            _xrRigRigidbody = system.xrRig.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 thrustForce = ComputeThrustVector();
            _xrRigRigidbody.AddForce(thrustForce * forceSize);
        }

        /// <summary>
        /// Differentiate between the motion controller hands.
        /// </summary>
        private enum MotionControllerHand
        {
            LeftHand,
            RightHand,
        }

        /// <summary>
        /// Determines the current direction the palm of the <paramref name="hand"/> is facing.
        /// </summary>
        /// <param name="hand">The hand to inquire about.</param>
        Vector3 GetPalmFacingDirection(MotionControllerHand hand)
        {
            InputDevice inputDevice = new InputDevice();
            Vector3 palmFacingDirection = Vector3.one;

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

            Quaternion rotation = Quaternion.identity;
            if (inputDevice.isValid)
                inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);

            return rotation * palmFacingDirection;
        }

        /// <summary>
        /// Determines the current pressed amount of the grip button on the motion controller held
        /// in <paramref name="hand"/>.
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        float GetGripValue(MotionControllerHand hand)
        {
            InputDevice inputDevice = new InputDevice();

            switch (hand)
            {
                case MotionControllerHand.LeftHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                    break;
                case MotionControllerHand.RightHand:
                    inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                    break;
            }

            float value = 0.0f;
            if (inputDevice.isValid)
                inputDevice.TryGetFeatureValue(CommonUsages.grip, out value);

            return value;
        }

        /// <summary>
        /// Determines the thrust force vector by summing up the opposite directions the two palms are facing.
        /// </summary>
        /// <returns></returns>
        Vector3 ComputeThrustVector()
        {
            // Variable to accumulate the vectors in
            Vector3 thrustForce = Vector3.zero;

            // Compute the left hand palm direction only if the left grip button is pressed
            float leftGripPressure = GetGripValue(MotionControllerHand.LeftHand);
            if (leftGripPressure >= 0.1)
                thrustForce += -GetPalmFacingDirection(MotionControllerHand.LeftHand) * leftGripPressure;
            // Compute the right hand palm direction only if the right grip button is pressed
            float rightGripPressure = GetGripValue(MotionControllerHand.RightHand);
            if (rightGripPressure >= 0.1)
                thrustForce += -GetPalmFacingDirection(MotionControllerHand.RightHand) * rightGripPressure;

            return thrustForce;
        }
    }
}