using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strikers
{
    // How a clip behaves once it runs past its last frame.
    public enum LoopMode { Once, Loop, PingPong, HoldLast }

    // An immutable, shared animation definition: a slice of frames from one sheet
    // plus timing. Authored as JSON and loaded once by AnimationLibrary; many
    // entities reference the same Clip by id (see PLAN.md §4).
    public class Clip
    {
        public string Id;
        public Texture2D Texture;   // the source sheet
        public Rectangle[] Frames;  // source rectangle per frame
        public float FrameDuration; // seconds each frame is shown (uniform)
        public LoopMode Loop;

        // One full pass through every frame, in seconds.
        public float Duration => FrameDuration * Frames.Length;
    }
}
