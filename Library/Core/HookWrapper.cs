using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace RoeHack.Library.DirectXHook
{
    public class HookWrapper<TDelegate> : IDisposable where TDelegate : class
    {
        private LocalHook localHook;

        public TDelegate Target { get; private set; }

        public HookWrapper(IntPtr target, Delegate proxy, Object callback)
        {
            Target = Marshal.GetDelegateForFunctionPointer<TDelegate>(target);

            localHook = LocalHook.Create(target, proxy, callback);

            // Activate hooks on all threads except the current thread
            localHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
        }

        public void Dispose()
        {
            localHook.ThreadACL.SetInclusiveACL(new Int32[] { 0 });
            localHook.Dispose();
        }
    }
}