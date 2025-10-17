using UnityEngine;

public class LockController : MonoBehaviour
{
    private LockDigit[] digits;

    void Start()
    {
        digits = GetComponentsInChildren<LockDigit>();
    }

    public string GetCurrentCode()
    {
        string code = "";
        foreach (var d in digits)
            code += d.GetValue().ToString();
        return code;
    }
}
