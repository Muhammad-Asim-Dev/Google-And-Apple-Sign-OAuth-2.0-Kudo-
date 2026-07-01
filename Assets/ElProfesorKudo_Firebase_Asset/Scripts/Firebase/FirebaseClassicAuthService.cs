using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using Firebase.Firestore;

namespace ElProfesorKudo.Firebase.Auth
{
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.User.Data.Service;
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;
    public class FirebaseClassicAuthService : Singleton<FirebaseClassicAuthService>
    {
        #region Register
        public void Register(string email, string password)
        {
            StartCoroutine(RegisterCoroutine(email, password));
        }

        private IEnumerator RegisterCoroutine(string email, string password)
        {
            Task<AuthResult> registerTask = FirebaseCoreService.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            bool timedOutRegister = false;

            yield return Utils.WaitForTaskWithTimeout(registerTask, 30f, () =>
            {
                timedOutRegister = true;
                CustomLogger.LogError("Registration timeout (30s)");
            });

            if (timedOutRegister)
            {
                yield break;
            }

            if (registerTask.Exception != null)
            {
                HandleFirebaseException(registerTask.Exception);
                FirebaseCallbacks.InvokeRegisterFailed(GetFirebaseErrorMessage(registerTask.Exception));
                yield break;
            }

            AuthResult result = registerTask.Result;
            FirebaseCoreService.Instance.SetCurrentUser(result.User);
            CustomLogger.LogInfo("Registration Successful: " + FirebaseCoreService.Instance.CurrentUser.Email);
            FirebaseCallbacks.InvokeRegisterSuccess();
            StartCoroutine(SendEmailVerificationCoroutine());
        }
        #endregion Register

        #region Login
        public void Login(string email, string password)
        {
            StartCoroutine(LoginCoroutine(email, password));
        }

        private IEnumerator LoginCoroutine(string email, string password)
        {
            Task<AuthResult> loginTask = FirebaseCoreService.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);

            bool isTimedOut = false;

            // Attente avec timeout via Utils
            yield return StartCoroutine(Utils.WaitForTaskWithTimeout(loginTask, 30f, () =>
            {
                isTimedOut = true;
                CustomLogger.LogError("Login timed out after 30 seconds.");
            }));

            if (isTimedOut)
            {
                yield break;
            }

            if (loginTask.IsFaulted)
            {
                HandleFirebaseException(loginTask.Exception);
                FirebaseCallbacks.InvokeLoginFailed(GetFirebaseErrorMessage(loginTask.Exception));
                yield break;
            }

            // Succès
            AuthResult result = loginTask.Result;
            FirebaseCoreService.Instance.SetCurrentUser(result.User);

            if (!FirebaseCoreService.Instance.CurrentUser.IsEmailVerified)
            {
                CustomLogger.LogWarning("Email not verified. Please check your inbox.");
                FirebaseCallbacks.InvokeLoginFailed("Email not verified. Please check your inbox.");
                yield break;
            }

            CustomLogger.LogInfo("Login Successful: " + FirebaseCoreService.Instance.CurrentUser.Email);
            FirebaseCallbacks.InvokeLoginSuccess();
        }

        public void Logout()
        {
            FirebaseCoreService.Instance.Auth.SignOut();
            FirebaseUserDataService.Instance.SetCurrentUserData(null);
            FirebaseCallbacks.InvokeLogout();
            CustomLogger.LogDebug("User signed out.");
        }
        #endregion Login
        private IEnumerator SendEmailVerificationCoroutine()
        {
            // Send email verification
            Task sendEmailTask = FirebaseCoreService.Instance.CurrentUser.SendEmailVerificationAsync();
            bool timedOut = false;

            yield return Utils.WaitForTaskWithTimeout(sendEmailTask, 30f, () =>
            {
                timedOut = true;
                CustomLogger.LogError("Verification email timeout (30s)");
            });

            if (timedOut)
            {
                yield break;
            }

            if (sendEmailTask.Exception != null)
            {
                CustomLogger.LogError("Failed to send verification email: " + sendEmailTask.Exception.Message);
            }
            else
            {
                CustomLogger.LogDebug("Verification email sent. Please check your inbox.");
                string emailVerificationMsg = "Verification email sent. Please check your inbox.";
                FirebaseCallbacks.InvokeEmailVerificationSent(emailVerificationMsg);
            }
        }

