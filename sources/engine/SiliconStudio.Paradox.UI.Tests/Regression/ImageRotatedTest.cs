﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ScrollViewer"/> 
    /// </summary>
    public class ImageRotatedTest : UITestGameBase
    {
        private const int WindowWidth = 1024;
        private const int WindowHeight = 512;

        public ImageRotatedTest()
        {
            CurrentVersion = 6;
            GraphicsDeviceManager.PreferredBackBufferWidth = WindowWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = WindowHeight;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var sprites = Asset.Load<SpriteSheet>("RotatedImages");
            var img1 = new ImageElement { Source = sprites["NRNR"], StretchType = StretchType.Fill };
            var img2 = new ImageElement { Source = sprites["RNR"], StretchType = StretchType.Fill };
            var img3 = new ImageElement { Source = sprites["NRR"], StretchType = StretchType.Fill };
            var img4 = new ImageElement { Source = sprites["RR"], StretchType = StretchType.Fill };

            img1.SetGridColumnSpan(2);
            img2.SetGridColumnSpan(2);
            img2.SetGridRow(1);
            img3.SetGridRowSpan(2);
            img3.SetGridColumn(2);
            img4.SetGridRowSpan(2);
            img4.SetGridColumn(3);

            var grid = new UniformGrid
            {
                Rows = 2, 
                Columns = 4,
                Children = { img1, img2, img3, img4 }
            };

            UIComponent.RootElement = grid;
        }

        [Test]
        public void RunImageRotatedTest()
        {
            RunGameTest(new ImageRotatedTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageRotatedTest())
                game.Run();
        }
    }
}