using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class UnityRelay : MonoBehaviour
{
    private static RelayServerData relayServerData;
    private static string joinCode;
    private async void Start(){
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Ušao: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [ContextMenu("CreateRelay")]
    public static async void CreateRelay()
    {
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            relayServerData = allocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        } catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public static async Task JoinRelay(string joinCode)
    {
        try{
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            relayServerData = joinAllocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        } catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public static string GetJoinCode()
    {
        return joinCode;
    }
}
