﻿using WyvernFramework;
using WyvernFramework.Sprites;
using Bloom.RenderPasses;
using Bloom.ImageEffects;
using VulkanCore;
using System.Numerics;
using Bloom.Handlers;

namespace Bloom.Scenes
{
    public class GameScene : Scene
    {
        public override string Description => "The main game scene";

        private BasicRenderPass TriangleRenderPass;
        private ClearEffect ClearEffect;
        private PlayerHandler PlayerHandler;
        private BulletHandler BulletHandler;
        private TimerHandler TimerHandler;
        private Animation TestAnimation;

        public GameScene(WyvernWindow window) : base(nameof(GameScene), window)
        {
            Content.Add<Texture2D>("TestBullets", "bullets.png");
        }

        /// <summary>
        /// Called when starting the scene
        /// </summary>
        public override void OnStart()
        {
            TriangleRenderPass = new BasicRenderPass(Graphics);
            ClearEffect = new ClearEffect(Graphics, TriangleRenderPass);
            ClearEffect.Start();
            ClearEffect.ClearColor = new ClearColorValue(0.5f, 0.7f, 0.9f);
            PlayerHandler = new PlayerHandler(1);
            BulletHandler = new BulletHandler(this, 200000, ClearEffect);
            BulletHandler.Start();
            TimerHandler = new TimerHandler(this);

            TestAnimation = new Animation(new Animation.Instruction[] {
                Animation.Instruction.SetScale(0f, new Vector2(64f, 64f)),
                Animation.Instruction.LerpScale(0f, 1f, new Vector2(16f, 16f)),
                Animation.Instruction.None(20f)
            });
            TimerHandler.StartTimer(0.5, () =>
            {
                var tex = Content["TestBullets"] as Texture2D;
                var rand = new System.Random();
                for (var i = 0; i < 3000; i++)
                {
                    var x = (rand.Next() % 10) * 18;
                    var rect = new Rect2D(1 + x, 1 + x, 16, 16);
                    BulletHandler.FireRaw(Vector3.Zero, (float)(rand.NextDouble() * 360.0), 60f, tex, rect, TestAnimation);
                }
                Debug.Info(BulletHandler.Bullets.Count.ToString());
            }, -1);
        }

        /// <summary>
        /// Called when ending the scene
        /// </summary>
        public override void OnEnd()
        {
            TriangleRenderPass.Dispose();
            ClearEffect.End();
            BulletHandler.End();
        }

        /// <summary>
        /// Called when updating the scene
        /// </summary>
        public override void OnUpdate()
        {
            TimerHandler.Update();
        }

        /// <summary>
        /// Called when drawing the scene
        /// </summary>
        /// <param name="imageIndex"></param>
        /// <param name="start"></param>
        /// <param name="finished"></param>
        public override void OnDraw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            ClearEffect.Draw(start, Graphics.SwapchainAttachmentImages[imageIndex]);
            BulletHandler.Draw(ClearEffect.FinishedSemaphore, imageIndex, out var bulletsFinished);
            finished = bulletsFinished;
        }
    }
}
