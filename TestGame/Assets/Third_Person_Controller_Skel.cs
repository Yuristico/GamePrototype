using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class Third_Person_Controller_Skel : MonoBehaviour
{
    private Animator skeletonAnimations;
    public Transform cam;
    private Rigidbody skeletonPhysics;
    private CapsuleCollider capsuleCollider;
    private PlayerInputAction action;
    private Vector3 velocity;

    [Header("Jump")]
    public float jumpHeight = 5f;
    [Header("Gravity")]
    public float gravity = -9.81f;
    [Header("Movement Speed")]

    public float movementSpeed = 3f;
    public float sprintSpeed = 5f;
    public float acceleration = 2f;
    public float deceleration = 2f;

    private float distToGround;
    private float speed;
    private bool isSprinting;
    private bool isAttacking;
    
    private bool isBlocking;


    private void Awake()
    {
        skeletonPhysics = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        skeletonAnimations = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        distToGround = capsuleCollider.bounds.extents.y;
        action = new PlayerInputAction();
        action.Player.Enable();
        speed = movementSpeed;
        isSprinting = false;
    }

    private void RotateCamera()
    {
        Quaternion camRotation = cam.transform.rotation;
        transform.rotation = Quaternion.Euler(0f, camRotation.eulerAngles.y, 0f);
    }
    private void FixedUpdate()
    {
        RotateCamera();
        if (!isBlocking)
        {
            Move();    
        } 
    }

    private void Move()
    {
        Vector2 movementVector = action.Player.Move.ReadValue<Vector2>();
        Vector3 cameraForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 movement = (movementVector.x * cam.right + movementVector.y * cameraForward).normalized;
        
        Vector3 newPosition;

        Vector3 targetVelocity = movement * speed;
        if (!isGrounded())
        {
            skeletonAnimations.SetBool("isIdle", false);
            skeletonAnimations.SetBool("isLanding", true);
            velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * acceleration);
            if (movement == Vector3.zero)
            {
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * deceleration);
            }
            newPosition = skeletonPhysics.position + velocity * Time.fixedDeltaTime;

        }
        else
        {
            newPosition = skeletonPhysics.position + targetVelocity * Time.fixedDeltaTime;
            skeletonAnimations.SetBool("isJumping", false);
            skeletonAnimations.SetBool("isLanding", false);
        }

        skeletonPhysics.MovePosition(newPosition);

        if (movement.magnitude != 0)
        {
           
            if (isSprinting)
            {
                skeletonAnimations.SetBool("isIdle", false);
                skeletonAnimations.SetBool("isWalking", false);
                skeletonAnimations.SetBool("isRunning", true);


            }
            else
            {
                skeletonAnimations.SetBool("isWalking", true);
                skeletonAnimations.SetBool("isIdle", false);
            }

        }
        else
        {
            skeletonAnimations.SetBool("isWalking", false);
            if (isGrounded())
            {
                skeletonAnimations.SetBool("isIdle", true);
                skeletonAnimations.SetBool("isRunning", false);
            }
        }

    }
    private bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.3f);
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded())
        {
            skeletonAnimations.SetBool("isJumping", true);
            skeletonAnimations.SetBool("isIdle", false);
            skeletonPhysics.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
       
    }

    public void Block(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skeletonAnimations.SetBool("isRunning",false);
            skeletonAnimations.SetBool("isWalking", false);
            skeletonAnimations.SetBool("isBlocking", true);
            isBlocking = true;
        }
        else if(context.canceled)
        {
            skeletonAnimations.SetBool("isBlocking", false);
           isBlocking = false;
        }
    }

    private IEnumerator AttackCoroutine()
    {
  
        skeletonAnimations.SetBool("isAttacking", true);
        isAttacking = true;

   
        yield return new WaitForSeconds(0.72f); 

 
        skeletonAnimations.SetBool("isAttacking", false);
        isAttacking = false;
    }


    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            speed = sprintSpeed;
            isSprinting = true;
        }
        else if (context.canceled)
        {
            speed = movementSpeed;
            skeletonAnimations.SetBool("isRunning", false);
            isSprinting = false;
        }
    }
}
