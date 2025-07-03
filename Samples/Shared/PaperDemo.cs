﻿using FontStashSharp;
using Prowl.PaperUI;
using Prowl.PaperUI.Extras;
using Prowl.Vector;

using System.Drawing;
using System.Reflection;

namespace Shared
{
    public static partial class PaperDemo
    {
        public static FontSystem fontSystem;
        static SpriteFontBase fontSmall;
        static SpriteFontBase fontMedium;
        static SpriteFontBase fontLarge;
        public static SpriteFontBase fontTitle;

        // Track state for interactive elements
        static double sliderValue = 0.5f;
        static int selectedTabIndex = 0;
        static Vector2 chartPosition = new Vector2(0, 0);
        static double zoomLevel = 1.0f;
        static bool[] toggleState = { true, false, true, false, true };

        // Sample data for visualization
        static double[] dataPoints = { 0.2f, 0.5f, 0.3f, 0.8f, 0.4f, 0.7f, 0.6f };
        static readonly string[] tabNames = { "Dashboard", "Analytics", "Profile", "Settings", "Windows" };

        //Theme
        static Color backgroundColor;
        static Color cardBackground;
        static Color primaryColor;
        static Color secondaryColor;
        static Color textColor;
        static Color lightTextColor;
        static Color[] colorPalette;
        static bool isDark;

        static double time = 0;

        static string searchText = "";
        static bool searchFocused = false;

        public static void Initialize()
        {
            ToggleTheme();
            fontSystem = new FontSystem();

            // Load fonts with different sizes
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream("Shared.EmbeddedResources.font.ttf"))
            {
                if (stream == null) throw new Exception("Could not load font resource");
                fontSystem.AddFont(stream);
            }
            using (Stream? stream = assembly.GetManifestResourceStream("Shared.EmbeddedResources.fa-regular-400.ttf"))
            {
                if (stream == null) throw new Exception("Could not load font resource");
                fontSystem.AddFont(stream);
            }
            using (Stream? stream = assembly.GetManifestResourceStream("Shared.EmbeddedResources.fa-solid-900.ttf"))
            {
                if (stream == null) throw new Exception("Could not load font resource");
                fontSystem.AddFont(stream);
            }

            fontSmall = fontSystem.GetFont(19);
            fontMedium = fontSystem.GetFont(26);
            fontLarge = fontSystem.GetFont(32);
            fontTitle = fontSystem.GetFont(40);

            DefineStyles();
        }

        public static void RenderUI()
        {
            // Update time for animations
            time += 0.016f; // Assuming ~60fps

            //TestWindows();

            // Main container with light gray background
            using (Paper.Column("MainContainer")
                .BackgroundColor(backgroundColor)
                //.Style(BoxStyle.Solid(backgroundColor))
                .Enter())
            {
                // A stupid simple way to benchmark the performance of the UI (Adds the entire ui multiple times)
                for (int i = 0; i < 1; i++)
                {
                    Paper.PushID((ulong)i);
                    // Top navigation bar
                    RenderTopNavBar();

                    // Main content area
                    using (Paper.Row("ContentArea")
                        .Enter())
                    {
                        // Left sidebar
                        RenderSidebar();

                        // Content area (tabs content)
                        RenderMainContent();
                    }

                    // Footer
                    RenderFooter();
                    Paper.PopID();
                }
            }
        }

        public static bool isWindowAOpen = true;
        public static bool isWindowBOpen = true;

        private static void TestWindows()
        {
            // Window Tests
            WindowManager.SetWindowFont(fontMedium);
            WindowManager.Window("MyTestWindowA", ref isWindowAOpen, "Test Window", () => {
            // Window content rendering
            using (Paper.Column("WindowInnerContent")
                    .Enter())
                {
                    WindowManager.Window("MyTestWindowB", ref isWindowBOpen, "Recursive Window", () => {
                        // Window content rendering
                        using (Paper.Column("WindowInnerContent")
                            .Enter())
                        {
                            using (Paper.Box("Title")
                                .Height(40)
                                .Text(Text.Center("Hello from Window System", fontLarge, textColor))
                                .Enter()) { }

                            using (Paper.Box("Content")
                                .Text(Text.Left("This is content inside the window. You can close, resize, and drag this window.", fontMedium, textColor))
                                .Enter()) { }

                            using (Paper.Box("Button")
                                .PositionType(PositionType.SelfDirected)
                                .Width(200)
                                .Height(200)
                                .Margin(Paper.Stretch(), 0, Paper.Stretch(), 0)
                                .BackgroundColor(primaryColor)
                                //.Style(BoxStyle.SolidRounded(primaryColor, 8f))
                                //.HoverStyle(BoxStyle.SolidRounded(secondaryColor, 12f))
                                //.ActiveStyle(BoxStyle.SolidRounded(primaryColor, 16f))
                                //.FocusedStyle(BoxStyle.SolidRoundedWithBorder(backgroundColor, textColor, 20f, 1f))
                                .Text(Text.Center("Click Me", fontMedium, Color.White))
                                .OnClick((rect) => Console.WriteLine("Button in window clicked!"))
                                .Enter()) { }
                        }
                    });
                }
            });


            //var myWindowB = ImGui.CreateWindow(
            //    fontMedium,
            //    "My OtherWindow",
            //    new Vector2(100, 400),
            //    new Vector2(200, 100),
            //    (window) => {
            //        // Window content rendering
            //        using (ImGui.Column("WindowInnerContent")
            //            .Enter())
            //        {
            //            using (ImGui.LayoutBox("Title")
            //                .Height(40)
            //                .Text(Text.Center("Why Hello There", fontLarge, textColor))
            //                .Enter()) { }
            //        }
            //    }
            //);
        }

