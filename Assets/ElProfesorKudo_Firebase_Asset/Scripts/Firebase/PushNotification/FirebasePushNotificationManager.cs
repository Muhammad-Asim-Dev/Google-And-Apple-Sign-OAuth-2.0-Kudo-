using Firebase.Messaging;
using UnityEngine;

namespace ElProfesorKudo.Firebase.PushNotification
{
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.PushNotification.Android;
    using ElProfesorKudo.Firebase.PushNotification.IOS;
    using ElProfesorKudo.Firebase.User.Data.Service;
    using ElProfesorKudo.Firebase.User.Data.Tracker;

    public class FirebasePushNotificationManager : Singleton<FirebasePushNotificationManager>
    {

        private FirebasePushNotificationAndroid _androidPushNotificationHelper = new FirebasePushNotificationAndroid();
        private FirebasePushNotificationIOS _iOSPushNotificationHelper = new FirebasePushNotificationIOS();
        private string _pendingFcmToken;
        public string PendingFcmToken { get => _pendingFcmToken; }

        public void InitFirebaseMessaging()
        {
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            //Request permission IOS
            _iOSPushNotificationHelper.RequestNotificationPermissionIOS();

            //Request permission Android
            _androidPushNotificationHelper.RequestNotificationPermissionAndroid();
            _androidPushNotificationHelper.InitNotificationHelperAndroid();
            CustomLogger.LogInfo("Firebase Messaging Init");
        }


        private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            CustomLogger.LogInfo("FCM Token: " + token.Token);
            _pendingFcmToken = token.Token;
            TrySendToken();
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string title = e.Message.Notification?.Title ?? "No Title";
            string body = e.Message.Notification?.Body ?? "No Body";

            CustomLogger.LogInfo("Notification received:");
            CustomLogger.LogInfo("Title: " + title);
            CustomLogger.LogInfo("Body: " + body);
            _androidPushNotificationHelper.NotificationCallAndroid(title, body);
            _iOSPushNotificationHelper.NotificationCallIOS(title, body);
        }


        public void TrySendToken()
        {
            if (!string.IsNullOrEmpty(_pendingFcmToken) && FirebaseCoreService.Instance.CurrentUser != null)
            {
                FirebaseUserDataService.Instance.UpdateUserFieldIfChanged(
                    FirebaseUserDataFieldTracker.FieldUpdateFactory.FcmToken(_pendingFcmToken),
                    null);

                CustomLogger.LogInfo("FCM Token send to Firestore for user");
                _pendingFcmToken = null;
            }
        }

        private void OnDestroy()
        {
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
        }
    }
}
