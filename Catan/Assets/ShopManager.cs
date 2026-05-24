using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private Button ShopButton, BuyButton;
    private int[] res = new int[5]{0, 0, 0, 0, 0};
    private Player player;
    void Start()
    {
        for(int i=0; i<ShopPanel.transform.childCount; i++)
        {
            Button[] buttons = ShopPanel.transform.GetChild(i).GetComponentsInChildren<Button>();
            for(int j=0; j<buttons.Length; j++)
            {
                Button btn = buttons[j];
                if(btn.Equals(BuyButton)) continue;
                btn.onClick.AddListener(() => ChangeQuantity(btn));
            }
        }
        ShopPanel.SetActive(false);
        ShopButton.onClick.AddListener(()=>ShopPanel.SetActive(!ShopPanel.activeSelf));
        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        BuyButton.onClick.AddListener(()=>Buy());
    }

    void Update()
    {
        
    }

    private void ChangeQuantity(Button buttonClicked)
    {
        TMP_InputField quantity = buttonClicked.transform.parent.Find("Quantity").GetComponent<TMP_InputField>();
        int num;
        if (buttonClicked.name.Equals("More"))
        {
            int.TryParse(quantity.text, out num);
            num++;
            quantity.text = num.ToString();
        }
        else
        {
            int .TryParse(quantity.text, out num);
            if(num<=0) return;
            num--;
            quantity.text = num.ToString();
        }
        DetermineTotalCost();
    }

    private void DetermineTotalCost()
    {
        int[] resources = new int[5] {0, 0, 0, 0, 0};
        int quan;
        String s;
        for(int i=0; i<ShopPanel.transform.childCount; i++)
        {
            GameObject gameObject = ShopPanel.transform.GetChild(i).gameObject;
            if(!(gameObject.name.Equals("Road") || gameObject.name.Equals("Village") || gameObject.name.Equals("Town"))) continue;
            s = gameObject.transform.Find("Quantity").GetComponent<TMP_InputField>().text;
            int.TryParse(s, out quan);
            if (gameObject.name.Equals("Road"))
            {
                resources[2] += quan;
                resources[3] += quan;
            }else if (gameObject.name.Equals("Village"))
            {
                resources[0] +=quan;
                resources[1] +=quan;
                resources[2] +=quan;
                resources[3] += quan;
            }else if (gameObject.name.Equals("Town"))
            {
                resources[0] += quan*2;
                resources[4] += quan*3;
            }
        }
        GameObject totalcost = ShopPanel.transform.Find("TotalCost").gameObject;
        for(int i=0; i<totalcost.transform.childCount; i++)
        {
            GameObject child = totalcost.transform.GetChild(i).gameObject;
            if(child.name.Equals("Wheat")) child.GetComponent<TMP_Text>().text = resources[0].ToString() + "X";
            else if(child.name.Equals("Sheep")) child.GetComponent<TMP_Text>().text = resources[1].ToString() + "X";
            else if(child.name.Equals("Wood")) child.GetComponent<TMP_Text>().text = resources[2].ToString() + "X";
            else if(child.name.Equals("Brick")) child.GetComponent<TMP_Text>().text = resources[3].ToString() + "X";
            else if(child.name.Equals("Ore")) child.GetComponent<TMP_Text>().text = resources[4].ToString() + "X";
        }
        res = resources;
    }

    private void Buy()
    {
        if(res.All(v=>v==0)) return;
        if(res[0] > player.GetQuantityOfResource(ResourceType.Wheat)) return;
        if(res[1] > player.GetQuantityOfResource(ResourceType.Sheep)) return;
        if(res[2] > player.GetQuantityOfResource(ResourceType.Forest)) return;
        if(res[3] > player.GetQuantityOfResource(ResourceType.Brick)) return;
        if(res[4] > player.GetQuantityOfResource(ResourceType.Ore)) return;

        int n;
        TMP_InputField tMP_InputField;
        for(int i=0; i<ShopPanel.transform.childCount; i++)
        {
            if(tMP_InputField = ShopPanel.transform.GetChild(i).GetComponentInChildren<TMP_InputField>()){
                int.TryParse(tMP_InputField.text, out n);
                player.AddStructure(ShopPanel.transform.GetChild(i).name, n);
            }
        }
        player.AddResource(ResourceType.Wheat, -res[0]);
        player.AddResource(ResourceType.Sheep, -res[1]);
        player.AddResource(ResourceType.Forest, -res[2]);
        player.AddResource(ResourceType.Brick, -res[3]);
        player.AddResource(ResourceType.Ore, -res[4]);
    }

}
