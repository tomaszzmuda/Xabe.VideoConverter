using System;
using System.IO;
using System.Text;

namespace Xabe.VideoConverter
{
    internal static class MovieHasher
    {
        public static byte[] ComputeMovieHash(string filename)
        {
            byte[] result;
            using(Stream input = File.OpenRead(filename))
            {
                result = ComputeMovieHash(input);
            }
            return result;
        }

        public static byte[] ComputeMovieHash(Stream input)
        {
            long lhash, streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            while(i < 65536 / sizeof(long) &&
                  input.Read(buffer, 0, sizeof(long)) > 0)
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while(i < 65536 / sizeof(long) &&
                  input.Read(buffer, 0, sizeof(long)) > 0)
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        public static string ToHexadecimal(this byte[] bytes)
        {
            var hexBuilder = new StringBuilder();
            foreach(byte t in bytes)
            {
                hexBuilder.Append(t.ToString("x2"));
            }
            return hexBuilder.ToString();
        }
    }
}
