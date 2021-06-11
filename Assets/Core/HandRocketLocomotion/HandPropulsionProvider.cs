using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Core.HandRocketLocomotion
{
    public class HandPropulsionProvider : LocomotionProvider
    {
        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Propulsion data from the left hand controller. Must be a Value Button Control.")]
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
        [Tooltip("The Input System Action that will be used to read Propulsion data from the right hand controller. Must be a Value Button Control.")]
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
        ActionBasedController m_LeftHandController;
        
        public ActionBasedController LeftHandController
        {
            get => m_LeftHandController;
            set => m_LeftHandController = value;
        }
        
        [SerializeField]
        ActionBasedController m_RightHandController;


        public ActionBasedController RightHandController
        {
            get => m_RightHandController;
            set => m_RightHandController = value;
        }
        
        [SerializeField]
        [Tooltip("Controls whether gravity affects this provider when a Character Controller is used.")]
        bool m_UseGravity = true;
        /// <summary>
        /// Controls whether gravity affects this provider when a <see cref="CharacterController"/> is used.
        /// </summary>
        public bool useGravity
        {
            get => m_UseGravity;
            set => m_UseGravity = value;
        }
        
        [SerializeField]
        [Tooltip("Controls whether gravity affects this provider when a Character Controller is used.")]
        GameObject m_RemoteControlledObject;

        /// <summary>
        /// Controls whether gravity affects this provider when a <see cref="CharacterController"/> is used.
        /// </summary>
        public GameObject RemoteControlledObject
        {
            get => m_RemoteControlledObject;
            set => m_RemoteControlledObject = value;
        }
        
        CharacterController m_CharacterController;
        private bool m_AttemptedGetCharacterController;

        Vector3 m_VerticalVelocity;
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_LeftHandPropulsionAction.EnableDirectAction();
            m_RightHandPropulsionAction.EnableDirectAction();
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
            bool leftHandGripped = (m_LeftHandPropulsionAction.action?.ReadValue<float>() ?? 0.0f) > .0f;
            bool rightHandGripped = (m_RightHandPropulsionAction.action?.ReadValue<float>() ?? 0.0f) > .0f;

            var desiredDirection= Vector3.zero;
            
            if (leftHandGripped)
            {
                Debug.Log("Left Hand is Gripped");
                desiredDirection += ComputeDesiredDirection(MotionControllerHand.LeftHand);
            }
            
            if (rightHandGripped)
            {
                Debug.Log("Right Hand is Gripped");
                desiredDirection += ComputeDesiredDirection(MotionControllerHand.RightHand);
            }
            desiredDirection = - desiredDirection;
            Debug.Log($"HandPropulsionProvider wants to move the rig in the direction: {desiredDirection}");
            m_RemoteControlledObject.transform.rotation = Quaternion.LookRotation(desiredDirection, new Vector3(desiredDirection.y, desiredDirection.z, desiredDirection.x));
            // MoveRigInDirection(desiredDirection.normalized);
        }

        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
        private void MoveRigInDirection(Vector3 direction)
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return;
            
            FindCharacterController();

            var motion = direction;
            
            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                // Step vertical velocity from gravity
                if (m_CharacterController.isGrounded || !m_UseGravity)
                {
                    m_VerticalVelocity = Vector3.zero;
                }
                else
                {
                    m_VerticalVelocity += Physics.gravity * Time.deltaTime;
                }

                motion += m_VerticalVelocity * Time.deltaTime;

                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    m_CharacterController.Move(motion);

                    EndLocomotion();
                }
            }
            else
            {
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    xrRig.rig.transform.position += motion;

                    EndLocomotion();
                }
            }
            
            
        }

        private enum MotionControllerHand
        {
            LeftHand,
            RightHand,
        }

        private Vector3 ComputeDesiredDirection(MotionControllerHand hand)
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return Vector3.zero;
            
            var rigTransform = xrRig.rig.transform;
            var rigUp = rigTransform.up;
            
            Vector3 directionInWorldSpace = Vector3.zero;
            
            switch (hand)
            {
                case MotionControllerHand.LeftHand:
                    directionInWorldSpace += m_LeftHandController.transform.right;
                    break;
                case MotionControllerHand.RightHand:
                    directionInWorldSpace -= m_RightHandController.transform.right;
                    break;
                default:
                    // ReSharper disable once HeapView.BoxingAllocation
                    Assert.IsTrue(false, $"{nameof(hand)}={hand} outside expected range.");
                    break;
            }
            
            

            return directionInWorldSpace;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        void FindCharacterController()
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return;

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                m_CharacterController = xrRig.rig.GetComponent<CharacterController>();
                m_AttemptedGetCharacterController = true;
            }
        }
    }
}
