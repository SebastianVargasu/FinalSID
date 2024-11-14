using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using TMPro;

public class SendFriendRequest : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameField;

    public void Close(){
        gameObject.SetActive(false);
    }

    public async void Send(){
        await SendRequest();
    }

    public async Task SendRequest()
    {
        if(userNameField.text.Length == 0){
            Debug.LogError("Es necesario ingresar un username");
            return;
        }

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference playersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players");

        // Buscar al usuario por su username
        Query friendQuery = playersRef.OrderByChild("username").EqualTo(userNameField.text).LimitToFirst(1);
        
        try
        {
            // Obtener el primer DataSnapshot de la consulta
            DataSnapshot snapshot = (await friendQuery.GetValueAsync()).Children.FirstOrDefault();
            
            if (snapshot != null)
            {
                string friendId = snapshot.Key; // El ID del amigo
                string friendUserName = snapshot.Child("username").Value.ToString(); // El username del amigo

                // Verificar si ya son amigos
                DatabaseReference userFriendsRef = playersRef.Child(userId).Child("friends");
                DataSnapshot userFriendSnapshot = (await userFriendsRef.Child(friendId).GetValueAsync());
                
                if (userFriendSnapshot.Exists)
                {
                    // Si ya son amigos, no enviar la solicitud
                    Debug.Log("Ya eres amigo de " + friendUserName);
                    return;
                }

                // Verificar si ya existe una solicitud pendiente
                DatabaseReference sentRequestRef = playersRef.Child(userId).Child("sentFriendRequests").Child(friendId);
                DataSnapshot sentRequestSnapshot = (await sentRequestRef.GetValueAsync());

                if (sentRequestSnapshot.Exists)
                {
                    Debug.Log("Ya has enviado una solicitud a este usuario.");
                    return; // Si la solicitud ya fue enviada, no hacer nada más
                }

                // Si no hay solicitud pendiente, crear la solicitud
                // Enviar solicitud desde el remitente
                await sentRequestRef.SetValueAsync(new Dictionary<string, object>
                {
                    { "username", friendUserName },
                    { "status", "pending" }
                });

                // Recibir solicitud en el destinatario
                DatabaseReference receivedRequestRef = playersRef.Child(friendId).Child("receivedFriendRequests").Child(userId);
                await receivedRequestRef.SetValueAsync(new Dictionary<string, object>
                {
                    { "username", FirebaseAuth.DefaultInstance.CurrentUser.DisplayName },
                    { "status", "pending" }
                });

                Debug.Log("Solicitud de amistad enviada a " + friendUserName);
            }
            else
            {
                Debug.LogError("No se encontró el usuario con ese username");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al enviar la solicitud: " + ex.Message);
        }
    }
}
