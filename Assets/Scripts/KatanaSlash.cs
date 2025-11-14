using UnityEngine;
using UnityEngine.InputSystem;

public enum ElementType
{
    Fire,
    Ice,
    Water
}

public class KatanaSlash : MonoBehaviour
{
    [Header("References")]
    public Transform swordTip;
    public GameObject slashPrefab;
    public AudioSource slashSound;
    public bool debugLogs = true;

    [Header("Settings")]
    public float minDistance = 0.05f;
    public float lifetime = 1.0f;

    [Header("Element Settings")]
    public ElementType currentElement = ElementType.Fire;

    [Header("Controls")]
    public InputActionReference elementSwitchAction; // assign via Inspector or Input Manager

    private Vector3 lastTipPos;

    [Header("Element Audio")]
    public AudioClip fireSlashWhoosh;
    public AudioClip iceSlashWhoosh;
    public AudioClip waterSlashWhoosh;

    public AudioClip fireSwitchSound;
    public AudioClip iceSwitchSound;
    public AudioClip waterSwitchSound;

    private void OnEnable()
    {
        if (elementSwitchAction != null)
            elementSwitchAction.action.performed += OnElementSwitch;
    }

    private void OnDisable()
    {
        if (elementSwitchAction != null)
            elementSwitchAction.action.performed -= OnElementSwitch;
    }

    private void Start()
    {
        if (swordTip == null)
        {
            Debug.LogError("[KatanaSlash] swordTip not assigned!");
            enabled = false;
            return;
        }
        lastTipPos = swordTip.position;
    }

    private void Update()
    {
        Vector3 tipPos = swordTip.position;
        float dist = Vector3.Distance(lastTipPos, tipPos);

        if (dist >= minDistance)
        {
            SpawnSlashSegment(lastTipPos, tipPos);
            lastTipPos = tipPos;
        }
    }

    private void OnElementSwitch(InputAction.CallbackContext ctx)
    {
        currentElement = GetNextElement(currentElement);

        // --- Play element switch sound ---
        AudioClip switchClip = null;

        switch (currentElement)
        {
            case ElementType.Fire:
                switchClip = fireSwitchSound;
                break;

            case ElementType.Ice:
                switchClip = iceSwitchSound;
                break;

            case ElementType.Water:
                switchClip = waterSwitchSound;
                break;
        }

        if (slashSound && switchClip != null)
        {
            slashSound.volume = 1f;
            slashSound.PlayOneShot(switchClip);
        }


        if (debugLogs)
            Debug.Log($"[KatanaSlash] Switched to {currentElement}");
    }

    private ElementType GetNextElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return ElementType.Ice;
            case ElementType.Ice: return ElementType.Water;
            case ElementType.Water: return ElementType.Fire;
            default: return ElementType.Fire;
        }
    }

    private void SpawnSlashSegment(Vector3 startTip, Vector3 endTip)
    {
        if (slashPrefab == null) return;

        Vector3 mid = (startTip + endTip) * 0.5f;
        Vector3 swingDir = (endTip - startTip).normalized;

        Vector3 playerForward = Camera.main ? Camera.main.transform.forward : transform.forward;
        Vector3 forward = Vector3.ProjectOnPlane(playerForward, swingDir).normalized;
        if (forward.sqrMagnitude < 0.0001f) forward = transform.forward;

        Quaternion rot = Quaternion.LookRotation(forward, swingDir);
        GameObject go = Instantiate(slashPrefab, mid, rot);

        var slash = go.GetComponent<AnimeSlash>();
        if (slash != null)
            slash.element = currentElement;

        Vector3 scale = go.transform.localScale;
        scale.y = Vector3.Distance(startTip, endTip);
        go.transform.localScale = scale;

        // --- Pick correct whoosh sound ---
        AudioClip whoosh = null;

        switch (currentElement)
        {
            case ElementType.Fire:
                whoosh = fireSlashWhoosh;
                break;
            case ElementType.Ice:
                whoosh = iceSlashWhoosh;
                break;
            case ElementType.Water:
                whoosh = waterSlashWhoosh;
                break;
        }

        if (slashSound && whoosh != null)
        {
            slashSound.volume = 0.125f;
            slashSound.PlayOneShot(whoosh);
        }


        Destroy(go, lifetime);

        if (debugLogs)
            Debug.Log($"[Slash] Spawned {currentElement} slash. Start={startTip}, End={endTip}");
    }
}
