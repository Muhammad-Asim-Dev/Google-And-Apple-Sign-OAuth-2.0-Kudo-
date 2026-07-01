using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

namespace ElProfesorKudo.Firebase.UI
{
    using ElProfesorKudo.Firebase.Auth;
    using ElProfesorKudo.Firebase.GoogleSignIn.Android;
    using ElProfesorKudo.Firebase.GoogleSignIn;
    using ElProfesorKudo.Firebase.GoogleSignIn.iOS;
    using ElProfesorKudo.Firebase.AppleSignIn;
    using ElProfesorKudo.Firebase.AppleSignIn.iOS;
    using ElProfesorKudo.Firebase.AppleSignIn.Android;
    using ElProfesorKudo.Firebase.User.Data;
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.User.Data.Service;
    using ElProfesorKudo.Firebase.PopUp;
    using ElProfesorKudo.Firebase.User.Data.Tracker;
    using System;

    public class FirebaseAuthUIController : Singleton<FirebaseAuthUIController>
    {
        [Header("Login UI")]
        [SerializeField] private TMP_InputField _loginEmailInput;
        [SerializeField] private TMP_InputField _loginPasswordInput;

        [Header("Register UI")]
        [SerializeField] private TMP_InputField _registerEmailInput;
        [SerializeField] private TMP_InputField _registerPasswordInput;
        [SerializeField] private TMP_InputField _registerConfirmPasswordInput;
        [SerializeField] private TextMeshProUGUI _notificationTextMeshPro;

        [Header("Forget Password UI")]
        [SerializeField] private TMP_InputField _emailRetrieveInput;
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI _scoreTextMeshPro;

        [Header("Parent Panels")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        [SerializeField] private GameObject _forgetPasswordPanel;
        [SerializeField] private GameObject _googleSignInPanel;
        [SerializeField] private GameObject _appleSignInPanel;
        [SerializeField] private GameObject _uiDataUserPanel;
        [SerializeField] private GameObject _scorePanel;

        [Header("Data User Display UI")]
        [SerializeField] private TextMeshProUGUI _createdAtTextMeshPro;
        [SerializeField] private TextMeshProUGUI _lastLoginTextMeshPro;
        [SerializeField] private TextMeshProUGUI _emailTextMeshPro;
        [SerializeField] private TextMeshProUGUI _descriptionTextMeshPro;
        [SerializeField] private TMP_InputField _descriptionEditInputField;
        [SerializeField] private Image _profilePictureImage;

        [Header("Class Google Sign In IOS - Android")]
        [SerializeField] private FirebaseGoogleSignInAndroid _androidGoogleSignIn;
        [SerializeField] private FirebaseGoogleSignInIOS _iosGoogleSignIn;
        private FirebaseAbstractGoogleSignIn _googleSignInHandler;

        [Header("Class Apple Sign In IOS - Android")]
        [SerializeField] private FirebaseAppleSignInAndroid _androidAppleSignIn;
        [SerializeField] private FirebaseAppleSignInIOS _iosAppleSignIn;
        private FirebaseAbstractAppleSignIn _appleSignInHandler;


        protected override void Awake()
        {
            base.Awake();
#if UNITY_ANDROID
            _googleSignInHandler = _androidGoogleSignIn;
            _appleSignInHandler = _androidAppleSignIn;
#elif UNITY_IOS
            _googleSignInHandler = _iosGoogleSignIn;
            _appleSignInHandler = _iosAppleSignIn;
#else
            CustomLogger.LogWarning("Platform not supported");
#endif
        }

        #region Show Panel
        public void ShowLoginPanel()
        {
            HideAllPanel();
            _loginPanel.SetActive(true);
        }
        public void ShowRegisterPanel()
        {
            HideAllPanel();
            _registerPanel.SetActive(true);
        }
        public void ShowForgetPasswordPanel()
        {
            HideAllPanel();
            _forgetPasswordPanel.SetActive(true);
        }
        public void ShowDeletedPopUp()
        {
            HideAllPanel();
            PopUpManager.Instance.InstancePopUp(
                PopUpObject.PopUpType.POP_UP_TYPE_T_B_V_C,
                "Delete Account",
                "Do you Really want do delete your account ?",
                "Yes",
                "No",
                null,
                () =>
                {
                    FirebaseClassicAuthService.Instance.DeleteUserData(FirebaseClassicAuthService.Instance.DeleteCurrentUser);
                });
        }
        public void ShowInfoUserPanel()
        {
            _uiDataUserPanel.SetActive(true);
        }
        public void ShowAppleSignInPanel()
        {
            HideAllPanel();
            _appleSignInPanel.SetActive(true);
        }
        public void ShowScorePanel()
        {
            HideAllPanel();
            _scorePanel.SetActive(true);
        }
        public void ShowGoogleSignInPanel()
        {
            HideAllPanel();
            _googleSignInPanel.SetActive(true);
        }
        #endregion Show Panel

        #region On Click Function
        public void OnClickLogin()
        {
            string email = _loginEmailInput.text;
            string password = _loginPasswordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                CustomLogger.LogWarning("Email and password are required for login.");
                return;
            }

            FirebaseClassicAuthService.Instance.Login(email, password);
        }
        public void OnClickLogout()
        {
            FirebaseClassicAuthService.Instance.Logout();
        }

