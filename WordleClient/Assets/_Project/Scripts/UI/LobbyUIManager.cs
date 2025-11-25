using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nameText.text = PlayerManager.Instance.username;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
