using System.Numerics;
using WyvernFramework;
using WyvernFramework.Sprites;

namespace Bloom
{
    public static class Extensions
    {
        public static Vector2 GetSize(this Texture2D texture)
        {
            return new Vector2(texture.Image.Extent.Width, texture.Image.Extent.Height);
        }

        public static SpriteImage GetSpriteImage(this SpriteInstance spriteInstance)
        {
            var texSize = spriteInstance.Texture.GetSize();
            var leftTop = new Vector2(spriteInstance.Rectangle.X, spriteInstance.Rectangle.Y) / texSize;
            var size = new Vector2(spriteInstance.Rectangle.Z, spriteInstance.Rectangle.W) / texSize;
            return new SpriteImage(spriteInstance.Texture, leftTop, size);
        }
    }
}
