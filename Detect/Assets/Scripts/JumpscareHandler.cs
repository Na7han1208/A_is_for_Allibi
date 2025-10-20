using UnityEngine;
using System.Collections;

public class JumpscareHandler : MonoBehaviour
{
    public static JumpscareHandler Instance;
    public GameObject JumpscareImage;

    void Awake()
    {
        Instance = this;
    }

    public void TriggerJumpscare()
    {
        StartCoroutine(DoJumpscare());
    }

    private IEnumerator DoJumpscare()
    {
        Debug.Log("DOING JUMPSCARE");
        yield return new WaitForSeconds(Random.Range(1f, 5f));
        SoundManager.Instance.PlayComplex("Jumpscare", transform);
        JumpscareImage.SetActive(true);
        yield return new WaitForSeconds(3f);
        JumpscareImage.SetActive(false);
    }
}
