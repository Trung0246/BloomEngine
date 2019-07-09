using WyvernFramework;
using System.Numerics;

namespace Bloom
{
    public struct TextureRegion
    {
        public Texture2D Texture;
        public Vector4 Rectangle;

        public TextureRegion(Texture2D tex, Vector4 rect)
        {
            Texture = tex;
            Rectangle = rect;
        }
    }
}
