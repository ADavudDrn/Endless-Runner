using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed;
    public float maxSpeed;

    private int desiredLane = 1;        //0:left  1:middle  2:right
    public float laneDistance = 2.5f;      //Distance between two lanes

    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;

    public float jumpForce;
    public float gravity = -20f;

    public Animator animator;
    private bool isSliding = false;
    void Start()
    {
        controller = GetComponent<CharacterController>();

    }
    private void Update()
    {
        if (!PlayerManager.isGameStarted)
            return;

        //Increase Speed
        if(forwardSpeed < maxSpeed)
        {
            forwardSpeed += 0.1f * Time.deltaTime;
        }

        animator.SetBool("isGameStarted", true);

        direction.z = forwardSpeed;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.15f, groundLayer);

        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            if (SwipeManager.swipeUp)
            {
                Jump();           
            }
        }
        else
        {
            direction.y += gravity * Time.deltaTime;

            if(SwipeManager.swipeDown)
                direction.y = -jumpForce / 2;
        }

        if (SwipeManager.swipeDown && !isSliding)
        {
            StartCoroutine(Slide());
        }

        //Gather the inputs for which lane we should be

        if (SwipeManager.swipeRight)
        {
            desiredLane++;
            if(desiredLane == 3)
            {
                desiredLane = 2;
            }
        }
        if (SwipeManager.swipeLeft)
        {
            desiredLane--;
            if (desiredLane == -1)
            {
                desiredLane = 0;
            }
        }

        //Calculate where we should be

        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;


        if(desiredLane == 0)
        {
            targetPosition += Vector3.left * laneDistance;
        }
        else if(desiredLane == 2)
        {
            targetPosition += Vector3.right * laneDistance;
        }

        //transform.position = Vector3.Lerp(transform.position, targetPosition, 50 * Time.fixedDeltaTime);
        //controller.center = controller.center;

        if(transform.position != targetPosition)
        {
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 25 * Time.deltaTime;
            if (moveDir.sqrMagnitude < diff.sqrMagnitude)
            {
                controller.Move(moveDir);
            }
            else
            {
                controller.Move(diff);
            }
        }
        
        //Move Player
        controller.Move(direction * Time.deltaTime);
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.tag == "Obstacle")
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().PlaySound("GameOver");
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);

        //Character Controller Collider change
        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;

        yield return new WaitForSeconds(1.3f);

        controller.center = new Vector3(0, 0, 0);
        controller.height = 2;

        animator.SetBool("isSliding", false);
        isSliding = false;
    }
}