        #region ForgotPassword

        public void SendPasswordResetEmail(string email)
        {
            StartCoroutine(SendPasswordResetEmailCoroutine(email));
        }

        private IEnumerator SendPasswordResetEmailCoroutine(string email)
        {
            Task sendResetEmailTask = FirebaseCoreService.Instance.Auth.SendPasswordResetEmailAsync(email);
            bool timedOut = false;

            yield return Utils.WaitForTaskWithTimeout(sendResetEmailTask, 30f, () =>
            {
                timedOut = true;
                CustomLogger.LogError("Password reset email timeout (30s)");
            });

            if (timedOut)
            {
                FirebaseCallbacks.InvokePasswordResetFailed("Timeout while sending password reset email.");
                yield break;
            }

            if (sendResetEmailTask.Exception != null)
            {
                string error = GetFirebaseErrorMessage(sendResetEmailTask.Exception);
                CustomLogger.LogError("Failed to send password reset email: " + error);
                FirebaseCallbacks.InvokePasswordResetFailed(error);
            }
            else
            {
                CustomLogger.LogInfo("Password reset email sent successfully to " + email);
                FirebaseCallbacks.InvokePasswordResetSuccess("Password reset email sent. Please check your inbox.");
            }
        }

        #endregion ForgotPassword

        #region Error Handle

        private void HandleFirebaseException(AggregateException ex)
        {
            CustomLogger.LogError(GetFirebaseErrorMessage(ex));
        }

        private string GetFirebaseErrorMessage(AggregateException ex)
        {
            FirebaseException firebaseEx = ex?.GetBaseException() as FirebaseException;
            if (firebaseEx == null) return "Unknown Firebase error";

            AuthError code = (AuthError)firebaseEx.ErrorCode;

            switch (code)
            {
                case AuthError.InvalidEmail: return "Invalid email format.";
                case AuthError.WrongPassword: return "Incorrect password.";
                case AuthError.UserNotFound: return "User not found.";
                case AuthError.EmailAlreadyInUse: return "Email already in use.";
                case AuthError.WeakPassword: return "Weak password.";
                case AuthError.NetworkRequestFailed: return "Network error.";
                default: return "Firebase error: " + code;
            }
        }
        #endregion Error Handle

        #region Deleted User

        public void DeleteCurrentUser()
        {
            FirebaseUser user = FirebaseCoreService.Instance.CurrentUser;

            if (user != null)
            {
                user.DeleteAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        CustomLogger.LogError("Deletion canceled.");
                        FirebaseCallbacks.InvokeDeleteCurrentUserFailed("Deletion canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        CustomLogger.LogError("Error during deletion: " + task.Exception);
                        FirebaseCallbacks.InvokeDeleteCurrentUserFailed(task.Exception?.Message ?? "Unknown error");
                        return;
                    }

                    CustomLogger.LogInfo("User deleted successfully.");
                    FirebaseCallbacks.InvokeDeleteCurrentUserSuccess();
                });
            }
            else
            {
                CustomLogger.LogWarning("No user is currently signed in.");
                FirebaseCallbacks.InvokeDeleteCurrentUserFailed("No user is currently signed in.");
            }
        }

        public void DeleteUserData(Callback callbackDeleteUser)
        {

            if (FirebaseCoreService.Instance.CurrentUser != null)
            {
                FirebaseFirestore db = FirebaseCoreService.Instance.FirestoreDatabase;
                db.Collection("users").Document(FirebaseCoreService.Instance.CurrentUser.UserId).DeleteAsync()
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            CustomLogger.LogError("Error on deleted Data User : " + task.Exception);
                            FirebaseCallbacks.InvokeDeleteUserDataFailed(task.Exception?.Message ?? "Unknown error");
                            return;
                        }
                        CustomLogger.LogInfo("user data Deleted");
                        FirebaseCallbacks.InvokeDeleteUserDataSuccess();
                        callbackDeleteUser?.Invoke();
                    });
            }
            else
            {
                CustomLogger.LogWarning("Warning no User connected");
                FirebaseCallbacks.InvokeDeleteUserDataFailed("No user is currently signed in.");
            }
        }

        #endregion Deleted User
    }
}
