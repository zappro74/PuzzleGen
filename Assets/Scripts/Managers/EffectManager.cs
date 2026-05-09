using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EffectManager : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject missileEffect;
    [SerializeField] private GameObject elephantEffect;
    [SerializeField] private GameObject jetEffect;
    [SerializeField] private GameObject planeEffect;

    public void PlayMissile()
    {
        PlayEffect(missileEffect);
    }

    public void PlayElephant()
    {
        PlayEffect(elephantEffect);
    }

    public void PlayJet()
    {
        PlayEffect(jetEffect);
    }

    public void PlayPlane()
    {
        PlayEffect(planeEffect);
    }

    public void PlayRandom()
    {
        GameObject[] effects =
        {
            missileEffect,
            elephantEffect,
            jetEffect,
            planeEffect
        };

        GameObject chosen = effects[Random.Range(0, effects.Length)];

        PlayEffect(chosen);
    }

    private void PlayEffect(GameObject effectObject)
    {
        StartCoroutine(PlayRoutine(effectObject));
    }

    private IEnumerator PlayRoutine(GameObject effectObject)
    {
        effectObject.SetActive(true);

        VideoPlayer player = effectObject.GetComponent<VideoPlayer>();

        player.Stop();
        player.time = 0;
        player.Play();

        yield return new WaitUntil(() => player.isPlaying);

        while (player.isPlaying)
        {
            yield return null;
        }

        effectObject.SetActive(false);
    }
}