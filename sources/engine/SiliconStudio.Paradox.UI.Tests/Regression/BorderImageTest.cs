﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ImageElement"/> 
    /// </summary>
    public class BorderImageTest : UITestGameBase
    {
        private StackPanel stackPanel;

        public BorderImageTest()
        {
            CurrentVersion = 6;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var sprite = new Sprite(Asset.Load<Texture>("BorderButton")) { Borders = new Vector4(64, 64, 64, 64) };

            var bi1 = new ImageElement { Source = sprite, Height = 150 };
            var bi2 = new ImageElement { Source = sprite, Height = 300 };
            var bi3 = new ImageElement { Source = sprite, Height = 500 };

            stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(bi1);
            stackPanel.Children.Add(bi2);
            stackPanel.Children.Add(bi3);

            UIComponent.RootElement = new ScrollViewer { Content = stackPanel, ScrollMode = ScrollingMode.HorizontalVertical };
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(DrawSmallerElement).TakeScreenshot();
            FrameGameSystem.Draw(DrawRealSizeElement).TakeScreenshot();
            FrameGameSystem.Draw(DrawBiggerElement).TakeScreenshot();
        }

        public void DrawSmallerElement()
        {
            if(stackPanel != null)
                stackPanel.ScrolllToElement(0);
        }
        public void DrawRealSizeElement()
        {
            if (stackPanel != null)
                stackPanel.ScrolllToElement(1);
        }
        public void DrawBiggerElement()
        {
            if (stackPanel != null)
                stackPanel.ScrolllToElement(2);
        }

        [Test]
        public void RunBorderImageTest()
        {
            RunGameTest(new BorderImageTest());
        }
        
        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new BorderImageTest())
                game.Run();
        }
    }
}