        public void OnClickRegister()
        {
            string email = _registerEmailInput.text;
            string password = _registerPasswordInput.text;
            string confirmPassword = _registerConfirmPasswordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                SetTexNotification("All fields are required for registration.");
                CustomLogger.LogWarning("All fields are required for registration.");
                return;
            }

            if (password != confirmPassword)
            {
                SetTexNotification("Passwords do not match.");
                CustomLogger.LogWarning("Passwords do not match.");
                return;
            }
            SetTexNotification("");
            CustomLogger.LogInfo("OnRegisterClicked - stacktrace:\n" + Environment.StackTrace);
            FirebaseClassicAuthService.Instance.Register(email, password);
        }

        public void OnClickResetPassword()
        {
            FirebaseClassicAuthService.Instance.SendPasswordResetEmail(_emailRetrieveInput.text);
        }

        public void OnClickSavedData()
        {
            FirebaseUserDataService.Instance.UpdateUserFieldIfChanged(
                FirebaseUserDataFieldTracker.FieldUpdateFactory.Description(_descriptionEditInputField.text),
                null);
        }
        public void OnClickLoadData()
        {
            LoadData();
        }

        public void OnClickSignInApple()
        {
            _appleSignInHandler.SignIn();
        }
        public void OnClickSignOutApple()
        {
            _appleSignInHandler.SignOut();
        }

        public void OnClickPlusOne()
        {
            UpdateScoreUserData(1);
        }
        public void OnClickMinusOne()
        {
            UpdateScoreUserData(-1);
        }

        public void OnClickSignInGoogle()
        {
            _googleSignInHandler.SignIn();
        }
        public void OnClickSignOutGoogle()
        {
            _googleSignInHandler.SignOut();
        }

        #endregion On Click Function

        #region Notification

        public void SetTexNotification(string text)
        {
            _notificationTextMeshPro.text = text;
        }

        #endregion Notification

        #region Panel Management

        public void HideAllPanel()
        {
            _uiDataUserPanel.SetActive(false);
            _loginPanel.SetActive(false);
            _registerPanel.SetActive(false);
            _forgetPasswordPanel.SetActive(false);
            _googleSignInPanel.SetActive(false);
            _appleSignInPanel.SetActive(false);
            _scorePanel.SetActive(false);
            PopUpManager.Instance.ForceClosePopUp();
            SetTexNotification(null);
        }

        #endregion Panel Management

        #region User Data Display

        private void DisplayInfoUser(FirebaseUserData firebaseUserData)
        {
            _createdAtTextMeshPro.text = firebaseUserData.CreatedAt.ToString();
            _lastLoginTextMeshPro.text = firebaseUserData.LastLogin.ToString();
            _emailTextMeshPro.text = firebaseUserData.Email;
            _descriptionTextMeshPro.text = firebaseUserData.Description;
            StartCoroutine(Utils.DownloadImageFromWeb(firebaseUserData.ProfilePicture, (loadedSprite) =>
            {
                if (loadedSprite != null)
                {
                    _profilePictureImage.sprite = loadedSprite;
                }
            }));
        }
        public void CheckAndDisplayInfoUser(FirebaseUserData userData)
        {
            if (FirebaseCoreService.Instance.CurrentUser.IsEmailVerified)
            {
                HideAllPanel();
                DisplayInfoUser(userData);
                ShowInfoUserPanel();
            }
        }

        #endregion User Data Display

        #region Data Management

        public void LoadData()
        {
            CustomLogger.LogInfo("On load data");
            FirebaseUserDataService.Instance.GetFirebaseUserDataIfExists(
                FirebaseAuth.DefaultInstance.CurrentUser,
                (userData) =>
                {
                    CustomLogger.LogInfo("Try Display info user");
                    if (userData != null)
                    {
                        CheckAndDisplayInfoUser(userData);
                    }
                    else
                    {
                        CustomLogger.LogInfo("No data get");
                    }
                });
        }

        private void UpdateScoreUserData(int value)
        {
            int newScore = Int32.Parse(_scoreTextMeshPro.text) + value;
            FirebaseUserDataService.Instance.UpdateUserFieldIfChanged(
                                        FirebaseUserDataFieldTracker.FieldUpdateFactory.Score(newScore),
                                        (isComplete) =>
                                        {
                                            _scoreTextMeshPro.text = isComplete ? newScore.ToString() : _scoreTextMeshPro.text;
                                        });
        }

        #endregion Data Management
    }
}
