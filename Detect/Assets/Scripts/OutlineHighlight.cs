using UnityEngine;

public class OutlineHighlight : MonoBehaviour
{
    private Material materialInstance;

    void Start()
    {
        materialInstance = GetComponent<Renderer>().material;
        DisableOutline();
    }

    public void EnableOutline(float width)
    {

    }

    public void DisableOutline()
    {
        
    }
}
