using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalNetworkAPI.Networking;
using Unity.Netcode;

namespace LethalNetworkAPI;

public class LethalNetworkMessage<T>
{
    #region Public Constructors
    
    /// <summary>
    /// Create a new network message of an <a href="https://www.newtonsoft.com/json/help/html/SerializationGuide.htm">allowed type.</a>
    /// </summary>
    /// <param name="guid">An identifier for the message. GUIDs are specific to a per-mod basis.</param>
    /// <example><code> customStringMessage = new LethalNetworkMessage&lt;string&gt;(guid: "customStringMessageGuid");</code></example>
    public LethalNetworkMessage(string guid)
    {
        _messageGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{guid}";
        NetworkHandler.OnMessage += ReceiveMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with guid \"{_messageGuid}\" has been created.");
#endif
    }
    
    #endregion

    
    
    #region Public Methods and Event
    
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    public void SendServer(T data)
    {
        Parser.SendServerMessageOfValidType(_messageGuid, new []{data});
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
    }
    
    /// <summary>
    /// Send collection of data to the server/host.
    /// </summary>
    /// <param name="data">The collection of data of type <typeparamref name="T"/> to send.</param>
    public void SendServer(T[] data)
    {
        Parser.SendServerMessageOfValidType(_messageGuid, data.ToArray());
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
    }
    
    

    /// <summary>
    /// Send data to a specific client.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientId">The client to send the data to.</param>
    public void SendClient(T data, ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        
        Parser.SendClientMessageOfValidType(_messageGuid, new []{data}, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { clientId } } } );
    }
    
    /// <summary>
    /// Send collection of data to a specific client.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientId">The client to send the data to.</param>
    public void SendClient(IEnumerable<T> data, ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        
        Parser.SendClientMessageOfValidType(_messageGuid, data.ToArray(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { clientId } } } );
    }
    
    
    
    /// <summary>
    /// Send data to specific clients.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientIds">The clients to send the data to.</param>
    public void SendClients(T data, IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var allowedClientIds = clientIds.Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray();
        
        if (!allowedClientIds.Any()) return;
        
        Parser.SendClientMessageOfValidType(_messageGuid, new []{data}, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = allowedClientIds } } );
    }
    
    /// <summary>
    /// Send collection of data to specific clients.
    /// </summary>
    /// <param name="data">The collection of data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientIds">The clients to send the data to.</param>
    public void SendClients(IEnumerable<T> data, IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var allowedClientIds = clientIds.Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray();
        
        if (!allowedClientIds.Any()) return;
        
        Parser.SendClientMessageOfValidType(_messageGuid, data.ToArray(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = allowedClientIds } } );
    }
    
    
    
    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="receiveOnHost">Whether the host client should receive as well. Only set to <c>false</c> when absolutely necessary</param>
    public void SendAllClients(T data, bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        
        if (receiveOnHost)
            Parser.SendClientMessageOfValidType(_messageGuid, new []{data});
        else
        {
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray();

            if (!clientIds.Any()) return;
            
            Parser.SendClientMessageOfValidType(_messageGuid, new []{data}, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {data}");
#endif
    }

    /// <summary>
    /// Send collection of data to all clients.
    /// </summary>
    /// <param name="data">The collection of data of type <typeparamref name="T"/> to send.</param>
    /// <param name="receiveOnHost">Whether the host client should receive as well. Only set to <c>false</c> when absolutely necessary</param>
    public void SendAllClients(IEnumerable<T> data, bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        
        if (receiveOnHost)
            Parser.SendClientMessageOfValidType(_messageGuid, data.ToArray());
        else
        {
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray();

            if (!clientIds.Any()) return;
            
            Parser.SendClientMessageOfValidType(_messageGuid, data.ToArray(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {data}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when a message is received by the server.
    /// </summary>
    /// <example><code>customStringMessage.OnServerReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod(string data)</code></example>
    public event Action<T> OnServerReceived;
    
    
    /// <summary>
    /// The callback to invoke when a message is received by the client.
    /// </summary>
    /// <example><code>customStringMessage.OnClientReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod(string data)</code></example>
    public event Action<T> OnClientReceived;

    #endregion

    private void ReceiveMessage(string guid, object data, bool isServerMessage)
    {
        if (guid != _messageGuid) return;

        if (isServerMessage)
            OnServerReceived?.Invoke((T)data);
        else
            OnClientReceived?.Invoke((T)data);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }

    private readonly string _messageGuid;
}