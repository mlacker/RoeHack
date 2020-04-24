using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RoeHack.Library.DirectXHooker
{

    public class DriectX9Hooker : IDirectXHooker
    {
        private readonly Parameter parameter;
        private readonly ILog logger;

        private HookWrapper<DrawIndexedPrimitiveDelegate> hookDrawIndexedPrimitive;
        private HookWrapper<PresentDelegate> hookPresent;

        private bool firsted = true;

        private Texture textureBack;
        private Texture textureFront;

        public DriectX9Hooker(Parameter parameter, ILog logger)
        {
            this.parameter = parameter;
            this.logger = logger;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate int DrawIndexedPrimitiveDelegate(IntPtr devicePtr, PrimitiveType primitiveType, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int SetStreamSourceDelegate(IntPtr devicePtr, uint streamNumber, IntPtr streamDataPtr, uint offsetInBytes, uint stride);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int SetTextureDelegate(IntPtr devicePtr, uint sampler, IntPtr texturePtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int PresentDelegate(IntPtr devicePtr, SharpDX.Rectangle[] sourceRect, SharpDX.Rectangle[] destRect, IntPtr destWindowOverridePtr, IntPtr dirtyRegionPtr);


        public void Hooking()
        {
            var address = GetAddress();

            hookDrawIndexedPrimitive = new HookWrapper<DrawIndexedPrimitiveDelegate>(
                address[82], new DrawIndexedPrimitiveDelegate(DrawIndexedPrimitiveHook), this);

            hookPresent = new HookWrapper<PresentDelegate>(
                address[17], new PresentDelegate(PresentHook), this);
        }

        public int DrawIndexedPrimitiveHook(IntPtr devicePtr, PrimitiveType arg0, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount)
        {
            var device = (Device)devicePtr;

            device.GetStreamSource(0, out var streamData, out var offsetInBytes, out var stride);
            streamData?.Dispose();

            var vSize = 0;
            var vShader = device.VertexShader;
            if (vShader != null)
            {
                if (vShader.Function.BufferSize != null)
                {
                    vSize = vShader.Function.BufferSize;
                    vShader.Function.Dispose();
                }
                vShader.Dispose();
            }

            if (IsPlayers(stride, vSize, numVertices, primCount))
            {
                //设置墙后颜色
                device.SetRenderState(RenderState.Lighting, false);

                device.SetRenderState(RenderState.DepthBias, 100000f);
                //device.SetRenderState(RenderState.ZEnable, false);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                device.SetTexture(0, textureFront);
                hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);

                device.SetRenderState(RenderState.ZEnable, true);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                
                device.SetRenderState(RenderState.DepthBias, 0.000001f);
                device.SetTexture(0, textureFront);
                hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
            }
            return hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
        }

        public int PresentHook(IntPtr devicePtr, SharpDX.Rectangle[] pSourceRect, SharpDX.Rectangle[] pDestRect, IntPtr hDestWindowOverride, IntPtr pDirtyRegion)
        {
            if (firsted)
            {
                firsted = false;
                SetColor(devicePtr);
            }
            //DoCapture((Device)devicePtr);
            return hookPresent.Target(devicePtr, pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
        }



        Surface _renderTargetCopy;
        Surface _resolvedTarget;
        bool isCapturing = false;
        void DoCapture(Device device)
        {
            try
            {
                if (!isCapturing)
                {
                    //SharpDX.Rectangle rect;
                    //SharpDX.DataRectangle lockedRect = LockRenderTarget(_renderTargetCopy, out rect);
                    //Copy the data from the render target
                    //System.Threading.Tasks.Task.Factory.StartNew(() =>
                    //{
                    //    lock (_lockRenderTarget)
                    //    {
                    //        ProcessCapture(rect.Width, rect.Height, lockedRect.Pitch, _renderTargetCopy.Description.Format.ToPixelFormat(), lockedRect.DataPointer, _requestCopy);
                    //    }
                    //});
                    isCapturing = true;
                    using (Surface renderTarget = device.GetRenderTarget(0))
                    {
                        var width = renderTarget.Description.Width;
                        var height = renderTarget.Description.Height;
                        if (_renderTargetCopy == null)
                        {
                            CreateResources(device, width, height, renderTarget.Description.Format);
                        }
                        device.StretchRectangle(renderTarget, _resolvedTarget, TextureFilter.None);
                        device.GetRenderTargetData(_resolvedTarget, _renderTargetCopy);
                        var data=Surface.ToStream(_renderTargetCopy, ImageFileFormat.Png);
                        
                        StreamToFile(data, "d://2.png");
                        isCapturing = false;
                    }
                }
            }
            catch (Exception e)
            {
                this.logger.Error("截图错误", e);
            }
        }


        private SharpDX.DataRectangle LockRenderTarget(Surface _renderTargetCopy, out SharpDX.Rectangle rect)
        {
            rect = new SharpDX.Rectangle(0, 0, _renderTargetCopy.Description.Width, _renderTargetCopy.Description.Height);
            return _renderTargetCopy.LockRectangle(rect, LockFlags.ReadOnly);
        }


        public void StreamToFile(Stream stream, string fileName)

        {

            // 把 Stream 转换成 byte[]

            byte[] bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始

            stream.Seek(0, SeekOrigin.Begin);

            // 把 byte[] 写入文件

            FileStream fs = new FileStream(fileName, FileMode.Create);

            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(bytes);

            bw.Close();

            fs.Close();

        }



        private void CreateResources(Device device, int width, int height, Format format)
        {
            // Create offscreen surface to use as copy of render target data
            _renderTargetCopy = ToDispose1(Surface.CreateOffscreenPlain(device, width, height, format, Pool.SystemMemory));

            // Create our resolved surface (resizing if necessary and to resolve any multi-sampling)
            _resolvedTarget = ToDispose1(Surface.CreateRenderTarget(device, width, height, format, MultisampleType.None, 0, false));
        }

        protected DisposeCollector DisposeCollector { get; set; }

        /// <summary>
        /// Adds a disposable object to the list of the objects to dispose.
        /// </summary>
        /// <param name="toDisposeArg">To dispose.</param>
        protected internal T ToDispose1<T>(T toDisposeArg)
        {
            if (!ReferenceEquals(toDisposeArg, null))
            {
                if (DisposeCollector == null)
                    DisposeCollector = new DisposeCollector();
                return DisposeCollector.Collect(toDisposeArg);
            }
            return default(T);
        }

        /// <summary>
        /// Adds a disposable object to the list of the objects to dispose.
        /// </summary>
        /// <param name="toDisposeArg">To dispose.</param>
        protected internal T ToDispose<T>(T toDisposeArg)
        {
            if (!ReferenceEquals(toDisposeArg, null))
            {
                if (DisposeCollector == null)
                    DisposeCollector = new DisposeCollector();
                return DisposeCollector.Collect(toDisposeArg);
            }
            return default(T);
        }



        private bool IsPlayers(int stride, int vSize, int numVertices, int primCount)
        {
            if (stride == 72 && vSize > 1500 && vSize < 3525)
            {
                return true;
            }

            return false;
        }

        private void SetColor(IntPtr devicePtr)
        {
            int _texWidth = 1, _texHeight = 1;
            Bitmap bmFront = new Bitmap(_texWidth, _texHeight);
            Graphics gFront = Graphics.FromImage(bmFront); //创建b1的Graphics
            gFront.FillRectangle(Brushes.Blue, new System.Drawing.Rectangle(0, 0, _texWidth, _texHeight));
            string fileNameFront = "..//Front.jpg";
            bmFront.Save(fileNameFront);

            Bitmap bmBack = new Bitmap(_texWidth, _texHeight);
            Graphics gBack = Graphics.FromImage(bmBack); //创建b1的Graphics
            gBack.FillRectangle(Brushes.Red, new System.Drawing.Rectangle(0, 0, _texWidth, _texHeight));
            string fileNameBack = "..//back.jpg";
            bmBack.Save(fileNameBack);

            textureBack = Texture.FromFile((Device)devicePtr, fileNameFront);
            textureFront = Texture.FromFile((Device)devicePtr, fileNameBack);

        }

        public void Dispose()
        {
            hookDrawIndexedPrimitive.Dispose();
            hookPresent.Dispose();
        }

        #region Moved

        private List<IntPtr> GetAddress()
        {
            var address = new List<IntPtr>();

            using (var d3d = new Direct3D())
            using (var renderForm = new Form())
            using (var device = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderForm.Handle }))
            {
                IntPtr vTable = Marshal.ReadIntPtr(device.NativePointer);
                for (int i = 0; i < 119; i++)
                    address.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            }

            return address;
        }

        #endregion
    }
}