using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Advances each Animator's clip and writes the current frame's source rectangle
    // into its Sprite, so the RenderSystem stays animation-agnostic. The system just
    // plays whatever ClipId is set; gameplay systems decide which clip that is
    // (see PLAN.md §4). Clip definitions come from the shared AnimationLibrary.
    public class AnimationSystem : EntityProcessingSystem
    {
        private readonly AnimationLibrary _library;
        private ComponentMapper<Animator> _animatorMapper;
        private ComponentMapper<Sprite> _spriteMapper;

        public AnimationSystem(AnimationLibrary library)
            : base(Aspect.All(typeof(Animator), typeof(Sprite)))
        {
            _library = library;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _animatorMapper = mapperService.GetMapper<Animator>();
            _spriteMapper = mapperService.GetMapper<Sprite>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var animator = _animatorMapper.Get(entityId);
            var clip = _library.Get(animator.ClipId);
            if (clip == null || clip.Frames.Length == 0)
                return;

            animator.Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds * animator.Speed;

            var frame = ResolveFrame(clip, animator.Elapsed, out var finished);
            animator.Finished = finished;
            _spriteMapper.Get(entityId).SourceRect = clip.Frames[frame];

            // When a finished clip has a queued follow-up, switch to it and restart.
            if (finished && animator.QueuedClipId != null)
            {
                animator.ClipId = animator.QueuedClipId;
                animator.QueuedClipId = null;
                animator.Elapsed = 0f;
                animator.Finished = false;
            }
        }

        // Maps elapsed time to a frame index for the clip's loop mode. `finished` is
        // true once a non-looping clip has reached (and is holding) its last frame.
        private static int ResolveFrame(Clip clip, float elapsed, out bool finished)
        {
            finished = false;
            int count = clip.Frames.Length;
            if (count == 1)
            {
                finished = elapsed >= clip.Duration;
                return 0;
            }

            int step = (int)(elapsed / clip.FrameDuration);

            switch (clip.Loop)
            {
                case LoopMode.Loop:
                    return step % count;

                case LoopMode.PingPong:
                    int period = 2 * (count - 1);
                    int m = step % period;
                    return m < count ? m : period - m;

                case LoopMode.Once:
                case LoopMode.HoldLast:
                default:
                    if (step >= count)
                    {
                        finished = true;
                        return count - 1;
                    }
                    return step;
            }
        }
    }
}
