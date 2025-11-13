using UnityEngine;

public class AnimeSlash : MonoBehaviour
{
    [Header("Slash Settings")]
    public float speed = 20f;
    public float lifetime = 1.2f;
    public float hitRadius = 1.5f;
    public bool debugLogs = false;

    [Header("Effects")]
    public AudioSource spawnSound;
    public GameObject impactEffect;

    [Header("Element")]
    public ElementType element = ElementType.Fire;

    private float spawnTime;
    private Vector3 lastPosition;

    void Start()
    {
        spawnTime = Time.time;
        lastPosition = transform.position;

        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, 0f, scale.z);

        if (spawnSound)
            spawnSound.Play();
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        float distance = Vector3.Distance(lastPosition, transform.position);
        Vector3 scale = transform.localScale;
        scale.y += distance;
        transform.localScale = scale;

        lastPosition = transform.position;

        if (Time.time - spawnTime > lifetime)
            Destroy(gameObject);

        CheckForHits();
    }

    void CheckForHits()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (Collider hit in hits)
        {
            if (IsElementWeakTo(hit.tag))
            {
                if (impactEffect)
                    Instantiate(impactEffect, hit.transform.position, Quaternion.identity);

                Destroy(hit.gameObject); // Destroy target
                if (debugLogs)
                    Debug.Log($"[{element}] Slash destroyed {hit.name} (tag: {hit.tag})!");
            }
        }
    }

    bool IsElementWeakTo(string targetTag)
    {
        // Match the rock-paper-scissors rule
        switch (element)
        {
            case ElementType.Fire:
                return targetTag == "Ice";
            case ElementType.Ice:
                return targetTag == "Water";
            case ElementType.Water:
                return targetTag == "Fire";
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
