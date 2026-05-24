using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    struct Message
    {
        private int sender;
        private bool ischecked;
        private FixedString4096Bytes text;
        public Message(int sender, FixedString4096Bytes text, bool ischecked)
        {
            this.sender = sender;
            this.text = text;
            this.ischecked = ischecked;
        }
        public int GetSender()
        {
            return sender;
        }
        public FixedString4096Bytes GetText()
        {
            return text;
        }
    }
    [SerializeField] private Button chatButton, sendButton;
    [SerializeField] private GameObject Chat, MessageFrom, MessageTo;
    [SerializeField] private TMP_Dropdown contacts, receiver;
    [SerializeField] private ScrollRect messages;
    [SerializeField] private TMP_InputField input;
    IReadOnlyDictionary<ulong, NetworkClient> clients;
    Dictionary<string, ulong> clientUlongs = new Dictionary<string, ulong>();
    Dictionary<ulong, int> clientIndex = new Dictionary<ulong, int>();
    Dictionary<int, List<Message>> messagePlatforms = new Dictionary<int, List<Message>>();
    int lastcheckedMessage = 0, currentListIndex=-1;
    void Start()
    {
        clients = NetworkManager.Singleton.ConnectedClients;

        foreach(ulong key in clients.Keys)
        {
            clientUlongs.Add(clients[key].PlayerObject.GetComponent<Player>().GetName() + key, key);
            messagePlatforms.Add((int)key, new List<Message>());
        }
        messagePlatforms.Add(-1, new List<Message>());
        CreateContactDropMenu();
        chatButton.onClick.AddListener(()=>{Chat.SetActive(!Chat.activeSelf);});
        sendButton.onClick.AddListener(()=>SendMessage());
        contacts.onValueChanged.AddListener((a) =>ChangeContact(a));
    }

    public void CreateContactDropMenu()
    {
        List<string> strings = clientUlongs.Keys.ToList();
        foreach(KeyValuePair<string, ulong> keyValuePair in clientUlongs)
        {
            if(keyValuePair.Value == NetworkManager.Singleton.LocalClientId)
            {
                strings.Remove(keyValuePair.Key);
                break;
            }
        }
        strings.Insert(0, "Public");
        contacts.AddOptions(strings);
        receiver.AddOptions(strings);
        Chat.SetActive(false);
    }
    public void SendMessage()
    {
        FixedString4096Bytes text = input.text;
        string receive = receiver.options[receiver.value].text;
        if (receive.Equals("Public"))
        {
            Debug.Log(text + "; " + receive);
            DistribueMessageServerRpc(text, -1);
        }
        else
        {
            DistribueMessageServerRpc(text, (int)clientUlongs[receive]);
            if (messagePlatforms[(int)clientUlongs[receive]].Count >= 100)
            {
                messagePlatforms[(int)clientUlongs[receive]].RemoveAt(0);
            }
            messagePlatforms[(int)clientUlongs[receive]].Add(new Message((int)NetworkManager.LocalClientId, text, false));
            GenerateMessage((int)clientUlongs[receive]);
        }

    }

    [ServerRpc(RequireOwnership =false)]
    public void DistribueMessageServerRpc(FixedString4096Bytes text, int receive, ServerRpcParams serverRpcParams = default)
    {
        ulong sender = serverRpcParams.Receive.SenderClientId;
        if(receive == -1)
        {
            ReceiveMessageClientRpc(text, sender,true, default);
        }
        else
        {
            ReceiveMessageClientRpc(text, sender,false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{(ulong)receive}}});
        }
    }
    [ClientRpc()]
    public void ReceiveMessageClientRpc(FixedString4096Bytes text, ulong sender, bool ispublic, ClientRpcParams clientRpcParams)
    {
        if (ispublic)
        {
            Message message = new Message((int)sender, text, false);
            if(messagePlatforms[-1].Count>=100) messagePlatforms[-1].RemoveAt(0);
            messagePlatforms[-1].Add(message);
            if (currentListIndex == -1)
            {
                GenerateMessage(-1);
            }
        }
        else
        {
            Message message = new Message((int)sender, text, false);
            if(messagePlatforms[(int)sender].Count>=100) messagePlatforms[(int)sender].RemoveAt(0);
            messagePlatforms[(int)sender].Add(message);
            if (currentListIndex == (int)sender)
            {
                GenerateMessage((int)sender);
            }
        }
    }
    public void GenerateMessage(int listIndex)
    {
        List<Message> pom = messagePlatforms[listIndex];
        Message mess;
        int i;
        for(i=lastcheckedMessage; i<pom.Count; i++)
        {
            Debug.Log(i);
            mess = pom[i];
            if (mess.GetSender() == (int)NetworkManager.LocalClientId)
            {
                Debug.Log("Generiram");
                GameObject _gameObject = Instantiate(MessageTo);
                _gameObject.transform.SetParent(messages.content, false);
                _gameObject.transform.Find("Text").GetComponent<TMP_Text>().text = mess.GetText().ToSafeString();
            }
            else
            {
                GameObject _gameObject = Instantiate(MessageFrom);
                _gameObject.transform.SetParent(messages.content, false);
                _gameObject.transform.Find("Text").GetComponent<TMP_Text>().text = mess.GetText().ToSafeString();
                string s = clients[(ulong)mess.GetSender()].PlayerObject.GetComponent<Player>().GetName();
                _gameObject.transform.Find("Sender").GetComponent<TMP_Text>().text = "From: " + s;
            }
        }
        lastcheckedMessage = i;
    }

    public void ChangeContact(int listIndex)
    {
        foreach(Transform child in messages.content)
        {
            Destroy(child.gameObject);
        }
        lastcheckedMessage=0;
        string pom = contacts.options[listIndex].text;
        if(pom.Equals("Public")) currentListIndex =-1;
        else currentListIndex = (int)clientUlongs[pom];
        GenerateMessage(currentListIndex);
    }
}
