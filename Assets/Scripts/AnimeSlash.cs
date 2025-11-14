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

    [Header("Death Sounds (By Enemy Tag)")]
    public AudioClip fireDeathSound;
    public AudioClip iceDeathSound;
    public AudioClip waterDeathSound;

    [Header("Spatial Audio Settings")]
    public Transform player;
    public float maxHearingDistance = 20f;

    void Start()
    {
        spawnTime = Time.time;
        lastPosition = transform.position;

        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, 0f, scale.z);

        if (spawnSound)
            spawnSound.Play();

        player = Camera.main.transform;
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

                AudioClip deathClip = null;

                switch (hit.tag)
                {
                    case "Fire":
                        deathClip = fireDeathSound;
                        break;

                    case "Ice":
                        deathClip = iceDeathSound;
                        break;

                    case "Water":
                        deathClip = waterDeathSound;
                        break;
                }

                PlayFakeSpatialisedSound(deathClip, hit.transform.position);

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

    void PlayFakeSpatialisedSound(AudioClip clip, Vector3 soundPos)
    {
        if (clip == null || player == null)
            return;

        // Direction from player to sound
        Vector3 dir = soundPos - player.position;
        dir.y = 0;

        float distance = dir.magnitude;
        if (distance > maxHearingDistance)
            distance = maxHearingDistance;

        Vector3 dirNorm = dir.normalized;

        // Angle relative to player's forward
        float angle = Vector3.SignedAngle(player.forward, dirNorm, Vector3.up);

        // Stereo pan -1 (left) to 1 (right)
        float pan = Mathf.Clamp(angle / 90f, -1f, 1f);

        // Distance-based volume
        float volume = 1f - (distance / maxHearingDistance);

        // We create a temporary audio source so AnimeSlash doesn't need one on itself
        GameObject temp = new GameObject("TempSpatialAudio");
        AudioSource a = temp.AddComponent<AudioSource>();

        a.spatialBlend = 0f;     // force stereo
        a.panStereo = pan;
        a.volume = volume;

        a.PlayOneShot(clip);

        Destroy(temp, clip.length + 0.2f);
    }
}
