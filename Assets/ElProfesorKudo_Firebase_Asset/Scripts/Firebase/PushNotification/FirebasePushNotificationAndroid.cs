using UnityEngine;
namespace ElProfesorKudo.Firebase.PushNotification.Android
{
    using ElProfesorKudo.Firebase.Common;
    public class FirebasePushNotificationAndroid
    {
        private AndroidJavaObject _notificationHelperPlugin;
        private AndroidJavaObject _activity;

        public void InitNotificationHelperAndroid()
        {
#if UNITY_ANDROID
            InitNotificationHelper();
#endif
        }
        public void RequestNotificationPermissionAndroid()
        {
#if UNITY_ANDROID
            RequestNotificationPermission();
#endif
        }
        public void NotificationCallAndroid(string titleNotification, string bodyNotification)
        {
#if UNITY_ANDROID
            NotificationCall(titleNotification, bodyNotification);
#endif
        }
        private void InitNotificationHelper()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _notificationHelperPlugin = new AndroidJavaClass("com.elprofesorkudo.notificationhelper.NotificationHelper");
            }
        }

        private void RequestNotificationPermission()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    AndroidJavaClass permissionClass = new AndroidJavaClass("androidx.core.app.ActivityCompat");
                    string permission = "android.permission.POST_NOTIFICATIONS";
                    permissionClass.CallStatic("requestPermissions", activity, new string[] { permission }, 101);
                }
            }
        }

        private void NotificationCall(string titleNotification, string bodyNotification)
        {
            //_notificationHelperPlugin.CallStatic("showForegroundNotificationDefault", _activity, titleNotification, bodyNotification);
            _notificationHelperPlugin.CallStatic("showForegroundNotificationCustom", _activity, titleNotification, bodyNotification);
        }
    }
}
