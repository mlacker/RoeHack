using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.DXGI;

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

        D3D12.Device device;
        D3D12.DescriptorHeap descriptorHeapBackBuffers;
        D3D12.DescriptorHeap descriptorHeapImGuiRender;
        D3D12.GraphicsCommandList commandList;
        D3D12.Fence fence;
        long fenceValue;
        D3D12.CommandQueue commandQueue;

        struct FrameContext
        {
            public D3D12.CommandAllocator commandAllocator;
            public D3D12.Resource mainRanderTargetResource;
            public D3D12.CpuDescriptorHandle mainRanderTargetDescriptor;
        }

        int buffersCounts = -1;
        FrameContext[] frameContext;

        static bool init = false;
        bool shutdown = false;

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
            var swapChain = (SwapChain)swapChainPtr;

            if (!init)
            {
                // remarks
                device = swapChain.GetDevice<D3D12.Device>();

                var sdesc = swapChain.Description;
                sdesc.Flags = SwapChainFlags.AllowModeSwitch;
                //sdesc.OutputHandle = globals::mainWindow;
                //sdesc.IsWindowed = ((GetWindowLongPtr(globals::mainWindow, GWL_STYLE) & WS_POPUP) != 0) ? false : true;

                buffersCounts = sdesc.BufferCount;
                frameContext = new FrameContext[buffersCounts];

                var descriptorImGuiRender = new D3D12.DescriptorHeapDescription();
                descriptorImGuiRender.Type = D3D12.DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView;
                descriptorImGuiRender.DescriptorCount = buffersCounts;
                descriptorImGuiRender.Flags = D3D12.DescriptorHeapFlags.ShaderVisible;

                descriptorHeapImGuiRender = device.CreateDescriptorHeap(descriptorImGuiRender);

                var allocator = device.CreateCommandAllocator(D3D12.CommandListType.Direct);

                for (int i = 0; i < buffersCounts; i++)
                {
                    //frameContext[i] = new FrameContext();
                    frameContext[i].commandAllocator = allocator;
                }

                commandList = device.CreateCommandList(0, D3D12.CommandListType.Direct, allocator, null);

                var descriptorBackBuffers = new D3D12.DescriptorHeapDescription();
                descriptorBackBuffers.Type = D3D12.DescriptorHeapType.RenderTargetView;
                descriptorBackBuffers.DescriptorCount = buffersCounts;
                descriptorBackBuffers.Flags = D3D12.DescriptorHeapFlags.None;
                descriptorBackBuffers.NodeMask = 1;

                descriptorHeapBackBuffers = device.CreateDescriptorHeap(descriptorBackBuffers);

                var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(D3D12.DescriptorHeapType.RenderTargetView);
                var rtvHandle = descriptorHeapBackBuffers.CPUDescriptorHandleForHeapStart;

                for (int i = 0; i < buffersCounts; i++)
                {
                    D3D12.Resource backBuffer;

                    frameContext[i].mainRanderTargetDescriptor = rtvHandle;
                    backBuffer = swapChain.GetBackBuffer<D3D12.Resource>(i);
                    device.CreateRenderTargetView(backBuffer, null, rtvHandle);
                    frameContext[i].mainRanderTargetResource = backBuffer;
                    rtvHandle.Ptr += rtvDescriptorSize;
                }

                //ImGui_ImplWin32_Init(globals::mainWindow);
                //ImGui_ImplDX12_Init(d3d12Device, buffersCounts,
                //    DXGI_FORMAT_R8G8B8A8_UNORM, d3d12DescriptorHeapImGuiRender,
                //    d3d12DescriptorHeapImGuiRender->GetCPUDescriptorHandleForHeapStart(),
                //    d3d12DescriptorHeapImGuiRender->GetGPUDescriptorHandleForHeapStart());

                //ImGui_ImplDX12_CreateDeviceObjects();

                init = true;
            }

            if (shutdown == false)
            {
                if (commandQueue == null)
                    return hookPresent.Target(swapChainPtr, syncInterval, flags);

                var currentFrameContext = frameContext[swapChain.Description.BufferCount];
                currentFrameContext.commandAllocator.Reset();

                var barrier = new D3D12.ResourceBarrier();
                barrier.Type = D3D12.ResourceBarrierType.Transition;
                barrier.Flags = D3D12.ResourceBarrierFlags.None;
                barrier.Transition = new D3D12.ResourceTransitionBarrier(
                    currentFrameContext.mainRanderTargetResource,
                    -1,
                    D3D12.ResourceStates.Present,
                    D3D12.ResourceStates.RenderTarget
                    );

                commandList.Reset(currentFrameContext.commandAllocator, null);
                commandList.ResourceBarrier(barrier);
                commandList.SetRenderTargets(1, currentFrameContext.mainRanderTargetDescriptor, null);
                commandList.SetDescriptorHeaps(descriptorHeapImGuiRender);

                //ImGui::Render();
                //ImGui_ImplDX12_RenderDrawData(ImGui::GetDrawData(), d3d12CommandList);

                //barrier.Transition.StateBefore = D3D12.ResourceStates.RenderTarget;
                //barrier.Transition.StateAfter = D3D12.ResourceStates.Present;

                commandList.ResourceBarrier(barrier);
                commandList.Close();

                commandQueue.ExecuteCommandLists(commandList);
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
            if (commandQueue == null)
            {
                commandQueue = (D3D12.CommandQueue)commandQueuePtr;
            }

            hookExecuteCommandLists.Target(commandQueuePtr, numCommandLists, commandListsOut);
        }

        public void SignalHook(IntPtr commandQueuePtr, D3D12.Fence fenceRef, long value)
        {
            if (commandQueue != null && (D3D12.CommandQueue)commandQueuePtr == commandQueue)
            {
                fence = fenceRef;
                fenceValue = value;
            }

            hookSignalDelegate.Target(commandQueuePtr, fenceRef, value);
        }

        private bool IsPlayers(int stride, int vSize, int numVertices, int primCount)
        {
            if (stride == 72 && vSize > 1000 && vSize < 3025)
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            shutdown = true;
            // remarks
            device.Dispose();
            descriptorHeapBackBuffers.Dispose();
            descriptorHeapImGuiRender.Dispose();
            commandList.Dispose();
            fence.Dispose();
            commandQueue.Dispose();
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