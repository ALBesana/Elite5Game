using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLayerController : MonoBehaviour
{
    [SerializeField] private Vector4 crouchingSize;
    [SerializeField] private Vector4 standingSize;
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed= 1;
    [SerializeField] private float sprintSpeed = 2;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Ground Check Settings")]
    [SerializeField] private float jumpForce = 45f;
    [SerializeField] private Transform groundCheckpoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask IsGround;

    private Rigidbody2D rb;
    private float xAxis;
    private bool isSprint;
    private BoxCollider2D bc;

    public static PLayerController Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            Instance = this;
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        Move();
        Jump();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprint = true;
        }
        else
        {
            isSprint = false;
        }

        if (isSprint == true)
        {
            rb.velocity = new Vector2(sprintSpeed * xAxis, rb.velocity.y);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rb.velocity = new Vector2(crouchSpeed * xAxis, rb.velocity.y);
            bc.size = crouchingSize;
        }
        else
        {
            bc.size = standingSize;
        }
        
    }

    public bool Grounded()
    {
        if(Physics2D.Raycast(groundCheckpoint.position, Vector2.down, groundCheckY, IsGround) 
            || Physics2D.Raycast(groundCheckpoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, IsGround)
            || Physics2D.Raycast(groundCheckpoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, IsGround))
        {
            return true;
        }
        else
        {
            return false; 
        }
    }
    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        if (Input.GetButtonDown("Jump") && Grounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }
    }

}
