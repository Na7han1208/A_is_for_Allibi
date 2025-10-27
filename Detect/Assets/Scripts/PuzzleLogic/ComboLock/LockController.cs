using UnityEngine;

public class LockController : MonoBehaviour
{
    private LockDigit[] digits;
    public string correctCode;

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

    public void CheckCode()
    {
        if (GetCurrentCode() == correctCode)
        {
            CompletePuzzle();
        }
    }
    
    public void CompletePuzzle()
    {
        Debug.Log("Lock SOLVED");
    }
}
