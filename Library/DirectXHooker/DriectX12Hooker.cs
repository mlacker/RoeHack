using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
        public delegate void ExecuteCommandListsDelegate(IntPtr commandQueuePtr, int numCommandLists, IntPtr[] commandListsOut);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void SignalDelegate(IntPtr commandQueuePtr, IntPtr fenceRef, long value);

        private HookWrapper<PresentDelegate> hookPresent;
        private HookWrapper<DrawInstancedDelegate> hookDrawInstanced;
        private HookWrapper<DrawIndexedInstancedDelegate> hookDrawIndexedInstanced;
        private HookWrapper<ExecuteCommandListsDelegate> hookExecuteCommandLists;
        private HookWrapper<SignalDelegate> hookSignalDelegate;

        private bool init = false;
        private SwapChain3 swapChain;
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
                swapChain = SwapChain3.FromPointer<SwapChain3>(swapChainPtr);
                device = swapChain.GetDevice<D3D12.Device>();

                init = true;
            }

            return swapChain.Present(syncInterval, flags);
        }

        public void DrawInstancedHook(IntPtr commandListPtr, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            hookDrawInstanced.Target(commandListPtr, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
        }

        public void DrawIndexedInstancedHook(IntPtr commandListPtr, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            hookDrawIndexedInstanced.Target(commandListPtr, indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
        }

        public void ExecuteCommandListsHook(IntPtr commandQueuePtr, int numCommandLists, IntPtr[] commandListsOut)
        {
            hookExecuteCommandLists.Target(commandQueuePtr, numCommandLists, commandListsOut);
        }

        public void SignalHook(IntPtr commandQueuePtr, IntPtr fenceRef, long value)
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

        private SwapChain3 _swapChain;
        private SharpDX.Direct3D12.Device _device12;
        private CommandQueue _commandQueue;
        private CommandAllocator _commandAllocator;
        private GraphicsCommandList _commandList;

        private List<IntPtr> GetProcAddress()
        {
            var address = new List<IntPtr>();

            _device12 = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);
            using (var renderForm = new Form())
            {
                using (var factory = new SharpDX.DXGI.Factory4())
                {
                    _commandQueue
                        = _device12.CreateCommandQueue(new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct));

                    _commandAllocator
                        = _device12.CreateCommandAllocator(CommandListType.Direct);

                    _commandList
                        = _device12.CreateCommandList(CommandListType.Direct, _commandAllocator, null);

                    var swapChainDesc = new SharpDX.DXGI.SwapChainDescription()
                    {
                        BufferCount = 2,
                        ModeDescription = new SharpDX.DXGI.ModeDescription(100, 100, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                        Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                        SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                        OutputHandle = renderForm.Handle,
                        Flags = SwapChainFlags.AllowModeSwitch,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        IsWindowed = true
                    };

                    var tempSwapChain = new SharpDX.DXGI.SwapChain(factory, _commandQueue, swapChainDesc);
                    _swapChain = tempSwapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
                    tempSwapChain.Dispose();
                }

                if (_device12 != null && _swapChain != null)
                {
                    address.AddRange(GetVTblAddresses(_device12.NativePointer, 44));
                    address.AddRange(GetVTblAddresses(_commandQueue.NativePointer, 19));
                    address.AddRange(GetVTblAddresses(_commandAllocator.NativePointer, 9));
                    address.AddRange(GetVTblAddresses(_commandList.NativePointer, 60));
                    address.AddRange(GetVTblAddresses(_swapChain.NativePointer, 18));

                    _device12.Dispose();
                    _device12 = null;

                    _commandQueue.Dispose();
                    _commandQueue = null;

                    _commandAllocator.Dispose();
                    _commandAllocator = null;

                    _commandList.Dispose();
                    _commandList = null;

                    _swapChain.Dispose();
                    _swapChain = null;
                }
            }

            return address;
        }

        protected List<IntPtr> GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
            {
                var ptr = Marshal.ReadIntPtr(vTable, i * IntPtr.Size);
                vtblAddresses.Add(ptr); // using IntPtr.Size allows us to support both 32 and 64-bit processes
            }

            return vtblAddresses;
        }

        #endregion
    }
}