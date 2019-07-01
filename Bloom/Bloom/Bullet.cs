using System.Numerics;
using Bloom.Handlers;
using WyvernFramework;
using WyvernFramework.Sprites;

namespace Bloom
{
    public class Bullet
    {
        public BulletHandler Handler { get; }
        public SpriteInstance Sprite { get; }

        public Vector3 Position
        {
            get => Sprite.Position;
            set
            {
                Sprite.Position = value;
            }
        }

        public Vector3 Velocity
        {
            get => Sprite.Velocity;
            set
            {
                Sprite.Velocity = value;
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

        public float Rotation
        {
            get => Sprite.Rotation;
            set
            {
                Sprite.Rotation = value;
            }
        }

        public Vector4 Rectangle
        {
            get => Sprite.Rectangle;
            /*set
            {
                Sprite.Rectangle = value;
            }*/
        }

        public Texture2D Texture
        {
            get => Sprite.Texture;
            /*set
            {
                Sprite.Texture = value;
            }*/
        }

        public Bullet(BulletHandler handler, SpriteInstance sprite)
        {
            Handler = handler;
            Sprite = sprite;
            handler.Register(this);
        }
    }
}
