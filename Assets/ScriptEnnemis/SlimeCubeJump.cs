using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlimeCubeJump : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public bool autoFindPlayer = true;
    
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float horizontalSpeed = 5f;
    public float timeBetweenJumps = 2f;
    public bool startJumpingImmediately = true;
    
    [Header("Behavior Settings")]
    public float attackDistance = 5f;
    public float minAngleOffset = -90f;
    public float maxAngleOffset = 90f;
    
    [Header("Squash and Stretch")]
    public float squashAmount = 0.6f;
    public float stretchAmount = 1.4f;
    public float squashSpeed = 12f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer = 1;
    public float groundCheckDistance = 0.55f;
    
    private Rigidbody rb;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private float nextJumpTime;
    private bool isGrounded;
    private bool isInAttackRange;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        if (autoFindPlayer)
        {
            FindPlayer();
        }
        
        if (startJumpingImmediately)
        {
            nextJumpTime = Time.time + 0.5f;
        }
        else
        {
            nextJumpTime = Time.time + timeBetweenJumps;
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
        CheckDistanceToPlayer();
        AutoJump();
        UpdateSquashAndStretch();
    }
    
    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    private void CheckDistanceToPlayer()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isInAttackRange = distance <= attackDistance;
        }
    }
    
    private void AutoJump()
    {
        if (isGrounded && Time.time >= nextJumpTime)
        {
            Vector3 jumpDirection = Vector3.up;
            
            if (player != null)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0f;
                
                Vector3 finalDirection;
                
                if (isInAttackRange)
                {
                    finalDirection = directionToPlayer;
                }
                else
                {
                    float randomAngle = Random.Range(minAngleOffset, maxAngleOffset);
                    finalDirection = Quaternion.Euler(0f, randomAngle, 0f) * directionToPlayer;
                }
                
                jumpDirection = (Vector3.up + finalDirection * (horizontalSpeed / jumpForce)).normalized;
                
                transform.forward = finalDirection;
            }
            
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            
            nextJumpTime = Time.time + timeBetweenJumps;
        }
    }
    
    private void UpdateSquashAndStretch()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            targetScale = new Vector3(
                originalScale.x * (1f + (1f - squashAmount) * 0.3f),
                originalScale.y * squashAmount,
                originalScale.z * (1f + (1f - squashAmount) * 0.3f)
            );
        }
        else if (rb.linearVelocity.y > 0.5f)
        {
            targetScale = new Vector3(
                originalScale.x * (1f / stretchAmount),
                originalScale.y * stretchAmount,
                originalScale.z * (1f / stretchAmount)
            );
        }
        else if (rb.linearVelocity.y < -0.5f)
        {
            targetScale = new Vector3(
                originalScale.x * (1f / stretchAmount),
                originalScale.y * stretchAmount,
                originalScale.z * (1f / stretchAmount)
            );
        }
        else
        {
            targetScale = originalScale;
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * squashSpeed);
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
            float distance = Vector3.Distance(transform.position, player.position);
            bool inRange = distance <= attackDistance;
            
            Gizmos.color = inRange ? Color.red : Color.yellow;
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;
            directionToPlayer.Normalize();
            
            Gizmos.DrawRay(transform.position, directionToPlayer * 2f);
            
            Gizmos.color = inRange ? new Color(1f, 0.5f, 0f) : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
            
            if (!inRange)
            {
                Vector3 leftBound = Quaternion.Euler(0f, minAngleOffset, 0f) * directionToPlayer;
                Vector3 rightBound = Quaternion.Euler(0f, maxAngleOffset, 0f) * directionToPlayer;
                Gizmos.DrawRay(transform.position, leftBound * 2f);
                Gizmos.DrawRay(transform.position, rightBound * 2f);
            }
        }
    }
}
