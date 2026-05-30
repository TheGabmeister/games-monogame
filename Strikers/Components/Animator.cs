namespace Extended.Components
{
    // Per-entity animation playback state — pure data (see PLAN.md §4). Gameplay
    // systems set ClipId; AnimationSystem advances Elapsed and writes the current
    // frame into the Sprite. Clip definitions live in the shared AnimationLibrary.
    public class Animator
    {
        public string ClipId;       // which clip in the AnimationLibrary to play
        public float Elapsed;       // seconds into the current clip
        public float Speed;         // playback rate multiplier (1 = normal)
        public string QueuedClipId; // optional clip to switch to when this one ends
        public bool Finished;       // set by AnimationSystem when a non-looping clip ends

        public Animator(string clipId, float speed = 1f)
        {
            ClipId = clipId;
            Speed = speed;
        }
    }
}
