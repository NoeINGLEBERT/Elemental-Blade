using UnityEngine;

public class FakeSpatialAudio : MonoBehaviour
{
    [Header("References")]
    public Transform listener; // Usually the VR headset or player head
    public AudioSource audioSource; // Must be stereo-enabled

    [Header("Sound Settings")]
    public AudioClip soundToPlay;
    public float maxDistance = 20f;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySpatialSound()
    {
        if (listener == null || audioSource == null || soundToPlay == null)
            return;

        Vector3 direction = transform.position - listener.position;
        direction.y = 0;

        float distance = direction.magnitude;
        if (distance > maxDistance) distance = maxDistance;

        // Normalize direction
        Vector3 dirNormalized = direction.normalized;

        // Angle between listener forward and sound direction
        float angle = Vector3.SignedAngle(listener.forward, dirNormalized, Vector3.up);

        // Convert angle to stereo pan (-1 = full left, 1 = full right)
        float pan = Mathf.Clamp(angle / 90f, -1f, 1f);

        // Distance volume falloff (linear)
        float volume = 1f - (distance / maxDistance);

        audioSource.panStereo = pan;
        audioSource.volume = volume;

        audioSource.PlayOneShot(soundToPlay);
    }
}
