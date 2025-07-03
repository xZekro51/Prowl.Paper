// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Prowl.Quill;
using Prowl.Vector;

using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace VeldridSample;
internal class VeldridGUIRenderer : ICanvasRenderer
{
    private readonly Sdl2Window _window;
    public Sdl2Window Window => _window;

    public VeldridGUIInput Input => _input ??= new VeldridGUIInput(_window);

    private VeldridGUIInput _input;

    private readonly GraphicsDevice _graphicsDevice;
    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    private CommandList? _commandList;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;
    private Shader[]? _shaders;
    private Pipeline? _pipeline;

    private DeviceBuffer? _brushBuffer;
    private ResourceLayout? _brushLayout;
    private ResourceSet? _brushSet;

    private Texture? _defaultTexture;
    private TextureView? _defaultTextureView;
    private ResourceLayout? _textureLayout;
    private ResourceSet? _textureSet;
    private Sampler? _sampler;

    private DeviceBuffer? _projectionBuffer;
    private ResourceLayout? _projectionLayout;
    private ResourceSet? _projectionSet;

    private readonly float _dpiScale;

    public float DPIScale => _dpiScale;

    private struct Vertex
    {
        public Vector2 Position;
        public Vector2 TexCoord;
        public RgbaFloat Color;
        public const uint SizeInBytes = 48; // 8 + 8 + 16 bytes
    }

    private struct BrushBuffer
    {
        public Matrix4x4 BrushMat;
        public Vector4 BrushColor1;
        public Vector4 BrushColor2;
        public Vector4 BrushParams;
        public Vector2 BrushParams2;
        public int BrushType;
        public int Padding;
        public const uint SizeInBytes = 256; // 64 + 32 + 32 + 32 + 16 + 4 + 4 bytes
    }

    private struct ProjectionBuffer
    {
        public Matrix4x4 Projection;
        public const uint SizeInBytes = 64; // 4x4 matrix = 64 bytes
    }

    /*private const string BASE_VertexCode = @"
    #version 450

    layout(location = 0) in vec2 Position;
    layout(location = 1) in vec2 TexCoord;
    layout(location = 2) in vec4 Color;

    layout(set = 3, binding = 0) uniform ProjectionBuffer {
        mat4 Projection;
    };

    layout(location = 0) out vec2 fsin_TexCoord;
    layout(location = 1) out vec4 fsin_Color;
    layout(location = 2) out vec2 fsin_Position;

    void main()
    {
        gl_Position = Projection * vec4(Position, 0, 1);
        fsin_TexCoord = TexCoord;
        fsin_Position = Position;
        fsin_Color = Color;
    }";


    private const string BASE_FragmentCode = @"
    #version 450

    layout(location = 0) in vec2 fsin_TexCoord;
    layout(location = 1) in vec4 fsin_Color;
    layout(location = 2) in vec2 fsin_Position;

    layout(location = 0) out vec4 fsout_Color;

    layout(set = 0, binding = 0) uniform texture2D Texture;
    layout(set = 0, binding = 1) uniform sampler Sampler;

    // Scissoring uniforms
    layout(set = 1, binding = 0) uniform ScissorBuffer {
        mat4 scissorMat;
        vec2 scissorExt;
    };

    // Brush uniforms
    layout(set = 2, binding = 0) uniform BrushBuffer {
        mat4 brushMat;
        vec4 brushColor1;    // Start color
        vec4 brushColor2;    // End color
        vec4 brushParams;    // x,y = start point, z,w = end point (or center+radius for radial)
        vec2 brushParams2;   // x = Box radius, y = Box Feather
        int brushType;       // 0=none, 1=linear, 2=radial, 3=box
    };

    float calculateBrushFactor() {
        // No brush
        if (brushType == 0) return 0.0;
        
        vec2 transformedPoint = (brushMat * vec4(fsin_Position, 0.0, 1.0)).xy;

        // Linear brush
        if (brushType == 1) {
            vec2 startPoint = brushParams.xy;
            vec2 endPoint = brushParams.zw;
            vec2 line = endPoint - startPoint;
            float lineLength = length(line);
            
            if (lineLength < 0.001) return 0.0;
            
            vec2 posToStart = transformedPoint - startPoint;
            float projection = dot(posToStart, line) / (lineLength * lineLength);
            return clamp(projection, 0.0, 1.0);
        }
        
        // Radial brush
        if (brushType == 2) {
            vec2 center = brushParams.xy;
            float innerRadius = brushParams.z;
            float outerRadius = brushParams.w;
            
            if (outerRadius < 0.001) return 0.0;
            
            float distance = smoothstep(innerRadius, outerRadius, length(transformedPoint - center));
            return clamp(distance, 0.0, 1.0);
        }
        
        // Box brush
        if (brushType == 3) {
            vec2 center = brushParams.xy;
            vec2 halfSize = brushParams.zw;
            float radius = brushParams2.x;
            float feather = brushParams2.y;
            
            if (halfSize.x < 0.001 || halfSize.y < 0.001) return 0.0;
            
            vec2 q = abs(transformedPoint - center) - (halfSize - vec2(radius));
            float dist = min(max(q.x,q.y),0.0) + length(max(q,0.0)) - radius;
            return clamp((dist + feather * 0.5) / feather, 0.0, 1.0);
        }
        
        return 0.0;
    }

    float scissorMask(vec2 p) {
        if(scissorExt.x <= 0.0) return 1.0;
        
        vec2 transformedPoint = (scissorMat * vec4(p, 0.0, 1.0)).xy;
        vec2 distanceFromEdges = abs(transformedPoint) - scissorExt;
        vec2 smoothEdges = vec2(0.5, 0.5) - distanceFromEdges;
        return clamp(smoothEdges.x, 0.0, 1.0) * clamp(smoothEdges.y, 0.0, 1.0);
    }

    void main_test()
    {
        fsout_Color = fsin_Color;
    }

    void main()
    {
        vec2 pixelSize = fwidth(fsin_TexCoord);
        vec2 edgeDistance = min(fsin_TexCoord, 1.0 - fsin_TexCoord);
        float edgeAlpha = smoothstep(0.0, pixelSize.x, edgeDistance.x) * smoothstep(0.0, pixelSize.y, edgeDistance.y);
        edgeAlpha = clamp(edgeAlpha, 0.0, 1.0);
        
        float mask = scissorMask(fsin_Position);
        vec4 color = fsin_Color;

        // Apply brush if active
        if (brushType > 0) {
            float factor = calculateBrushFactor();
            color = mix(brushColor1, brushColor2, factor);
        }
        
        vec4 texColor = texture(sampler2D(Texture, Sampler), fsin_TexCoord);
        color *= texColor;
        color *= edgeAlpha * mask;
        fsout_Color = color;
    }";
    */

