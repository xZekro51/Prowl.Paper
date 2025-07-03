using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

using FontStashSharp;

using Prowl.PaperUI;
//using Prowl.PaperUI.Input;

using Shared;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using Color = System.Drawing.Color;

namespace VeldridSample
{
    class Program
    {
        private static Stopwatch _stopwatch = new Stopwatch();
        private static double _lastFrameTime;

        static void Main(string[] args)
        {
            /*VeldridRenderer renderer = new VeldridRenderer(800,600);

            while(renderer.Window.Exists)
            {
                renderer.Window.PumpEvents();
                double currentTime = _stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - _lastFrameTime;
                _lastFrameTime = currentTime;
                renderer.Draw();
            }

            renderer.DisposeResources();*/
            
             
            var veldridGUIRenderer = new VeldridGUIRenderer(1080, 850);
            Paper.Initialize(veldridGUIRenderer, veldridGUIRenderer.Window.Width, veldridGUIRenderer.Window.Height);

            veldridGUIRenderer.OnWindowResized += () => Paper.SetResolution(veldridGUIRenderer.Window.Width, veldridGUIRenderer.Window.Height);

            PaperDemo.Initialize();

            _stopwatch.Start();
            _lastFrameTime = _stopwatch.Elapsed.TotalSeconds;


            var fontSystem = new FontSystem();

            // Load fonts with different sizes
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream("VeldridSample.EmbeddedResources.font.ttf"))
            {
                if (stream == null) throw new Exception("Could not load font resource");
                fontSystem.AddFont(stream);
            }

            var font = fontSystem.GetFont(56); // Get the default font at size 16

            while (veldridGUIRenderer.Window.Exists)
            {
                var inputSnapshot = veldridGUIRenderer.Window.PumpEvents();

                veldridGUIRenderer.Input.UpdateInput(inputSnapshot);

                double currentTime = _stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - _lastFrameTime;
                _lastFrameTime = currentTime;

                RenderPaper(veldridGUIRenderer, font, deltaTime);
            }
            veldridGUIRenderer.DisposeRenderer();
            
            

        }

        public static void RenderPaper(VeldridGUIRenderer renderer, SpriteFontBase font, double deltaTime)
        {
            // Begin the UI frame with deltaTime
            Paper.BeginFrame(deltaTime);//, new Vector2(renderer.DPIScale,renderer.DPIScale));

            //PaperDemo.RenderUI();
            // Define your UI

            // Main content
            //var random = new System.Random();
            ///var val = random.Next(0, 256);
            //Paper.Box("MainContent").BackgroundColor(System.Drawing.Color.FromArgb(random.Next(0,256), random2.Next(0, 256), random3.Next(0, 256)));
            //Paper.Box("MainContent").Position(0, 0).Size(1).BackgroundColor(System.Drawing.Color.FromArgb(220,220,220));//(System.Drawing.Color.FromArgb(val, val, val));

            //Paper.Box("MainContent").BackgroundColor(System.Drawing.Color.FromArgb(220, 220, 220));//(System.Drawing.Color.FromArgb(val, val, val));

            /*// Upgrade box
            using (Paper.Box("UpgradeBox")
                .Margin(15)
                .Height(Paper.Auto)
                .Rounded(8)
                .BackgroundColor(Color.FromArgb(132,132,132))
                .AspectRatio(0.5f)
                .Enter())
            {
            }*/

            using (Paper.Box("MainBox")
                .Margin(5)
                .BackgroundColor(Color.FromArgb(112, 112, 112))
                .Enter())
            {
                using (Paper.Box("UpgradeContent2")
                .Height(17)
                .BackgroundColor(Color.FromArgb(52, 52, 52))
                .PositionType(PositionType.SelfDirected)
                .Top(300)
                .Clip()
                .Enter())
                {
                    Paper.Box("UpgradeText")
                    .Text(Text.Center("Upgrade to Pro", font, Color.White));
                }
            }

            /*using (Paper.Column("MainContainer")
                .BackgroundColor(System.Drawing.Color.FromArgb(240, 240, 240))
                .Enter())
            {
                // A header
                using (Paper.Box("Header")
                    .Height(60)
                    .BackgroundColor(System.Drawing.Color.FromArgb(50, 120, 200))
                    .Text(Text.Center("My Application", font, Color.White))
                    .Enter()) { }

                // Content area
                using (Paper.Row("Content").Enter())
                {
                    // Sidebar
                    Paper.Box("Sidebar")
                        .Width(200)
                        .BackgroundColor(System.Drawing.Color.FromArgb(220, 220, 220));

                    // Main content
                    Paper.Box("MainContent");
                }
            }*/

            // End the UI frame
            Paper.EndFrame();
        }

        /*private static void HandleInput(InputSnapshot snapshot)
        {
            foreach (var e in snapshot.KeyEvents)
            {
                if (e.Down)
                    _app.KeyDown((PaperKey)e.Key);
                else
                    _app.KeyUp((PaperKey)e.Key);
            }

            foreach (var c in snapshot.KeyCharPresses)
            {
                _app.TextInput(c);
            }

            _app.MousePosition(snapshot.MousePosition);
            _app.MouseScroll(snapshot.WheelDelta);

            foreach (var e in snapshot.MouseEvents)
            {
                switch (e.MouseButton)
                {
                    case MouseButton.Left:
                        if (e.Down)
                            _app.MouseDown(PaperMouseButton.Left);
                        else
                            _app.MouseUp(PaperMouseButton.Left);
                        break;
                    case MouseButton.Right:
                        if (e.Down)
                            _app.MouseDown(PaperMouseButton.Right);
                        else
                            _app.MouseUp(PaperMouseButton.Right);
                        break;
                    case MouseButton.Middle:
                        if (e.Down)
                            _app.MouseDown(PaperMouseButton.Middle);
                        else
                            _app.MouseUp(PaperMouseButton.Middle);
                        break;
                }
            }
        }

        public string GetClipboardText() => SDL2.SDL_GetClipboardText();

        public void SetClipboardText(string text) => SDL2.SDL_SetClipboardText(text);*/
    }
}
