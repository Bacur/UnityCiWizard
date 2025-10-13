using System;
using UnityEngine;

namespace CiWizard.Editor
{
    public class BuildLogHandler : ILogHandler
    {
        private static readonly ILogHandler UnityLogHandler = Debug.unityLogger.logHandler;

        private int count = 0;

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            SetColor(31);
            UnityLogHandler.LogException(exception, context);
            SetColor(0);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            int colorCode;
            switch (logType)
            {
                case LogType.Error:
                    colorCode = 31;
                    break;
                case LogType.Assert:
                    colorCode = 96;
                    break;
                case LogType.Warning:
                    colorCode = 93;
                    break;
                case LogType.Log:
                    colorCode = 92;
                    break;
                case LogType.Exception:
                    colorCode = 95;
                    break;
                default:
                    colorCode = 0;
                    break;
            }

            SetColor(colorCode);
            UnityLogHandler.LogFormat(logType, context, format, args);
            SetColor(0);
        }

        public static void WriteSectionBegin(string sectionName, string sectionHeader, bool isCollapsed = false)
        {
            SetColor(96);
            Console.WriteLine((char)27);
            Console.Write($"[0Ksection_start:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{sectionName}[collapsed={isCollapsed}]");
            Console.Write((char)13);
            Console.Write((char)27);
            Console.Write("[0K");
            Console.Write(sectionHeader);
            Console.WriteLine();
            SetColor(0);
        }

        public static void WriteSectionEnd(string sectionName)
        {
            SetColor(96);
            Console.WriteLine((char)27);
            Console.Write($"[0Ksection_end:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{sectionName}");
            Console.Write((char)13);
            Console.Write((char)27);
            Console.Write("[0K");
            Console.WriteLine();
            SetColor(0);
        }

        private static void SetColor(int colorCode)
        {
            Console.Write((char)27);
            Console.Write($"[{colorCode}m");
        }
    }
}