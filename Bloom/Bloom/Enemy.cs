using WyvernFramework;
using WyvernFramework.Sprites;
using Bloom.Handlers;
using Bloom.Scenes;
using System.Numerics;

namespace Bloom
{
    public class Enemy : Actor
    {
        public Enemy(float health)
            : base(GameScene.Current.EnemyHandler, health)
        {

        }

        protected override void OnRegister()
        {
            Debug.Info($"Enemy {this} registered", nameof(Enemy));

            Sprite = new SpriteInstance(
                    Handler.SpriteEffect,
                    Vector3.Zero, Vector3.Zero,
                    new Vector2(64f),
                    GameScene.Current.Content.GetLoadedContent<Texture2D>("TestEnemy"),
                    new Vector4(0f, 0f, 0.25f, 1f),
                    null
                );

            base.OnRegister();
        }

        protected override float OnDamage(float damage, Actor attacker)
        {
            Debug.Info($"Enemy {this} damaged for {damage} by {attacker}", nameof(Enemy));

            return base.OnDamage(damage, attacker);
        }

        protected override bool OnKill(Actor attacker)
        {
            Debug.Info($"Enemy {this} killed by {attacker}", nameof(Enemy));

            return base.OnKill(attacker);
        }

        public override string ToString()
        {
            return $"[{nameof(Enemy)} {ID}]";
        }
    }
}
