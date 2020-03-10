using RoeHack.Library.Core;
using RoeHack.Library.DirectXHooker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D;
using System.Windows.Forms;
using System.Drawing;
using RoeHack.Library.DirectXHooker.Dx11;

namespace RoeHack.Library.DirectXHooker
{
    enum D3D11DeviceVTbl : short
    {
        // IUnknown
        QueryInterface = 0,
        AddRef = 1,
        Release = 2,

        // ID3D11Device
        CreateBuffer = 3,
        CreateTexture1D = 4,
        CreateTexture2D = 5,
        CreateTexture3D = 6,
        CreateShaderResourceView = 7,
        CreateUnorderedAccessView = 8,
        CreateRenderTargetView = 9,
        CreateDepthStencilView = 10,
        CreateInputLayout = 11,
        CreateVertexShader = 12,
        CreateGeometryShader = 13,
        CreateGeometryShaderWithStreamOutput = 14,
        CreatePixelShader = 15,
        CreateHullShader = 16,
        CreateDomainShader = 17,
        CreateComputeShader = 18,
        CreateClassLinkage = 19,
        CreateBlendState = 20,
        CreateDepthStencilState = 21,
        CreateRasterizerState = 22,
        CreateSamplerState = 23,
        CreateQuery = 24,
        CreatePredicate = 25,
        CreateCounter = 26,
        CreateDeferredContext = 27,
        OpenSharedResource = 28,
        CheckFormatSupport = 29,
        CheckMultisampleQualityLevels = 30,
        CheckCounterInfo = 31,
        CheckCounter = 32,
        CheckFeatureSupport = 33,
        GetPrivateData = 34,
        SetPrivateData = 35,
        SetPrivateDataInterface = 36,
        GetFeatureLevel = 37,
        GetCreationFlags = 38,
        GetDeviceRemovedReason = 39,
        GetImmediateContext = 40,
        SetExceptionMode = 41,
        GetExceptionMode = 42,
    }

    public enum DXGISwapChainVTbl : short
    {
        // IUnknown
        QueryInterface = 0,
        AddRef = 1,
        Release = 2,
        // IDXGIObject
        SetPrivateData = 3,
        SetPrivateDataInterface = 4,
        GetPrivateData = 5,
        GetParent = 6,
        // IDXGIDeviceSubObject
        GetDevice = 7,
        // IDXGISwapChain
        Present = 8,
        GetBuffer = 9,
        SetFullscreenState = 10,
        GetFullscreenState = 11,
        GetDesc = 12,
        ResizeBuffers = 13,
        ResizeTarget = 14,
        GetContainingOutput = 15,
        GetFrameStatistics = 16,
        GetLastPresentCount = 17,
    }

    public enum D3D11DeviceCONTEXTVTbl
    {
        // IUnknown
        QueryInterface = 0,
        AddRef = 1,
        Release = 2,
        GetDevice = 3,
        DrawIndexed = 12,
        PSSetShaderResources = 8
    }

    class DriectX11Hooker : IDirectXHooker
    {
        private const int DXGI_SWAPCHAIN_METHOD_COUNT = 18;
        private const int D3D11_DEVICE_METHOD_COUNT = 43;
        private int D3D11_DEVICECONTEXT_METHOD_COUNT = 113;

        List<IntPtr> _d3d11VTblAddresses = null;
        List<IntPtr> _pContextVTablAddresses = null;
        List<IntPtr> _dxgiSwapChainVTblAddresses = null;

        private readonly Parameter parameter;

        protected ShaderResourceView srvFront { get; set; }
        protected ShaderResourceView srvback { get; set; }


        protected DepthStencilState depthStencilStateDisabled { get; set; }
        protected DepthStencilState depthStencilStateReadonly { get; set; }
        protected bool isFirst { get; set; }

        private HookWrapper<DXGISwapChain_PresentDelegate> DXGISwapChain_PresentHookPrimitive;
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DXGISwapChain_PresentDelegate(IntPtr swapChainPtr, int syncInterval, /* int */ SharpDX.DXGI.PresentFlags flags);

