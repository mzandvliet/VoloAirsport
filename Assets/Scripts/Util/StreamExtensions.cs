using System.IO;

namespace RamjetAnvil.Volo.Util {
    public static class StreamExtensions {
        public static void CopyTo(this Stream input, Stream output, int bufferSize = 4096) {
            var buffer = new byte[bufferSize];
            while (true) {
                var bytesReadCount = input.Read(buffer, 0, buffer.Length);
                if (bytesReadCount <= 0) {
                    return;
                }
                output.Write(buffer, 0, bytesReadCount);
            }
        }
    }
}