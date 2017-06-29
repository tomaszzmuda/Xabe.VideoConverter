using System;
using System.IO;
using System.Threading.Tasks;

namespace Xabe.VideoConverter.FFMpeg
{
    public delegate void ChangedEventHandler(object sender, ConvertProgressEventArgs e);


    public interface IFFMpeg: IDisposable
    {
        event ChangedEventHandler OnChange;

        Task<bool> ConvertMedia(FileInfo input, string outputPath);
    }
}
