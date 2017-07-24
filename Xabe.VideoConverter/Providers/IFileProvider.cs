using System.IO;
using System.Threading.Tasks;

namespace Xabe.VideoConverter.Providers
{
    public interface IFileProvider
    {
        Task<FileInfo> GetNext();
        Task Refresh();
    }
}
