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
        public Vector3 moveDirection;

        [HideInInspector] public Transform myTransform;
        [HideInInspector] public AnimatorHandler animationHandler;

        public new Rigidbody rigidbody;

        public GameObject normalCamera;
        public float jumpSpeed = 10f;

        private bool jumping = false;

        [Header("Ground and Air detection stats")] 
        [SerializeField] float groundDetectionRayStartPoint = 0.4f;
        [SerializeField] float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField] float groundDetectionRayDistance = 0.2f;
        
        LayerMask ignoreForGroundCheck;

        public float inAirTimer;

        [Header("Movement Stats")]
        [SerializeField] float movementSpeed = 5;
        [SerializeField] float sprintSpeed = 7;
        [SerializeField] float rotationSpeed = 10;
        [SerializeField] float fallingSpeed = 45;
        
        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animationHandler = GetComponentInChildren<AnimatorHandler>();
            playerManager = GetComponent<PlayerManager>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animationHandler.Initialize();
            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
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
            if (inputHandler.rollFlag || playerManager.isInteracting)
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

            //var keyboard = Keyboard.current;
            //if (keyboard.spaceKey.wasPressedThisFrame && !jumping)
            //{
            //    jumping = true;
            //    projectedVelocity.y = jumpSpeed;
            //}
            //else
            //{
            //    projectedVelocity.y = rigidbody.velocity.y - 10 * Time.deltaTime;
            //}

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

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            
            origin.y += groundDetectionRayStartPoint;
            Debug.DrawRay(origin, myTransform.forward * 0.4f, Color.blue, 0.1f, false);
            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }

            if (playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 10f);
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin += dir * groundDetectionRayDistance; // ?
            targetPosition = myTransform.position;
            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
            {
                // If we are not fully in collision with the ground, but not far off the ground
                // we move to fully collide with the ground
                playerManager.isGrounded = true;
                targetPosition.y = hit.point.y;
                normalVector = hit.normal;

                if (playerManager.isInAir)
                {
                    if (inAirTimer > 0.5f)
                    {
                        Debug.Log("You are in air for " + inAirTimer);
                        // Get a landing animation first
                        // animationHandler.PlayTargetAnimation("Land", true);
                    }
                    else
                    {
                        // animationHandler.PlayTargetAnimation("Empty", true);
                    }
                    inAirTimer = 0;
                    playerManager.isInAir = false;
                }
            }
            else // Otherwise we start falling
            {
                playerManager.isGrounded = false;

                if (!playerManager.isInAir)
                {
                    if (!playerManager.isInteracting)
                    {
                        // animationHandler.PlayTargetAnimation("Falling", true);                        
                    }
                    Vector3 velocity = rigidbody.velocity;
                    velocity.Normalize();
                    rigidbody.velocity = velocity * movementSpeed / 2f;
                    playerManager.isInAir = true;
                }                
            }

            if (playerManager.isGrounded)
            {
                if (playerManager.isInteracting || inputHandler.moveAmount > 0)
                {
                    Debug.Log("Delta: " + delta);
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, delta * 10);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
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