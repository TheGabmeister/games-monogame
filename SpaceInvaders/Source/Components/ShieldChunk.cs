using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class ShieldChunk : Component, ITriggerListener
    {
        SoundEffect _shieldHit;

        public override void OnAddedToEntity()
        {
            _shieldHit = Entity.Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.ShieldHit);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            _shieldHit.Play();
            ExplosionHelper.SpawnShieldPuff(Entity.Scene, Entity.Transform.Position);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