    protected readonly string VertexCode;
    protected readonly string FragmentCode;

    public delegate void WindowResizeEvent();
    public event WindowResizeEvent OnWindowResized;

    public VeldridGUIRenderer(int width, int height)
    {

        WindowCreateInfo windowInfo = new WindowCreateInfo()
        {
            X = 100,
            Y = 100,
            WindowWidth = (int)(width),
            WindowHeight = (int)(height),
            WindowTitle = "Veldrid - GUI Renderer"
        };
        _window = VeldridStartup.CreateWindow(ref windowInfo);
        _window.Resized += _window_Resized;

        // Get DPI scale
        _dpiScale = Graphics.GetDpiForWindow(_window.Handle) / 96.0f; // 96 is the default DPI

        Console.WriteLine($"DPI: {_dpiScale}");

        _input = new VeldridGUIInput(_window);

        var assembly = Assembly.GetExecutingAssembly();
        using (Stream? stream = assembly.GetManifestResourceStream("VeldridSample.EmbeddedResources.Paper-VertexShader.glsl"))
        {
            if (stream == null)
                throw new InvalidOperationException("Could not find embedded vertex shader resource");

            using (StreamReader reader = new StreamReader(stream)) {
                VertexCode = reader.ReadToEnd();
            }
        }

        using (Stream? stream = assembly.GetManifestResourceStream("VeldridSample.EmbeddedResources.Paper-FragmentShader.glsl"))
        {
            if (stream == null)
                throw new InvalidOperationException("Could not find embedded vertex shader resource");

            using (StreamReader reader = new StreamReader(stream))
            {
                FragmentCode = reader.ReadToEnd();
            }
        }

        GraphicsDeviceOptions options = new GraphicsDeviceOptions
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
            SwapchainDepthFormat = null
        };
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options);

        CreateResources();
    }

    private void _window_Resized()
    {
        if (OnWindowResized != null)
            OnWindowResized();

        _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
    }

    public object CreateTexture(uint width, uint height)
    {
        var factory = _graphicsDevice.ResourceFactory;
        TextureDescription textureDescription = new TextureDescription(
            width: width,
            height: height,
            depth: 1,
            mipLevels: 1,
            arrayLayers: 1,
            format: PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.Storage,
            TextureType.Texture2D);

        var texture = factory.CreateTexture(textureDescription);
        return factory.CreateTextureView(texture);
    }

    public Vector2Int GetTextureSize(object texture)
    {
        if (texture is not TextureView tex)
        {
            throw new ArgumentException("Texture must be of type Veldrid.Texture", nameof(texture));
        }

        return new Vector2Int((int)tex.Target.Width, (int)tex.Target.Height);
    }

    public void SetTextureData(object texture, Prowl.Vector.IntRect bounds, byte[] data)
    {
        if (texture is not TextureView textureView)
        {
            throw new ArgumentException("Texture must be of type Veldrid.Texture", nameof(texture));
        }

        _graphicsDevice.UpdateTexture(
            textureView.Target,
            data,
            (uint)(bounds.x),
            (uint)(bounds.y),
            0,  // z offset
            (uint)(bounds.width),
            (uint)(bounds.height),
            1,  // depth
            0,  // mip level
            0); // array layer
    }

    private void CreateResources()
    {
        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        // Create projection buffer
        _projectionBuffer = factory.CreateBuffer(new BufferDescription(ProjectionBuffer.SizeInBytes, BufferUsage.UniformBuffer));

        // Create projection layout
        ResourceLayoutDescription projectionLayoutDesc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
        _projectionLayout = factory.CreateResourceLayout(projectionLayoutDesc);

        // Create projection resource set
        _projectionSet = factory.CreateResourceSet(new ResourceSetDescription(
            _projectionLayout,
            _projectionBuffer));

        // Create uniform buffers
        _brushBuffer = factory.CreateBuffer(new BufferDescription(BrushBuffer.SizeInBytes, BufferUsage.UniformBuffer));

        // Create texture layout and resources
        ResourceLayoutDescription texLayoutDesc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment));
        _textureLayout = factory.CreateResourceLayout(texLayoutDesc);


        // Create brush layout and resources
        ResourceLayoutDescription brushLayoutDesc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BrushBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment));
        _brushLayout = factory.CreateResourceLayout(brushLayoutDesc);

        // Create default white texture
        TextureDescription defaultTexDesc = new TextureDescription(
            1, 1, 1, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled,
            TextureType.Texture2D);
        _defaultTexture = factory.CreateTexture(defaultTexDesc);
        _defaultTextureView = factory.CreateTextureView(_defaultTexture);

        // Update default texture with white pixel
        byte[] whitePixel = new byte[] { 255, 255, 255, 255 };
        _graphicsDevice.UpdateTexture(_defaultTexture, whitePixel, 0, 0, 0, 1, 1, 1, 0, 0);

        // Create vertex buffer and index buffer
        _vertexBuffer = factory.CreateBuffer(new BufferDescription(1024 * Vertex.SizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _indexBuffer = factory.CreateBuffer(new BufferDescription(1024 * sizeof(uint), BufferUsage.IndexBuffer | BufferUsage.Dynamic));

        // We need to use VertexElementSemantic.TextureCoordinate because SPIR-V generates
        // all TEXCOORD semantics when translating to HLSL, hence thee neeed.

        // Create vertex layout
        VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        // Create shaders
        ShaderDescription vertexShaderDesc = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(VertexCode),
            "main");

        ShaderDescription fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(FragmentCode),
            "main");

        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        // Create sampler
        SamplerDescription samplerDesc = new SamplerDescription(
            SamplerAddressMode.Clamp,
            SamplerAddressMode.Clamp,
            SamplerAddressMode.Clamp,
            SamplerFilter.MinLinear_MagLinear_MipLinear,
            null,
            0,
            0,
            0,
            1,
            SamplerBorderColor.TransparentBlack);
        _sampler = factory.CreateSampler(samplerDesc);

        // Create resource sets
        _textureSet = factory.CreateResourceSet(new ResourceSetDescription(
            _textureLayout,
            _defaultTextureView,
            _sampler));


        _brushSet = factory.CreateResourceSet(new ResourceSetDescription(
            _brushLayout,
            _brushBuffer));


        var blendState = new BlendStateDescription
        {
            AttachmentStates = new BlendAttachmentDescription[]
            {
                new BlendAttachmentDescription
                {
                    BlendEnabled = true,
                    SourceColorFactor = BlendFactor.One,
                    DestinationColorFactor = BlendFactor.InverseSourceAlpha,
                    ColorFunction = BlendFunction.Add,
                    SourceAlphaFactor = BlendFactor.SourceAlpha,
                    DestinationAlphaFactor = BlendFactor.Zero,
                    AlphaFunction = BlendFunction.Add
                }
            }
        };

        // Create pipeline
        GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(
            blendState,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(
                cullMode: FaceCullMode.Front,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: false,
                scissorTestEnabled: true),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                new[] { vertexLayout },
                _shaders),
            new[] { _projectionLayout, _textureLayout, _brushLayout },
            _graphicsDevice.SwapchainFramebuffer.OutputDescription);

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        _commandList = factory.CreateCommandList();
    }
    private void UpdateProjection(float width, float height)
    {
        // Create orthographic projection matrix for 2D rendering
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width / DPIScale, height / DPIScale, 0, -1, 1);
        _graphicsDevice.UpdateBuffer(_projectionBuffer!, 0, projection);
    }

    private Matrix4x4 ToVeldridMatrix(Prowl.Vector.Matrix4x4 mat) => new Matrix4x4(
        (float)mat.M11, (float)mat.M12, (float)mat.M13, (float)mat.M14,
        (float)mat.M21, (float)mat.M22, (float)mat.M23, (float)mat.M24,
        (float)mat.M31, (float)mat.M32, (float)mat.M33, (float)mat.M34,
        (float)mat.M41, (float)mat.M42, (float)mat.M43, (float)mat.M44
    );


    public void RenderCalls(Canvas canvas, IReadOnlyList<DrawCall> drawCalls)
    {
        if (drawCalls.Count == 0)
            return;

        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        // Update projection matrix
        UpdateProjection(_window.Width, _window.Height);


        // Ensure our buffers are large enough
        uint requiredVertexBufferSize = (uint)(canvas.Vertices.Count * Vertex.SizeInBytes);
        uint requiredIndexBufferSize = (uint)(canvas.Indices.Count * sizeof(uint));

        if (_vertexBuffer!.SizeInBytes < requiredVertexBufferSize)
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(requiredVertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        if (_indexBuffer!.SizeInBytes < requiredIndexBufferSize)
        {
            _indexBuffer.Dispose();
            _indexBuffer = factory.CreateBuffer(new BufferDescription(requiredIndexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        }

        var random = new System.Random();
        var val = random.Next(0, 256);

        // Convert vertices to our format
        //Console.WriteLine($"Vertices Count: {canvas.Vertices.Count} ({System.Math.Min(4, canvas.Vertices.Count)})");
        Vertex[] vertices = new Vertex[canvas.Vertices.Count];
        for (int i = 0; i < canvas.Vertices.Count; i++)
        {
            var v = canvas.Vertices[i];
            //Console.WriteLine($"Vertex {i}: {v.x}-{v.y}/{v.u}-{v.v}");
            vertices[i] = new Vertex
            {
                //Position = (new Vector2(v.x, v.y) / new Vector2(_window.Width, _window.Height)) - new Vector2(0.5f,0.5f),
                Position = new Vector2(v.x / DPIScale, v.y / DPIScale),
                TexCoord = new Vector2(v.u, v.v),
                Color = new RgbaFloat(v.r / 255f, v.g / 255f, v.b / 255f, v.a / 255f)
            };
        }

        _commandList!.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);

        // Update buffers
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, canvas.Indices.ToArray());

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        _commandList.SetPipeline(_pipeline);

        // Set projection resource set (must be done before any draw calls)
        _commandList.SetGraphicsResourceSet(0, _projectionSet);

        int drawCallsCount = 0;
        uint indexOffset = 0;
        foreach (var drawCall in drawCalls)
        {
            // Set texture
            var textureView = drawCall.Texture as TextureView ?? _defaultTextureView;

            var resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                _textureLayout!,
                textureView,
                _sampler!));
            _commandList.SetGraphicsResourceSet(1, resourceSet);

            // Update and set scissor uniforms
            drawCall.GetScissor(out var scissor, out var extent);

            var mat = scissor;

            /*Console.WriteLine($"[{extent.x};{extent.y}]");
            Console.WriteLine($"{(float)mat.M11} {(float)mat.M12} {(float)mat.M13} {(float)mat.M14}");
            Console.WriteLine($"{(float)mat.M21} {(float)mat.M22} {(float)mat.M23} {(float)mat.M24}");
            Console.WriteLine($"{(float)mat.M31} {(float)mat.M32} {(float)mat.M33} {(float)mat.M34}");
            Console.WriteLine($"{(float)mat.M41} {(float)mat.M42} {(float)mat.M43} {(float)mat.M44}");*/

            if (extent.x > 1)
            {
                // Transform to screen coordinates
                Vector2 screenTopLeft = Vector2.Transform(Vector2.Zero, ToVeldridMatrix(scissor));

                var targetCoordinates = Vector2.Zero - screenTopLeft - new Vector2((float)extent.x, (float)extent.y);

                uint x = (uint)targetCoordinates.X;
                uint y = (uint)targetCoordinates.Y;
                uint width = (uint)(extent.x * 2);
                uint height = (uint)(extent.y * 2);

                //Console.WriteLine($"SCISSOR: [{x};{y}   -   {width}x{height}]");

                _commandList.SetScissorRect(0,
                    x, y,
                    width, height);
            }
            else
            {
                // Reset scissor to full viewport
                _commandList.SetScissorRect(0, 0, 0, (uint)_window.Width, (uint)_window.Height);
            }

            // Update and set brush uniforms
            var brush = drawCall.Brush;
            var brushData = new BrushBuffer
            {
                BrushMat = ToVeldridMatrix(brush.BrushMatrix),
                BrushColor1 = new Vector4(
                    brush.Color1.R / 255f,
                    brush.Color1.G / 255f,
                    brush.Color1.B / 255f,
                    brush.Color1.A / 255f),
                BrushColor2 = new Vector4(
                    brush.Color2.R / 255f,
                    brush.Color2.G / 255f,
                    brush.Color2.B / 255f,
                    brush.Color2.A / 255f),
                BrushParams = new Vector4(
                    (float)brush.Point1.x,
                    (float)brush.Point1.y,
                    (float)brush.Point2.x,
                    (float)brush.Point2.y),
                BrushParams2 = new Vector2(
                    (float)brush.CornerRadii,
                    (float)brush.Feather),
                BrushType = (int)brush.Type
            };
            _graphicsDevice.UpdateBuffer(_brushBuffer!, 0, brushData);
            _commandList.SetGraphicsResourceSet(2, _brushSet!);

            // Draw the elements
            _commandList.DrawIndexed(
                indexCount: (uint)drawCall.ElementCount,
                instanceCount: 1,
                indexStart: indexOffset,
                vertexOffset: 0,
                instanceStart: 0);

            indexOffset += (uint)drawCall.ElementCount;

            // Dispose temporary resource set
            resourceSet.Dispose();
            drawCallsCount++;
            if (drawCallsCount >= 5)
                break;
        }

        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    public void DisposeRenderer()
    {
        // Dispose shaders
        if (_shaders != null)
        {
            foreach (var shader in _shaders)
            {
                shader?.Dispose();
            }
        }

        // Dispose buffers
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        //_scissorBuffer?.Dispose();
        _brushBuffer?.Dispose();

        // Dispose pipeline
        _pipeline?.Dispose();

        // Dispose command list
        _commandList?.Dispose();

        // Dispose textures and views
        _defaultTexture?.Dispose();
        _defaultTextureView?.Dispose();

        // Dispose layouts
        _textureLayout?.Dispose();
        //_scissorLayout?.Dispose();
        _brushLayout?.Dispose();


        _projectionBuffer?.Dispose();
        _projectionLayout?.Dispose();
        _projectionSet?.Dispose();

        // Dispose resource sets
        _textureSet?.Dispose();
        //_scissorSet?.Dispose();
        _brushSet?.Dispose();

        // Dispose sampler
        _sampler?.Dispose();

        // Dispose graphics device
        _graphicsDevice?.Dispose();
    }

    public void Dispose()
    {
        // Dispose all resources
        _pipeline?.Dispose();
        if (_shaders != null)
        {
            foreach (var shader in _shaders)
                shader?.Dispose();
        }
        _commandList?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        //_scissorBuffer?.Dispose();
        _brushBuffer?.Dispose();
        _defaultTexture?.Dispose();
        _defaultTextureView?.Dispose();
        _textureLayout?.Dispose();
        //_scissorLayout?.Dispose();

        _projectionBuffer?.Dispose();
        _projectionLayout?.Dispose();
        _projectionSet?.Dispose();

        _brushLayout?.Dispose();
        _textureSet?.Dispose();
        //_scissorSet?.Dispose();
        _brushSet?.Dispose();
        _sampler?.Dispose();
        _graphicsDevice?.Dispose();
    }
}
