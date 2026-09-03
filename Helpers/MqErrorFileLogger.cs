using System.Text;

namespace sjam.Helpers
{
    public class MqErrorFileLogger
    {
        public static void SaveErrorLocally(string type, string content, string queueName, string? payload = null)
        {
            var folder = $"../MQ_Failed/{type}";
            Directory.CreateDirectory(folder);

            var file = $"{queueName}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            File.WriteAllText(Path.Combine(folder, file), content);
        }

        public static void SaveMqErrorLocally(string filePath, string data, string error)
        {
            string folderPath = Path.GetDirectoryName(filePath);
            CreateFolderIfNotExists(folderPath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string logFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}.text";
            string logPath = Path.Combine(folderPath, logFileName);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[DATA]------------------------------------");
            sb.AppendLine(data);
            sb.AppendLine("[ERROR]------------------------------------");
            sb.AppendLine(error);

            AppendToFile(logPath, sb.ToString());
        }

        private static void CreateFolderIfNotExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Folder does not exist: {folderPath}");
                Console.WriteLine($"Creating folder: {folderPath}");
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"Folder created: {folderPath}");
            }
        }

        private static void AppendToFile(string filePath, string content)
        {
            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, content);
            }
            else
            {
                CreateFolderIfNotExists(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, content);
            }
        }

        public static void SaveErrorLocally(string filePath, string data, string error)
        {
            string folderPath = Path.GetDirectoryName(filePath);
            CreateFolderIfNotExists(folderPath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string logFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}.text";
            string logPath = Path.Combine(folderPath, logFileName);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[DATA]------------------------------------");
            sb.AppendLine(data);
            sb.AppendLine("[ERROR]------------------------------------");
            sb.AppendLine(error);

            AppendToFile(logPath, sb.ToString());
        }
    }
}
