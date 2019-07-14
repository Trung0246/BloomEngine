using System.Numerics;
using WyvernFramework;

namespace Bloom
{
    public struct SpriteImage
    {
        /// <summary>
        /// The texture and region in the texture (in texture coordinates)
        /// </summary>
        public TextureRegion TextureRegion { get; }

        /// <summary>
        /// The sprite image's texture
        /// </summary>
        public Texture2D Texture => TextureRegion.Texture;

        /// <summary>
        /// The sprite image's top-left coordinates (in pixels) in the texture
        /// </summary>
        public Vector2 LeftTop => new Vector2(TextureRegion.Rectangle.X, TextureRegion.Rectangle.Y) * Texture.GetSize();

        /// <summary>
        /// The sprite image's size (in pixels) in the texture
        /// </summary>
        public Vector2 Size => new Vector2(TextureRegion.Rectangle.Z, TextureRegion.Rectangle.W) * Texture.GetSize();

        public SpriteImage(Texture2D texture, Vector2 leftTop, Vector2 size)
        {
            leftTop /= texture.GetSize();
            size /= texture.GetSize();
            TextureRegion = new TextureRegion(texture, new Vector4(leftTop, size.X, size.Y));
        }
    }
}
