using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    public UnityEvent OnClick = new();
    public void OnMouseDown()
    {
        OnClick?.Invoke();
    }
}
