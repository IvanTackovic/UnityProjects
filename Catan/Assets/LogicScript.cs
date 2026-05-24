using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogicScript : NetworkBehaviour
{

    [SerializeField] Button Hostbutton;
    [SerializeField] Button JoinButton;
    [SerializeField] Button SendButton;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_InputField nameEnter;
    void Start()
    {
        Hostbutton.onClick.AddListener(()=>SceneManager.LoadScene("CreateGameScene"));
        JoinButton.onClick.AddListener(()=>inputField.gameObject.SetActive(true));
        JoinButton.onClick.AddListener(()=>SendButton.gameObject.SetActive(true));
        SendButton.onClick.AddListener(async ()=>await JoinGame());
    }

    void Update()
    {
        
    }

    private string GetCode()
    {
        return inputField.text;
    }

    private async Task JoinGame()
    {
        string code = inputField.text;
        await UnityRelay.JoinRelay(code);
    }
    public void StoreName(string name)
    {
        if (name.Length > 30)return;
        Debug.Log(name);
        PlayerPrefs.SetString("name", name);
    }
}
