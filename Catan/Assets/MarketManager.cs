using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MarketManager : NetworkBehaviour
{
    struct Trade
    {
        string TradeId;
        private Dictionary<ResourceType, int> offers;
        private Dictionary<ResourceType, int> demands;
        bool toOther;
        private ulong senderOrReceiver;
        FixedString4096Bytes text;
        public Trade(Dictionary<ResourceType, int> offers, Dictionary<ResourceType, int> demands, bool toOther, FixedString4096Bytes text, ulong id, string TradeId)
        {
            this.offers = offers;
            this.demands = demands;
            this.toOther = toOther;
            this.text = text;
            this.senderOrReceiver = id;
            this.TradeId = TradeId;
        }
        public bool GetToOther()
        {
            return toOther;
        }
        public ulong GetSenderOrReceiver()
        {
            return senderOrReceiver;
        }
        public Dictionary<ResourceType, int> Getoffers()
        {
            return offers;
        }
        public Dictionary<ResourceType, int> GetDemands()
        {
            return demands;
        }
        public FixedString4096Bytes GetDesc()
        {
            return text;
        }
        public String GetTradeId()
        {
            return TradeId;
        }
        public String DemandsStringify()
        {
            string s=" ";
            foreach(KeyValuePair<ResourceType, int> keyValuePair in demands)
            {
                s = s + keyValuePair.Key.ToString() + ": " + keyValuePair.Value + "; ";
            }
            return s;
        }
        public String OffersStringify()
        {
            string s=" ";
            foreach(KeyValuePair<ResourceType, int> keyValuePair in offers)
            {
                s = s + keyValuePair.Key.ToString() + ": " + keyValuePair.Value + "; ";
            }
            return s;
        }
    }

    [SerializeField] private GameObject Market, NewTradeWindow, NewTradeBluePrint, Demands, Offerings;
    [SerializeField] private Button marketButton, makeTradeButton, descriptionButton, sendTrade, othersTradesButton, myTradesButton;
    [SerializeField] private ScrollRect trades;
    [SerializeField] private TMP_Dropdown receiver;
    [SerializeField] private TMP_InputField description;
    [SerializeField] private GameObject mainLogic;
    private IReadOnlyDictionary<ulong, NetworkClient> clients;
    private Dictionary<string, ulong> NameUlongMap = new Dictionary<string, ulong>();
    private Dictionary<Slider, ResourceType> sliders_demands = new Dictionary<Slider, ResourceType>();
    private Dictionary<Slider, ResourceType> sliders_offerings = new Dictionary<Slider, ResourceType>();
    private Dictionary<string, Trade> trade_log = new Dictionary<string, Trade>();
    private Dictionary<string, bool?> acceptance_log = new Dictionary<string, bool?>();
    private Player player;
    private bool myTradesOpen = true;

    void Start()
    {
        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        marketButton.onClick.AddListener(()=>Market.SetActive(!Market.activeSelf));
        makeTradeButton.onClick.AddListener(()=>{NewTradeWindow.SetActive(!NewTradeWindow.activeSelf); RefreshSliders();RefreshReceiver();});
        descriptionButton.onClick.AddListener(()=>description.gameObject.SetActive(!description.gameObject.activeSelf));
        sendTrade.onClick.AddListener(()=>CreateTrade());
        myTradesButton.onClick.AddListener(()=>{myTradesOpen=true; RefreshTrades("ffff", true);});
        othersTradesButton.onClick.AddListener(()=>{myTradesOpen=false; RefreshTrades("ffff", false);});

        clients = NetworkManager.Singleton.ConnectedClients;
        foreach(ulong id in clients.Keys)
        {
            NameUlongMap.Add(clients[id].PlayerObject.GetComponent<Player>().GetName() + id, id);
        }
        CreateSliders();
        foreach(Slider slider in sliders_demands.Keys)
        {
            slider.maxValue = 15;
        }
    }

    public void RefreshReceiver()
    {
        receiver.options.Clear();
        MainLogic mL = mainLogic.GetComponent<MainLogic>();
        if(mL.GetPersonOnTurnId() != NetworkManager.Singleton.LocalClientId)
        {
            foreach(KeyValuePair<string, ulong> keyValuePair in NameUlongMap)
            {
                if(keyValuePair.Value == mL.GetPersonOnTurnId())
                {
                    receiver.AddOptions(new List<String>(){keyValuePair.Key});
                    return;
                }
            }
        }
        List<String> strings;
        strings = NameUlongMap.Keys.ToList();
        foreach(KeyValuePair<string, ulong> keyValuePair in NameUlongMap)
        {
            if(keyValuePair.Value == NetworkManager.Singleton.LocalClientId)
            {
                strings.Remove(keyValuePair.Key);
                break;
            }
        }
        receiver.AddOptions(strings);
        return;
    }
    private void CreateSliders()
    {
        for(int i=0; i<Demands.transform.childCount; i++)
        {
            GameObject child = Demands.transform.GetChild(i).gameObject;
            if(child.name.Equals("Wheat")) {
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_demands.Add(slider, ResourceType.Wheat);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Sheep")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_demands.Add(slider, ResourceType.Sheep);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Wood")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_demands.Add(slider, ResourceType.Forest);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Brick")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_demands.Add(slider, ResourceType.Brick);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Ore")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_demands.Add(slider, ResourceType.Ore);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
        }
        for(int i=0; i<Offerings.transform.childCount; i++)
        {
            GameObject child = Offerings.transform.GetChild(i).gameObject;
            if(child.name.Equals("Wheat")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_offerings.Add(slider, ResourceType.Wheat);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Sheep")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_offerings.Add(slider, ResourceType.Sheep);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Wood")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_offerings.Add(slider, ResourceType.Forest);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Brick")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_offerings.Add(slider, ResourceType.Brick);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
            else if(child.name.Equals("Ore")){
                Slider slider = child.transform.Find("Slider").GetComponent<Slider>();
                sliders_offerings.Add(slider, ResourceType.Ore);
                slider.onValueChanged.AddListener((value)=>ChangeNum(slider, value));
            }
        }
    }

    public void RefreshSliders()
    {
        int num;
        foreach(Slider slider in sliders_offerings.Keys)
        {
            num = player.GetQuantityOfResource(sliders_offerings[slider]);
            slider.maxValue = num;
        }
    }
    private void ChangeNum(Slider slider, float value)
    {
        GameObject parent = slider.transform.parent.gameObject;
        TMP_Text text = parent.transform.Find("Num").GetComponent<TMP_Text>();
        text.text = value.ToString();
    }

    private void CreateTrade()
    {
        Dictionary<ResourceType, int> demands = new Dictionary<ResourceType, int>();
        Dictionary<ResourceType, int> offers = new Dictionary<ResourceType, int>();
        FixedString4096Bytes desc;
        foreach(KeyValuePair<Slider, ResourceType> keyValuePair in sliders_demands)
        {
            if(keyValuePair.Key.value>0) demands.Add(keyValuePair.Value, (int)keyValuePair.Key.value);
        }
        foreach(KeyValuePair<Slider, ResourceType> keyValuePair in sliders_offerings)
        {
            if(keyValuePair.Key.value>0) offers.Add(keyValuePair.Value, (int)keyValuePair.Key.value);
        }
        desc = description.text;
        ulong clientId = NameUlongMap[receiver.options[receiver.value].text];
         string tradeId = System.Guid.NewGuid().ToString();
        Trade tradeStore = new Trade(offers, demands, true, desc, clientId, tradeId);
        trade_log.Add(tradeId, tradeStore);

        SendOfferServerRpc(offers.Keys.ToArray(), offers.Values.ToArray(), demands.Keys.ToArray(), demands.Values.ToArray(), desc, tradeId, clientId);
        if (myTradesOpen) RefreshTrades(tradeId, true);

    }

    [ServerRpc(RequireOwnership =false)]
    public void SendOfferServerRpc(ResourceType[] offerType, int[] offerNum, ResourceType[] demandType, int[] demandNum, FixedString4096Bytes desc, FixedString128Bytes tradeId, ulong receiver, ServerRpcParams serverRpcParams = default)
    {
        ulong sender = serverRpcParams.Receive.SenderClientId;
        ReceiveOfferClientRpc(offerType, offerNum, demandType, demandNum, desc, tradeId, sender, 
                              new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){receiver}}});
    }
    [ClientRpc()]
    public void ReceiveOfferClientRpc(ResourceType[] offerType, int[] offerNum, ResourceType[] demandType, int[] demandNum, FixedString4096Bytes desc, FixedString128Bytes tradeId, ulong sender, ClientRpcParams clientRpcParams)
    {
        Dictionary<ResourceType, int> offers = new Dictionary<ResourceType, int>();
        Dictionary<ResourceType, int> demands = new Dictionary<ResourceType, int>();

        for(int i=0; i<offerType.Length; i++)
        {
            offers.Add(offerType[i], offerNum[i]);
        }
        for(int i=0; i<demandType.Length; i++)
        {
            demands.Add(demandType[i], demandNum[i]);
        }
        Trade trade = new Trade(offers, demands, false, desc, sender, tradeId.ToSafeString());
        trade_log.Add(tradeId.ToSafeString(), trade);
        if(!myTradesOpen) RefreshTrades(tradeId.ToString(), false);
    }

    public void RefreshTrades(string tradeid, bool mytrades)
    {
        if (tradeid.Equals("ffff"))
        {
            foreach(Transform child in trades.content)
            {
                Destroy(child.gameObject);
            }
            if (mytrades)
            {
                foreach(KeyValuePair<string, Trade> keyValuePair in trade_log)
                {
                    if (keyValuePair.Value.GetToOther())
                    {
                        GameObject _gameObject = Instantiate(NewTradeBluePrint);
                        _gameObject.transform.SetParent(trades.content, false);
                        _gameObject.transform.Find("Sender").GetComponent<TMP_Text>().text = "TO: " + clients[keyValuePair.Value.GetSenderOrReceiver()].PlayerObject.GetComponent<Player>().GetName();
                        _gameObject.transform.Find("Offer").GetComponent<TMP_Text>().text = "OFFFERS" + keyValuePair.Value.OffersStringify();
                        _gameObject.transform.Find("Want").GetComponent<TMP_Text>().text = "WANTS" + keyValuePair.Value.DemandsStringify();
                        _gameObject.transform.Find("Description").Find("Text").GetComponent<TMP_Text>().text = keyValuePair.Value.GetDemands().ToSafeString();
                        _gameObject.transform.Find("Description").gameObject.SetActive(false);
                        _gameObject.transform.Find("Accept").gameObject.SetActive(false);
                        _gameObject.transform.Find("Reject").gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                foreach(KeyValuePair<string, Trade> keyValuePair in trade_log)
                {
                    if (!keyValuePair.Value.GetToOther())
                    {
                        GameObject _gameObject = Instantiate(NewTradeBluePrint);
                        _gameObject.transform.SetParent(trades.content, false);
                        _gameObject.transform.Find("Sender").GetComponent<TMP_Text>().text = "FROM: " + clients[keyValuePair.Value.GetSenderOrReceiver()].PlayerObject.GetComponent<Player>().GetName();
                        _gameObject.transform.Find("Offer").GetComponent<TMP_Text>().text = "OFFFERS" + keyValuePair.Value.OffersStringify();
                        _gameObject.transform.Find("Want").GetComponent<TMP_Text>().text = "WANTS" + keyValuePair.Value.DemandsStringify();
                        _gameObject.transform.Find("Description").Find("Text").GetComponent<TMP_Text>().text = keyValuePair.Value.GetDemands().ToSafeString();
                        _gameObject.transform.Find("Description").gameObject.SetActive(false);
                        string id = keyValuePair.Value.GetTradeId();
                        _gameObject.transform.Find("Accept").GetComponent<Button>().onClick.AddListener(()=>StartCoroutine(Accept(id)));
                        _gameObject.transform.Find("Reject").GetComponent<Button>().onClick.AddListener(()=>Remove(id));
                    }
                }
            }
        }
        else
        {
            Trade trade = trade_log[tradeid];
            GameObject _gameObject = Instantiate(NewTradeBluePrint);
            _gameObject.transform.SetParent(trades.content, false);
            if(mytrades) _gameObject.transform.Find("Sender").GetComponent<TMP_Text>().text = "TO: " + clients[trade.GetSenderOrReceiver()].PlayerObject.GetComponent<Player>().GetName();
            else _gameObject.transform.Find("Sender").GetComponent<TMP_Text>().text = "FROM: " + clients[trade.GetSenderOrReceiver()].PlayerObject.GetComponent<Player>().GetName();
            _gameObject.transform.Find("Offer").GetComponent<TMP_Text>().text = "OFFFERS" + trade.OffersStringify();
            _gameObject.transform.Find("Want").GetComponent<TMP_Text>().text = "WANTS" + trade.DemandsStringify();
            _gameObject.transform.Find("DescriptionButton").GetComponent<Button>().onClick.AddListener(()=>_gameObject.transform.Find("Description").gameObject.SetActive(!_gameObject.transform.Find("Description").gameObject.activeSelf));
            _gameObject.transform.Find("Description").GetComponentInChildren<TMP_Text>().text = trade.GetDesc().ToSafeString();
            _gameObject.transform.Find("Description").gameObject.SetActive(false);
            if (mytrades)
            {
                _gameObject.transform.Find("Accept").gameObject.SetActive(false);
                _gameObject.transform.Find("Reject").gameObject.SetActive(false);
            }
            else
            {
                _gameObject.transform.Find("Accept").GetComponent<Button>().onClick.AddListener(()=>Accept(tradeid));
                _gameObject.transform.Find("Reject").GetComponent<Button>().onClick.AddListener(()=>Remove(tradeid));
            }
        }
    }

    public IEnumerator Accept(String tradeid)
    {
        bool b1=true;
        Trade trade = trade_log[tradeid];
        foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.GetDemands())
        {
            if(player.GetQuantityOfResource(keyValuePair.Key) < keyValuePair.Value)
            {
                b1=false;
                break;
            }
        }
        if (!b1)
        {
            Debug.Log("Nemaš resurse");
            yield break;
        }
        acceptance_log.Add(tradeid, null);
        CheckResourceServerRpc(tradeid, trade.GetSenderOrReceiver(), false, false);
        yield return new WaitWhile(()=>acceptance_log[tradeid]==null);
        if (!acceptance_log[tradeid].Value)
        {
           Debug.Log("Drugi nema resurse");
        }
        else
        {
            Debug.Log("Trade odrađen");
            foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.GetDemands())
            {
                player.AddResource(keyValuePair.Key, -keyValuePair.Value);
            }
            foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.Getoffers())
            {
                player.AddResource(keyValuePair.Key, keyValuePair.Value);
            }
            ChangeResourceServerRpc(tradeid, trade.GetSenderOrReceiver());
            trade_log.Remove(tradeid);
            RemoveServerRpc(tradeid, trade.GetSenderOrReceiver());
            RefreshTrades("ffff", false);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void CheckResourceServerRpc(FixedString4096Bytes tradeId, ulong receiver, bool answer, bool canBeAccepted)
    {
        Debug.Log("U SERVER");
        CheckResourceClientRpc(tradeId, canBeAccepted, answer, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{receiver}}});
    }
    [ClientRpc()]
    public void CheckResourceClientRpc(FixedString4096Bytes tradeId, bool canBeAccepted, bool answer, ClientRpcParams clientRpcParams)
    {
        if (!answer)
        {
            Debug.Log("U CLIENT");
            bool pom=true;
            Trade trade = trade_log[tradeId.ToSafeString()];
            foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.Getoffers())
            {
                if(player.GetQuantityOfResource(keyValuePair.Key) < keyValuePair.Value)
                {
                    pom=false;
                    break;
                }
            }
            CheckResourceServerRpc(tradeId, trade.GetSenderOrReceiver(), true, pom);
        }
        else
        {
            acceptance_log[tradeId.ToSafeString()] = canBeAccepted;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void ChangeResourceServerRpc(FixedString4096Bytes tradeId, ulong receiver)
    {
        ChangeResourceClientRpc(tradeId, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){receiver}}});
    }
    [ClientRpc()]
    public void ChangeResourceClientRpc(FixedString4096Bytes tradeId, ClientRpcParams clientRpcParams)
    {
        Trade trade = trade_log[tradeId.ToSafeString()];
        foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.Getoffers())
        {
            player.AddResource(keyValuePair.Key, -keyValuePair.Value);
        }
        foreach(KeyValuePair<ResourceType, int> keyValuePair in trade.GetDemands())
        {
            player.AddResource(keyValuePair.Key, keyValuePair.Value);
        }
        trade_log.Remove(tradeId.ToSafeString());
    }

    public void Remove(String tradeId)
    {
        ulong receiver = trade_log[tradeId].GetSenderOrReceiver();
        RemoveServerRpc(tradeId, receiver);
        trade_log.Remove(tradeId);
        RefreshTrades("ffff", false);
    }

    [ServerRpc(RequireOwnership =false)]
    public void RemoveServerRpc(FixedString4096Bytes tradeId, ulong receiver)
    {
        RemoveClientRpc(tradeId, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){receiver}}});
    }
    [ClientRpc()]
    public void RemoveClientRpc(FixedString4096Bytes tradeid, ClientRpcParams clientRpcParams)
    {
        trade_log.Remove(tradeid.ToSafeString());
        RefreshTrades("ffff", true);
    }
}
