using System;
using System.Threading;
using UnityEngine;

namespace Sych.ShareAssets.Runtime.Tools
{
    internal static class UnityThread
    {
        private static SynchronizationContext _context;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => _context = SynchronizationContext.Current ?? new SynchronizationContext();

        public static void Post(Action action)
        {
            if (action == null) return;
            if (SynchronizationContext.Current == _context)
                action();
            else
                _context.Post(_ => action(), null);
        }
    }

    public static class UnityThreadExtensions
    {
        public static void InvokeInUnityThread(this Action action)
            => UnityThread.Post(action);

        public static void InvokeInUnityThread<T>(this Action<T> action, T arg)
            => UnityThread.Post(() => action(arg));
    }
}