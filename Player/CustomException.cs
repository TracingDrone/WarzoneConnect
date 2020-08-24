using System;
using System.Runtime.Serialization;
using WarzoneConnect.Properties;

// ReSharper disable CommentTypo

namespace WarzoneConnect.Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CustomException
    {
        [Serializable]
        public class FileNotExistException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public FileNotExistException()
            {
            }

            public FileNotExistException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.FileNotExist}")
            {
            }

            public FileNotExistException(string message, Exception inner) : base(message, inner)
            {
            }

            protected FileNotExistException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class ExecInstalledException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public ExecInstalledException()
            {
            }

            public ExecInstalledException(string execName) : base(
                $"{execName} - {CustomException_TextResource.ExecInstalled}")
            {
            }

            public ExecInstalledException(string message, Exception inner) : base(message, inner)
            {
            }

            protected ExecInstalledException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class NameConflictException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public NameConflictException()
            {
            }

            public NameConflictException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.NameConflict}")
            {
            }

            public NameConflictException(string message, Exception inner) : base(message, inner)
            {
            }

            protected NameConflictException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class MismatchedFormatException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public MismatchedFormatException()
            {
            }

            public MismatchedFormatException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.MismatchedFormat}")
            {
            }

            public MismatchedFormatException(string message, Exception inner) : base(message, inner)
            {
            }

            protected MismatchedFormatException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class UnknownArgumentException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public UnknownArgumentException()
            {
            }

            public UnknownArgumentException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.UnknownArgument}")
            {
            }

            public UnknownArgumentException(string message, Exception inner) : base(message, inner)
            {
            }

            protected UnknownArgumentException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class UnknownCommandException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public UnknownCommandException() : base(CustomException_TextResource.UnknownCommand)
            {
            }

            public UnknownCommandException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.UnknownCommand}")
            {
            }

            public UnknownCommandException(string message, Exception inner) : base(message, inner)
            {
            }

            protected UnknownCommandException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class DllRequirementException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public DllRequirementException() : base(CustomException_TextResource.DLLRequirement)
            {
            }

            public DllRequirementException(string commandName) : base(
                $"{commandName} - {CustomException_TextResource.DLLRequirement}")
            {
            }

            public DllRequirementException(string message, Exception inner) : base(message, inner)
            {
            }

            protected DllRequirementException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }
    }
}