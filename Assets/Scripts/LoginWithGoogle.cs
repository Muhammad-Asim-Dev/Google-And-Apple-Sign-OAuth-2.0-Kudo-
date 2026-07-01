using System;
using System.Collections;
using System.Collections.Generic;
// using Firebase.Extensions;
// using Google;
using System.Threading.Tasks;
using ElProfesorKudo.Firebase.AppleSignIn;
using ElProfesorKudo.Firebase.AppleSignIn.Android;
using ElProfesorKudo.Firebase.AppleSignIn.iOS;
using ElProfesorKudo.Firebase.Common;
using ElProfesorKudo.Firebase.GoogleSignIn;
using ElProfesorKudo.Firebase.GoogleSignIn.Android;
using ElProfesorKudo.Firebase.GoogleSignIn.iOS;
using Firebase.Auth;
using UnityEngine;
using TMPro;
// using Firebase.Auth;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class LoginWithGoogle : MonoBehaviour
{
    #region Dead Code

        // [Header("Google API")]
        // private string GoogleAPI = "413415639685-49etd8mro4bq04pcfldhjpb0tcp4mqe4.apps.googleusercontent.com"; // Replace with your actual WebClientID
        // private GoogleSignInConfiguration configuration;
        //
        // [Header("Firebase Auth")]
        // private FirebaseAuth auth;
        // private FirebaseUser user;
        //
        // [Header("UI References")]
        // // public TextMeshProUGUI Username, UserEmail;
        // // public GameObject LoginPanel, UserPanel;
        // // public Image UserProfilePic;
        //
        // private string imageUrl;
        // private bool isGoogleSignInInitialized = false;
        //
        // private void Start()
        // {
        //     InitFirebase();
        // }
        //
        // void InitFirebase()
        // {
        //     auth = FirebaseAuth.DefaultInstance;
        // }
        //
        // public void Login()
        // {
        //     if (!isGoogleSignInInitialized)
        //     {
        //         GoogleSignIn.Configuration = new GoogleSignInConfiguration
        //         {
        //             RequestIdToken = true,
        //             WebClientId = GoogleAPI,
        //             RequestEmail = true
        //         };
        //         isGoogleSignInInitialized = true;
        //     }
        //
        //     GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        //     {
        //         if (task.IsCanceled)
        //         {
        //             Debug.LogWarning("Google sign-in was canceled.");
        //             return;
        //         }
        //
        //         if (task.IsFaulted)
        //         {
        //             Debug.LogError("Google sign-in encountered an error: " + task.Exception);
        //             return;
        //         }
        //
        //         GoogleSignInUser googleUser = task.Result;
        //
        //         Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
        //
        //         auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        //         {
        //             if (authTask.IsCanceled)
        //             {
        //                 Debug.LogWarning("Firebase auth was canceled.");
        //                 return;
        //             }
        //
        //             if (authTask.IsFaulted)
        //             {
        //                 Debug.LogError("Firebase auth failed: " + authTask.Exception);
        //                 return;
        //             }
        //
        //             user = auth.CurrentUser;
        //
        //             // Username.text = user.DisplayName;
        //             // UserEmail.text = user.Email;
        //
        //             // Save locally
        //             PlayerPrefs.SetString("PlayerName", user.DisplayName);
        //             PlayerPrefs.SetString("PlayerEmail", user.Email);
        //             PlayerPrefs.SetInt("IsLoggedIn", 1);
        //             PlayerPrefs.Save();
        //
        //             StartCoroutine(GetFirebaseToken());
        //         });
        //     });
        // }
        //
        // private IEnumerator GetFirebaseToken()
        // {
        //     var tokenTask = user.TokenAsync(false);
        //
        //     yield return new WaitUntil(() => tokenTask.IsCompleted);
        //
        //     if (tokenTask.IsFaulted || tokenTask.IsCanceled)
        //     {
        //         Debug.LogError(tokenTask.Exception);
        //         yield break;
        //     }
        //
        //     string idToken = tokenTask.Result;
        //
        //     Debug.Log(idToken);
        //     GameManager.instance.loginPanel.SetActive(false);
        //     yield return StartCoroutine(GameManager.instance.logInApiHandler.FirebaseLoginApi(idToken));
        // }
        //
        // // User SignOut From Firebase First Then again Sign IN With Google
        // public void SignOut()
        // {
        //     GoogleSignIn.DefaultInstance.SignOut();
        //     // LoginPanel.SetActive(true);
        //     // UserPanel.SetActive(false);
        // }

    #endregion

    [FormerlySerializedAs("Username")] public TextMeshProUGUI username;
    [FormerlySerializedAs("UserEmail")] public TextMeshProUGUI userEmail;

    [Header("Class Google Sign In IOS - Android")]
    [SerializeField] private FirebaseGoogleSignInAndroid _androidGoogleSignIn;
    [SerializeField] private FirebaseGoogleSignInIOS _iosGoogleSignIn;
    private FirebaseAbstractGoogleSignIn _googleSignInHandler;

    [Header("Class Apple Sign In IOS - Android")]
    [SerializeField] private FirebaseAppleSignInAndroid _androidAppleSignIn;
    [SerializeField] private FirebaseAppleSignInIOS _iosAppleSignIn;
    private FirebaseAbstractAppleSignIn _appleSignInHandler;

    private void Awake()
    {
#if UNITY_ANDROID
        _googleSignInHandler = _androidGoogleSignIn;
        _appleSignInHandler = _androidAppleSignIn;
        
        _androidGoogleSignIn.OnLoginSuccess += OnGoogleLoginSuccess;
        _androidAppleSignIn.OnLoginSuccess += OnAppleLoginSuccess;
        
        
#elif UNITY_IOS
            _googleSignInHandler = _iosGoogleSignIn;
            _appleSignInHandler = _iosAppleSignIn;
        
        
        _iosGoogleSignIn.OnLoginSuccess += OnGoogleLoginSuccess;
        _iosAppleSignIn.OnLoginSuccess += OnAppleLoginSuccess;
#else
            CustomLogger.LogWarning("Platform not supported");
#endif
    }
    
    
    public void OnClickSignInApple()
    {
        _appleSignInHandler.SignIn();
    }
    
    
    public void OnClickSignOutApple()
    {
        _appleSignInHandler.SignOut();
    }
    
    
    public void OnClickSignInGoogle()
    {
        _googleSignInHandler.SignIn();
    }
    
    private void OnGoogleLoginSuccess(FirebaseUser user)
    {
        username.text = user.DisplayName;
        userEmail.text = user.UserId;
        Debug.Log(user.DisplayName);
        Debug.Log(user.Email);
    }
    
    private void OnAppleLoginSuccess(FirebaseUser user)
    {
        username.text = user.DisplayName;
        userEmail.text = user.UserId;
        Debug.Log(user.DisplayName);
        Debug.Log(user.Email);
    }
    
    
    public void OnClickSignOutGoogle()
    {
        _googleSignInHandler.SignOut();
    }
}
