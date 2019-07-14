using System;
using System.Numerics;
using VulkanCore;
using WyvernFramework;
using Bloom.Scenes;
using Bloom.Handlers;

namespace Bloom
{
    /// <summary>
    /// The app window
    /// </summary>
    public class AppWindow : WyvernWindow
    {
        private GameScene GameScene { get; }

        /// <summary>
        /// App window constructor
        /// </summary>
        public AppWindow() : base(new Vector2(1280, 960), "Test App", 60.0)
        {
            GameScene = new GameScene(this);
        }

        /// <summary>
        /// Called when window starts
        /// </summary>
        public override void OnStart()
        {
            ScriptHandler.Init();
            GameScene.Start();
        }

        /// <summary>
        /// Called when window closes
        /// </summary>
        public override void OnClose()
        {
            Debug.WriteLine("CLOSING!!!");
            ScriptHandler.Close();
        }

        /// <summary>
        /// Called when updating logic
        /// </summary>
        public override void OnUpdate()
        {
            //GC.Collect();
            GameScene.Update();
            Title = $"Test App | Bullets={GameScene.Current.BulletHandler.Bullets.Count}" +
                $" UPS={1.0 / SmoothedUpdateDuration:0.00}" +
                $" FPS={1.0 / SmoothedDrawDuration:0.00}";
        }

        /// <summary>
        /// Called when drawing to a swapchain image
        /// </summary>
        /// <param name="start">The semaphore signaling when drawing should start</param>
        /// <param name="imageIndex">The swapchain image index</param>
        /// <param name="finished">The semaphore we will signal when drawing is done</param>
        protected override void OnDraw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            GameScene.Draw(start, imageIndex, out finished);
        }
    }
}
