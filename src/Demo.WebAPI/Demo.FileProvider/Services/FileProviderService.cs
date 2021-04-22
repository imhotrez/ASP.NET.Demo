using System.IO;
using System.Linq;

namespace Demo.FileProvider.Services {
    public class FileProviderService {
        public bool Save(int id, byte[] body) {
            var stringId = id.ToString();
            var folderPath = stringId.Aggregate(string.Empty, (current, t) => current + $"{t}\\");
            folderPath = $"D:\\FileStorage\\{folderPath}";
            if (!Directory.Exists(folderPath)) {
                var dirInfo = Directory.CreateDirectory(folderPath);
            }
            
            File.WriteAllBytes(folderPath,body);
            return true;
        }
    }
}