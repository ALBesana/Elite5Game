using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using UnityEngine.UI; 

public class PLayerController : MonoBehaviour
{

    [SerializeField] private Vector2 crouchingSize;
    [SerializeField] private Vector2 standingSize;
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

    [Header("Attack Settings")]
    bool attack;
    float timeBetweenAtk, timeSinceAtk;
    [SerializeField] Transform sideAttackTrans;
    [SerializeField] Vector2 sideAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;

    [Header("Recoil Setting")]
    [SerializeField] float recoilXSteps = 5;
    [SerializeField] float recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    int stepXRecoiled;

    [Header("Health Setting")]
    public int health;
    public int maxHealth;
    
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] public GameObject targetObject;
    private bool isNearTarget = false;
    Transform targetPosition; 

    public PlayerStateList pState;
    private Rigidbody2D rb;
    private float xAxis;
    Animator anim;
    private bool isSprint;
    private bool isCrouch;
    private BoxCollider2D bc;
    private bool ADmove;

    [SerializeField] private GameObject pauseMenu; 

    private bool isPaused = false;
    
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
        Health = maxHealth;
        Time.timeScale = 1f;
    }


    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bc = GetComponent<BoxCollider2D>();
         if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTrans.position, sideAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
         if (isPaused)
        {
            return;
        }
        GetInputs();
        Move();
        Jump();
        Attack();
        Flip();
        Recoil();

        if (health <= 0)
        {
            SceneManager.LoadScene(2);
        }
        if (Input.GetKeyDown(KeyCode.E) && isNearTarget)
        {
            ChangeScene();
        }
    }
     public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        pauseMenu.SetActive(true); 
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        pauseMenu.SetActive(false); 
    }
    public void RetryGame()
    {
        SceneManager.LoadScene(1);
        isPaused = false;
        Time.timeScale = 1f; 
        pauseMenu.SetActive(false); 
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == targetObject) // Ensure this is the Capybara
        {
            isNearTarget = true;
             if (interactionText != null)
            {
                interactionText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == targetObject)
        {
            isNearTarget = false;
            if (interactionText != null)
            {
                interactionText.gameObject.SetActive(false);
            }
        }
    }
    void ChangeScene()  
    {
        SceneManager.LoadScene(3);
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        attack = Input.GetButtonDown("Attack");
    }
    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = true;
        }
    }
    void Attack()
    {
        timeSinceAtk += Time.deltaTime;
        if (attack && timeSinceAtk >= timeBetweenAtk)
        {
            timeSinceAtk = 0;
            anim.SetTrigger("Attacking");
            Hit(sideAttackTrans, sideAttackArea, ref pState.recoilingX, recoilXSpeed);
        }
    }
    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer); 
        if(objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for (int i =0; i < objectsToHit.Length; i++)
        {
            if(objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().Enemyhit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }

    private void Move()
    {

        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouch && Grounded())
        {
            isSprint = true;
            currentSpeed = sprintSpeed;
        }
        else
        {
            isSprint = false;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Grounded())
        {
            if (!isCrouch)
            {
                isCrouch = true;
                bc.size = crouchingSize;
            }
            currentSpeed = crouchSpeed;
            anim.SetBool("Crouching", true);

            anim.SetBool("CrouchWalking", xAxis != 0);
        }
        else if (isCrouch)
        {
            isCrouch = false;
            bc.size = standingSize;
            anim.SetBool("Crouching", false);
            anim.SetBool("CrouchWalking", false);
        }

        rb.velocity = new Vector2(currentSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", xAxis != 0 && Grounded() && !isCrouch);
        
        anim.speed = isSprint ? sprintSpeed / walkSpeed : 1;
    }

    void Recoil()
    {
        if(pState.recoilingX)
        {
            if(pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }


        if(pState.recoilingX && stepXRecoiled < recoilXSteps)
        {
            stepXRecoiled++;
        }
        else
        {
        StopRecoilX();
        }
    }
        
    void StopRecoilX()
    {
        stepXRecoiled = 0;
        pState.recoilingX = false;
    }

    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);  
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    public int Health
    {
        get {return health; }
        set
        {
            if(health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
            }
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

        anim.SetBool("Jumping", !Grounded());
    }
}