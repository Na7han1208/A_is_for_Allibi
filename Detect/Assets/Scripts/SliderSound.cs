using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderSoundPreview : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Slider slider;
    private bool isDragging;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        if (!SoundManager.Instance.IsPlaying("Marker"))
            SoundManager.Instance.PlayComplex("Marker", transform);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        SoundManager.Instance.Stop("Marker");
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isDragging) return;

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        SoundManager.Instance.UpdateVolumes();
    }
}
