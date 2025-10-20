using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject SettingsCanvas;
    
    [Header("Buttons")]
    public Button Subtitles;
    public Button Jumpscares;

    public Sprite buttonOff;
    public Sprite buttonOn;

    [Header("Sliders")]
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;
    public Slider MouseSensSlider;
    public Slider ControllerSenseSlider;

    public GameObject JumpscareImage;
    private bool hasSeenJumpscare;

    void Start()
    {
        ShowUI(false);

        SetSubtitles(PlayerPrefs.GetInt("UseSubtitles", 1) == 1); // WHY DOENST PLAYER PREFS HAVE BOOL AHHHHHHHH
        SetJumpscares(false);

        MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        MouseSensSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        ControllerSenseSlider.value = PlayerPrefs.GetFloat("ControllerSensitivity", 1f);

        MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        MouseSensSlider.onValueChanged.AddListener(SetMouseSensitivity);
        ControllerSenseSlider.onValueChanged.AddListener(SetControllerSensitivity);
    }

    public void ShowUI(bool show)
    {
        SettingsCanvas.SetActive(show);
    }

    public void toggleJumpscares()
    {
        SetJumpscares(PlayerPrefs.GetInt("UseJumpscares", 1) == 0);
        if(!hasSeenJumpscare && PlayerPrefs.GetInt("UseJumpscares") == 1)
        {
            JumpscareHandler.Instance.TriggerJumpscare();
        }
    }

    public void toggleSubtitles()
    {
        SetSubtitles(PlayerPrefs.GetInt("UseSubtitles", 1) == 0);
    }

    private void SetSubtitles(bool useSubtitles)
    {
        Subtitles.image.sprite = useSubtitles ? buttonOn : buttonOff;
        PlayerPrefs.SetInt("UseSubtitles", useSubtitles ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetJumpscares(bool useJumpscares)
    {
        Jumpscares.image.sprite = useJumpscares ? buttonOn : buttonOff;
        PlayerPrefs.SetInt("UseJumpscares", useJumpscares ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        SoundManager.Instance?.UpdateVolumes();
    }

    private void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        SoundManager.Instance?.UpdateVolumes();
    }

    private void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        FindFirstObjectByType<FPController>().UpdateSensitivity();
    }

    private void SetControllerSensitivity(float value)
    {
        PlayerPrefs.SetFloat("ControllerSensitivity", value);
        PlayerPrefs.Save();
        FindFirstObjectByType<FPController>().UpdateSensitivity();
    }
}
