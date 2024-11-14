using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class ContactScreen : MonoBehaviour
{
    [SerializeField] private ContactHolder usernamePrefab; 
    [SerializeField] private Transform displayFriendsParent;   


    [SerializeField] private RequestHolder requestHolderPrefab;
    [SerializeField] private Transform displayRequestsParent;

    private List<GameObject> contactsGenerated = new List<GameObject>();

    private void OnEnable() {
        foreach (var contact in contactsGenerated) {
            Destroy(contact);
        }
        GetFriendsStatus();
        GetFriendRequests();
    }


    #region "Get Friends"
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
                string friendUsername = friendSnapshot.Child("username").Value.ToString();

                CreateUsernameDisplay(friendUsername);
            }
            else
            {
                Debug.LogError("Error al obtener los datos del amigo: " + task.Exception);
            }
        });
    }

    private void CreateUsernameDisplay(string username)
    {
        ContactHolder usernameObject = Instantiate(usernamePrefab, displayFriendsParent);
        contactsGenerated.Add(usernameObject.gameObject);
        usernameObject.SetContactName(username);
    }
    #endregion


    public void GetFriendRequests()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference playersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players");

        // Obtener solicitudes recibidas
        playersRef.Child(userId).Child("receivedFriendRequests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot request in snapshot.Children)
                {
                    string friendId = request.Key;
                    string friendUserName = request.Child("username").Value.ToString(); 
                    SpawnRequest(friendUserName, friendId);
                    Debug.Log("Solicitud recibida de: " + friendId);
                }
            }
        });
    }

    private void SpawnRequest(string username, string id){
        RequestHolder requestHolder = Instantiate(requestHolderPrefab, displayRequestsParent);
        contactsGenerated.Add(requestHolder.gameObject);
        requestHolder.SetUsername(username, id);
    }

}
