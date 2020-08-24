using System.Collections.Generic;
using System.IO;
using System.Resources;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    internal static class MediaBuilder
    {
        internal static void BuildVideoResources(IEnumerable<string> fileNames)
        {
            if (File.Exists(@".\Media.resource"))
                File.Delete(@".\Media.resource");
            using var resourceWriter = new ResourceWriter("Media.resource");
            foreach (var fileName in fileNames)
                resourceWriter.AddResource(fileName, File.ReadAllBytes(@".\Resources\" + fileName + ".mp4"));
        } //resx添加视频时用

        internal static IEnumerable<MediaPlayer.MediaFile> GetVideoResources(List<string> fileNames)
        {
            var mediaList = new List<MediaPlayer.MediaFile>();
            if (File.Exists("Media.resource"))
            {
                using var resourceReader = new ResourceReader("Media.resource");
                var en = resourceReader.GetEnumerator();
                while (en.MoveNext())
                    if (fileNames.Contains((string)en.Key))
                        mediaList.Add(new MediaPlayer.MediaFile(en.Key?.ToString()));
                resourceReader.Dispose(); 
            }
            return mediaList; 
        } //resx取回视频时用
    }
}