﻿using System.Numerics;
using VulkanCore;
using WyvernFramework;
using Bloom.Scenes;

namespace Bloom
{
    /// <summary>
    /// The app window
    /// </summary>
    public class AppWindow : WyvernWindow
    {
        private GameScene MenuScene { get; }

        /// <summary>
        /// App window constructor
        /// </summary>
        public AppWindow() : base(new Vector2(1280, 960), "Test App", 60.0)
        {
            // Create the menu scene
            MenuScene = new GameScene(this);
            // Start the menu scene
            MenuScene.Start();
        }

        /// <summary>
        /// Called when updating logic
        /// </summary>
        public override void OnUpdate()
        {
            // Update the menu scene
            MenuScene.Update();
            // Set title
            Title = $"Test App | UPS={1.0 / SmoothedUpdateDuration:0.00} FPS={1.0 / SmoothedDrawDuration:0.00}";
        }

        /// <summary>
        /// Called when drawing to a swapchain image
        /// </summary>
        /// <param name="start">The semaphore signaling when drawing should start</param>
        /// <param name="imageIndex">The swapchain image index</param>
        /// <param name="finished">The semaphore we will signal when drawing is done</param>
        protected override void OnDraw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            // Draw the menu scene
            MenuScene.Draw(start, imageIndex, out finished);
        }
    }
}
