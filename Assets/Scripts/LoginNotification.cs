using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class LoginNotification : MonoBehaviour
{
    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    private Dictionary<string, bool> friendStatusCache = new Dictionary<string, bool>();
    private HashSet<string> friendsBeingListened = new HashSet<string>();

    [SerializeField] private NotificationHolder notificationPrefab; // Prefab para la notificación
    [SerializeField] private Transform notificationHolder;   // Contenedor de las notificaciones


    public void OnLogin()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != null)
        {
            ListenToFriendsStatus();
        }
        else
        {
            Debug.Log("No hay usuario logueado");
        }
    }

    private void ListenToFriendsStatus()
    {
        string userId = auth.CurrentUser.UserId;
        DatabaseReference friendsRef = databaseRef.Child("players").Child(userId).Child("friends");

        friendsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot friendsSnapshot = task.Result;
                foreach (DataSnapshot friend in friendsSnapshot.Children)
                {
                    string friendId = friend.Key;
                    if (!friendsBeingListened.Contains(friendId))
                    {
                        StartListeningToFriendStatus(friendId);
                        friendsBeingListened.Add(friendId);
                    }
                }
            }
            else
            {
                Debug.LogError("Error al obtener los amigos: " + task.Exception);
            }
        });
    }

    private void StartListeningToFriendStatus(string friendId)
    {
        DatabaseReference friendStatusRef = databaseRef.Child("players").Child(friendId).Child("isOnline");

        friendStatusRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Error al escuchar cambios de estado: " + args.DatabaseError.Message);
                return;
            }

            bool isOnline = bool.Parse(args.Snapshot.Value.ToString());

            // Verifica si el estado cambió de offline a online
            if (friendStatusCache.ContainsKey(friendId))
            {
                bool previousStatus = friendStatusCache[friendId];
                if (!previousStatus && isOnline)
                {
                    ShowNotification(friendId);
                }
            }
            else if (isOnline)
            {
                // Si el amigo no estaba en el caché y ahora está online, mostrar la notificación
                ShowNotification(friendId);
            }

            // Actualiza el estado del amigo en el caché
            friendStatusCache[friendId] = isOnline;
        };
    }

    private void ShowNotification(string friendId)
    {
        // Obtiene el nombre de usuario del amigo
        DatabaseReference friendUsernameRef = databaseRef.Child("players").Child(friendId).Child("username");

        friendUsernameRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                string friendUsername = task.Result.Value.ToString();

                // Instancia la notificación visual
                NotificationHolder notification = Instantiate(notificationPrefab, notificationHolder);
                notification.SetName(friendUsername);

                Debug.Log($"{friendUsername} se ha conectado");
            }
            else
            {
                Debug.LogError("Error al obtener el nombre de usuario del amigo: " + task.Exception);
            }
        });
    }
}
