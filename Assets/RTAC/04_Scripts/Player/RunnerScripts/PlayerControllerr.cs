//using Menu;

//using Saving;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)/*, typeof(AppearanceManager)*/)]
public class PlayerControllerr : MonoBehaviour
{
    [Header("-- Movement Settings --")] 
    [SerializeField, Tooltip("how fast the character walks in meters/second")] private float walkingSpeed = 10;
    [SerializeField, Tooltip("how fast the character runs in Meters/second")] private float runningforce = 20;
    [SerializeField] private int maxJumps = 2;
    [SerializeField, Tooltip("")] private float jumpForce = 4;
    [SerializeField, Tooltip("")] private float dashForce = 5;
    [SerializeField] private float turnLerpSpeed = 10;

    [Header("-- Camera Settings --")] 
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private bool thirdPerson = true;
    [SerializeField, Tooltip("The camera that rotates around the player")] private GameObject cameraGameObject;
    [SerializeField, Tooltip("The maximum angle the camera can look up or down.")] private float maxVerticalCameraAngle = 45;
    [SerializeField, Tooltip("What should be the camera's position in relation to the player.")] private Vector3 cameraOffset = new Vector3(0, 0, -6);
    [SerializeField, Tooltip("what position in relation to the player should the camera be looking at.")] private Vector3 cameraLookPosition = new Vector3(0, 2, 0);

    [Header("-- Animation Settings --")]
    [SerializeField] private Animator animator;
    [SerializeField] private float sprintSpeed = 15;
    
    //private AppearanceManager appearanceManager;
    private Rigidbody rigidBody;
    private float movementVelocity;
    private Vector3 movementDireciton;
    private bool dashThisFrame;
    private bool jumpThisFrame;
    
    private float currentCameraXRotation = 0;
    private float currentCameraYRotation = 0;

    private int jumpsLeft;
    private bool jumpHeldDown;
    private float touchingGround = 0;
    private Vector3 groundNormal;

    private float dashCooldown;
    private float timeSinceLeftGround;
    private float timeSinceJump;