        private static void ToggleTheme()
        {
            isDark = !isDark;

            if (isDark)
            {
                //Dark
                backgroundColor = Color.FromArgb(255, 18, 18, 23);
                cardBackground = Color.FromArgb(255, 30, 30, 46);
                primaryColor = Color.FromArgb(255, 94, 104, 202);
                secondaryColor = Color.FromArgb(255, 162, 155, 254);
                textColor = Color.FromArgb(255, 226, 232, 240);
                lightTextColor = Color.FromArgb(255, 148, 163, 184);
                colorPalette = [
                    Color.FromArgb(255, 94, 234, 212),   // Cyan
                    Color.FromArgb(255, 162, 155, 254),  // Purple  
                    Color.FromArgb(255, 249, 115, 22),   // Orange
                    Color.FromArgb(255, 248, 113, 113),  // Red
                    Color.FromArgb(255, 250, 204, 21)    // Yellow
                ];
            }
            else
            {

                //Light
                backgroundColor = Color.FromArgb(255, 243, 244, 246);
                cardBackground = Color.FromArgb(255, 255, 255, 255);
                primaryColor = Color.FromArgb(255, 59, 130, 246);
                secondaryColor = Color.FromArgb(255, 16, 185, 129);
                textColor = Color.FromArgb(255, 31, 41, 55);
                lightTextColor = Color.FromArgb(255, 107, 114, 128);
                colorPalette = [
                    Color.FromArgb(255, 59, 130, 246),   // Blue
                    Color.FromArgb(255, 16, 185, 129),   // Teal  
                    Color.FromArgb(255, 239, 68, 68),    // Red
                    Color.FromArgb(255, 245, 158, 11),   // Amber
                    Color.FromArgb(255, 139, 92, 246)    // Purple
                ];
            }

            // Redefine styles with new theme colors
            DefineStyles();
        }

        private static void RenderTopNavBar()
        {
            using (Paper.Row("TopNavBar")
                .Height(70)
                .Style("container")
                .Margin(15, 15, 15, 0)
                .Enter())
            {
                // Logo
                using (Paper.Box("Logo")
                    .Width(180)
                    .Enter())
                {
                    Paper.Box("LogoInner")
                        .Size(50)
                        .Margin(10)
                        .Text(Text.Center(Icons.Newspaper, fontLarge, lightTextColor));

                    Paper.Box("LogoText")
                        .PositionType(PositionType.SelfDirected)
                        .Left(50 + 15)
                        .Text(Text.Left("PaperUI Demo", fontTitle, textColor));
                }

                // Spacer
                using (Paper.Box("Spacer").Enter()) { }

                // Search bar - now using text-field style
                Paper.Box("SearchTextField")
                    .TextField(searchText, fontMedium, newValue => searchText = newValue, "Search...")
                    .Style("text-field")
                    .SetScroll(Scroll.ScrollX)
                    .Margin(0, 15, 15, 0);

                // Theme Switch - using icon-button style
                Paper.Box("LightIcon")
                    .Style("icon-button")
                    .Margin(0, 10, 15, 0)
                    .Text(Text.Center(Icons.Lightbulb, fontMedium, lightTextColor))
                    .OnClick((rect) => ToggleTheme());

                // Notification icon
                Paper.Box("NotificationIcon")
                    .Style("icon-button")
                    .Margin(0, 10, 15, 0)
                    .Text(Text.Center(Icons.CircleExclamation, fontMedium, lightTextColor))
                    .OnClick((rect) => Console.WriteLine("Notifications clicked"));

                // User Profile
                Paper.Box("UserProfile")
                    .Width(40)
                    .Height(40)
                    .Rounded(40)
                    .BackgroundColor(secondaryColor)
                    .Margin(0, 15, 15, 0)
                    .Text(Text.Center("M", fontMedium, Color.White))
                    .OnClick((rect) => Console.WriteLine("Profile clicked"));
            }
        }

        private static void RenderSidebar()
        {
            using (Paper.Column("Sidebar")
                .Style("sidebar")  // Automatic hover expansion with transitions
                .Margin(15)
                .Enter())
            {
                // Menu header
                Paper.Box("MenuHeader")
                    .Height(60)
                    .Text(Text.Center("Menu", fontMedium, textColor));

                string[] menuIcons = { Icons.House, Icons.ChartBar, Icons.User, Icons.Gear, Icons.WindowMaximize };
                string[] menuItems = { "Dashboard", "Analytics", "Users", "Settings", "Windows" };

                for (int i = 0; i < menuItems.Length; i++)
                {
                    int index = i;
                    bool isSelected = selectedTabIndex == index;

                    using (Paper.Box($"MenuItemContainer_{i}")
                        .Style("menu-item")
                        .StyleIf(isSelected, "menu-item-selected")
                        .OnClick((rect) => selectedTabIndex = index)
                        .Clip()
                        .Enter())
                    {
                        Paper.Box($"MenuItemIcon_{i}")
                            .Width(55)
                            .Height(50)
                            .Text(Text.Center(menuIcons[i], fontSmall, textColor));

                        Paper.Box($"MenuItem_{i}")
                            .Width(100)
                            .PositionType(PositionType.SelfDirected)
                            .Left(50 + 15)
                            .Text(Text.Center($"{menuItems[i]}", fontSmall, textColor));
                    }
                }

                // Spacer
                using (Paper.Box("SidebarSpacer").Enter()) { }

                // Upgrade box
                using (Paper.Box("UpgradeBox")
                    .Margin(15)
                    .Height(Paper.Auto)
                    .Rounded(8)
                    .BackgroundColor(primaryColor)
                    .AspectRatio(0.5f)
                    .Enter())
                {
                    using (Paper.Column("UpgradeContent")
                        .Margin(15)
                        .Clip()
                        .Enter())
                    {
                        Paper.Box("UpgradeText")
                            .Text(Text.Center("Upgrade to Pro", fontMedium, Color.White));

                        Paper.Box("UpgradeButton")
                            .Style("button")
                            .Height(30)
                            .BackgroundColor(Color.White)
                            .Text(Text.Center("Upgrade", fontSmall, primaryColor))
                            .OnClick((rect) => Console.WriteLine("Upgrade clicked"));
                    }
                }
            }
        }

