using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                device.SetRenderState(RenderState.ZEnable, false);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                device.SetTexture(0, textureBack);
                hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);

                device.SetRenderState(RenderState.ZEnable, true);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
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

            return hookPresent.Target(devicePtr, pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
        }

        private bool IsPlayers(int stride, int vSize, int numVertices, int primCount)
        {
            if (stride == 72 && vSize > 1000 && vSize < 3025)
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
            gFront.FillRectangle(Brushes.Gold, new Rectangle(0, 0, _texWidth, _texHeight));
            string fileNameFront = "..//Front.jpg";
            bmFront.Save(fileNameFront);

            Bitmap bmBack = new Bitmap(_texWidth, _texHeight);
            Graphics gBack = Graphics.FromImage(bmBack); //创建b1的Graphics
            gBack.FillRectangle(Brushes.Red, new Rectangle(0, 0, _texWidth, _texHeight));
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