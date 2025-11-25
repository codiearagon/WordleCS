using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyButton : MonoBehaviour
{
    public InputActionReference activateKey;
    private Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void OnEnable()
    {
        activateKey.action.performed += OnActivateKeyPerformed;
        activateKey.action.Enable();
    }

    void OnDisable()
    {
        activateKey.action.performed -= OnActivateKeyPerformed;
        activateKey.action.Disable();
    }

    private void OnActivateKeyPerformed(InputAction.CallbackContext context)
    {
        button.onClick.Invoke();
    }
}
