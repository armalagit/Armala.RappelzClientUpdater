using System.Management;

namespace HelperFunctions {

    static class Fingerprint {

        public static string Get {
            get {
                string processorId = string.Empty;
                try {

                    // Create a new instance of the ManagementClass with the specified class name
                    using ManagementClass managementClass = new ManagementClass("win32_processor");

                    // Retrieve all instances of the class
                    using ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                    // Cast the collection to a ManagementObject and retrieve the first item
                    ManagementObject managementObject = managementObjectCollection?.Cast<ManagementObject>().FirstOrDefault();

                    // Check if the object is not null before accessing its properties
                    if (managementObject != null && managementObject.Properties["processorID"] != null) {
                        processorId = managementObject.Properties["processorID"].Value?.ToString() ?? string.Empty;
                    }
                } catch (ManagementException ex) {

                    // Handle the exception if the ManagementClass cannot be instantiated
                    // or if no instances of the class are available
                    Serilog.Log.Error($"An error occurred: {ex.Message}\n{ex.StackTrace}");
                }
                return processorId;
            }
        }
    }
}