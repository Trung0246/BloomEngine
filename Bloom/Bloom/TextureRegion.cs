using WyvernFramework;
using System.Numerics;

namespace Bloom
{
    public struct TextureRegion
    {
        public Texture2D Texture { get; }
        public Vector4 Rectangle { get; }

        public TextureRegion(Texture2D tex, Vector4 rect)
        {
            Texture = tex;
            Rectangle = rect;
        }

        public TextureRegion(Texture2D tex, Vector2 leftTopTexCoord, Vector2 sizeTexCoord)
        {
            Texture = tex;
            Rectangle = new Vector4(leftTopTexCoord, sizeTexCoord.X, sizeTexCoord.Y);
        }
    }
}
