using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using System;

using D3D11 = SharpDX.Direct3D11;

namespace DirectXSample
{
    class KikuSimairme : IDisposable
    {
        private RenderForm renderForm;

        private const int width = 1280;
        private const int height = 720;

        public KikuSimairme()
        {
            renderForm = new RenderForm("社会我荼哥");
            renderForm.ClientSize = new System.Drawing.Size(width, height);

            InitializeDeviceResources();
            InitializeTriangle();
            InitializeShaders();
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        private SwapChain swapChain;
        private D3D11.Device device;
        private D3D11.DeviceContext deviceContext;
        private D3D11.RenderTargetView renderTargetView;
        private Viewport viewport;

        private void InitializeDeviceResources()
        {
            var backBufferDesc =
                new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            var swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };
            
            D3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                D3D11.DeviceCreationFlags.None,
                swapChainDesc,
                out device, out swapChain);
            deviceContext = device.ImmediateContext;
            
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
            {
                renderTargetView = new D3D11.RenderTargetView(device, backBuffer);
            }

            viewport = new Viewport(0, 0, width, height);
            deviceContext.Rasterizer.SetViewport(viewport);
        }

        private void Draw()
        {
            deviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            deviceContext.ClearRenderTargetView(renderTargetView, ColorToRaw4(Color.Coral));

            deviceContext.InputAssembler.SetVertexBuffers(0,
                new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            deviceContext.Draw(vertices.Length, 0);

            swapChain.Present(1, PresentFlags.None);

            RawColor4 ColorToRaw4(Color color)
            {
                const float n = 255f;
                return new RawColor4(color.R / n, color.G / n, color.B / n, color.A / n);
            }
        }

        private Vector3[] vertices = new Vector3[]
           {new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f)};

        private D3D11.Buffer triangleVertexBuffer;

        private void InitializeTriangle()
        {
            triangleVertexBuffer = D3D11.Buffer.Create<Vector3>(device, D3D11.BindFlags.VertexBuffer, vertices);
        }

        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;
        private D3D11.InputLayout inputLayout;
        private ShaderSignature inputSignature;
        private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
        {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0)
        };

        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("VertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new D3D11.VertexShader(device, vertexShaderByteCode);
            }
            inputLayout = new D3D11.InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayout;

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("PixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new D3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            device.Dispose();
            deviceContext.Dispose();
            renderForm.Dispose();

            triangleVertexBuffer.Dispose();

            vertexShader.Dispose();
            pixelShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();
        }
    }
}
