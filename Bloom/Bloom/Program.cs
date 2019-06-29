using System;
using WyvernFramework;
using VkGLFW3;

namespace Bloom
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WyvernWindow.Init();
            if (!VkGlfw.VulkanSupported)
                throw new PlatformNotSupportedException("Vulkan unsupported on this machine!");

            using (var window = new AppWindow())
                window.Start();

            WyvernWindow.Terminate();
        }
    }
}
