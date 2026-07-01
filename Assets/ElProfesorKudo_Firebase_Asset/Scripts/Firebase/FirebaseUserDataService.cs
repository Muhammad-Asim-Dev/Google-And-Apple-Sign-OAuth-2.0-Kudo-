using System;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElProfesorKudo.Firebase.User.Data.Service
{
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.User.Data;
    using ElProfesorKudo.Firebase.User.Data.Tracker;
    using ElProfesorKudo.Firebase.PushNotification;

    public class FirebaseUserDataService : Singleton<FirebaseUserDataService>
    {
        private FirebaseUserData _currentUserData;
        public FirebaseUserData CurrentUserData { get => _currentUserData; }
        private bool _isLoading = false;

        protected override void Awake()
        {
            base.Awake();
        }

        public void GetFirebaseUserDataIfExists(FirebaseUser user, Callback<FirebaseUserData> onResult)
        {
            if (user == null)
            {
                CustomLogger.LogWarning("[FirebaseUserDataService] No user is currently logged in.");
                onResult?.Invoke(null);
                return;
            }

            // If data already here we return already
            if (_currentUserData != null)
            {
                CustomLogger.LogWarning("CurrentUser Already here");
                onResult?.Invoke(_currentUserData);
                return;
            }

            if (_isLoading)
            {
                CustomLogger.LogDebug("Already loading user data → ignoring this call");
                return;
            }
            _isLoading = true;
            // We refresh data here cause sometime even if you verify mail, the value is not updated so we force a refresh
            if (!user.IsEmailVerified)
            {
                CustomLogger.LogDebug("Force refreshing to get last email information");
                RefreshFirebaseUserInfo(user, () => FetchUserDataFromFirestoreDatabase(user, onResult));
            }
            else
            {
                CustomLogger.LogDebug("The email is verify we fetch the data from FirestoreDatabase");
                FetchUserDataFromFirestoreDatabase(user, onResult);

            }
        }

        private void FetchUserDataFromFirestoreDatabase(FirebaseUser user, Callback<FirebaseUserData> onResult)
        {
            FirebaseFirestore db = FirebaseCoreService.Instance.FirestoreDatabase;
            string userId = user.UserId;
            DocumentReference userDoc = db.Collection("users").Document(userId);
            CustomLogger.LogDebug("We get the reference of the user to make a request to Firebase");

            userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                CustomLogger.LogDebug("task.Result " + task.Result);
                if (task.IsFaulted)
                {
                    CustomLogger.LogWarning("Data not find or error");
                    _isLoading = false;
                    onResult?.Invoke(null);
                }
                else
                {
                    EnsureUserDataExistsAndLoad(user, userDoc, task, onResult);
                }
            });
        }

        private void EnsureUserDataExistsAndLoad(FirebaseUser user, DocumentReference userDoc, Task<DocumentSnapshot> task, Callback<FirebaseUserData> onResult)
        {
            DocumentSnapshot snapshot = task.Result;
            CustomLogger.LogDebug("Checking User have data");
            if (!snapshot.Exists)
            {
                CustomLogger.LogInfo("No Data create for user yet");
                CustomLogger.LogInfo("Creating new Firestore document for user: " + user.UserId);
                // Use FirebaseUserData constructor
                FirebaseUserData newUserData = new FirebaseUserData(user.Email, user.DisplayName, user.PhotoUrl?.ToString(), null, 0, FirebasePushNotificationManager.Instance.PendingFcmToken);

                // SetAsync with FirebaseUserData object
                userDoc.SetAsync(newUserData).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompleted)
                    {
                        CustomLogger.LogInfo("User Firestore document created.");
                        _currentUserData = newUserData;
                        _isLoading = false;
                        onResult?.Invoke(newUserData);
                    }
                    else
                    {
                        CustomLogger.LogError("[FirebaseUserDataService] Failed to create user document: " + setTask.Exception);
                        _isLoading = false;
                    }
                });
            }
            else
            {
                CustomLogger.LogInfo("User already have data create");
                userDoc.UpdateAsync("last_login", Timestamp.FromDateTime(DateTime.UtcNow));
                FirebaseUserData data = task.Result.ConvertTo<FirebaseUserData>();
                _currentUserData = data;
                _isLoading = false;
                onResult?.Invoke(data);
                FirebasePushNotificationManager.Instance.TrySendToken();
            }
        }

        public void SetCurrentUserData(FirebaseUserData currentUserData)
        {
            _currentUserData = currentUserData;
        }

        //This is only to fetch latest info from firebase basic info 
        private void RefreshFirebaseUserInfo(FirebaseUser user, Callback fetchDataUser)
        {
            user.ReloadAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    CustomLogger.LogInfo("User reloaded.");
                    bool isVerified = user.IsEmailVerified;
                    CustomLogger.LogDebug("IsEmailVerified: " + isVerified);
                    fetchDataUser?.Invoke();
                }
                else
                {
                    CustomLogger.LogError("[FirebaseUserDataService] Failed to reload user: " + task.Exception);
                }
            });
        }

        // Create 2 function one for update only one Element and the other a list of element
        public void UpdateUserFieldIfChanged(FirebaseUserDataFieldTracker.IFieldUpdate fieldToUpdate, Callback<bool> onComplete)
        {
            UpdateUserFieldsIfChanged(new List<FirebaseUserDataFieldTracker.IFieldUpdate> { fieldToUpdate }, onComplete);
        }
        public void UpdateUserFieldsIfChanged(List<FirebaseUserDataFieldTracker.IFieldUpdate> fieldsToUpdate, Callback<bool> onComplete)
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null)
            {
                CustomLogger.LogWarning("No user logged in");
                onComplete?.Invoke(false);
                return;
            }

            FirebaseFirestore db = FirebaseCoreService.Instance.FirestoreDatabase;
            DocumentReference userDoc = db.Collection("users").Document(user.UserId);

            // Create a new object to avoid data lost when rewrite the data
            FirebaseUserData updateData = new FirebaseUserData
            {
                Email = _currentUserData.Email,
                DisplayName = _currentUserData.DisplayName,
                CreatedAt = _currentUserData.CreatedAt,
                ProfilePicture = _currentUserData.ProfilePicture,
                LastLogin = _currentUserData.LastLogin,
                Description = _currentUserData.Description,
                Score = _currentUserData.Score,
                FcmToken = _currentUserData.FcmToken
            };

            bool hasChanged = false;
            foreach (FirebaseUserDataFieldTracker.IFieldUpdate field in fieldsToUpdate)
            {
                if (field.ApplyIfChanged(_currentUserData, updateData))
                {
                    hasChanged = true;
                }
            }

            if (!hasChanged)
            {
                CustomLogger.LogInfo("No fields changed, skipping Firestore update.");
                onComplete?.Invoke(true);
                return;
            }

            // Update Firestore
            userDoc.SetAsync(updateData, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    CustomLogger.LogInfo("User fields updated successfully.");
                    // Update local cache
                    _currentUserData = updateData;
                    onComplete?.Invoke(true);
                }
                else
                {
                    CustomLogger.LogError("Failed to update user fields: " + task.Exception);
                    onComplete?.Invoke(false);
                }
            });
        }
    }
}
