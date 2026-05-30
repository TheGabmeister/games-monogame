using System.Collections.Generic;
using Strikers.Components;
using Microsoft.Xna.Framework;

namespace Strikers
{
    // The stage's wave timeline: a precomputed, time-ordered list of enemy spawns the
    // EnemySpawnSystem plays back. The timeline is singleton flow control, not a "many
    // similar things" problem, so it's a plain class the system reads from rather than
    // ECS state (see PLAN.md §6). Waves are authored by tuning numbers in CreateDefault;
    // the system just spawns whatever is due and declares STAGE CLEAR once the list is
    // exhausted and the playfield is empty.
    public class Stage
    {
        public readonly struct Spawn
        {
            public readonly float Time;        // seconds from stage start
            public readonly EnemyType Type;
            public readonly Vector2 Position;
            public readonly Vector2 Velocity;

            public Spawn(float time, EnemyType type, Vector2 position, Vector2 velocity)
            {
                Time = time;
                Type = type;
                Position = position;
                Velocity = velocity;
            }
        }

        private readonly List<Spawn> _spawns = new();

        public IReadOnlyList<Spawn> Spawns => _spawns;

        // Enemies enter from just above the top edge.
        private const float EntryY = -40f;

        // Adds a line of `count` enemies of `type`, one every `interval` seconds starting
        // at `time`, entering at X positions swept evenly from `xStart` to `xEnd`. All move
        // at (vx, speed); leave vx 0 for a straight descent.
        private void AddLine(float time, EnemyType type, int count, float interval,
                             float xStart, float xEnd, float speed, float vx = 0f)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                float x = MathHelper.Lerp(xStart, xEnd, t);
                _spawns.Add(new Spawn(time + i * interval, type,
                    new Vector2(x, EntryY), new Vector2(vx, speed)));
            }
        }

        // The one v1 stage: fodder sweeps, fighters, a couple of gunships, building to a
        // final pair of gunships. Tuned so the player wants the weapon power-ups the
        // gunships drop to clear the finale.
        public static Stage CreateDefault()
        {
            var s = new Stage();
            s.AddLine( 1.0f, EnemyType.Popcorn, 6, 0.25f,  80f, 520f, 150f);
            s.AddLine( 5.0f, EnemyType.Popcorn, 6, 0.25f, 520f,  80f, 150f);
            s.AddLine( 9.0f, EnemyType.Fighter, 3, 0.80f, 120f, 480f,  90f);
            s.AddLine(15.0f, EnemyType.Popcorn, 8, 0.20f,  60f, 540f, 170f);
            s.AddLine(20.0f, EnemyType.Gunship, 1, 0.00f, 300f, 300f,  70f);
            s.AddLine(21.0f, EnemyType.Popcorn, 6, 0.30f, 100f, 500f, 150f);
            s.AddLine(28.0f, EnemyType.Fighter, 4, 0.60f,  80f, 520f,  90f);
            s.AddLine(34.0f, EnemyType.Popcorn,10, 0.18f,  60f, 540f, 180f);
            s.AddLine(42.0f, EnemyType.Gunship, 2, 1.35f, 180f, 420f,  65f);
            s.AddLine(44.0f, EnemyType.Fighter, 3, 0.70f, 120f, 480f, 100f);
            s.AddLine(52.0f, EnemyType.Gunship, 2, 1.20f, 160f, 440f,  60f);
            return s;
        }
    }
}
