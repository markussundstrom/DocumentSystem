using System.Net;

namespace DocumentSystem.Services
{
    ///<summary>
    ///Class <c>FileService</c> handles file operations for the 
    ///Document system.
    ///</summary>
    public class FileService {
        private readonly string m_storagePath;

        public FileService() {
            m_storagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "App_Data", "DocumentStorage"
            );
            Directory.CreateDirectory(m_storagePath);
        }


        public async Task StoreFile(string fileName, IFormFile file) {
            string filePath = Path.Combine(m_storagePath, fileName);
            if (File.Exists(filePath)) {
                throw new ArgumentException("File already exists");
            }
            using (var stream = System.IO.File.Create(filePath)) {
                await file.CopyToAsync(stream);
            }
            return;
        }


        public byte[] GetFile(string fileName) {
            string filePath = Path.Combine(m_storagePath, fileName);
            if (!File.Exists(filePath)) {
                throw new ArgumentException("File not found in storage");
            }
            //FileStream stream = new FileStream(
            //        filePath, FileMode.Open, FileAccess.Read);
            byte[] fileContents = File.ReadAllBytes(filePath);
            return fileContents;
        }
    }
}
