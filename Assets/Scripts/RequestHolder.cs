using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class RequestHolder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameField;

    string _friendId;

    public void SetUsername(string username, string friendId)
    {
        usernameField.text = username;
        _friendId = friendId;
    }


    public void AcceptRequest()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference playersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players");

        // Agregar a la lista de amigos del jugador
        DatabaseReference userFriendsRef = playersRef.Child(userId).Child("friends").Child(_friendId);
        userFriendsRef.SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("amigo agregado");
            }
            else
            {
                Debug.LogError("Error al agregar amigo");
            }
        });

        // Agregar al amigo a la lista de amigos del destinatario
        DatabaseReference friendFriendsRef = playersRef.Child(_friendId).Child("friends").Child(userId);
        friendFriendsRef.SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("amigo agregado en remitente");
            }
            else
            {
                Debug.LogError("Error al agregar amigo en remitente");
            }
        });

        // Eliminar la solicitud pendiente
        DatabaseReference sentRequestRef = playersRef.Child(userId).Child("receivedFriendRequests").Child(_friendId);
        sentRequestRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Solicitud eliminada");
            }
            else
            {
                Debug.LogError("Error eliminar la solicitud");
            }
        });

        DatabaseReference receivedRequestRef = playersRef.Child(_friendId).Child("sentFriendRequests").Child(userId);
        receivedRequestRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Solicitud eliminada en el remitente");
            }
            else
            {
                Debug.LogError("Error eliminar la solicitud en el remitente");
            }
        });

        Debug.Log("Solicitud de amistad aceptada");
        Destroy(gameObject);
    }

    public void RejectFriendRequest()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference playersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players");

        // Eliminar la solicitud pendiente
        DatabaseReference sentRequestRef = playersRef.Child(userId).Child("receivedFriendRequests").Child(_friendId);
        sentRequestRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Solicitud eliminada");
            }
            else
            {
                Debug.LogError("Error eliminar la solicitud");
            }
        });

        DatabaseReference receivedRequestRef = playersRef.Child(_friendId).Child("sentFriendRequests").Child(userId);
        receivedRequestRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Solicitud eliminada en el remitente");
            }
            else
            {
                Debug.LogError("Error eliminar la solicitud en el remitente");
            }
        });

        Debug.Log("Solicitud de amistad rechazada");
        Destroy(gameObject);
    }



}
