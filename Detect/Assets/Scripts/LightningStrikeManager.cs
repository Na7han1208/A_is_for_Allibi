using System.Collections;
using UnityEngine;

public class LightningStrikeManager : MonoBehaviour
{
    [SerializeField] private Material greySkybox;
    [SerializeField] private GameObject lightning;
    [SerializeField] private Color color;
    private bool hasStruck = false;
    [SerializeField] ParticleSystem rainfx;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Playground"))
        {
            if (hasStruck) return;
            hasStruck = true;
            SoundManager.Instance.PlayComplex("Lightning", transform);
            FindFirstObjectByType<RainSoundManager>().SetSystemActive(true);
            RenderSettings.skybox = greySkybox;
            RenderSettings.fogDensity = 0.3f;
            RenderSettings.fogColor = color;
            StartCoroutine(lightningStrike());
            rainfx.Play();
        }
    }

    private IEnumerator lightningStrike()
    {
        lightning.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        lightning.SetActive(false);
    }
}