        private HookWrapper<DXGIdeviceContext_DrawIndexedDelegate> DXGIdeviceContext_DrawIndexedPrimitive;
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DXGIdeviceContext_DrawIndexedDelegate(IntPtr deviceContextPtr, int IndexCount, int StartIndexLocation, int BaseVertexLocation);


        public DriectX11Hooker(Parameter parameter)
        {
            this.parameter = parameter;
        }

        public void Hooking()
        {
            GetAddress();
            DXGISwapChain_PresentHookPrimitive = new HookWrapper<DXGISwapChain_PresentDelegate>(
                _dxgiSwapChainVTblAddresses[(int)DXGISwapChainVTbl.Present], new DXGISwapChain_PresentDelegate(Present), this);

            DXGIdeviceContext_DrawIndexedPrimitive = new HookWrapper<DXGIdeviceContext_DrawIndexedDelegate>(
                _pContextVTablAddresses[(int)D3D11DeviceCONTEXTVTbl.DrawIndexed], new DXGIdeviceContext_DrawIndexedDelegate(DrawIndexed), this);

        }

        protected IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            return GetVTblAddresses(pointer, 0, numberOfMethods);
        }

        protected IntPtr[] GetVTblAddresses(IntPtr pointer, int startIndex, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = startIndex; i < startIndex + numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size)); // using IntPtr.Size allows us to support both 32 and 64-bit processes

            return vtblAddresses.ToArray();
        }


        ShaderResourceView GetShaderResourceView(SharpDX.Direct3D11.Device device, Brush brushes)
        {
            var _texWidth = 1;
            var _texHeight = 1;
            Bitmap re = new Bitmap(1, 1);
            Graphics g1 = Graphics.FromImage(re); //创建b1的Graphics
            g1.FillRectangle(brushes, new System.Drawing.Rectangle(0, 0, _texWidth, _texHeight));
            var bmData = re.LockBits(new System.Drawing.Rectangle(0, 0, _texWidth, _texHeight), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = _texWidth;
            texDesc.Height = _texHeight;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            texDesc.SampleDescription.Count = 1;
            texDesc.SampleDescription.Quality = 0;
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;
            SharpDX.DataBox data;
            data.DataPointer = bmData.Scan0;
            data.RowPitch = _texWidth * 4;
            data.SlicePitch = 0;
            var _fontSheetTex = new SharpDX.Direct3D11.Texture2D(device, texDesc, new[] { data });
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
            srvDesc.Texture2D.MipLevels = 1;
            srvDesc.Texture2D.MostDetailedMip = 0;
            g1.Dispose();
            re.Dispose();
            return new ShaderResourceView(device, _fontSheetTex, srvDesc);
        }

        private int Present(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            SwapChain swapChain = (SharpDX.DXGI.SwapChain)swapChainPtr;
            swapChain.GetDevice(typeof(SharpDX.Direct3D11.Device).GUID, out IntPtr deviceOut);
            var device = new SharpDX.Direct3D11.Device(deviceOut);
            DeviceContext deviceContext = new DeviceContext(device);
            if (this.isFirst)
            {
                isFirst = false;
                this.srvFront = GetShaderResourceView(device, Brushes.Gold);
                this.srvback = GetShaderResourceView(device, Brushes.Red);

                var stencilDesc = new DepthStencilStateDescription()
                {
                    DepthComparison = Comparison.Less,
                    IsDepthEnabled = true,
                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    },
                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    }
                };
                stencilDesc.IsDepthEnabled = false;
                stencilDesc.DepthWriteMask = DepthWriteMask.All;
                depthStencilStateDisabled = new DepthStencilState(deviceContext.Device, stencilDesc);


                stencilDesc.IsDepthEnabled = true;
                stencilDesc.DepthWriteMask = DepthWriteMask.All;
                stencilDesc.DepthComparison = Comparison.GreaterEqual;
                stencilDesc.IsStencilEnabled = false;
                stencilDesc.StencilReadMask = 0xFF;
                stencilDesc.StencilWriteMask = 0x0;

                stencilDesc.FrontFace.FailOperation = StencilOperation.Zero;
                stencilDesc.FrontFace.DepthFailOperation = StencilOperation.Zero;
                stencilDesc.FrontFace.PassOperation = StencilOperation.Keep;
                stencilDesc.FrontFace.Comparison = Comparison.Equal;

                stencilDesc.BackFace.FailOperation = StencilOperation.Zero;
                stencilDesc.BackFace.DepthFailOperation = StencilOperation.Zero;
                stencilDesc.BackFace.PassOperation = StencilOperation.Zero;
                stencilDesc.BackFace.Comparison = Comparison.Never;
                depthStencilStateReadonly = new DepthStencilState(deviceContext.Device, stencilDesc);
            }

            Dx11.DXFont dXFont = new Dx11.DXFont(device, deviceContext);
            dXFont.Initialize("宋体", 14, FontStyle.Bold, true);
            DXSprite dXSprite = new DXSprite(device, deviceContext);
            dXSprite.DrawString(50, 50, "ceshi", 255, 255, 233, 233, dXFont);


            swapChain.Present(syncInterval, flags);
            return SharpDX.Result.Ok.Code;
        }

        int DrawIndexed(IntPtr deviceContextPtr, int IndexCount, int StartIndexLocation, int BaseVertexLocation)
        {
            SharpDX.Direct3D11.Buffer bufferA;
            Format format;
            int offset;
            DeviceContext deviceContext = ((DeviceContext)deviceContextPtr);
            deviceContext.InputAssembler.GetIndexBuffer(out bufferA, out format, out offset); ;
            bufferA.Dispose();
            if (IsPlayer())
            {
                deviceContext.OutputMerger.SetDepthStencilState(depthStencilStateDisabled, 1);
                deviceContext.PixelShader.SetShaderResource(1, this.srvback);
                deviceContext.DrawIndexed(IndexCount, StartIndexLocation, BaseVertexLocation);

                deviceContext.PixelShader.SetShaderResource(1, this.srvFront);
                deviceContext.OutputMerger.SetDepthStencilState(depthStencilStateReadonly, 30);
                deviceContext.DrawIndexed(IndexCount, StartIndexLocation, BaseVertexLocation);
            }
            else
            {
                DXGIdeviceContext_DrawIndexedPrimitive.Target(deviceContextPtr, IndexCount, StartIndexLocation, BaseVertexLocation);
            }

            return SharpDX.Result.Ok.Code;
        }

        bool IsPlayer()
        {
            return true;
        }


        private void GetAddress()
        {
            SharpDX.Direct3D11.Device device;
            SwapChain swapChain;
            SharpDX.Direct3D11.DeviceContext deviceContext;
            using (var renderForm = new Form())
            {
                SharpDX.Direct3D11.Device.CreateWithSwapChain(
                   DriverType.Hardware,
                   DeviceCreationFlags.None,
                   CreateSwapChainDescription(renderForm.Handle),
                   out device,
                   out swapChain);

                if (device != null && swapChain != null)
                {
                    deviceContext = new DeviceContext(device);
                    using (device)
                    {
                        _d3d11VTblAddresses.AddRange(GetVTblAddresses(device.NativePointer, D3D11_DEVICE_METHOD_COUNT));

                        using (swapChain)
                        {
                            _dxgiSwapChainVTblAddresses.AddRange(GetVTblAddresses(swapChain.NativePointer, DXGI_SWAPCHAIN_METHOD_COUNT));
                        }
                        using (deviceContext)
                        {
                            _pContextVTablAddresses.AddRange(GetVTblAddresses(deviceContext.NativePointer, D3D11_DEVICECONTEXT_METHOD_COUNT));
                        }
                    }
                }
            }
        }




        public static SharpDX.DXGI.SwapChainDescription CreateSwapChainDescription(IntPtr windowHandle)
        {
            return new SharpDX.DXGI.SwapChainDescription
            {
                BufferCount = 1,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new SharpDX.DXGI.ModeDescription(1920, 1080, new Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                OutputHandle = windowHandle,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput
            };
        }


        public void Dispose()
        {
            DXGISwapChain_PresentHookPrimitive.Dispose();
            DXGIdeviceContext_DrawIndexedPrimitive.Dispose();
        }
    }
}
