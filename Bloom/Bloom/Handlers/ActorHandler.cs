using WyvernFramework;
using WyvernFramework.Sprites;
using Bloom.Scenes;
using Bloom.RenderPasses;
using VulkanCore;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Bloom.Handlers
{
    public class ActorHandler : IDebug
    {
        public virtual string Name => nameof(ActorHandler);
        public virtual string Description => "An actor handler";

        public List<Actor> Actors { get; }
        public int MaxActors { get; }

        private GameScene Scene { get; }
        private Graphics Graphics => Scene.Graphics;
        private ImageEffect PreviousEffect { get; }
        private BasicRenderPass RenderPass;
        public SpriteEffect SpriteEffect;

        public ActorHandler(GameScene scene, int maxActors, ImageEffect previousEffect = null)
        {
            Scene = scene;
            Actors = new List<Actor>(maxActors);
            MaxActors = maxActors;
            PreviousEffect = previousEffect;
        }

        public virtual void Start()
        {
            RenderPass = new BasicRenderPass(Graphics);
            SpriteEffect = new SpriteEffect(
                    Graphics,
                    RenderPass,
                    MaxActors,
                    PreviousEffect?.FinalLayout ?? ImageLayout.Undefined,
                    PreviousEffect?.FinalAccess ?? Accesses.None,
                    PreviousEffect?.FinalStage ?? PipelineStages.TopOfPipe
                );
            SpriteEffect.Start();
            SpriteEffect.SetCamera(Vector2.Zero, new Vector2(Graphics.Window.Size.X, -Graphics.Window.Size.Y));
        }

        public virtual void End()
        {
            RenderPass.Dispose();
            SpriteEffect.End();
        }

        public virtual void Update()
        {

        }

        public virtual void Draw(Semaphore start, int imageIndex, out Semaphore finished)
        {
            SpriteEffect.Draw(start, Graphics.SwapchainAttachmentImages[imageIndex]);
            finished = SpriteEffect.FinishedSemaphore;
        }

        public virtual void Register(Actor actor)
        {
            if (Actors.Count >= MaxActors)
                throw new InvalidOperationException($"Actor count has reached the maximum for {Name}");
            Actors.Add(actor);
        }
    }
}
