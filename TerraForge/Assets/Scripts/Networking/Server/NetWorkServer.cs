using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetWorkServer : IDisposable
{
    private NetworkManager networkManager;

    private Dictionary<ulong, string> clientIdAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdtoUserData = new Dictionary<string, UserData>();

    public NetWorkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
        
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        this.networkManager.OnServerStarted += OnNetworkReady;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdAuth.TryGetValue(clientId, out string authId))
        {
            clientIdAuth.Remove(clientId);
            authIdtoUserData.Remove(authId);
        }
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdtoUserData[userData.userAuthId] = userData;
        Debug.Log(userData.userName);

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public void Dispose()
    {
        if (networkManager == null) { return; }

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
