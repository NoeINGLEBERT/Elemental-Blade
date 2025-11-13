using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChaserEnemy : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public bool autoFindPlayer = true;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer = 1;
    public float groundCheckDistance = 1f;
    public bool stayOnGround = true;
    
    private Rigidbody rb;
    private bool isGrounded;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        if (autoFindPlayer)
        {
            FindPlayer();
        }
    }
    
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
            }
        }
    }
    
    private void Update()
    {
        CheckGroundStatus();
    }
    
    private void FixedUpdate()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
        }
    }
    
    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    private void MoveTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        if (distanceToPlayer > stopDistance)
        {
            directionToPlayer.y = 0f;
            directionToPlayer.Normalize();
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            
            Vector3 moveVelocity = directionToPlayer * moveSpeed;
            
            if (stayOnGround && isGrounded)
            {
                moveVelocity.y = rb.linearVelocity.y;
            }
            
            rb.linearVelocity = moveVelocity;
        }
        else
        {
            if (stayOnGround && isGrounded)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
        
        if (player != null)
        {
            Gizmos.color = Color.red;
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;
            Gizmos.DrawRay(transform.position, directionToPlayer.normalized * 2f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}
