using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sjam.Helpers
{
    public static class JsonHelper
    {
        public static string ObjectToJsonNullIgnore<T>(T model)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Serialize the model to JSON
            string jsonString = JsonSerializer.Serialize(model, options);
            return jsonString;
        }
        public static string ObjectToJson(object obj)
        {
            try
            {
                // Use JsonSerializer to serialize the object to a JSON string
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the serialization
                Console.WriteLine("Error converting object to JSON: " + ex.Message);
                return null; // Return null or handle the error as appropriate for your use case
            }
        }

        internal static T JsonToObject<T>(string? v)
        {
            T obj;

            // Use JsonSerializer to serialize the object to a JSON string
            return JsonSerializer.Deserialize<T>(v);
        }

        public static void SaveJsonToFile(string jsonString, string filePath)
        {
            // Step 4: Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Step 5: Write the JSON string to the file
            File.WriteAllText(filePath, jsonString);

            //Console.WriteLine($"JSON saved to: {filePath}");
        }

        public static string FetchDataFromJsonFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Step 1: Read the JSON file content
                string jsonData = File.ReadAllText(filePath);
                return jsonData;
            }
            else
            {
                Console.WriteLine("File does not exist at the specified path.");
                return null;
            }
        }

        // Convert JSON to byte array (UTF-8)
        public static byte[] JsonToBytes(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