        private static void RenderMainContent()
        {
            return;
            using (Paper.Column("MainContent")
                .Margin(0, 15, 15, 15)
                .Enter())
            {
                // Tabs navigation
                RenderTabsNavigation();

                // Tab content based on selected tab
                switch (selectedTabIndex)
                {
                    case 0: RenderDashboardTab(); break;
                    case 1: RenderAnalyticsTab(); break;
                    case 2: RenderProfileTab(); break;
                    case 3: RenderSettingsTab(); break;
                    case 4: RenderWindowsTab(); break;
                    default: RenderDashboardTab(); break;
                }
            }
        }

        private static void RenderTabsNavigation()
        {
            using (Paper.Row("TabsNav")
                .Height(60)
                .Style("container")
                .Enter())
            {
                for (int i = 0; i < tabNames.Length; i++)
                {
                    int index = i;
                    bool isSelected = i == selectedTabIndex;
                    Color tabColor = isSelected ? primaryColor : lightTextColor;
                    double tabWidth = 1.0f / tabNames.Length;

                    using (Paper.Box($"Tab_{i}")
                        .Width(Paper.Stretch(tabWidth))
                        .Style("tab")
                        .Text(Text.Center(tabNames[i], fontMedium, tabColor))
                        .OnClick((rect) => selectedTabIndex = index)
                        .Enter())
                    {
                        // Show indicator line for selected tab
                        if (isSelected)
                        {
                            Paper.Box($"TabIndicator_{i}")
                                .Height(4)
                                .BackgroundColor(primaryColor)
                                .Rounded(2);
                        }
                    }
                }
            }
        }

