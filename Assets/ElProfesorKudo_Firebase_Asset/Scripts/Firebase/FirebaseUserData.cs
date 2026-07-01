using Firebase.Firestore;
using System;

namespace ElProfesorKudo.Firebase.User.Data
{

    [FirestoreData]
    public class FirebaseUserData
    {
        [FirestoreProperty("email")]
        public string Email { get; set; }

        [FirestoreProperty("display_name")]
        public string DisplayName { get; set; }

        [FirestoreProperty("created_at")]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty("profile_picture")]
        public string ProfilePicture { get; set; }

        [FirestoreProperty("last_login")]
        public Timestamp LastLogin { get; set; }

        [FirestoreProperty("description")]
        public string Description { get; set; }

        [FirestoreProperty("score")]
        public int Score { get; set; }

        [FirestoreProperty("fcm_token")]
        public string FcmToken { get; set; }

        public FirebaseUserData() { }

        public FirebaseUserData(string email, string displayName, string profilePictureUrl, string description, int score, string fcmToken)
        {
            Email = email;
            DisplayName = displayName ?? "New user";
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
            ProfilePicture = profilePictureUrl ?? "https://upload.wikimedia.org/wikipedia/commons/a/ac/Default_pfp.jpg";
            LastLogin = Timestamp.FromDateTime(DateTime.UtcNow);
            Description = description ?? "This is a default description of user";
            Score = score;
            FcmToken = fcmToken;
        }
    }
}
