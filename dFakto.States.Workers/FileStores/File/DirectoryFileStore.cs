using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.StepFunctions.Model.Internal.MarshallTransformations;
using dFakto.States.Workers.Interfaces;
using dFakto.States.Workers.Internals;

namespace dFakto.States.Workers.FileStores.File
{
    public class DirectoryFileStore : IFileStore
    {
        private readonly string _fileStoreName;
        public const string TYPE = "file";
        
        private readonly string _basePath;

        public DirectoryFileStore(string fileStoreName,  DirectoryFileStoreConfig config)
        {
            _fileStoreName = fileStoreName;
            _basePath = config.BasePath;
        }
        
        public Task<string> CreateFileToken(string fileName)
        {
            var now = DateTime.UtcNow;
            
            FileToken token = new FileToken(TYPE,_fileStoreName);
            token.Path = Path.Combine(_basePath, now.Year.ToString(), now.Month.ToString("00"), now.Day.ToString("00"),
                fileName);
            
            return Task.FromResult(token.ToString());
        }

        public Task<string> GetFileName(string fileToken)
        {
            var token = FileToken.Parse(fileToken,_fileStoreName);
            
            return Task.FromResult(Path.GetFileName(token.Path));
        }

        public async Task<Stream> OpenRead(string fileToken)
        {
            var token = FileToken.Parse(fileToken,_fileStoreName);
            
            if(!await Exists(token))
                throw new FileNotFoundException();
            
            return new FileStream(token.Path, FileMode.Open);
        }

        public Task<Stream> OpenWrite(string token)
        {
            var fileToken = FileToken.Parse(token,_fileStoreName);

            string localPath = fileToken.Path;
            string dir = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            return Task.FromResult((Stream) new FileStream(localPath, FileMode.Create));
        }
        
        public Task Delete(string fileToken)
        {
            var token = FileToken.Parse(fileToken,_fileStoreName);
            if (System.IO.File.Exists(token.Path))
            {
                System.IO.File.Delete(token.Path);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> Exists(string fileToken)
        {
            return await Exists(FileToken.Parse(fileToken,_fileStoreName));
        }
        
        private Task<bool> Exists(FileToken fileToken)
        {
            return Task.FromResult(System.IO.File.Exists(fileToken.Path));
        }

        public void Dispose()
        {
        }
    }
}