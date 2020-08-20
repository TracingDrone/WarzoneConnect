using System.Collections.Generic;
using System.IO;
using System.Resources;
using WarzoneConnect.Player;

namespace WarzoneConnect.Planner
{
    internal static class MediaBuilder
    {
        internal static void BuildVideoResources(string resourceName) //TODO 需要修改！
        {
            if(File.Exists(@".\"+resourceName+".resource"))
                File.Delete(@".\"+resourceName+".resource");
            using var resourceWriter = new ResourceWriter(resourceName+".resource");
            foreach (var fileName in Directory.GetFiles(@".\Resources\"+resourceName))
            {
                resourceWriter.AddResource(fileName,File.ReadAllBytes(fileName));
            }
        } //resx添加视频时用
        
        internal static IEnumerable<MediaPlayer.MediaFile> GetVideoResources(string resourceName)
        {
            var mediaList = new List<MediaPlayer.MediaFile>();
            using var resourceReader = new ResourceReader(resourceName+".resource");
            var en = resourceReader.GetEnumerator();
            while (en.MoveNext())
            {
                mediaList.Add(new MediaPlayer.MediaFile(en.Key?.ToString()));
            }
            resourceReader.Dispose();
            return mediaList;
        } //resx取回视频时用
    }
}
