using WyvernFramework;
using WyvernFramework.Sprites;
using Bloom.RenderPasses;
using Bloom.ImageEffects;
using VulkanCore;
using System.Numerics;

namespace Bloom.Scenes
{
    public class GameScene : Scene
    {
        public override string Description => "The main game scene";

        private BasicRenderPass TriangleRenderPass;
        private ClearEffect ClearEffect;
        private SpriteEffect SpriteEffect;
        private TransitionEffect TransitionEffect;

        public GameScene(WyvernWindow window) : base("Menu", window)
        {
            Content.Add<Texture2D>("TriangleTexture", "test.png");
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
            SpriteEffect = new SpriteEffect(
                    Graphics,
                    TriangleRenderPass,
                    2000,
                    ClearEffect.FinalLayout,
                    ClearEffect.FinalAccess,
                    ClearEffect.FinalStage
                );
            SpriteEffect.Start();
            var tex = Content["TriangleTexture"] as Texture2D;
            new SpriteInstance(SpriteEffect, Vector3.Zero, Vector3.Zero, new Vector2(24f, 24f), tex, new Rect2D(0, 0, 24, 24), null);
            TransitionEffect = new TransitionEffect(
                    Graphics,
                    SpriteEffect.FinalLayout,
                    SpriteEffect.FinalAccess,
                    SpriteEffect.FinalStage,
                    ImageLayout.ColorAttachmentOptimal,
                    Accesses.MemoryRead,
                    PipelineStages.BottomOfPipe
                );
            TransitionEffect.Start();
        }

        /// <summary>
        /// Called when ending the scene
        /// </summary>
        public override void OnEnd()
        {
            // Dispose of render pass
            TriangleRenderPass.Dispose();
            // End clear effect
            ClearEffect.End();
            // End triangle effect
            SpriteEffect.End();
        }

        /// <summary>
        /// Called when updating the scene
        /// </summary>
        public override void OnUpdate()
        {
        }

        /// <summary>
        /// Called when drawing the scene
        /// </summary>
        /// <param name="imageIndex"></param>
        /// <param name="start"></param>
        /// <param name="finished"></param>
        public override void OnDraw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            // Clear screen
            ClearEffect.Draw(start, Graphics.SwapchainAttachmentImages[imageIndex]);
            // Draw triangle
            SpriteEffect.Draw(ClearEffect.FinishedSemaphore, Graphics.SwapchainAttachmentImages[imageIndex]);
            // We are finished when the triangle is drawn
            finished = SpriteEffect.FinishedSemaphore;
        }
    }
}
