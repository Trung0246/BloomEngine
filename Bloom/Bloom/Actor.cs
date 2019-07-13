using System;
using System.Numerics;
using Bloom.Handlers;
using WyvernFramework.Sprites;

namespace Bloom
{
    public class Actor
    {
        private static ulong NextID = 0;

        private float _maxHealth;
        private float _health;

        protected ActorHandler Handler;
        protected SpriteInstance Sprite;

        /// <summary>
        /// Unique Actor ID
        /// </summary>
        public ulong ID { get; private set; }

        /// <summary>
        /// Whether the actor is alive
        /// </summary>
        public bool Alive { get; private set; } = true;

        /// <summary>
        /// The actor's maximum health
        /// </summary>
        public float MaxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = MathF.Max(0f, value);
                _health = MathF.Min(_maxHealth, _health);
            }
        }

        /// <summary>
        /// The actor's health
        /// </summary>
        public float Health
        {
            get => _health;
            set
            {
                _health = MathF.Max(0f, MathF.Min(_maxHealth, value));
            }
        }

        public Vector3 Position
        {
            get => Sprite.Position;
            set
            {
                Sprite.Position = value;
            }
        }

        public float Rotation
        {
            get => Sprite.Rotation;
            set
            {
                Sprite.Rotation = value;
            }
        }

        public Vector2 Scale
        {
            get => Sprite.Scale;
            set
            {
                Sprite.Scale = value;
            }
        }

        /// <summary>
        /// Called when the actor is damaged
        /// </summary>
        public SqClosure OnDamageEvent;

        /// <summary>
        /// Called when the actor is killed
        /// </summary>
        public SqClosure OnKillEvent;

        public Actor(ActorHandler handler, float health)
        {
            ID = unchecked(NextID++);
            MaxHealth = health;
            Health = health;
            Handler = handler;
            handler.Register(this);
            OnRegister();
        }

        /// <summary>
        /// Deal damage to the actor
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        public void Damage(float damage, Actor attacker = null)
        {
            damage = OnDamage(damage, attacker);
            Health -= OnDamageEvent.CallAsClosure(damage, attacker).GetReturnAs<float>(0);
            if (Health < 0.00001f)
            {
                Kill();
            }
        }

        /// <summary>
        /// Kill the actor
        /// </summary>
        /// <param name="attacker"></param>
        public void Kill(Actor attacker = null)
        {
            if (OnKill(attacker) && OnKillEvent.CallAsClosure(attacker).GetReturnAs<bool>(0))
            {
                Health = 0;
                Alive = false;
            }
        }

        /// <summary>
        /// Called when registered in an ActorHandler
        /// </summary>
        protected virtual void OnRegister()
        {
        }

        /// <summary>
        /// Called when damaged
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        /// <returns>Damage to be applied</returns>
        protected virtual float OnDamage(float damage, Actor attacker)
        {
            return damage;
        }

        /// <summary>
        /// Called when killed
        /// </summary>
        /// <param name="attacker"></param>
        /// <returns>Whether the actor has actually been killed</returns>
        protected virtual bool OnKill(Actor attacker)
        {
            return true;
        }

        public override string ToString()
        {
            return $"[{nameof(Actor)} {ID}]";
        }
    }
}
