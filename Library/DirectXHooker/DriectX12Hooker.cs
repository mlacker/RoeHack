using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using D3D12 = SharpDX.Direct3D12;

namespace RoeHack.Library.DirectXHooker
{
    public class DriectX12Hooker : IDirectXHooker
    {
        private readonly Parameter parameter;
        private readonly ILog logger;

        public DriectX12Hooker(Parameter parameter, ILog logger)
        {
            this.parameter = parameter;
            this.logger = logger;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate SharpDX.Result PresentDelegate(IntPtr swapChainPtr, int syncInterval, PresentFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void DrawInstancedDelegate(IntPtr commandListPtr, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void DrawIndexedInstancedDelegate(IntPtr commandListPtr, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void ExecuteCommandListsDelegate(IntPtr commandQueuePtr, int numCommandLists, D3D12.CommandList[] commandListsOut);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void SignalDelegate(IntPtr commandQueuePtr, D3D12.Fence fenceRef, long value);

        private HookWrapper<PresentDelegate> hookPresent;
        private HookWrapper<DrawInstancedDelegate> hookDrawInstanced;
        private HookWrapper<DrawIndexedInstancedDelegate> hookDrawIndexedInstanced;
        private HookWrapper<ExecuteCommandListsDelegate> hookExecuteCommandLists;
        private HookWrapper<SignalDelegate> hookSignalDelegate;

        private bool init = false;
        private D3D12.Device device;

        public void Hooking()
        {
            var address = GetProcAddress();

            hookPresent = new HookWrapper<PresentDelegate>(
                address[140], new PresentDelegate(PresentHook), this);

            hookDrawInstanced = new HookWrapper<DrawInstancedDelegate>(
                address[84], new DrawInstancedDelegate(DrawInstancedHook), this);

            hookDrawIndexedInstanced = new HookWrapper<DrawIndexedInstancedDelegate>(
                address[85], new DrawIndexedInstancedDelegate(DrawIndexedInstancedHook), this);

            hookExecuteCommandLists = new HookWrapper<ExecuteCommandListsDelegate>(
                address[54], new ExecuteCommandListsDelegate(ExecuteCommandListsHook), this);

            hookSignalDelegate = new HookWrapper<SignalDelegate>(
                address[58], new SignalDelegate(SignalHook), this);
        }

        public SharpDX.Result PresentHook(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            if (!init)
            {
                var swapChain = (SwapChain)swapChainPtr;
                this.device = swapChain.GetDevice<D3D12.Device>();

                init = true;
            }

            return hookPresent.Target(swapChainPtr, syncInterval, flags);
        }

        public void DrawInstancedHook(IntPtr commandListPtr, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            hookDrawInstanced.Target(commandListPtr, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
        }

        public void DrawIndexedInstancedHook(IntPtr commandListPtr, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            hookDrawIndexedInstanced.Target(commandListPtr, indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
        }

        public void ExecuteCommandListsHook(IntPtr commandQueuePtr, int numCommandLists, D3D12.CommandList[] commandListsOut)
        {
            hookExecuteCommandLists.Target(commandQueuePtr, numCommandLists, commandListsOut);
        }

        public void SignalHook(IntPtr commandQueuePtr, D3D12.Fence fenceRef, long value)
        {
            hookSignalDelegate.Target(commandQueuePtr, fenceRef, value);
        }

        private bool IsPlayers()
        {
            return false;
        }

        public void Dispose()
        {
            SharpDX.Utilities.Dispose(ref device);
        }

        #region Moved

        private List<IntPtr> GetProcAddress()
        {
            var address = new List<IntPtr>();

            // TODO

            return address;
        }

        #endregion
    }
}