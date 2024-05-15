﻿using System.IO.Compression;
using System.Text.Json;

namespace Eurofurence.App.Common.Compression
{
    public static class ZipHelper
    {
        public static void AddAsJson(this ZipArchive archive, string entryName, object item, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            var entry = archive.CreateEntry(entryName, compressionLevel);
            var stream = entry.Open();

            JsonSerializer.Serialize(stream, item);
        }

        public static void AddAsBinary(this ZipArchive archive, string entryName, byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            var entry = archive.CreateEntry(entryName, compressionLevel);
            using (var stream = entry.Open())
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public static T ReadAsJson<T>(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);
            var stream = entry.Open();

            return JsonSerializer.Deserialize<T>(stream);
        }

        public static byte[] ReadAsBinary(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);
            using (var stream = entry.Open())
            {
                byte[] data = new byte[entry.Length];
                stream.Read(data, 0, data.Length);

                return data;
            }
        }
    }
}
