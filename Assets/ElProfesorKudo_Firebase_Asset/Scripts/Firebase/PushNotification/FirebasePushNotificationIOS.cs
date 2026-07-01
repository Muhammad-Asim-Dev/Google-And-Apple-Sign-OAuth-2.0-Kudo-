using System.Runtime.InteropServices;
namespace ElProfesorKudo.Firebase.PushNotification.IOS
{
    public class FirebasePushNotificationIOS
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _IOSNotification_RequestPermission();

        [DllImport("__Internal")]
        private static extern void _IOSNotification_Send(string title, string body, string sound);
#endif
        public void RequestNotificationPermissionIOS()
        {
            RequestNotificationPermission();
        }
        public void NotificationCallIOS(string titleNotification, string bodyNotification, string soundName = "alert.wav")
        {
            NotificationCall(titleNotification, bodyNotification, soundName);
        }
        private void RequestNotificationPermission()
        {
#if UNITY_IOS
            _IOSNotification_RequestPermission();
#endif
        }
        private void NotificationCall(string titleNotification, string bodyNotification, string soundName)
        {
#if UNITY_IOS
            _IOSNotification_Send(titleNotification, bodyNotification, soundName);
#endif
        }
    }
}