    //reset the doublejump when making contact with an object
    private void OnCollisionEnter(Collision collision)
    {
        if (jumpsLeft != maxJumps)
        {
            //for each contact point in the collision
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= -0.1)
                {
                    jumpsLeft = maxJumps;
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        touchingGround = 1;
        timeSinceLeftGround = 0;

        //get the average normal of all contact points
        Vector3 contactNormalSum = new Vector3(0, 0, 0);
        int noOfContacts = collision.contactCount;
        for (int i = 0; i < noOfContacts; i++)
        {
            contactNormalSum += collision.GetContact(i).normal;
        }
        groundNormal = contactNormalSum / noOfContacts;
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingGround = 0;
        groundNormal = Vector3.up;
    }
    
    
    void FixedUpdate()
    {
        if (jumpThisFrame && jumpsLeft > 0)
        {
            if (timeSinceLeftGround < 0.25f && jumpsLeft == maxJumps)
            {
                rigidBody.AddForce(groundNormal.normalized * jumpForce, ForceMode.Impulse);
            }
            if (jumpsLeft == 1)
            {
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f, rigidBody.velocity.z);
            }
            
            if(animator)
                animator.SetTrigger("Jump");

            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsLeft -= 1;
            timeSinceJump = 0;
        }
        if (timeSinceLeftGround < 10)
        {
            timeSinceLeftGround += Time.fixedDeltaTime;
        }

        if(timeSinceJump < 10)
        {
            timeSinceJump += Time.fixedDeltaTime;
        }
        

        if (dashCooldown > 0)
        {
            dashCooldown -= Time.fixedDeltaTime;
        }
        if (dashThisFrame && dashCooldown <= 0)
        {
            if(animator)
            {
                animator.SetTrigger("Dash");
            }
            rigidBody.AddForce(cameraGameObject.transform.rotation * (dashForce * Vector3.forward), ForceMode.Impulse);
            dashCooldown = 1f;
        }
        jumpThisFrame = false;
        dashThisFrame = false;

        //sprinting is always on when touching the ground
        float forceToAdd = (touchingGround * movementVelocity);

        Vector3 force = movementDireciton * forceToAdd;

        Vector3 travellingDir = rigidBody.velocity;
        float dumbFriction = touchingGround * ( travellingDir.magnitude);
        Vector3 dumbFrictionDir = new Vector3(-travellingDir.x, 0, -travellingDir.z).normalized;
        Vector3 dumbFrictionForce = dumbFriction * dumbFrictionDir;

        rigidBody.AddForce(dumbFrictionForce);
        rigidBody.AddForce(force);

        if(animator)
        {
            animator.SetFloat("MovementSpeed", rigidBody.velocity.magnitude / sprintSpeed);
            animator.SetBool("Airborne", timeSinceLeftGround > 0.75f || (timeSinceJump < 0.8f && timeSinceLeftGround > 0.01f));
        }
    }
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            thirdPerson = !thirdPerson;
            //appearanceManager.SetVisibility(thirdPerson);
        }
        if(Input.GetMouseButtonDown(0))
        {
            dashThisFrame = true;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpThisFrame = true;
        }
        
        movementDireciton = Quaternion.Euler(0, cameraGameObject.transform.eulerAngles.y, 0) * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z), turnLerpSpeed * Time.deltaTime), Vector3.up);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        if(Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            movementVelocity = isSprinting ? runningforce : walkingSpeed;
        }
        else
        {
            movementVelocity = 0;
        }
        
        if(cameraGameObject /*&& !MenuHandler.theMenuHandler.Paused*/)
        {
            if(thirdPerson)
            {
                currentCameraXRotation += Input.GetAxisRaw("Mouse Y");
                currentCameraYRotation += Input.GetAxisRaw("Mouse X");
                //clamp the camera rotation to be less than the max and greater than the min
                currentCameraXRotation = Mathf.Clamp(currentCameraXRotation, -maxVerticalCameraAngle, maxVerticalCameraAngle);
                //set the position and rotation of the camera according to the current camera rotation variables.
                RaycastHit hit = new RaycastHit();
                Ray ray = new Ray(gameObject.transform.position + cameraLookPosition, Quaternion.Euler(-currentCameraXRotation, currentCameraYRotation, 0) * Vector3.back);
                if(Physics.SphereCast(ray, 0.25f, out hit, cameraOffset.magnitude))
                {
                    cameraGameObject.transform.position = hit.point + hit.normal * 0.25f;
                }
                else
                {
                    cameraGameObject.transform.position = gameObject.transform.position + cameraLookPosition + Quaternion.Euler(-currentCameraXRotation, currentCameraYRotation, 0) * cameraOffset;
                }
                cameraGameObject.transform.LookAt(transform.position + cameraLookPosition);
            }
            else
            {
                currentCameraXRotation -= Input.GetAxisRaw("Mouse Y");
                currentCameraYRotation += Input.GetAxisRaw("Mouse X");
                //clamp the camera rotation to be less than the max and greater than the min
                currentCameraXRotation = Mathf.Clamp(currentCameraXRotation, -maxVerticalCameraAngle, maxVerticalCameraAngle);
                //set the position and rotation of the camera according to the current camera rotation variables.
                cameraGameObject.transform.rotation = Quaternion.Euler(currentCameraXRotation, currentCameraYRotation, 0);
                Vector3 cameraPosition = Vector3.Lerp(cameraGameObject.transform.position, gameObject.transform.position + firstPersonOffset, 0.25f);
                cameraGameObject.transform.position = cameraPosition;
            }
        }
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //appearanceManager = GetComponent<AppearanceManager>();
        rigidBody = GetComponent<Rigidbody>();
    }
}
