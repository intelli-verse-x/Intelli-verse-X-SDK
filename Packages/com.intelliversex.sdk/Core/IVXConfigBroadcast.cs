using System;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Soft bridge so Bootstrap can notify V2 managers without an asmdef cycle.
    /// Payload is typically an <c>IVXBootstrapConfig</c> ScriptableObject.
    /// </summary>
    public static class IVXConfigBroadcast
    {
        public static event Action<object> BootstrapConfigAvailable;

        public static void NotifyBootstrapConfig(object config)
        {
            if (config == null)
                return;
            var handlers = BootstrapConfigAvailable;
            if (handlers != null)
                handlers.Invoke(config);
        }
    }
}
