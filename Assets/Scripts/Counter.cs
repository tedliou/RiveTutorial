using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Counter : MonoBehaviour
{
    private TMP_Text _tmpText;
    private int _count;

    private void Start()
    {
        _tmpText = GetComponent<TMP_Text>();
        _count = 0;
        UpdateText();
    }

    private void UpdateText() => _tmpText.text = _count.ToString();

    public void Increase()
    {
        _count++;
        UpdateText();
    }
}
