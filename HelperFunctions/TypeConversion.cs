using Newtonsoft.Json.Linq;
using System;

namespace HelperFunctions {

    static class TypeConversion {

        public static T? Convert<T>(this JToken token, string identifier) {

            // Check if token is null or if identifier is null or empty
            if (token == null || string.IsNullOrEmpty(identifier)) {

                // Return default value of type T
                return default;
            }

            // Get convertable object using the identifier
            JToken? convertableObject = token[identifier];

            // Check if convertableObject is null or empty
            if (convertableObject == null) {

                // Return default value of type T
                return default;
            }

            // Try to convert the object to type T
            try {
                T? convertedObject = convertableObject.ToObject<T>();
                return convertedObject;
            } catch (Exception ex) {
                // Handle the exception if the conversion fails
                Serilog.Log.Error($"An error occurred: {ex.Message}\n{ex.StackTrace}");
                return default;
            }
        }
    }
}