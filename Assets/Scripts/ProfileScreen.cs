using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;

public class ProfileScreen : MonoBehaviour
{
    private DatabaseReference databaseRef;
    private FirebaseAuth auth;

    [SerializeField] private TextMeshProUGUI usernameField;
    [SerializeField] private TextMeshProUGUI matchCountField;
    [SerializeField] private TextMeshProUGUI contactsAmountField;

    [SerializeField] private Transform contactsHolder;
    [SerializeField] private ContactHolder usernameHolder;

    private Dictionary<string, GameObject> contactsGenerated = new Dictionary<string, GameObject>(); // Usamos un diccionario para evitar duplicados

    private void OnEnable()
    {
        // Limpiar la lista de contactos cada vez que se active la pantalla
        foreach (var contact in contactsGenerated.Values)
        {
            Destroy(contact);
        }
        contactsGenerated.Clear();

        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != null)
        {
            GetPlayerData();
            GetFriendsStatus();
            ListenForFriendOnlineStatusChanges();  // Escuchar cambios en el estado "isOnline"
        }
        else
        {
            Debug.Log("No hay usuario logueado");
        }
    }

    public void GetPlayerData()
    {
        string userId = auth.CurrentUser.UserId;
        DatabaseReference playerRef = databaseRef.Child("players").Child(userId);

        playerRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string username = snapshot.Child("username").Value.ToString();
                int matchCount = int.Parse(snapshot.Child("matchCount").Value.ToString());
                bool isOnline = bool.Parse(snapshot.Child("isOnline").Value.ToString());

                // Obtener los amigos y contar la cantidad de amigos
                DataSnapshot friendsSnapshot = snapshot.Child("friends");
                int friendCount = (int)friendsSnapshot.ChildrenCount;  // Cuenta los amigos

                usernameField.text = username;
                matchCountField.text = "Veces emparejado: " + matchCount.ToString();
                contactsAmountField.text = "N° de Contactos: " + friendCount.ToString();

                Debug.Log("Usuario: " + username + ", Emparejamientos: " + matchCount + ", Online: " + isOnline + ", Amigos: " + friendCount);
            }
            else
            {
                Debug.LogError("Error al obtener los datos del jugador: " + task.Exception);
            }
        });
    }

    public void GetFriendsStatus()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference friendsRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(userId).Child("friends");

        friendsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot friendsSnapshot = task.Result;
                foreach (DataSnapshot friend in friendsSnapshot.Children)
                {
                    string friendId = friend.Key;
                    GetFriendUsernameAndStatus(friendId);
                }
            }
        });
    }

    private void GetFriendUsernameAndStatus(string friendId)
    {
        DatabaseReference friendRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(friendId);

        friendRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot friendSnapshot = task.Result;

                // Obtener el username del amigo
                string friendUsername = friendSnapshot.Child("username").Value.ToString();

                // Verificar el estado en línea del amigo
                bool isOnline = bool.Parse(friendSnapshot.Child("isOnline").Value.ToString());

                // Solo mostramos amigos en línea
                if (isOnline)
                {
                    // Verificar si el amigo ya fue generado
                    if (!contactsGenerated.ContainsKey(friendId))
                    {
                        SpawnContact(friendUsername, friendId);
                    }
                }
                else
                {
                    // Si el amigo está fuera de línea y ya estaba generado, lo eliminamos
                    if (contactsGenerated.ContainsKey(friendId))
                    {
                        Destroy(contactsGenerated[friendId]);
                        contactsGenerated.Remove(friendId);
                    }
                }

                Debug.Log("Amigo: " + friendUsername + " (" + friendId + ") está " + (isOnline ? "en línea" : "fuera de línea"));
            }
            else
            {
                Debug.LogError("Error al obtener los datos del amigo: " + task.Exception);
            }
        });
    }

    private void SpawnContact(string username, string friendId)
    {
        ContactHolder h = Instantiate(usernameHolder, contactsHolder);
        h.SetContactName(username);
        contactsGenerated[friendId] = h.gameObject;  // Guardamos el contacto usando el ID del amigo como clave
    }

    // Escuchar cambios en el estado "isOnline" de los amigos
    private void ListenForFriendOnlineStatusChanges()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference friendsRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(userId).Child("friends");

        friendsRef.ChildAdded += (sender, args) =>
        {
            string friendId = args.Snapshot.Key;
            ListenToFriendOnlineStatus(friendId);
        };
    }

    // Escuchar cambios en el estado de "isOnline" de un amigo específico
    private void ListenToFriendOnlineStatus(string friendId)
    {
        DatabaseReference friendRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(friendId);

        friendRef.Child("isOnline").ValueChanged += (sender, args) =>
        {
            // Actualizamos el estado de "isOnline" solo para el amigo específico que cambió su estado
            DataSnapshot snapshot = args.Snapshot;
            bool isOnline = bool.Parse(snapshot.Value.ToString());

            // Si el amigo se pone en línea, lo generamos; si se pone fuera de línea, lo eliminamos
            if (isOnline)
            {
                if (!contactsGenerated.ContainsKey(friendId))
                {
                    GetFriendUsernameAndStatus(friendId);  // Regeneramos el contacto si está en línea
                }
            }
            else
            {
                if (contactsGenerated.ContainsKey(friendId))
                {
                    Destroy(contactsGenerated[friendId]);
                    contactsGenerated.Remove(friendId);  // Eliminamos el contacto si está fuera de línea
                }
            }
        };
    }

    public void LogOut()
    {
        // Dejar de escuchar los cambios de estado
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference friendsRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(userId).Child("friends");

        friendsRef.ChildAdded -= (sender, args) =>
        {
            string friendId = args.Snapshot.Key;
            ListenToFriendOnlineStatus(friendId);
        };
        auth.SignOut();
    }

    private void OnDisable()
    {
        if(auth.CurrentUser == null)
            return;
            
        // Dejar de escuchar los cambios de estado
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference friendsRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(userId).Child("friends");

        friendsRef.ChildAdded -= (sender, args) =>
        {
            string friendId = args.Snapshot.Key;
            ListenToFriendOnlineStatus(friendId);
        };
    }
}
