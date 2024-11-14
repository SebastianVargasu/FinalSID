
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginAndRegister : MonoBehaviour
{
    [Header("Login Fields")]
    [SerializeField] private TMP_InputField emailLoginInput;
    [SerializeField] private TMP_InputField passwordLoginInput;
    [SerializeField] private LoginNotification loginNotification;


    [Header("Register Fields")]
    [SerializeField] private TMP_InputField emailRegisterInput;
    [SerializeField] private TMP_InputField passwordRegisterInput;
    [SerializeField] private TMP_InputField usernameRegisterInput;

    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private Button logOutButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button LoginButton;
    [SerializeField] private Button RegisterButton;
    [SerializeField] private Button ResetpasswordButton;
    private FirebaseAuth auth;
    private FirebaseUser user;

    void Start()
    {
        InitializeFirebase();
    }

    public void GoToPlay()
    {
        SceneManager.LoadScene("PlayScene");
    }

    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            Debug.Log(auth.CurrentUser);
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                GameUserInfo.Instance.Logout();
                AppNavegation.Instance.ShowLoginScreen();
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                GameUserInfo.Instance.UserLoggedIn();
                GameUserInfo.Instance.UpdateOnlineStatus(true);
                loginNotification.OnLogin();
                AppNavegation.Instance.ShowProfileScreen();
            }
        }
        else if (auth.CurrentUser == null && user == null)
        {
            // Se necesita login
            AppNavegation.Instance.ShowLoginScreen();
        }
    }

    public void RegisterUser()
    {
        if (usernameRegisterInput.text.Length < 1)
        {
            Debug.LogError("Username is required");
            return;
        }
        auth.CreateUserWithEmailAndPasswordAsync(emailRegisterInput.text, passwordRegisterInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception.Message);

                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            UserProfile userProfile = new UserProfile { DisplayName = usernameRegisterInput.text };
            result.User.UpdateUserProfileAsync(userProfile).ContinueWith(task2 =>
            {
                Debug.Log("Firebase user created successfully:" + result.User.DisplayName + " " + result.User.UserId);
                FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(result.User.UserId).Child("username").SetValueAsync(result.User.DisplayName);
            });

            GameUserInfo.Instance.SavePlayerData(usernameRegisterInput.text);
        });
    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailLoginInput.text, passwordLoginInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception.Message);

                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

        });
    }

    public void ResetPassword()
    {
        auth.SendPasswordResetEmailAsync(emailLoginInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                logText.text = "SendPasswordResetEmailAsync was canceled.";

                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception.Message);
                return;
            }

            Debug.Log("Successfully sent password reset email.");
        });
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
