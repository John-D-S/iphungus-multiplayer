//using Menu;

//using Saving;

using AltarChase.Networking;

using Mirror;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Rigidbody), typeof(NetworkIdentity))]
public class RunnerController : NetworkBehaviour
{
    [Header("-- Movement Settings --")] 
    [SerializeField, Tooltip("how fast the character walks in meters/second")] private float walkingSpeed = 10;
    [SerializeField, Tooltip("how fast the character runs in Meters/second")] private float runningforce = 20;
    [SerializeField] private int maxJumps = 2;
    [SerializeField, Tooltip("")] private float jumpForce = 4;
    [SerializeField, Tooltip("")] private float dashForce = 5;
    [SerializeField] private float turnLerpSpeed = 10;

    [Header("-- Camera Settings --")] 
    [SerializeField] private bool thirdPerson = true;
    /*[SerializeField, Tooltip("The camera that rotates around the player")]*/ private GameObject cameraGameObject;
    [SerializeField, Tooltip("The maximum angle the camera can look up or down.")] private float maxVerticalCameraAngle = 45;
    [SerializeField, Tooltip("How fast the camera's transform lerps to its target position")] private float cameraLerpSpeed = 1f;
    [SerializeField, Tooltip("What should be the camera's position in relation to the player.")] private Vector3 cameraOffset = new Vector3(0, 0, -6);
    [SerializeField, Tooltip("what position in relation to the player should the camera be looking at.")] private Vector3 cameraLookPosition = new Vector3(0, 2, 0);
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.5f, 0);

    [Header("-- Animation Settings --")] 
    [SerializeField] private NetworkAnimator netAnimator;
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
    private float touchingGround = 0;
    private Vector3 groundNormal;

    private float dashCooldown;
    private float timeSinceLeftGround;
    private float timeSinceJump;

    private Vector3 lastCheckpointPosition;

    [HideInInspector] public string characterName = "blumbo";
    
    [Command]
    public void CmdSetCharacterName(string _name) => RpcSetCharacterName(_name);

    [ClientRpc]
    public void RpcSetCharacterName(string _name)
    {
        SetCharacterName(_name);
    }

    public void SetCharacterName(string _name)
    {
        characterName = _name;
    }

    public void ReturnToLastCheckpoint()
    {
        Debug.Log("ReturnToLastCheckpointCalled");
        transform.position = lastCheckpointPosition;
        rigidBody.velocity = Vector3.zero;
    }
    
    public override void OnStartLocalPlayer()
    {
        SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
        Lobby lobby = FindObjectOfType<Lobby>();
        CustomNetworkManager.AddPlayer(this);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Killzone"))
        {
            //Debug.Log("shouldBeDead");
            ReturnToLastCheckpoint();
        }
        else if(other.CompareTag("Checkpoint"))
        {
            lastCheckpointPosition = other.transform.position;
        }
        else if(other.CompareTag("Finish"))
        {
            FinishGame();
        }
    }

    [Server]
    private void FinishGame()
    {
        FindObjectOfType<PopUp>().RpcPopupText($"{characterName} has won the race!");
        RpcFinishGame();
    }

    [ClientRpc]
    public void RpcFinishGame()
    {
        MatchManager.instance.CallLoadMainMenu(5);
    }
    
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
        if(isLocalPlayer)
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

                if(netAnimator)
                {
                    netAnimator.SetTrigger("Jump");
                }

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
                //if(animator)
                //{
                    //animator.SetTrigger("Dash");
                //}
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

            if(netAnimator)
            {
                netAnimator.animator.SetFloat("MovementSpeed", rigidBody.velocity.magnitude / sprintSpeed);
                //animator.SetBool("E", timeSinceLeftGround > 0.75f || (timeSinceJump < 0.8f && timeSinceLeftGround > 0.01f));
            }
        }
    }
    
    private void Update()
    {
        if(isLocalPlayer)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                ReturnToLastCheckpoint();
            }
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
            
            if(cameraGameObject)
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
                    if(Physics.SphereCast(ray, 0.25f, out hit, cameraOffset.magnitude, ~0, QueryTriggerInteraction.Ignore))
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
                    Vector3 cameraPosition = Vector3.Lerp(cameraGameObject.transform.position, gameObject.transform.position + firstPersonOffset, cameraLerpSpeed * Time.deltaTime);
                    cameraGameObject.transform.position = cameraPosition;
                }
            }
        }
    }
    
    private void Start()
    {
        cameraGameObject = FindObjectOfType<Camera>().gameObject;
        //appearanceManager = GetComponent<AppearanceManager>();
        rigidBody = GetComponent<Rigidbody>();
        if(!isLocalPlayer)
        {
            rigidBody.isKinematic = true;
        }
        lastCheckpointPosition = transform.position;
    }
}
