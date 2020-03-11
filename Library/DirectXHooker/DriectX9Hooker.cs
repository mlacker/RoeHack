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

        private HookWrapper<Direct3D9Device_DrawIndexedPrimitiveDelegate> hookDrawIndexedPrimitive;
        private HookWrapper<PresentDelegate> hookPresent;
        private HookWrapper<Direct3D9Device_SetTextureDelegate> hookSetTexture;
        private HookWrapper<Direct3D9Device_SetStreamSourceDelegate> hookSetStreamSource;

        private bool firsted = true;
        private SharpDX.Direct3D9.Font font;
        private int vSize;
        private uint stride;

        private Texture textureBack;
        private Texture textureFront;

        public DriectX9Hooker(Parameter parameter, ILog logger)
        {
            this.parameter = parameter;
            this.logger = logger;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate int Direct3D9Device_DrawIndexedPrimitiveDelegate(IntPtr devicePtr, PrimitiveType arg0, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Direct3D9Device_SetStreamSourceDelegate(IntPtr devicePtr, uint StreamNumber, IntPtr pStreamData, uint OffsetInBytes, uint sStride);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Direct3D9Device_SetTextureDelegate(IntPtr devicePtr, uint Sampler, IntPtr pTexture);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int PresentDelegate(IntPtr devicePtr, SharpDX.Rectangle[] pSourceRect, SharpDX.Rectangle[] pDestRect, IntPtr hDestWindowOverride, IntPtr pDirtyRegion);


        public void Hooking()
        {
            var address = GetAddress();

            hookDrawIndexedPrimitive = new HookWrapper<Direct3D9Device_DrawIndexedPrimitiveDelegate>(
                address[82], new Direct3D9Device_DrawIndexedPrimitiveDelegate(DrawIndexedPrimitiveHook), this);

            hookPresent = new HookWrapper<PresentDelegate>(
                address[17], new PresentDelegate(PresentHook), this);

            hookSetTexture = new HookWrapper<Direct3D9Device_SetTextureDelegate>(
                address[65], new Direct3D9Device_SetTextureDelegate(SetTextureHook),
                this);

            hookSetStreamSource = new HookWrapper<Direct3D9Device_SetStreamSourceDelegate>(
                address[100], new Direct3D9Device_SetStreamSourceDelegate(SetStreamSourceHook),
                this);
        }

        public int DrawIndexedPrimitiveHook(IntPtr devicePtr, PrimitiveType arg0, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount)
        {
            var device = new Device(devicePtr);
            if (IsPlayers((int)stride, vSize, numVertices, primCount))
            {
                //设置墙后颜色
                device.SetRenderState(RenderState.Lighting, false);
                device.SetRenderState(RenderState.ZEnable, false);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                device.SetTexture(0, textureBack);
                hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);

                device.SetRenderState(RenderState.ZEnable, true);
                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                device.SetTexture(0, textureFront); hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
            }

            return hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
        }

        public int PresentHook(IntPtr devicePtr, SharpDX.Rectangle[] pSourceRect, SharpDX.Rectangle[] pDestRect, IntPtr hDestWindowOverride, IntPtr pDirtyRegion)
        {
            if (firsted)
            {
                firsted = false;
                SetFont(devicePtr);
                SetColor(devicePtr);
            }

            //this.font.DrawText(null, "挂载成功", 50, 50, SharpDX.Color.Red);

            return hookPresent.Target(devicePtr, pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
        }

        public int SetTextureHook(IntPtr devicePtr, uint Sampler, IntPtr pTexture)
        {
            var device = new Device(devicePtr);

            var vShader = device.VertexShader;
            if (vShader != null)
            {
                if (vShader.Function.BufferSize != null)
                {
                    this.vSize = vShader.Function.BufferSize;
                    vShader.Function.Dispose();
                }
                vShader.Dispose();
            }

            return hookSetTexture.Target(devicePtr, Sampler, pTexture);
        }

        public int SetStreamSourceHook(IntPtr devicePtr, uint StreamNumber, IntPtr pStreamData, uint OffsetInBytes, uint sStride)
        {
            if (StreamNumber == 0)
            {
                this.stride = sStride;
            }
            return hookSetStreamSource.Target(devicePtr, StreamNumber, pStreamData, OffsetInBytes, sStride);
        }

        private bool IsPlayers(int stride, int vSize, int numVertices, int primCount)
        {
            if (stride == 72 && vSize > 1000 && vSize < 3025)
            {
                return true;
            }

            return false;
        }

        private void SetFont(IntPtr devicePtr)
        {
            this.font = new SharpDX.Direct3D9.Font((Device)devicePtr, new FontDescription()
            {
                Height = 40,
                FaceName = "Arial",
                Italic = false,
                Width = 0,
                MipLevels = 1,
                CharacterSet = FontCharacterSet.Default,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased,
                PitchAndFamily = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare,
                Weight = FontWeight.Bold
            });
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
            hookSetStreamSource.Dispose();
            hookSetTexture.Dispose();
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