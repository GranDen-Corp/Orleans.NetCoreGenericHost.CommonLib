using System;
using System.Runtime.Serialization;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Abstract class for various Orleans hosting configuration library load failed or initiated failed exception.
    /// </summary>
    [Serializable]
    public abstract class OrleansLibLoadFailedException : Exception
    {
        private static string customMsg;

        /// <summary>
        /// Nuget package name
        /// </summary>
        public abstract string LibNugetName { get; protected set; }

        /// <summary>
        /// Prompt string in exception message.
        /// </summary>
        public abstract string LibPurpose { get; protected set; }

        /// <summary>
        /// exception constructor
        /// </summary>
        /// <param name="message"></param>
        protected OrleansLibLoadFailedException(string message) : base(message)
        {
        }

        /// <summary>
        /// base constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        protected OrleansLibLoadFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected OrleansLibLoadFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// exception constructor
        /// </summary>
        /// <param name="innerException"></param>
        protected OrleansLibLoadFailedException(Exception innerException) :
            this(customMsg, innerException)
        {
            customMsg = ComposeMessage();
        }


#pragma warning disable 1591
        protected string ComposeMessage()
#pragma warning restore 1591
        {
            return $"Load {LibPurpose} library failed, Please install {LibNugetName} nuget(s) and configure properly";
        }

    }
}
