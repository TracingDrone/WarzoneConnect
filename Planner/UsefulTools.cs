using System.Text.RegularExpressions;

namespace WarzoneConnect.Planner
{
    public static class UsefulTools
    {
        internal static string RetrieveHostInfo(string hostInfo, string assignedInfo)
        {
            try
            {
                return Regex.Matches(hostInfo, $@"{assignedInfo}:(?<{assignedInfo}>.*)\r\n")[0].Groups[assignedInfo]
                    .Value;
            }
            catch
            {
                return null;
            }
            
        }

        internal static int AnalyseSystem(string info) //获取系统信息
        {
            if (info.Contains("HackTool"))
                return 0;
            if (info.Contains("UBurst"))
                return 1;
            return info.Contains("Door") ? 2 : 3;
        }

        internal static int GetLength(string input) //兼容中文的string.length
        {
            return System.Text.Encoding.Default.GetBytes(input).Length;
        }
    }
}