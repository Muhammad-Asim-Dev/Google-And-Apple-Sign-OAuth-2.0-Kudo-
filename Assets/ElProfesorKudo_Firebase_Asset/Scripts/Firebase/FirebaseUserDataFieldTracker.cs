
using System;
using System.Collections.Generic;

namespace ElProfesorKudo.Firebase.User.Data.Tracker
{
    using ElProfesorKudo.Firebase.Common;
    public class FirebaseUserDataFieldTracker
    {
        // Creation on the interface to allow us to use different type (int ,string, object...) and update them at once        
        public interface IFieldUpdate
        {
            bool ApplyIfChanged(FirebaseUserData currentData, FirebaseUserData updateData);
        }

        public class FieldUpdate<T> : IFieldUpdate
        {
            // Function to get the current value of the field from FirebaseUserData
            public Func<FirebaseUserData, T> GetCurrentData;

            // Action to apply the new value to FirebaseUserData
            public Callback<FirebaseUserData> SetNewData;

            // The new Data we want to set
            public T NewData;

            // This method checks if the current value is different from the new value
            // If it is different, it applies the new value and returns true
            // Otherwise, it does nothing and returns false
            public bool ApplyIfChanged(FirebaseUserData currentData, FirebaseUserData updateData)
            {
                T oldValue = GetCurrentData(currentData); // read current value
                if (!EqualityComparer<T>.Default.Equals(oldValue, NewData)) // compare
                {
                    SetNewData(updateData); // apply new value if changed
                    return true;
                }
                return false; // no change
            }
        }

        public static class FieldUpdateFactory
        {
            // Factory to update the Description field
            public static FieldUpdate<string> Description(string newValue) =>
                new FieldUpdate<string>
                {
                    GetCurrentData = d => d.Description, // Function to get the current Description
                    SetNewData = d => d.Description = newValue, // Action to set the new Description
                    NewData = newValue // The new value to apply
                };

            // Factory to update the Score field
            public static FieldUpdate<int> Score(int newValue) =>
                new FieldUpdate<int>
                {
                    GetCurrentData = d => d.Score,
                    SetNewData = d => d.Score = newValue,
                    NewData = newValue
                };

            // Factory to update the FcmToken field
            public static FieldUpdate<string> FcmToken(string newValue) =>
                new FieldUpdate<string>
                {
                    GetCurrentData = d => d.FcmToken,
                    SetNewData = d => d.FcmToken = newValue,
                    NewData = newValue
                };

            // YOu can add more specific to your field to update the date you want
        }

    }

}
