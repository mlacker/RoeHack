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

        private bool initOnce = true;

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

            if (IsPlayers(stride, numVertices, primCount))
            {
                device.SetTexture(0, textureBack);
                device.SetRenderState(RenderState.ZEnable, false);

                hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);

                device.SetTexture(0, textureFront);
                device.SetRenderState(RenderState.ZEnable, true);
            }

            return hookDrawIndexedPrimitive.Target(devicePtr, arg0, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
        }

        public int PresentHook(IntPtr devicePtr, SharpDX.Rectangle[] pSourceRect, SharpDX.Rectangle[] pDestRect, IntPtr hDestWindowOverride, IntPtr pDirtyRegion)
        {
            if (initOnce)
            {
                SetColor(devicePtr);
                initOnce = false;
            }

            return hookPresent.Target(devicePtr, pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
        }

        struct Element
        {
            public int numVertices;
            public int primCount;

            public Element(int numVertices, int primCount)
            {
                this.numVertices = numVertices;
                this.primCount = primCount;
            }

            public override string ToString()
            {
                return $"NumVertices: {numVertices}, PrimCount: {primCount}";
            }
        }

        private List<Element> elements = new List<Element>()
        {
            // 雪地滑翔翼背包
            new Element(1791, 2934),
            // 海岛滑翔翼背包
            new Element(2006, 3100),
            // 自行车背包, 滑雪背包
            new Element(4127, 4458),
            // 抓钩背包, 登山背包
            new Element(4313, 6517),
        };

        private bool IsPlayers(int stride, int numVertices, int primCount)
        {
            var element = new Element(numVertices, primCount);

            if (stride == 72)
            {
                if (elements.Contains(element))
                {
                }
                else
                {
                    //elements.Add(element);
                    //logger.Debug(element.ToString());
                }

                return true;
            }

            return false;
        }

        private void SetColor(IntPtr devicePtr)
        {
            int _texWidth = 1, _texHeight = 1;
            Bitmap bmFront = new Bitmap(_texWidth, _texHeight);
            Graphics gFront = Graphics.FromImage(bmFront);
            gFront.FillRectangle(Brushes.Gold, new Rectangle(0, 0, _texWidth, _texHeight));
            string fileNameFront = "..//Front.jpg";
            bmFront.Save(fileNameFront);

            Bitmap bmBack = new Bitmap(_texWidth, _texHeight);
            Graphics gBack = Graphics.FromImage(bmBack);
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