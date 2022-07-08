using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MM
{
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject;
        InputHandler inputHandler;
        Vector3 moveDirection;

        [HideInInspector] public Transform myTransform;
        [HideInInspector] public AnimatorHandler animationHandler;

        public new Rigidbody rigidbody;

        public GameObject normalCamera;
        public float jumpSpeed = 10f;

        private bool jumping = false;

        [Header("Movement Stats")] 
        [SerializeField] float movementSpeed = 5;
        [SerializeField] float sprintSpeed = 7;
        [SerializeField] float rotationSpeed = 10;
        
        
        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animationHandler = GetComponentInChildren<AnimatorHandler>();
            playerManager = GetComponent<PlayerManager>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animationHandler.Initialize();
        }

        #region Movement

        Vector3 normalVector;
        private Vector3 targetPosition;
        private void HandleRotation(float delta)
        {
            Vector3 targetDirection = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

            targetDirection = cameraObject.forward * inputHandler.vertical;
            targetDirection += cameraObject.right * inputHandler.horizontal;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = myTransform.forward;
            }

            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            if (inputHandler.rollFlag)
            {
                return;
            }
            
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            moveDirection.Normalize();

            float speed = movementSpeed;
            if (inputHandler.sprintFlag)
            {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
            }
            else
            {
                playerManager.isSprinting = false;
            }

            moveDirection *= speed;
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);

            var keyboard = Keyboard.current;
            if (keyboard.spaceKey.wasPressedThisFrame && !jumping)
            {
                jumping = true;
                projectedVelocity.y = jumpSpeed;
            }
            else
            {
                projectedVelocity.y = rigidbody.velocity.y - 10 * Time.deltaTime;
            }

            rigidbody.velocity = projectedVelocity;

            animationHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);
            if (animationHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        #endregion

        #region Actions

        public void HandleRollingAndSprinting(float delta)
        {
            if (animationHandler.anim.GetBool("isInteracting"))
            {
                return;
            }

            if (inputHandler.rollFlag)
            {
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;
                if (inputHandler.moveAmount > 0)
                {
                    animationHandler.PlayTargetAnimation("Roll", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                //else
                //{
                //    animationHandler.PlayTargetAnimation("Backstep", true);
                //}
            }
        }

        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            var contact = collision.GetContact(0);
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5)
            {
                jumping = false;
            }

            if (collision.gameObject.CompareTag("Movable"))
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5)
                {
                    gameObject.transform.SetParent(collision.gameObject.transform, true);
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Movable"))
            {
                gameObject.transform.parent = null;
            }
        }
    }
}