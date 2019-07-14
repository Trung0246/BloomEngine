using WyvernFramework;
using WyvernFramework.Sprites;
using Bloom.Scenes;
using Bloom.RenderPasses;
using VulkanCore;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace Bloom.Handlers
{
    public class BulletHandler : IDebug
    {
        public string Name => nameof(BulletHandler);
        public string Description => "The bullet handler";

        public List<Bullet> Bullets { get; }
        public int MaxBullets { get; }

        private GameScene Scene { get; }
        private Graphics Graphics => Scene.Graphics;
        private ImageEffect PreviousEffect { get; }
        private BasicRenderPass RenderPass;
        public SpriteEffect SpriteEffect;

        public Vector4 Bounds = new Vector4(-640, -480, 1280, 960);

        public BulletHandler(GameScene scene, int maxBullets, ImageEffect previousEffect = null)
        {
            Scene = scene;
            Bullets = new List<Bullet>(maxBullets);
            MaxBullets = maxBullets;
            PreviousEffect = previousEffect;
        }

        public void Start()
        {
            RenderPass = new BasicRenderPass(Graphics);
            SpriteEffect = new SpriteEffect(
                    Graphics,
                    RenderPass,
                    MaxBullets,
                    PreviousEffect?.FinalLayout ?? ImageLayout.Undefined,
                    PreviousEffect?.FinalAccess ?? Accesses.None,
                    PreviousEffect?.FinalStage ?? PipelineStages.TopOfPipe
                );
            SpriteEffect.Start();
            SpriteEffect.SetCamera(Vector2.Zero, new Vector2(Graphics.Window.Size.X, -Graphics.Window.Size.Y));
        }

        public void End()
        {
            RenderPass.Dispose();
            SpriteEffect.End();
        }

        public void Update()
        {
            if (Bullets.Count == 0)
                return;
            for (var i = 0; i < Bullets.Count; i++)
            {
                Bullets[i].UpdateNotImportant();
            }
            var exists = Bullets.Where(e => e.Exists).ToArray();
            Bullets.Clear();
            Bullets.AddRange(exists);
        }

        public void Draw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            SpriteEffect.Draw(start, Graphics.SwapchainAttachmentImages[imageIndex]);
            finished = SpriteEffect.FinishedSemaphore;
        }

        public Bullet FireRaw(Vector3 pos, float angle, float speed, SpriteImage image, Animation anim = null)
        {
            var vel = new Vector3(MathX.DCos(angle) * speed, MathX.DSin(angle) * speed, 0f);
            var tex = image.Texture;
            var sprite = new SpriteInstance(SpriteEffect, pos, vel, image.Size, tex, image.TextureRegion.Rectangle, anim);
            sprite.Rotation = MathX.DegToRad(angle - 90f);
            var bullet = new Bullet(this, sprite);
            return bullet;
        }

        public void Register(Bullet bullet)
        {
            if (Bullets.Count >= MaxBullets)
                throw new InvalidOperationException($"Bullet count has reached the maximum for {Name}");
            Bullets.Add(bullet);
        }
    }
}
