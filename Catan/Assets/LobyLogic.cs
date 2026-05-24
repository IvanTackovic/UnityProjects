using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobyLogic : NetworkBehaviour
{
    [SerializeField] private TMP_Text codeInput;
    private Button blue, red, green, yellow, start;
    private List<Button> buttons;
    private Player player;
    void Start()
    {
        blue = GameObject.Find("blue").GetComponent<Button>();
        red = GameObject.Find("red").GetComponent<Button>();
        green = GameObject.Find("green").GetComponent<Button>();
        yellow = GameObject.Find("yellow").GetComponent<Button>();
        start = GameObject.Find("Start").GetComponent<Button>();
        buttons = new List<Button>{blue, red, green, yellow};
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        blue.onClick.AddListener(()=>ChooseColor(blue, Color.blue, "blue"));
        red.onClick.AddListener(()=>ChooseColor(red, Color.red, "red"));
        green.onClick.AddListener(()=>ChooseColor(green, Color.green, "green"));
        yellow.onClick.AddListener(()=>ChooseColor(yellow, Color.yellow, "yellow"));
        start.onClick.AddListener(() => StartGame());
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        codeInput.text = "Code: " + UnityRelay.GetJoinCode();
        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() =>
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null);

        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        Debug.Log("PLAYER FOUND: " + player.name);
    }
    
    void OnClientConnected(ulong clientId)
    {
        FixedString32Bytes code = new FixedString32Bytes(codeInput.text);
        SendCodeClientRpc(code, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{clientId}}});
        if(clientId == 0) return;
        foreach(Button button in buttons)
        {
            if(!button.interactable || (player.GetColor() != null && player.GetColor().Equals(button.gameObject.name))) 
            {DisableButtonClientRpc(false, button.gameObject.name, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){clientId}}});}
        }
    }

    [ClientRpc]
    private void SendCodeClientRpc(FixedString32Bytes code, ClientRpcParams clientRpcParams)
    {
        codeInput.text = code.Value;
    }

    void ChooseColor(Button button, Color color, String color_name)
    {
        if(player.GetColor() != null)
        {
            GameObject.Find(player.GetColor()).transform.Find("Tick").gameObject.SetActive(false);
            DisableButtonServerRpc(true, player.GetColor());
        }
        player.SetColor(color, color_name);
        GameObject tick = button.transform.Find("Tick").gameObject;
        tick.SetActive(true);
        DisableButtonServerRpc(false, button.gameObject.name);
    }

    [ServerRpc(RequireOwnership =false)]
    public void DisableButtonServerRpc(bool state, FixedString32Bytes button, ServerRpcParams serverRpcParams = default)
    {
        List<ulong> clinentIds = NetworkManager.Singleton.ConnectedClientsIds.ToList<ulong>();
        clinentIds.Remove(serverRpcParams.Receive.SenderClientId);
        DisableButtonClientRpc(state, button, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = clinentIds}});
    }

    [ClientRpc()]
    public void DisableButtonClientRpc(bool state, FixedString32Bytes button, ClientRpcParams clientRpcParams)
    {
        Button _button = GameObject.Find(button.Value).GetComponent<Button>();
        _button.interactable =state;
        GameObject X = _button.transform.Find("X").gameObject;
        X.SetActive(!state);
    }

    private void StartGame()
    {
        if(!IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
