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
    public class EnemyHandler : ActorHandler
    {
        public override string Name => nameof(EnemyHandler);
        public override string Description => "The enemy handler";

        public IEnumerable<Enemy> Enemies => Actors.Select(
                (actor) =>
                {
                    if (actor is Enemy enemy)
                        return enemy;
                    throw new InvalidOperationException($"Actors contains actor that is not an {typeof(Enemy).Name}");
                }
            );

        public EnemyHandler(GameScene scene, int maxEnemies, ImageEffect previousEffect = null)
            : base(scene, maxEnemies, previousEffect)
        {
        }
    }
}
