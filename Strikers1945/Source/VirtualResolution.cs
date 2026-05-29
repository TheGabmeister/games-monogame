namespace Extended
{
    // The fixed portrait canvas all gameplay math is expressed in (see PLAN.md §4).
    // In Phase 0 the back buffer is sized to match; render-target scaling to an
    // arbitrary window is a later polish step.
    public static class VirtualResolution
    {
        public const int Width = 600;
        public const int Height = 800;
    }
}
