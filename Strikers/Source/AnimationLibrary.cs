using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Strikers
{
    // Loads sprite-sheet animation clips from JSON under Content/animations/ and hands
    // them out by id. A plain service (not ECS): clip definitions are shared global
    // data, so they live outside the world (see PLAN.md §4/§6). The JSON is raw-copied
    // by the pipeline, not built into .xnb, so we read it directly off disk; the sheets
    // it references are normal pipeline textures loaded via Content.Load.
    public class AnimationLibrary
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, Clip> _clips = new();

        public AnimationLibrary(ContentManager content)
        {
            _content = content;
        }

        public Clip Get(string id) =>
            _clips.TryGetValue(id, out var clip) ? clip : null;

        // Reads every *.json in Content/animations/. The clip id is the file name
        // without its extension (explosion.json -> "explosion").
        public void Load()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, _content.RootDirectory, "animations");
            if (!Directory.Exists(dir))
                return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var path in Directory.EnumerateFiles(dir, "*.json"))
            {
                var dto = JsonSerializer.Deserialize<ClipDto>(File.ReadAllText(path), options);
                if (dto == null)
                    continue;

                var id = Path.GetFileNameWithoutExtension(path);
                _clips[id] = BuildClip(id, dto);
            }
        }

        private Clip BuildClip(string id, ClipDto dto)
        {
            var texture = _content.Load<Texture2D>(dto.Sheet);
            var columns = texture.Width / dto.FrameWidth;

            var frames = new Rectangle[dto.Frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                var index = dto.Frames[i];
                frames[i] = new Rectangle(
                    (index % columns) * dto.FrameWidth,
                    (index / columns) * dto.FrameHeight,
                    dto.FrameWidth,
                    dto.FrameHeight);
            }

            var loop = Enum.TryParse<LoopMode>(dto.Loop, ignoreCase: true, out var parsed)
                ? parsed
                : LoopMode.Once;

            return new Clip
            {
                Id = id,
                Texture = texture,
                Frames = frames,
                FrameDuration = dto.Fps > 0f ? 1f / dto.Fps : 0.1f,
                Loop = loop,
            };
        }

        // Mirrors the animation JSON schema (see PLAN.md §4/§9). Frame triggers are part
        // of the planned schema but not consumed yet, so they aren't modelled here.
        private class ClipDto
        {
            public string Sheet { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public string Loop { get; set; }
            public float Fps { get; set; }
            public int[] Frames { get; set; }
        }
    }
}
