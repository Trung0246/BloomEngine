﻿using VulkanCore;
using WyvernFramework;

namespace Bloom.ImageEffects
{
    public class TransitionEffect : ImageEffect
    {
        public TransitionEffect(
                Graphics graphics, ImageLayout initialLayout, Accesses initialAccess, PipelineStages initialStage,
                ImageLayout finalLayout, Accesses finalAccess, PipelineStages finalStage
            )
            : base(
                    nameof(TransitionEffect), graphics,
                    finalLayout, finalAccess, finalStage,
                    initialLayout, initialAccess, initialStage
                )
        {
        }

        protected override void OnRecordCommandBuffer(VKImage image, CommandBuffer buffer)
        {
            // Begin recording
            buffer.Begin(new CommandBufferBeginInfo());
            // Write commands
            buffer.CmdPipelineBarrier(
                    srcStageMask: InitialStage,
                    dstStageMask: FinalStage,
                    imageMemoryBarriers: new ImageMemoryBarrier[]
                    {
                            new ImageMemoryBarrier(
                                    image: image.Image,
                                    subresourceRange: image.SubresourceRange,
                                    srcAccessMask: InitialAccess,
                                    dstAccessMask: FinalAccess,
                                    oldLayout: InitialLayout,
                                    newLayout: FinalLayout
                                )
                    }
                );
            // Finish recording
            buffer.End();
        }

        public override void OnDraw(Semaphore start, VKImage image)
        {
            // Submit the command buffer
            Graphics.GraphicsQueueFamily.First.Submit(
                    start, InitialStage, GetCommandBuffer(image), FinishedSemaphore
                );
        }
    }
}
