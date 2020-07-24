using System.IO;

namespace JiraIntegrationTool.NativeHost.Utils
{
    public class StreamHelper
    {
        public static byte[] ReadBytes(Stream stream, int count)
        {
            var data = new byte[count];
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                    throw new EndOfStreamException($"End of stream reached.");
                remaining -= read;
                offset += read;
            }

            return data;
        }
    }
}
