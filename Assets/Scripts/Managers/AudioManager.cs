using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Load Sound")]
    [SerializeField] private AudioSource loadAudioSource;
    [SerializeField] private AudioClip loadSound;

    [Header("Audio Connections")]
    [SerializeField] private AudioSource dragAudio;
    [SerializeField] private AudioSource grabAudioSource;
    [SerializeField] private AudioClip[] grabSounds;
    [SerializeField] private AudioSource snapAudioSource;
    [SerializeField] private AudioClip[] snapSounds;

    [Header("Audio Settings")]
    [SerializeField] private float maxDragSpeed = 10f;
    [SerializeField] private float maxDragVolume = .8f;
    [SerializeField] private float dragSoundThreshold = 0.05f;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.3f;

    private float[] spectrum = new float[512];

    private void PlayLoadSound()
    {
        if (loadAudioSource == null || loadSound == null) return;
        StartCoroutine(PlayLoadSoundWithFadeOut());
    }
    private IEnumerator PlayLoadSoundWithFadeOut()
    {
        loadAudioSource.volume = 1f;
        loadAudioSource.PlayOneShot(loadSound);

        yield return new WaitForSeconds(loadSound.length - 1f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            loadAudioSource.volume = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            yield return null;
        }

        loadAudioSource.volume = 0f;
    }
    private IEnumerator FadeInWinMusic(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            winMusicSource.volume = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        winMusicSource.volume = 1f;
    }
    private IEnumerator PlayWinAudio()
    {
        if (winAudioSource != null && winSound != null)
        {
            winAudioSource.PlayOneShot(winSound);
            yield return new WaitForSeconds(winSound.length - 1);
        }

        if (winMusicSource != null && winMusic != null)
        {
            winMusicSource.clip = winMusic;
            winMusicSource.loop = true;
            winMusicSource.volume = 0f;
            winMusicSource.Play();
            StartCoroutine(FadeInWinMusic(4f));
        }
    }
    private void UpdateDragAudio(float speed, int groupSize, bool isSnapping)
    {
        if (speed < dragSoundThreshold)
        {
            dragAudio.volume = Mathf.Lerp(dragAudio.volume, 0f, Time.deltaTime * 12f);
            return;
        }

        float speed01 = Mathf.Clamp01(speed / maxDragSpeed);

        float groupVolumeBoost = Mathf.Clamp01(groupSize / 10f) * 0.2f;

        float targetVolume = (speed01 * maxDragVolume) + groupVolumeBoost;

        float groupPitchDrop = Mathf.Clamp01(groupSize / 10f) * 0.15f;

        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speed01) - groupPitchDrop;

        if (isSnapping)
        {
            targetVolume *= 0.5f;
            targetPitch *= 1.1f;
        }

        dragAudio.volume = Mathf.Lerp(dragAudio.volume, targetVolume, Time.deltaTime * 10f);

        dragAudio.pitch = Mathf.Lerp(dragAudio.pitch, targetPitch, Time.deltaTime * 10f);
    }
    private IEnumerator FadeOutDragAudio()
    {
        float startVolume = dragAudio.volume;

        while (dragAudio.volume > 0.01f)
        {
            dragAudio.volume = Mathf.Lerp(dragAudio.volume, 0f, Time.deltaTime * 12f);

            yield return null;
        }

        dragAudio.Stop();
        dragAudio.volume = 0f;
    }
    private void PlayGrabSound()
    {
        if (grabAudioSource == null || grabSounds == null || grabSounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, grabSounds.Length);

        grabAudioSource.volume = 4f;

        grabAudioSource.pitch = Random.Range(0.95f, 1.05f);

        grabAudioSource.PlayOneShot(grabSounds[randomIndex]);
    }

}
