using WyvernFramework;
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
        public static GameScene Current { get; private set; }

        public override string Description => "The main game scene";

        private BasicRenderPass TriangleRenderPass;
        private ClearEffect ClearEffect;
        private PlayerInterfaceHandler PlayerHandler;
        public TimerHandler TimerHandler { get; private set; }
        public BulletHandler BulletHandler { get; private set; }
        public EnemyHandler EnemyHandler { get; private set; }

        public GameScene(WyvernWindow window) : base(nameof(GameScene), window)
        {
            Content.Add<Texture2D>("TestBullets", "bullets.png");
            Content.Add<Texture2D>("TestEnemy", "enemy.png");
        }

        /// <summary>
        /// Called when starting the scene
        /// </summary>
        public override void OnStart()
        {
            Current = this;
            TriangleRenderPass = new BasicRenderPass(Graphics);
            ClearEffect = new ClearEffect(Graphics, TriangleRenderPass);
            ClearEffect.Start();
            ClearEffect.ClearColor = new ClearColorValue(0.5f, 0.7f, 0.9f);
            TimerHandler = new TimerHandler(this);
            PlayerHandler = new PlayerInterfaceHandler(1);
            BulletHandler = new BulletHandler(this, 200000, ClearEffect);
            BulletHandler.Start();
            EnemyHandler = new EnemyHandler(this, 10000, BulletHandler.SpriteEffect);
            EnemyHandler.Start();

            ScriptHandler.CallGlobal("TestEmitter");
        }

        /// <summary>
        /// Called when ending the scene
        /// </summary>
        public override void OnEnd()
        {
            Current = null;
            TriangleRenderPass.Dispose();
            ClearEffect.End();
            TimerHandler.End();
            //PlayerHandler.End();
            BulletHandler.End();
            EnemyHandler.End();
        }

        /// <summary>
        /// Called when updating the scene
        /// </summary>
        public override void OnUpdate()
        {
            TimerHandler.Update();
            //PlayerHandler.Update();
            BulletHandler.Update();
            EnemyHandler.Update();
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
            EnemyHandler.Draw(bulletsFinished, imageIndex, out var enemyFinished);
            finished = enemyFinished;
        }
    }
}
