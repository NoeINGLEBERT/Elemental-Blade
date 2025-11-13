using UnityEngine;

public class AnimeSlash : MonoBehaviour
{
    [Header("Slash Settings")]
    public float speed = 20f;             // forward motion speed
    public float lifetime = 1.2f;         // total lifetime
    public float hitRadius = 1.5f;        // radius for hits
    public bool debugLogs = false;

    [Header("Effects")]
    public AudioSource spawnSound;
    public GameObject impactEffect;

    private float spawnTime;
    private Vector3 lastPosition;

    void Start()
    {
        spawnTime = Time.time;
        lastPosition = transform.position;

        // Start with zero Y scale
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, 0f, scale.z);

        if (spawnSound)
            spawnSound.Play();
    }

    void Update()
    {
        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Grow along Y based on distance moved this frame
        float distance = Vector3.Distance(lastPosition, transform.position);
        Vector3 scale = transform.localScale;
        scale.y += distance; // add growth along Y
        transform.localScale = scale;

        lastPosition = transform.position;

        // Lifetime check
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }

        CheckForHits();
    }

    void CheckForHits()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // TODO: apply damage
                if (debugLogs)
                    Debug.Log($"Hit {hit.name}!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
