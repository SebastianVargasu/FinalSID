using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class GameUserInfo : MonoBehaviour
{
    public static GameUserInfo Instance { get; private set; }
    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private DatabaseReference onlineStatusRef;
    private string userId;

    private void Awake() {
        if(Instance != null){
            Destroy(this);
        }else{
            Instance = this;
        }
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

    }

    public void UserLoggedIn()
    {
        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            onlineStatusRef = databaseRef.Child("players").Child(userId).Child("isOnline");

            // Llama a la función para gestionar la presencia del usuario
            MonitorConnectionStatus();
        }
    }

    public void Logout()
    {
        UpdateOnlineStatus(false);
        auth.SignOut();

        Debug.Log("Sesión cerrada correctamente.");
    }

    public void SavePlayerData(string username)
    {
        DatabaseReference playerRef = databaseRef.Child("players").Child(userId);

        var playerData = new Dictionary<string, object>
        {
            { "username", username },
            { "friends", new Dictionary<string, bool>() },
            { "matchCount", 0 },
            { "isOnline", true }
        };

        playerRef.UpdateChildrenAsync(playerData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Datos generados correctamente");
            }
            else
            {
                Debug.LogError("Error al guardar los datos: " + task.Exception);
            }
        });
    }

    public void UpdateMatchCount(int newMatchCount)
    {
        DatabaseReference matchCountRef = databaseRef.Child("players").Child(userId).Child("matchCount");

        matchCountRef.SetValueAsync(newMatchCount).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Número de partidas actualizado");
            }
        });
    }

    public void UpdateOnlineStatus(bool isOnline)
    {
        onlineStatusRef.SetValueAsync(isOnline).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Conexión actualizada");
            }
        });
    }

    public void AddFriend(string friendId)
    {
        DatabaseReference friendsRef = databaseRef.Child("players").Child(userId).Child("friends").Child(friendId);

        friendsRef.SetValueAsync(true).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Contacto agregado");
            }
        });
    }

    private void MonitorConnectionStatus()
    {
        // Escucha el estado de conexión de Firebase
        DatabaseReference connectedRef = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");

        connectedRef.ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError == null && args.Snapshot.Exists && args.Snapshot.Value is bool isConnected && isConnected)
            {
                // Si está conectado, establece el estado a true
                onlineStatusRef.OnDisconnect().SetValue(false); // Se marcará como desconectado al perder la conexión
                UpdateOnlineStatus(true);
            }
        };
    }



    private void OnApplicationQuit()
    {
        UpdateOnlineStatus(false);
    }
}
