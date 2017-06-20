using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Eurofurence.App.Server.Services.Images
{
    public static class ImageHelper
    {
        private static readonly Dictionary<byte[], string> imageFormatDecoders = new Dictionary<byte[], string>
        {
            {new byte[] {0x42, 0x4D}, "image/bmp"},
            {new byte[] {0x47, 0x49, 0x46, 0x38, 0x37, 0x61}, "image/gif"},
            {new byte[] {0x47, 0x49, 0x46, 0x38, 0x39, 0x61}, "image/gif"},
            {new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}, "image/png"},
            {new byte[] {0xff, 0xd8}, "image/jpeg"}
        };

        public static string GetContentType(byte[] imageBytes)
        {
            var ms = new MemoryStream(imageBytes);

            using (var br = new BinaryReader(ms))
            {
                var maxMagicBytesLength = imageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;

                var magicBytes = new byte[maxMagicBytesLength];

                for (var i = 0; i < maxMagicBytesLength; i += 1)
                {
                    magicBytes[i] = br.ReadByte();

                    foreach (var kvPair in imageFormatDecoders)
                        if (magicBytes.StartsWith(kvPair.Key))
                            return kvPair.Value;
                }

                throw new ArgumentException("Could not recognise image format", "binaryReader");
            }
        }

        private static bool StartsWith(this byte[] thisBytes, byte[] thatBytes)
        {
            for (var i = 0; i < thatBytes.Length; i += 1)
                if (thisBytes[i] != thatBytes[i])
                    return false;
            return true;
        }
    }
}