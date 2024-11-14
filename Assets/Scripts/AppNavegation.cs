using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppNavegation : MonoBehaviour
{
    public static AppNavegation Instance { get; private set; }
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject registerScreen;
    [SerializeField] private GameObject profileScreen;
    [SerializeField] private GameObject contactsScreen;
    [SerializeField] private GameObject matchScreen;
    [SerializeField] private GameObject addFriendScreen;

    private void Awake() {
        if(Instance != null){
            Destroy(this);
        }else{
            Instance = this;
        }
    }

    public void ShowAddFriendScreen(){
        addFriendScreen.SetActive(true);
    }
    

    public void ShowLoginScreen(){
        loginScreen.SetActive(true);
        registerScreen.SetActive(false);
        profileScreen.SetActive(false);
        contactsScreen.SetActive(false);
        matchScreen.SetActive(false);
    }

    public void ShowRegisterScreen(){
        loginScreen.SetActive(false);
        registerScreen.SetActive(true);
        profileScreen.SetActive(false);
        contactsScreen.SetActive(false);
        matchScreen.SetActive(false);
    }

    public void ShowProfileScreen(){
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);
        profileScreen.SetActive(true);
        contactsScreen.SetActive(false);
        matchScreen.SetActive(false);
    }

    public void ShowContactsScreen(){
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);
        profileScreen.SetActive(false);
        contactsScreen.SetActive(true);
        matchScreen.SetActive(false);
    }

    public void ShowMatchScreen(){
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);
        profileScreen.SetActive(false);
        contactsScreen.SetActive(false);
        matchScreen.SetActive(true); 
    }
}
