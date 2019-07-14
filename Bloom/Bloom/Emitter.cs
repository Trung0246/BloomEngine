using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Bloom.Scenes;
using WyvernFramework.Sprites;

namespace Bloom
{
    public class Emitter
    {
        private struct EmitterState
        {
            public Emitter Emitter;
            public SpriteImage Image;
            public Animation Animation;
            public float Speed1;
            public float Speed2;
            public float Angle1;
            public float Angle2;
            public int Count1;
            public int Count2;
            public int Times;
            public float RepeatInterval;
        }

        public SpriteImage Image;
        public Animation Animation;
        public float Speed1 = 40;
        public float Speed2 = 20;
        public float Angle1 = 0;
        public float Angle2 = 0;
        public int Count1 = 1;
        public int Count2 = 1;
        public int Times = 1;
        public float RepeatInterval = 0f;

        /// <summary>
        /// Existing bullets that are owned by this emitter
        /// </summary>
        public IEnumerable<Bullet> Bullets
        {
            get => GameScene.Current.BulletHandler.Bullets.Where(e => e.ParentEmitter == this);
        }

        private EmitterState State
        {
            get
            {
                return new EmitterState
                {
                    Emitter = this,
                    Image = Image,
                    Animation = Animation,
                    Speed1 = Speed1,
                    Speed2 = Speed2,
                    Angle1 = Angle1,
                    Angle2 = Angle2,
                    Count1 = Count1,
                    Count2 = Count2,
                    Times = Times,
                    RepeatInterval = RepeatInterval
                };
            }
        }

        public Emitter()
        {
        }

        /// <summary>
        /// Fire the emitter once
        /// </summary>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        public void Fire(Vector2 position, float angle)
        {
            if (Times <= 0)
                return;
            var state = State;
            FirePattern(position, angle, state);
            if (Times > 1)
            {
                GameScene.Current.TimerHandler.StartTimer(
                        MathF.Max(0f, RepeatInterval),
                        (timer) =>
                        {
                            FirePattern(position, angle, state);
                        },
                        Times - 1
                    );
            }
        }

        /// <summary>
        /// Internal fire code
        /// </summary>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        /// <param name="state"></param>
        private static void FirePattern(Vector2 position, float angle, EmitterState state)
        {
            if (
                    state.Count1 == 0
                )
                throw new InvalidOperationException("Emitter Count1 is 0");
            if (
                    state.Count2 == 0
                )
                throw new InvalidOperationException("Emitter Count2 is 0");
            if (
                    state.Image.Size.LengthSquared() < 0.001f
                )
                throw new InvalidOperationException("Emitter Image has a size of 0");

            var bulletHandler = GameScene.Current.BulletHandler;
            var speedAdd = (state.Speed2 - state.Speed1) / state.Count2;
            var shotAngleAdd = state.Angle2 / state.Count1;
            var shotInd = 0;

            var speed = state.Speed1;
            for (var layer = 0; layer < state.Count2; layer++)
            {
                var shotAngle = angle + state.Angle1 - state.Angle2 / 2 + shotAngleAdd / 2;
                for (var shot = 0; shot < state.Count1; shot++)
                {
                    var pos = new Vector3(position + new Vector2(MathX.DCos(shotAngle), MathX.DSin(shotAngle)), 0f);
                    var bullet = bulletHandler.FireRaw(pos, shotAngle, speed, state.Image, state.Animation);
                    bullet.ParentEmitter = state.Emitter;
                    shotInd++;
                    shotAngle += shotAngleAdd;
                }
                speed += speedAdd;
            }
        }
    }
}
