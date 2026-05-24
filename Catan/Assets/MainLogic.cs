using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class MainLogic : NetworkBehaviour
{
    private const float sizeOfTile = 2.9f;
    private bool blueNumRead = false, redNumRead=false;
    private static bool firstturnover=false, secondturnover=false;
    private NetworkVariable<bool> numbersRead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> rolled7 = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> personOnTurn = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private GameObject tile, town, road, village, DiceBlue, DiceRed, PlayerPanel, Robber, MarketManager;
    [SerializeField] private Button TownButton, RoadButton, VillageButton, RollDice, ShowPlayers, EndTurn, Restart;
    [SerializeField] private TMP_Text yourTurn, youWIn, WheatNum, SheepNum, WoodNum, BrickNum, OreNum;
    [SerializeField] private ScrollRect scrollRect;
    private InputActionCam  inputActions;
    private Dictionary<ResourceType, int> resourceNum = new Dictionary<ResourceType, int>();
    private Dictionary<int, int> TileNumbers = new Dictionary<int, int>();
    private int gameSize, NumSum = 0;
    private static Dictionary<Vector2, StructCollider> structcolliders = new Dictionary<Vector2, StructCollider>();
    private static Dictionary<Vector2, RoadCollider> roadcolliders = new Dictionary<Vector2, RoadCollider>();
    //private static Dictionary<int, List<Structure>> tileNumber_Structure = new Dictionary<int, List<Structure>>();
    private static Dictionary<int, List<Tile>> number_Tile = new Dictionary<int, List<Tile>>();
    private List<ulong> ids;
    private Dice diceBlue, diceRed;
    private Player player;
    private Tile DessertTile;
    private Robber robber;
    void Start()
    {
        ids = NetworkManager.ConnectedClientsIds.ToList<ulong>();
        gameSize = CreateGameLogic.GetGameSize();
        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        player.OnStructNumChanged+=ChangeStructNum;
        personOnTurn.OnValueChanged += RefreshMarketClientRpc;
        TownButton.onClick.AddListener(()=>{if(player.GetQuantityOfStruct("Town")>0)CreateObjectServerRpc(0);});
        VillageButton.onClick.AddListener(()=>{if(player.GetQuantityOfStruct("Village")>0)CreateObjectServerRpc(1);});
        RoadButton.onClick.AddListener(()=>{if(player.GetQuantityOfStruct("Road")>0)CreateObjectServerRpc(2);});
        ShowPlayers.onClick.AddListener(()=>scrollRect.gameObject.SetActive(!scrollRect.gameObject.activeSelf));
        EndTurn.onClick.AddListener(()=>EndMyTurn());
        CreateScrollView();
        scrollRect.gameObject.SetActive(false);
        TownButton.gameObject.SetActive(false);
        RoadButton.gameObject.SetActive(false); 
        VillageButton.gameObject.SetActive(false);
        EndTurn.gameObject.SetActive(false);
        if(IsHost){
            GetGameSizeClientRpc(gameSize);
            resourceNum.Add(ResourceType.Wheat, gameSize*2);
            resourceNum.Add(ResourceType.Sheep, gameSize*2);
            resourceNum.Add(ResourceType.Forest, gameSize*2);
            resourceNum.Add(ResourceType.Ore, gameSize*2-1);
            resourceNum.Add(ResourceType.Brick, gameSize*2-1);
            resourceNum.Add(ResourceType.Dessert, gameSize-1);
            Restart.onClick.AddListener(()=>RestartGame());

            for(int i=2; i<=12; i++)
            {
                if(i==2 || i==7 || i==12) TileNumbers.Add(i, gameSize-1);
                else TileNumbers.Add(i, gameSize);
            }

            CreateBoard();
            AddRoadNeighboursGlobal();
            CreateDice();
            CreateRobber();
            StartTurnServerRpc();
        }
        diceBlue = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.First(v => v.CompareTag("BlueDice")).GetComponent<Dice>();
        diceRed = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.First(v => v.CompareTag("RedDice")).GetComponent<Dice>();
        robber = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.First(v =>v.CompareTag("Robber")).GetComponent<Robber>();
    }

    void Update()
    {
        
    }

    public void CreateBoard()
    {
        if(!IsServer) return;
        float lastX, lastY;
        lastX = - sizeOfTile * gameSize;
        lastY = (float)(3 * sizeOfTile * Math.Sqrt(3) / 2);
        float Y;
        int k = 3, randomNumber;
        for(int i=0; i<=gameSize; i++)
        {
            for(int j=0; j<k; j++)
            {
                Y = lastY - (float)(j * sizeOfTile * Math.Sqrt(3));
                GameObject _gameObject = Instantiate(tile, new Vector3(lastX, Y, 0), gameObject.transform.rotation);
                Transform transform = _gameObject.transform;
                transform.GetComponent<NetworkObject>().Spawn(true);
                Tile tile_ = (Tile) _gameObject.GetComponent<Tile>();
                ResourceType resourceType = RandomResource();
                tile_.SetresourceType(resourceType);
                if(resourceType == ResourceType.Dessert) DessertTile = tile_;
                randomNumber = RandomTileNumber(resourceType);
                tile_.SetNumber(randomNumber);
                if(number_Tile.ContainsKey(randomNumber)) {number_Tile[randomNumber].Add(tile_);}
                else {number_Tile.Add(randomNumber, new List<Tile>(){tile_});}
            }
            lastX += 3*sizeOfTile/2;
            lastY+=(float)(sizeOfTile*Math.Sqrt(3)/2);
            k++;
        }
        lastY -= (float)(sizeOfTile * Math.Sqrt(3));
        k-=2;
        for(int i=0; i<gameSize; i++)
        {
            for(int j=0; j<k; j++)
            {
                Y = lastY - (float)(j * sizeOfTile * Math.Sqrt(3));
                GameObject _gameObject = Instantiate(tile, new Vector3(lastX, Y, 0), gameObject.transform.rotation);
                Transform transform = _gameObject.transform;
                transform.GetComponent<NetworkObject>().Spawn(true);
                Tile tile_ = (Tile) _gameObject.GetComponent<Tile>();
                ResourceType resourceType = RandomResource();
                tile_.SetresourceType(resourceType);
                if(resourceType == ResourceType.Dessert) DessertTile = tile_;
                randomNumber = RandomTileNumber(resourceType);
                tile_.SetNumber(randomNumber);
                if(number_Tile.ContainsKey(randomNumber)) number_Tile[randomNumber].Add(tile_);
                else number_Tile.Add(randomNumber, new List<Tile>(){tile_});
            }
            lastX += 3*sizeOfTile/2;
            lastY-=(float)(sizeOfTile*Math.Sqrt(3)/2);
            k--;
        } 
    }

    private void CreateDice()
    {
        if(!IsHost) return;
        GameObject diceBlue = Instantiate(DiceBlue, new Vector3(10, 10, -20), gameObject.transform.rotation);
        Transform transformBlue = diceBlue.transform;
        transformBlue.GetComponent<NetworkObject>().Spawn(true);
        GameObject diceRed = Instantiate(DiceRed, new Vector3(-10, -10, -20), gameObject.transform.rotation);
        Transform transformRed = diceRed.transform;
        transformRed.GetComponent<NetworkObject>().Spawn(true);
    }

    private void CreateRobber()
    {
        GameObject _gameObject = Instantiate(Robber, DessertTile.gameObject.transform.position, gameObject.transform.rotation);
        Transform transform = _gameObject.transform;
        transform.GetComponent<NetworkObject>().Spawn(true);
        DessertTile.GetRobberCollider().GetComponent<RobberCollider>().SetIsOccupied(true);
        _gameObject.GetComponent<Robber>().SetLastInteractedColl(DessertTile.GetRobberCollider().GetComponent<RobberCollider>());
    }

    private ResourceType RandomResource()
    {
        List<ResourceType> list = resourceNum.Keys.ToList<ResourceType>();
        if(resourceNum.Values.All(v => v==0)) return list[UnityEngine.Random.Range(0, list.Count-1)];
        int r=-1, a=0;
        while(a == 0)
        {
            r = UnityEngine.Random.Range(0, list.Count);
            a = resourceNum[list[r]];
        }
        resourceNum[list[r]]--;
        return list[r];
    }

    private int RandomTileNumber(ResourceType resourceType)
    {
        int num=0;
        if(resourceType == ResourceType.Dessert)
        {
            TileNumbers[7]--;
            return 7;
        }
        if(TileNumbers.Values.All(v => v == 0))
        {
            
            while(num==0 || num == 7)
            {
                num = UnityEngine.Random.Range(2, 13);
            }
            return num;
        }

        int r=0;

        while(r==7 || num == 0)
        {
            r = UnityEngine.Random.Range(2, 13);
            num = TileNumbers[r];
        }
        TileNumbers[r]--;
        return r;
    }

    public static Dictionary<Vector2, StructCollider> GetSColl()
    {
        return structcolliders;
    }

    public static Dictionary<Vector2, RoadCollider> GetRColl()
    {
        return roadcolliders;
    }

    public static bool IsFirstSecondTurnOver()
    {
        return firstturnover && secondturnover;
    }

    public int GetPersonOnTurn()
    {
        return personOnTurn.Value;
    }
    public ulong GetPersonOnTurnId()
    {
        return ids[personOnTurn.Value];
    }
    private void ChangeStructNum(int n, string structure)
    {
        if(structure.Equals("Road")) RoadButton.GetComponentInChildren<TMP_Text>().text = "Road\n" + n.ToString();
        else if(structure.Equals("Village")) VillageButton.GetComponentInChildren<TMP_Text>().text = "Village\n" + n.ToString();
        else if(structure.Equals("Town")) TownButton.GetComponentInChildren<TMP_Text>().text = "Town\n" + n.ToString();
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetTileNumServerRpc(int n, string diceName, bool state)
    {
        if(!IsServer) return;
        Debug.Log("U metodi");
        if (diceName.ToLower().Contains("blue"))
        {
            Debug.Log("plavo");
            blueNumRead=state;
        }
        else if (diceName.ToLower().Contains("red"))
        {
            Debug.Log("crveno");
            redNumRead=state;
        }
        NumSum+=n;
        if(blueNumRead && redNumRead)
        {
            if(NumSum >5 && NumSum < 9)
            {
                rolled7.Value = true;
                robber.SetCanBeMovedServerRpc(true);
            }
            else{
            foreach(Tile tile in number_Tile[NumSum])
            {
                if(tile.GetRobberCollider().GetComponent<RobberCollider>().IsOccupied()) continue;
                foreach(StructCollider structCollider in tile.GetStructColliders())
                {
                    if (structCollider.IsOccupied())
                    {
                        Structure structure = structCollider.GetOccupier();
                        SendMaterialsClientRpc(tile.GetResourceType(), (structure.GetStructIndex()==1)?1:2, 
                            new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){structure.OwnerClientId}}});
                        
                    }
                }
            }
            }
            numbersRead.Value=true;
            redNumRead = false;
            blueNumRead = false;
            NumSum = 0;
        }
    }

    [ClientRpc()]
    public void SendMaterialsClientRpc(ResourceType resourceType, int quantity, ClientRpcParams clientRpcParams)
    {
        player.AddResource(resourceType, quantity);
    }
     [ClientRpc()]
     public void GetGameSizeClientRpc(int gamesize)
    {
        this.gameSize = gamesize;
    }
    //TO DO
    [ServerRpc(RequireOwnership =false)]
    public void CreateObjectServerRpc(int objectTag, ServerRpcParams serverRpcParams=default)
    {
        if(objectTag == 0)
        {
            GameObject _gameObject = Instantiate(town);
            Transform transform = _gameObject.transform;
            transform.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        }
        else if (objectTag == 1)
        {
            GameObject _gameObject = Instantiate(village);
            Transform transform = _gameObject.transform;
            transform.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        }
        else if (objectTag == 2)
        {
            GameObject _gameobject = Instantiate(road);
            Transform transform = _gameobject.transform;
            transform.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetNumberReadServerRpc(bool state)
    {
        numbersRead.Value = false;
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetRolled7ServerRpc(bool state)
    {
        rolled7.Value = state;
    }

    private void AddRoadNeighboursGlobal()
    {
        if(!IsHost) return;
        foreach(RoadCollider rc in MainLogic.GetRColl().Values)
        {
            foreach(StructCollider sc in rc.GetStructColliders())
            {
                foreach(RoadCollider _rc in sc.GetRoadNeighbours())
                {
                    if(_rc != rc) rc.AddNeighbour(_rc);
                }
            }
        }
    }

    public void CreateScrollView()
    {
        IReadOnlyDictionary<ulong, NetworkClient> players = NetworkManager.ConnectedClients;
        foreach(ulong id in ids)
        {
            NetworkClient player = players[id];
            GameObject player_panel = Instantiate(PlayerPanel);
            player_panel.transform.SetParent(scrollRect.content, false);
            player_panel.transform.Find("PlayerName").GetComponent<TMP_Text>().text = player.PlayerObject.GetComponent<Player>().GetName();
            Color color;
            if(UnityEngine.ColorUtility.TryParseHtmlString(player.PlayerObject.GetComponent<Player>().GetColorOnline(), out color)){
                player_panel.transform.Find("PlayerColor").GetComponent<Image>().color = color;
            }
        }
    }

    [ClientRpc()]
    public void StartMyTurnClientRpc(bool firstturn, ClientRpcParams clientRpcParams)
    {
        if(firstturn) StartCoroutine(FirstTurn());
        else StartCoroutine(OneTurn());
    }

    [ServerRpc(RequireOwnership =false)]
    public void StartTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!firstturnover)
        {
            if(serverRpcParams.Receive.SenderClientId != ids.Last())
            {
                personOnTurn.Value = (personOnTurn.Value+1)%ids.Count;
                StartMyTurnClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{ids[personOnTurn.Value]}}});
                return;
            }
            else
            {
                firstturnover=true;
                ids.Reverse();
            }
        }
        if (!secondturnover)
        {
            if(serverRpcParams.Receive.SenderClientId != ids.Last())
            {
                personOnTurn.Value = (personOnTurn.Value+1)%ids.Count;
                StartMyTurnClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{ids[personOnTurn.Value]}}});
                return;
            }
            else
            {
                secondturnover=true;
                ids.Reverse();
            }
        }
        personOnTurn.Value = (personOnTurn.Value+1)%ids.Count;
        StartMyTurnClientRpc(false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{ids[personOnTurn.Value]}}});
    }

    public IEnumerator OneTurn()
    {
        yourTurn.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yourTurn.gameObject.SetActive(false);

        RollDice.gameObject.SetActive(true);
        RollDice.enabled=true;
        yield return new WaitUntil(()=>numbersRead.Value);
        
        SetNumberReadServerRpc(false);
        RollDice.gameObject.SetActive(false);

        yield return new WaitForSeconds(2);
        diceBlue.SetActiveServerRpc(false);
        diceRed.SetActiveServerRpc(false);

        if (rolled7.Value)
        {
            Debug.Log("OVDJE");
            yield return new WaitUntil(()=>!robber.GetCanBeMoved());
            SetRolled7ServerRpc(false);
        }

        TownButton.gameObject.SetActive(true);
        RoadButton.gameObject.SetActive(true); 
        VillageButton.gameObject.SetActive(true);
        EndTurn.gameObject.SetActive(true);
    }

    public IEnumerator FirstTurn()
    {
        player.AddStructure("Road", 2);
        player.AddStructure("Village", 1);
        yourTurn.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yourTurn.gameObject.SetActive(false);

        TownButton.gameObject.SetActive(true);
        RoadButton.gameObject.SetActive(true); 
        VillageButton.gameObject.SetActive(true);
        EndTurn.gameObject.SetActive(true);
    }

    public void EndMyTurn()
    {
        if(player.GetQuantityOfStruct("Village")>0 || player.GetQuantityOfStruct("Town") > 0 || player.GetQuantityOfStruct("Road")>0) return;
        if (player.GetPoints() > (4 * gameSize + 2))
        {
            youWIn.gameObject.SetActive(true);
            SetActiveRestartServerRpc(true);
        } 
        TownButton.gameObject.SetActive(false);
        RoadButton.gameObject.SetActive(false); 
        VillageButton.gameObject.SetActive(false);
        EndTurn.gameObject.SetActive(false);
        StartTurnServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetActiveRestartServerRpc(bool state)
    {
        Restart.gameObject.SetActive(true);
    } 

    private void RestartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [ClientRpc()]
    public void RefreshMarketClientRpc(int previous, int New)
    {
        MarketManager marketManager = MarketManager.GetComponent<MarketManager>();
        marketManager.RefreshReceiver();
    }

    [ContextMenu("pRINT")]
    public void Print()
    {
        
    }

}