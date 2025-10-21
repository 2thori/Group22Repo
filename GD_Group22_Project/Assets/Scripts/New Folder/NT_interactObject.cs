using UnityEngine;
using UnityEngine.Events;

public class NT_interactObject : MonoBehaviour
{
    public string interactionText = "Press E to Interact";
    public UnityEvent onInteract;

    public string GetInteractionText() // Fixed method name typo
    {
        return interactionText;
    }

    public void Interact()
    {
        onInteract.Invoke();
    }
}