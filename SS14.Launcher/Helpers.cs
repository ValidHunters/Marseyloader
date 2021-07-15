using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SS14.Launcher
{
    public static class Helpers
    {
        public delegate void DownloadProgressCallback(long downloaded, long total);

        public static void ExtractZipToDirectory(string directory, Stream zipStream)
        {
            using var zipArchive = new ZipArchive(zipStream);
            zipArchive.ExtractToDirectory(directory);
        }

        public static void ClearDirectory(string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            foreach (var fileInfo in dirInfo.EnumerateFiles())
            {
                fileInfo.Delete();
            }

            foreach (var childDirInfo in dirInfo.EnumerateDirectories())
            {
                childDirInfo.Delete(true);
            }
        }

        public static void EnsureDirectoryExists(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static async Task DownloadToStream(this HttpClient client, string uri, Stream stream,
            DownloadProgressCallback? progress = null, CancellationToken cancel = default)
        {
            await Task.Run(async () =>
            {
                using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancel);
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancel);
                // await using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
                var totalLength = response.Content.Headers.ContentLength;
                if (totalLength.HasValue)
                {
                    progress?.Invoke(0, totalLength.Value);
                }

                var totalRead = 0L;
                var reads = 0L;
                const int bufferLength = 4096;
                var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
                var isMoreToRead = true;

                do
                {
                    var read = await contentStream.ReadAsync(buffer.AsMemory(0, bufferLength), cancel);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await stream.WriteAsync(buffer.AsMemory(0, read), cancel);

                        reads += 1;
                        totalRead += read;
                        if (totalLength.HasValue && reads % 20 == 0)
                        {
                            progress?.Invoke(totalRead, totalLength.Value);
                        }
                    }
                } while (isMoreToRead);
            }, cancel);
        }

        public static void OpenUri(Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.ToString()) {UseShellExecute = true});
        }

        private static readonly string[] ByteSuffixes =
        {
            "B",
            "KiB",
            "MiB",
            "GiB",
            "TiB",
            "PiB",
            "EiB",
            "ZiB",
            "YiB"
        };

        public static string FormatBytes(long bytes)
        {
            double d = bytes;
            var i = 0;
            for (; i < ByteSuffixes.Length && d >= 1024; i++)
            {
                d /= 1024;
            }

            return $"{Math.Round(d, 2)} {ByteSuffixes[i]}";
        }

        /// <summary>
        ///     Does a POST with JSON.
        /// </summary>
        public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string uri, T value)
        {
            var content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            return client.PostAsync(uri, content);
        }

        public static async Task<T> AsJson<T>(this HttpContent content)
        {
            var str = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