        private static void RenderDashboardTab()
        {
            using (Paper.Row("DashboardCards")
        .Height(120)
        .Margin(0, 0, 15, 0)
        .Enter())
            {
                string[] statNames = { "Total Users", "Revenue", "Projects", "Conversion" };
                string[] statValues = { "3,456", "$12,345", "24", "8.5%" };

                for (int i = 0; i < 4; i++)
                {
                    using (Paper.Box($"StatCard_{i}")
                        .Width(Paper.Stretch(0.25f))
                        .Style("stat-card")
                        .Hovered
                            .BorderColor(colorPalette[i % colorPalette.Length])  // Dynamic border color
                            .End()
                        .Margin(i == 0 ? 0 : (15 / 2f), i == 3 ? 0 : (15 / 2f), 0, 0)
                        .Enter())
                    {
                        // Card icon with conditional styling based on parent hover
                        Paper.Box($"StatIcon_{i}")
                            .Size(40)
                            .BackgroundColor(Color.FromArgb(150, colorPalette[i % colorPalette.Length]))
                            .Rounded(8)
                            .If(Paper.IsParentHovered)
                                .Rounded(20)
                                .End()
                            .Transition(GuiProp.Rounded, 0.3, Paper.Easing.QuartOut)
                            .Margin(15, 0, 15, 0)
                            .IsNotInteractable();

                        using (Paper.Column($"StatContent_{i}")
                            .Margin(10, 15, 15, 15)
                            .Enter())
                        {
                            Paper.Box($"StatLabel_{i}")
                                .Height(Paper.Pixels(25))
                                .Text(Text.Left(statNames[i], fontSmall, lightTextColor));

                            Paper.Box($"StatValue_{i}")
                                .Text(Text.Left(statValues[i], fontLarge, textColor));
                        }
                    }
                }
            }

            // Charts and graphs row
            using (Paper.Row("ChartRow")
                .Margin(0, 0, 15, 0)
                .Enter())
            {
                // Chart area
                using (Paper.Box("ChartArea")
                    .Width(Paper.Stretch(0.7f))
                    .Rounded(8)
                    .BackgroundColor(cardBackground)
                    //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                    .Enter())
                {
                    // Chart header
                    using (Paper.Row("ChartHeader")
                        .Height(60)
                        .Margin(20, 20, 20, 0)
                        .Enter())
                    {
                        using (Paper.Box("ChartTitle")
                            .Text(Text.Left("Performance Overview", fontMedium, textColor))
                            .Enter()) { }

                        using (Paper.Row("ChartControls")
                            .Width(280)
                            .Enter())
                        {
                            string[] periods = { "Day", "Week", "Month", "Year" };
                            foreach (var period in periods)
                            {
                                using (Paper.Box($"Period_{period}")
                                    .Width(60)
                                    .Height(30)
                                    .Rounded(8)
                                    .Margin(5, 5, 0, 0)
                                    .BackgroundColor(period == "Week" ? primaryColor : Color.FromArgb(50, 0, 0, 0))
                                    .Hovered
                                        .BackgroundColor(Color.FromArgb(50, primaryColor))
                                        .End()
                                    .Transition(GuiProp.BackgroundColor, 0.2f)
                                    .Text(Text.Center(period, fontSmall, period == "Week" ? Color.White : lightTextColor))
                                    .OnClick((rect) => Console.WriteLine($"Period {period} clicked"))
                                    .Enter()) { }
                            }
                        }
                    }

                    // Chart content
                    using (Paper.Box("Chart")
                        .Margin(20)
                        .OnDragging((e) => chartPosition += e.Delta)
                        .OnScroll((e) => zoomLevel = Math.Clamp(zoomLevel + e.Delta * 0.1f, 0.5f, 2.0f))
                        .Clip()
                        .Enter())
                    {
                        using (Paper.Box("ChartCanvas")
                            .Translate(chartPosition.x, chartPosition.y)
                            .Scale(zoomLevel)
                            //.TransformSelf((rect) => {
                            //    Transform t = Transform.CreateTranslation(chartPosition) * Transform.CreateScale(zoomLevel);
                            //    //t.RotateWithOrigin(Math.Abs(Math.Sin(time * 0.01f)), rect.Center.X, rect.Center.Y);
                            //    return t;
                            //})
                            .Enter())
                        {
                            // Draw a simple chart with animated data
                            Paper.AddActionElement((vg, rect) => {

                                // Draw grid lines
                                for (int i = 0; i <= 5; i++)
                                {
                                    double y = rect.y + (rect.height / 5) * i;
                                    vg.BeginPath();
                                    vg.MoveTo(rect.x, y);
                                    vg.LineTo(rect.x + rect.width, y);
                                    vg.SetStrokeColor(lightTextColor);
                                    vg.SetStrokeWidth(1);
                                    vg.Stroke();
                                }

                                // Draw animated data points
                                vg.BeginPath();
                                double pointSpacing = rect.width / (dataPoints.Length - 1);
                                double animatedValue;

                                // Draw fill
                                vg.MoveTo(rect.x, rect.y + rect.height);

                                for (int i = 0; i < dataPoints.Length; i++)
                                {
                                    animatedValue = dataPoints[i] + Math.Sin(time * 0.25f + i * 0.5f) * 0.1f;
                                    //animatedValue = Math.Clamp(animatedValue, 0.1f, 0.9f);
                                    animatedValue = Math.Min(Math.Max(animatedValue, 0.1f), 0.9f); // Clamp to [0.1, 0.9]

                                    double x = rect.x + i * pointSpacing;
                                    double y = rect.y + rect.height - (animatedValue * rect.height);

                                    if (i == 0)
                                        vg.MoveTo(x, y);
                                    else
                                        vg.LineTo(x, y);
                                }

                                // Complete the fill path
                                vg.LineTo(rect.x + rect.width, rect.y + rect.height);
                                vg.LineTo(rect.x, rect.y + rect.height);

                                // Fill with gradient
                                //var paint = vg.LinearGradient(
                                //    rect.x, rect.y,
                                //    rect.x, rect.y + rect.height,
                                //    Color.FromArgb(100, primaryColor),
                                //    Color.FromArgb(10, primaryColor));
                                //vg.SetFillPaint(paint);
                                vg.SaveState();
                                vg.SetLinearBrush(rect.x, rect.y, rect.x, rect.y + rect.height, Color.FromArgb(100, primaryColor), Color.FromArgb(10, primaryColor));
                                vg.FillComplex();
                                vg.RestoreState();

                                vg.ClosePath();

                                // Draw the line
                                vg.BeginPath();
                                for (int i = 0; i < dataPoints.Length; i++)
                                {
                                    animatedValue = dataPoints[i] + Math.Sin(time * 0.25f + i * 0.5f) * 0.1f;
                                    //animatedValue = Math.Clamp(animatedValue, 0.1f, 0.9f);
                                    animatedValue = Math.Min(Math.Max(animatedValue, 0.1f), 0.9f); // Clamp to [0.1, 0.9]

                                    double x = rect.x + i * pointSpacing;
                                    double y = rect.y + rect.height - (animatedValue * rect.height);

                                    if (i == 0)
                                        vg.MoveTo(x, y);
                                    else
                                        vg.LineTo(x, y);
                                }

                                vg.SetStrokeColor(primaryColor);
                                vg.SetStrokeWidth(3);
                                vg.Stroke();

                                // Draw points
                                for (int i = 0; i < dataPoints.Length; i++)
                                {
                                    animatedValue = dataPoints[i] + Math.Sin(time * 0.25f + i * 0.5f) * 0.1f;
                                    //animatedValue = Math.Clamp(animatedValue, 0.1f, 0.9f);
                                    animatedValue = Math.Min(Math.Max(animatedValue, 0.1f), 0.9f); // Clamp to [0.1, 0.9]

                                    double x = rect.x + i * pointSpacing;
                                    double y = rect.y + rect.height - (animatedValue * rect.height);

                                    vg.BeginPath();
                                    vg.Circle(x, y, 6);
                                    vg.SetFillColor(Color.White);
                                    vg.Fill();

                                    vg.BeginPath();
                                    vg.Circle(x, y, 4);
                                    vg.SetFillColor(primaryColor);
                                    vg.Fill();
                                }

                                vg.ClosePath();
                            });
                        }
                    }
                }

                // Side panel
                using (Paper.Column("SidePanel")
                    .Width(Paper.Stretch(0.3f))
                    .Margin(15, 0, 0, 0)
                    .Enter())
                {
                    // Activity panel
                    using (Paper.Box("ActivityPanel")
                        .BackgroundColor(cardBackground)
                        //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                        .Rounded(8)
                        .Enter())
                    {
                        // Panel header
                        using (Paper.Box("PanelHeader")
                            .Height(60)
                            .Margin(20, 20, 20, 0)
                            .Text(Text.Left("Recent Activity", fontMedium, textColor))
                            .Enter()) { }

                        // Activity items
                        string[] activities = {
                            "John updated the project",
                            "Alice completed a task",
                            "New user registered",
                            "Project deadline updated",
                            "Team meeting scheduled"
                        };

                        string[] timestamps = {
                            "5m ago", "23m ago", "1h ago", "2h ago", "3h ago"
                        };

                        for (int i = 0; i < activities.Length; i++)
                        {
                            using (Paper.Row($"Activity_{i}")
                                .Height(70)
                                .Margin(15, 15, i == 0 ? 5 : 0, 5)
                                .Enter())
                            {
                                // Activity icon
                                using (Paper.Box($"ActivityIcon_{i}")
                                    .Width(40)
                                    .Height(40)
                                    .Rounded(8)
                                    .Margin(0, 0, 15, 0)
                                    .BackgroundColor(Color.FromArgb(150, colorPalette[i % colorPalette.Length]))
                                    //.Style(BoxStyle.SolidRounded(Color.FromArgb(150, colorPalette[i % colorPalette.Length]), 20f))
                                    .Enter()) { }

                                // Activity content
                                using (Paper.Column($"ActivityContent_{i}")
                                    .Margin(10, 0, 0, 0)
                                    .Enter())
                                {
                                    using (Paper.Box($"ActivityText_{i}")
                                        .Height(Paper.Pixels(20))
                                        .Margin(0, 0, 15, 0)
                                        .Text(Text.Left(activities[i], fontSmall, textColor))
                                        .Enter()) { }

                                    using (Paper.Box($"ActivityTime_{i}")
                                        .Height(Paper.Pixels(20))
                                        .Text(Text.Left(timestamps[i], fontSmall, lightTextColor))
                                        .Enter()) { }
                                }
                            }

                            // Add separator except for the last item
                            if (i < activities.Length - 1)
                            {
                                Paper.Box($"Separator_{i}").Style("seperator");
                            }
                        }
                    }
                }
            }
        }

        private static void RenderAnalyticsTab()
        {
            using (Paper.Row("AnalyticsContent")
                .Enter())
            {
                using (Paper.Box("AnalyticsContent")
                    .BackgroundColor(cardBackground)
                    //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                    .Margin(0, 15 / 2, 15, 0)
                    .Enter())
                {
                    // Analytics header
                    using (Paper.Box("AnalyticsHeader")
                        .Height(80)
                        .Margin(20)
                        .Text(Text.Left("Analytics Dashboard", fontLarge, textColor))
                        .Enter()) { }

                    // Interactive slider as a demo control
                    using (Paper.Column("SliderSection")
                        .Height(100)
                        .Margin(20, 20, 0, 0)
                        .Enter())
                    {
                        using (Paper.Box("SliderLabel")
                            .Height(30)
                            .Text(Text.Left($"Green Amount: {sliderValue:F2}", fontMedium, textColor))
                            .Enter()) { }

                        using (Paper.Box("SliderTrack")
                            .Height(20)
                            .BackgroundColor(Color.FromArgb(30, 0, 0, 0))
                            //.Style(BoxStyle.SolidRounded(Color.FromArgb(30, 0, 0, 0), 10f))
                            .Margin(0, 0, 20, 0)
                            .OnHeld((e) => {
                                double parentWidth = e.ElementRect.width;
                                double pointerX = e.PointerPosition.x - e.ElementRect.x;

                                // Calculate new slider value based on pointer position
                                sliderValue = Math.Clamp(pointerX / parentWidth, 0f, 1f);
                            })
                            .Enter())
                        {
                            // Filled part of slider
                            using (Paper.Box("SliderFill")
                                .Width(Paper.Percent(sliderValue * 100))
                                .BackgroundColor(primaryColor)
                                //.Style(BoxStyle.SolidRounded(primaryColor, 10f))
                                .Enter())
                            {
                                // Slider handle
                                using (Paper.Box("SliderHandle")
                                    .Left(Paper.Percent(100, -10))
                                    .Width(20)
                                    .Height(20)
                                    .BackgroundColor(textColor)
                                    //.Style(BoxStyle.SolidRounded(textColor, 10f))
                                    .PositionType(PositionType.SelfDirected)
                                    .Enter()) { }
                            }
                        }
                    }

                    // "Analysis" mock content
                    using (Paper.Box("AnalyticsVisual")
                        .Margin(20)
                        .Enter())
                    {
                        // Add a simple pie chart visualization
                        Paper.AddActionElement((vg, rect) => {
                            double centerX = rect.x + rect.width / 2;
                            double centerY = rect.y + rect.height / 2;
                            double radius = Math.Min(rect.width, rect.height) * 0.4f;

                            double startAngle = 0;
                            double[] values = { sliderValue, 0.2f, 0.15f, 0.25f, 0.1f };

                            // Normalize Values
                            double total = values.Sum();
                            for (int i = 0; i < values.Length; i++)
                                values[i] /= total;


                            for (int i = 0; i < values.Length; i++)
                            {
                                // Calculate angles
                                double angle = values[i] * Math.PI * 2;
                                double endAngle = startAngle + angle;

                                // Draw pie slice
                                vg.BeginPath();
                                vg.MoveTo(centerX, centerY);
                                vg.Arc(centerX, centerY, radius, startAngle, endAngle);
                                vg.LineTo(centerX, centerY);
                                vg.SetFillColor(colorPalette[i % colorPalette.Length]);
                                vg.Fill();

                                // Draw outline
                                vg.BeginPath();
                                vg.MoveTo(centerX, centerY);
                                vg.Arc(centerX, centerY, radius, startAngle, endAngle);
                                vg.LineTo(centerX, centerY);
                                vg.SetStrokeColor(Color.White);
                                vg.SetStrokeWidth(2);
                                vg.Stroke();

                                // Draw percentage labels
                                double labelAngle = startAngle + angle / 2;
                                double labelRadius = radius * 0.7f;
                                double labelX = centerX + Math.Cos(labelAngle) * labelRadius;
                                double labelY = centerY + Math.Sin(labelAngle) * labelRadius;

                                string label = $"{values[i] * 100:F0}%";
                                vg.SetFillColor(Color.White);
                                //vg.TextAlign(Align.Center | Align.Middle);
                                //vg.FontSize(16);
                                //vg.Text(labelX, labelY, label);
                                vg.DrawText(fontSmall, label, labelX, labelY, Color.White);

                                // Move to next slice
                                startAngle = endAngle;
                            }

                            // Draw center circle
                            vg.BeginPath();
                            vg.Circle(centerX, centerY, radius * 0.4f);
                            vg.SetFillColor(Color.White);
                            vg.Fill();

                            // Draw center text
                            // Draw center text
                            //vg.FillColor(textColor);
                            //vg.TextAlign(NvgSharp.Align.Center | NvgSharp.Align.Middle);
                            //vg.FontSize(20);
                            //vg.Text(centerX, centerY, $"Analytics\n{(sliderValue * 100):F0}%");
                            //vg.Text(fontSmall, $"Analytics\n{(sliderValue * 100):F0}%", centerX, centerY);
                        });
                    }
                }

                using (Paper.Box("ScrollTest")
                    .BackgroundColor(cardBackground)
                    //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                    .Margin(15 / 2, 0, 15, 0)
                    .Enter())
                {
                    // Dynamic content amount based on time
                    int amount = (int)(Math.Abs(Math.Sin(time * 0.25)) * 25) + 10;

                    // Create a grid layout for items
                    using (Paper.Row("GridContainer")
                        .Enter())
                    {
                        // Left column - cards
                        using (Paper.Column("LeftColumn")
                            .Width(Paper.Stretch(0.6))
                            .SetScroll(Scroll.ScrollY)
                            .Enter())
                        {
                            double scrollState = Paper.GetElementStorage<ScrollState>(Paper.CurrentParent, "ScrollState", new ScrollState()).Position.y;

                            for (int i = 0; i < 10; i++)
                            {
                                // Calculate animations based on time and index
                                double hue = (i * 25 + time * 20) % 360;
                                double saturation = 0.7;
                                double value = 0.8;

                                // Convert HSV to RGB
                                double h = hue / 60;
                                int hi = (int)Math.Floor(h) % 6;
                                double f = h - Math.Floor(h);
                                double p = value * (1 - saturation);
                                double q = value * (1 - f * saturation);
                                double t = value * (1 - (1 - f) * saturation);

                                double r, g, b;

                                switch (hi)
                                {
                                    case 0: r = value; g = t; b = p; break;
                                    case 1: r = q; g = value; b = p; break;
                                    case 2: r = p; g = value; b = t; break;
                                    case 3: r = p; g = q; b = value; break;
                                    case 4: r = t; g = p; b = value; break;
                                    default: r = value; g = p; b = q; break;
                                }

                                // Convert to Color
                                Color itemColor = Color.FromArgb(255,
                                    (int)(r * 255),
                                    (int)(g * 255),
                                    (int)(b * 255));

                                // Custom icon for each card
                                string icon = Icons.GetRandomIcon(i);

                                using (Paper.Box($"Card_{i}")
                                    .Height(70)
                                    .Margin(10, 10, 5, 5)
                                    .BackgroundColor(Color.FromArgb(230, itemColor))
                                    .BorderColor(isDark ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(50, 0, 0, 0))
                                    .BorderWidth(1)
                                    .Rounded(12)
                                    .Enter())
                                {
                                    using (Paper.Row("CardContent")
                                        .Margin(10)
                                        .Enter())
                                    {
                                        // Icon
                                        using (Paper.Box($"CardIcon_{i}")
                                            .Width(50)
                                            .Height(50)
                                            .Rounded(25)
                                            .BackgroundColor(Color.FromArgb(60, 255, 255, 255))
                                            .Text(Text.Center(icon, fontMedium, textColor))
                                            .Enter()) { }

                                        // Content
                                        using (Paper.Column($"CardTextColumn_{i}")
                                            .Margin(10, 0, 0, 0)
                                            .Enter())
                                        {
                                            using (Paper.Box($"CardTitle_{i}")
                                                .Height(25)
                                                .Text(Text.Left($"Item {i}", fontMedium, textColor))
                                                .Enter()) { }

                                            using (Paper.Box($"CardDescription_{i}")
                                                .Text(Text.Left($"Interactive card with animations", fontSmall,
                                                    Color.FromArgb(200, textColor)))
                                                .Enter()) { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        private static void RenderProfileTab()
        {
            using (Paper.Row("ProfileContent")
                .Margin(0, 0, 15, 0)
                .Enter())
            {
                // Left panel - profile info
                using (Paper.Column("ProfileDetails")
                    .Width(Paper.Stretch(0.4f))
                    .BackgroundColor(cardBackground)
                    //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                    .Enter())
                {
                    // Profile header with avatar
                    using (Paper.Column("ProfileHeader")
                        .Height(250)
                        .Enter())
                    {
                        // Avatar
                        using (Paper.Row("AvatarSpot")
                            .Height(120)
                            .Margin(0, 0, 40, 20)
                            .Enter())
                        {
                            // Spacer to Center Avatar
                            using (Paper.Box("Spacer0").Enter()) { }

                            // Avatar
                            using (Paper.Box("Avatar")
                                .Width(120)
                                .Height(120)
                                .BackgroundColor(secondaryColor)
                                //.Style(BoxStyle.SolidRounded(secondaryColor, 60f))
                                .Text(Text.Center("J", fontTitle, Color.White))
                                .Enter()) { }

                            // Spacer to Center Avatar
                            using (Paper.Box("Spacer1").Enter()) { }
                        }


                        // User name
                        using (Paper.Box("UserName")
                            .Height(40)
                            .Text(Text.Center("John Doe", fontLarge, textColor))
                            .Enter()) { }

                        // User title
                        using (Paper.Box("UserTitle")
                            .Height(30)
                            .Text(Text.Center("Senior Developer", fontMedium, lightTextColor))
                            .Enter()) { }
                    }

                    // User stats
                    using (Paper.Row("UserStats")
                        .Height(80)
                        .Margin(20, 20, 0, 0)
                        .Enter())
                    {
                        string[] statLabels = { "Projects", "Tasks", "Teams" };
                        string[] statValues = { "24", "148", "5" };

                        for (int i = 0; i < statLabels.Length; i++)
                        {
                            using (Paper.Column($"Stat_{i}")
                                .Width(Paper.Stretch(1.0f / statLabels.Length))
                                .Enter())
                            {
                                using (Paper.Box($"StatValue_{i}")
                                    .Height(40)
                                    .Text(Text.Center(statValues[i], fontLarge, primaryColor))
                                    .Enter()) { }

                                using (Paper.Box($"StatLabel_{i}")
                                    .Height(30)
                                    .Text(Text.Center(statLabels[i], fontSmall, lightTextColor))
                                    .Enter()) { }
                            }
                        }
                    }

                    // Contact info
                    using (Paper.Column("ContactInfo")
                        .Margin(20)
                        .Enter())
                    {
                        string[] contactLabels = { "Email", "Phone", "Location", "Department" };
                        string[] contactValues = { "john.doe@example.com", "(555) 123-4567", "San Francisco, CA", "Engineering" };

                        for (int i = 0; i < contactLabels.Length; i++)
                        {
                            using (Paper.Row($"ContactRow_{i}")
                                .Height(50)
                                .Enter())
                            {
                                using (Paper.Box($"ContactLabel_{i}")
                                    .Width(100)
                                    .Text(Text.Left(contactLabels[i] + ":", fontSmall, lightTextColor))
                                    .Enter()) { }

                                using (Paper.Box($"ContactValue_{i}")
                                    .Text(Text.Left(contactValues[i], fontSmall, textColor))
                                    .Enter()) { }
                            }
                        }
                    }
                }

                // Right panel - profile activity
                using (Paper.Column("ProfileActivity")
                    .Width(Paper.Stretch(0.6f))
                    .Margin(15, 0, 0, 0)
                    .Enter())
                {
                    // Activity tracker
                    using (Paper.Box("ActivityTracker")
                        .Height(Paper.Stretch(0.6f))
                        .BackgroundColor(cardBackground)
                        //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                        .Enter())
                    {
                        // Header
                        using (Paper.Box("ActivityHeader")
                            .Height(60)
                            .Margin(20, 20, 0, 0)
                            .Text(Text.Left("Activity Tracker", fontMedium, textColor))
                            .Enter()) { }

                        // Week days
                        using (Paper.Row("WeekDays")
                            .Height(30)
                            .Margin(20, 20, 0, 0)
                            .Enter())
                        {
                            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                            foreach (var day in days)
                            {
                                using (Paper.Box($"Day_{day}")
                                    .Text(Text.Center(day, fontSmall, lightTextColor))
                                    .Enter()) { }
                            }
                        }

                        // Activity grid - contribution calendar
                        using (Paper.Box("ActivityGrid")
                            .Margin(20, 20, 0, 20)
                            .Enter())
                        {
                            // Render contribution graph
                            Paper.AddActionElement((vg, rect) => {
                                int days = 7;
                                int weeks = 4;
                                double cellWidth = rect.width / days;
                                double cellHeight = rect.height / weeks;
                                double cellSize = Math.Min(cellWidth, cellHeight) * 0.8f;
                                double cellMargin = Math.Min(cellWidth, cellHeight) * 0.1f;

                                for (int week = 0; week < days; week++)
                                {
                                    for (int day = 0; day < weeks; day++)
                                    {   
                                        // Calculate position
                                        double x = rect.x + week * cellWidth + cellMargin;
                                        double y = rect.y + day * cellHeight + cellMargin;

                                        // Generate intensity based on position and time
                                        double value = Math.Sin(week * 0.4f + day * 0.7f + time) * 0.5f + 0.5f;
                                        value = Math.Pow(value, 1.5f);

                                        // Draw cell
                                        vg.BeginPath();
                                        vg.RoundedRect(x, y, cellSize, cellSize, 3, 3, 3, 3);

                                        // Apply color based on intensity
                                        int alpha = (int)(40 + value * 215);
                                        vg.SetFillColor(Color.FromArgb(alpha, primaryColor));
                                        vg.Fill();
                                    }
                                }
                            });
                        }
                    }

                    // Skills section
                    using (Paper.Box("SkillsSection")
                        .Height(Paper.Stretch(0.4f))
                        .BackgroundColor(cardBackground)
                        //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                        .Margin(0, 0, 15, 0)
                        .Enter())
                    {
                        // Header
                        using (Paper.Box("SkillsHeader")
                            .Height(20)
                            .Margin(20, 20, 20, 0)
                            .Text(Text.MiddleLeft("Skills", fontMedium, textColor))
                            .Enter()) { }

                        // Skill bars
                        string[] skills = { "Programming", "Design", "Communication", "Leadership", "Problem Solving" };
                        double[] skillLevels = { 0.9f, 0.75f, 0.8f, 0.6f, 0.85f };

                        using (Paper.Column("SkillBars")
                            .Margin(20)
                            .Enter())
                        {
                            for (int i = 0; i < skills.Length; i++)
                            {
                                using (Paper.Column($"Skill_{i}")
                                    .Height(Paper.Stretch(1.0f / skills.Length))
                                    .Margin(0, 0, i == 0 ? 0 : 10, 0)
                                    .Enter())
                                {
                                    // Skill label
                                    using (Paper.Box($"SkillLabel_{i}")
                                        .Height(25)
                                        .Text(Text.Left(skills[i], fontSmall, textColor))
                                        .Enter()) { }

                                    // Skill bar
                                    using (Paper.Row($"SkillBarBg_{i}")
                                        .Height(15)
                                        .BackgroundColor(Color.FromArgb(30, 0, 0, 0))
                                        //.Style(BoxStyle.SolidRounded(Color.FromArgb(30, 0, 0, 0), 7.5f))
                                        .Enter())
                                    {
                                        // Animate the skill level with time
                                        double animatedLevel = skillLevels[i];

                                        using (Paper.Box($"SkillBarFg_{i}")
                                            .Width(Paper.Percent(animatedLevel * 100f))
                                            .BackgroundColor(colorPalette[i % colorPalette.Length])
                                            //.Style(BoxStyle.SolidRoundedWithBorder(colorPalette[i % colorPalette.Length], primaryColor, 7.5f, 2))
                                            .Enter()) { }

                                        // Percentage label
                                        using (Paper.Box($"SkillPercent_{i}")
                                            .Width(40)
                                            .Text(Text.Right($"{animatedLevel * 100:F0}%", fontSmall, lightTextColor))
                                            .Enter()) { }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RenderSettingsTab()
        {
            using (Paper.Row("SettingsContent")
                .Margin(0, 0, 15, 0)
                .Enter())
            {
                // Settings categories sidebar
                using (Paper.Column("SettingsCategories")
                    .Width(200)
                    .Style("container")
                    .Enter())
                {
                    string[] categories = {
               "General", "Account", "Appearance",
               "Notifications", "Privacy", "Security"
           };

                    for (int i = 0; i < categories.Length; i++)
                    {
                        bool isSelected = i == 0;
                        Color itemTextColor = isSelected ? primaryColor : textColor;
                        var index = i;

                        Paper.Box($"SettingsCat_{i}")
                            .Height(50)
                            .Margin(10, 10, 5, 5)
                            .Style("button")
                            .StyleIf(isSelected, "period-button-selected")
                            .Text(Text.Left($"  {categories[i]}", fontSmall, itemTextColor))
                            .OnClick((rect) => { Console.WriteLine($"Category {categories[index]} clicked"); });
                    }
                }

                // Settings content
                using (Paper.Column("SettingsOptions")
                    .Style("container")
                    .Margin(15, 0, 0, 0)
                    .Enter())
                {
                    // Settings header
                    Paper.Box("SettingsHeader")
                        .Height(80)
                        .Margin(20)
                        .Text(Text.Left("General Settings", fontLarge, textColor));

                    // Toggle options - much cleaner now!
                    string[] options = {
               "Enable notifications", "Dark mode", "Auto-save changes",
               "Show analytics data", "Email notifications"
           };

                    for (int i = 0; i < options.Length; i++)
                    {
                        using (Paper.Row($"Setting_{i}")
                            .Height(60)
                            .Margin(20, 20, i == 0 ? 0 : 5, 5)
                            .Enter())
                        {
                            // Option label
                            Paper.Box($"SettingLabel_{i}")
                                .Text(Text.Left(options[i], fontMedium, textColor));

                            // Toggle switch - much simpler with styles!
                            bool isOn = toggleState[i];
                            int index = i;

                            using (Paper.Box($"ToggleSwitch_{i}")
                                .Style("toggle")
                                .StyleIf(isOn, "toggle-on")
                                .StyleIf(!isOn, "toggle-off")
                                .OnClick((rect) => {
                                    toggleState[index] = !toggleState[index];
                                    Console.WriteLine($"Toggle {options[index]}: {!isOn}");
                                })
                                .Enter())
                            {
                                Paper.Box($"ToggleDot_{i}")
                                    .Style("toggle-dot")
                                    .Left(Paper.Pixels(isOn ? 32 : 4));
                            }
                        }

                        // Add separator except for the last item
                        if (i < options.Length - 1)
                        {
                            Paper.Box($"Separator_{i}").Style("separator");
                        }
                    }

                    // Save button
                    Paper.Box("SaveSettings")
                        .Style("button-primary")
                        .Text(Text.Center("Save Changes", fontMedium, Color.White))
                        .Margin(20, 0, 20, 20)
                        .OnClick((rect) => Console.WriteLine("Save settings clicked"));
                }
            }
        }
        private static void RenderWindowsTab()
        {
            using (Paper.Row("WindowsContent")
                .Margin(0, 0, 15, 0)
                .Enter())
            {
                // Settings content
                using (Paper.Column("SettingsOptions")
                    .BackgroundColor(cardBackground)
                    //.Style(BoxStyle.SolidRounded(cardBackground, 8f))
                    .Margin(15/2, 0, 0, 0)
                    .Clip()
                    .Enter())
                {
                    // Button to open windows
                    using (Paper.Box("OpenWindowsButton")
                        .Height(50)
                        .Margin(20)
                        .Text(Text.Center("Open Windows", fontMedium, textColor))
                        .Style("button.primary")
                        .OnClick((rect) => OpenWindows())
                        .Enter()) { }

                    TestWindows();
                }
            }
        }

        private static void OpenWindows()
        {
            isWindowAOpen = true;
            isWindowBOpen = true;
        }

        private static void RenderFooter()
        {
            using (Paper.Row("Footer")
                .Height(50)
                .Rounded(8)
                .BackgroundColor(cardBackground)
                //.Style(BoxStyle.SolidRoundedWithBorder(cardBackground, Color.FromArgb(30, 0, 0, 0), 4f, 1f))
                .Margin(15, 15, 0, 15)
                .Enter())
            {
                // Copyright
                using (Paper.Box("Copyright")
                    .Margin(15, 0, 0, 0)
                    .Text(Text.Left("© 2025 PaperUI Demo.", fontSmall, lightTextColor))
                    .Enter()) { }

                // FPS Counter
                Paper.Box("FPS").Text(Text.Left($"FPS: {1f / Paper.DeltaTime:F1}", fontSmall, lightTextColor));
                Paper.Box("NodeCounter").Text(Text.Left($"Nodes: {Paper.CountOfAllElements}", fontSmall, lightTextColor));
                Paper.Box("MS").Text(Text.Left($"Frame ms: {Paper.MillisecondsSpent}", fontSmall, lightTextColor));

                // Footer links
                string[] links = { "Terms", "Privacy", "Contact", "Help" };
                using (Paper.Row("FooterLinks")
                    .Enter())
                {
                    foreach (var link in links)
                    {
                        using (Paper.Box($"Link_{link}")
                            .Width(Paper.Stretch(1f / links.Length))
                            .Text(Text.Center(link, fontSmall, primaryColor))
                            .OnClick((rect) => Console.WriteLine($"Link {link} clicked"))
                            .Enter()) { }
                    }
                }
            }
        }
    }
}
