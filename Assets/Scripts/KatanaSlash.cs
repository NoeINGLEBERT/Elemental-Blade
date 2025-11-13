using UnityEngine;

public class ContinuousKatanaSlash : MonoBehaviour
{
    [Header("References")]
    public Transform swordTip;               // Katana tip
    public GameObject slashPrefab;           // Anime slash prefab
    public AudioSource slashSound;           // Optional
    public bool debugLogs = true;

    [Header("Settings")]
    public float minDistance = 0.05f;        // Minimum distance to spawn a slash segment
    public float lifetime = 1.0f;            // How long each slash prefab lasts

    private Vector3 lastTipPos;

    void Start()
    {
        if (swordTip == null)
        {
            Debug.LogError("[ContinuousKatanaSlash] swordTip not assigned!");
            enabled = false;
            return;
        }
        lastTipPos = swordTip.position;
    }

    void Update()
    {
        Vector3 tipPos = swordTip.position;
        float dist = Vector3.Distance(lastTipPos, tipPos);

        if (dist >= minDistance)
        {
            SpawnSlashSegment(lastTipPos, tipPos);
            lastTipPos = tipPos;
        }
    }

    void SpawnSlashSegment(Vector3 startTip, Vector3 endTip)
    {
        if (slashPrefab == null) return;

        Vector3 mid = (startTip + endTip) * 0.5f;
        Vector3 swingDir = (endTip - startTip).normalized;

        // Forward = player/camera forward projected perpendicular to swing
        Vector3 playerForward = Camera.main ? Camera.main.transform.forward : transform.forward;
        Vector3 forward = Vector3.ProjectOnPlane(playerForward, swingDir).normalized;
        if (forward.sqrMagnitude < 0.0001f) forward = transform.forward;

        Quaternion rot = Quaternion.LookRotation(forward, swingDir);

        GameObject go = Instantiate(slashPrefab, mid, rot);

        // Scale along swing direction
        Vector3 scale = go.transform.localScale;
        scale.y = Vector3.Distance(startTip, endTip); // Assuming prefab length along Y
        go.transform.localScale = scale;

        if (slashSound && !slashSound.isPlaying) slashSound.Play();
        Destroy(go, lifetime);

        if (debugLogs)
            Debug.Log($"[Slash] Spawned. Start={startTip}, End={endTip}, Dir={swingDir}, Length={scale.y:F2}");
    }
}
