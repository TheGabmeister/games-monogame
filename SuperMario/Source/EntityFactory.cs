using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class EntityFactory
    {
        readonly Dictionary<string, Action<Scene, TmxObject>> _factories = new();
        readonly GameState _gameState;

        public EntityFactory(GameState gameState)
        {
            _gameState = gameState;
            Register("PlayerStart", CreatePlayerStart);
            Register("Platform", CreatePlatform);
            Register("Mushroom", CreateMushroom);
            Register("FireFlower", CreateFireFlower);
            Register("OneUp", CreateOneUp);
            Register("Coin", CreateCoin);
            Register("LeftRightLift", CreateLeftRightLift);
            Register("UpDownLift", CreateUpDownLift);
            Register("Goomba", CreateGoomba);
            Register("GreenKoopaTroopa", CreateGreenKoopaTroopa);
            Register("RedKoopaTroopa", CreateRedKoopaTroopa);
            Register("GreenKoopaParatroopa", CreateGreenKoopaParatroopa);
            Register("RedKoopaParatroopa", CreateRedKoopaParatroopa);
            Register("BuzzyBeetle", CreateBuzzyBeetle);
            Register("Spiny", CreateSpiny);
            Register("PiranhaPlant", CreatePiranhaPlant);
            Register("HammerBro", CreateHammerBro);
            Register("Blooper", CreateBlooper);
            Register("BulletBillCannon", CreateBulletBillCannon);
            Register("Podoboo", CreatePodoboo);
            Register("GoalTrigger", CreateGoalTrigger);
            Register("KillVolume", CreateKillVolume);
        }

        public void Register(string type, Action<Scene, TmxObject> factory)
        {
            _factories[type] = factory;
        }

        public void Spawn(Scene scene, TmxObject obj)
        {
            if (_factories.TryGetValue(obj.Type, out var factory))
                factory(scene, obj);
        }

        void CreatePlayerStart(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            scene.CreateEntity("playerstart", center)
                .AddComponent(new PlayerStart());
        }

        public PlayerController CreatePlayer(Scene scene, Vector2 position)
        {
            var player = scene.CreateEntity("player", position);
            var renderer = player.AddComponent(new PrototypeSpriteRenderer(32, 48));
            renderer.SetColor(Color.Red);
            var blinker = player.AddComponent(new Blinker(renderer));
            player.AddComponent(new Mover());

            var collider = player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            return player.AddComponent(new PlayerController(renderer, blinker));
        }

        public void CreateFireball(Scene scene, Vector2 position, int facing, PlayerController owner)
        {
            const int size = 16;

            var fireball = scene.CreateEntity("fireball", position);
            fireball.AddComponent(new PrototypeSpriteRenderer(size, size)).SetColor(Color.OrangeRed);
            fireball.AddComponent(new Mover());

            var collider = fireball.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            collider.PhysicsLayer = 1 << PhysicsLayers.Projectile;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = fireball.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Projectile;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Enemy;

            fireball.AddComponent(new Fireball(owner, facing));
            fireball.AddComponent(new Lifetime(3f));
        }

        public void CreateHammer(Scene scene, Vector2 position, int facing)
        {
            const int size = 16;

            var hammer = scene.CreateEntity("hammer", position);
            hammer.AddComponent(new PrototypeSpriteRenderer(size, size)).SetColor(Color.Gray);
            hammer.AddComponent(new Mover());

            var hit = hammer.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.EnemyProjectile;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            hammer.AddComponent(new Hammer(facing));
            hammer.AddComponent(new Lifetime(Constants.HammerLifetime));
        }

        public void CreateBulletBill(Scene scene, Vector2 position, int facing)
        {
            const int size = 24;

            var bill = scene.CreateEntity("bulletbill", position);
            bill.AddComponent(new PrototypeSpriteRenderer(size, size)).SetColor(Color.Black);
            bill.AddComponent(new Mover());

            var hit = bill.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            bill.AddComponent(new DamagePlayerTrigger());
            bill.AddComponent(new BulletBill(facing));
            bill.AddComponent(new Lifetime(Constants.BulletBillLifetime));
        }

        public static Vector2 GetCenter(TmxObject obj)
        {
            var offset = new Vector2(obj.Width / 2f, obj.Height / 2f);

            if (obj.Rotation != 0)
            {
                var rad = MathHelper.ToRadians(obj.Rotation);
                var cos = Mathf.Cos(rad);
                var sin = Mathf.Sin(rad);
                offset = new Vector2(
                    cos * offset.X - sin * offset.Y,
                    sin * offset.X + cos * offset.Y);
            }

            return new Vector2(obj.X + offset.X, obj.Y + offset.Y);
        }

        void CreatePlatform(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var envLayer = 1 << PhysicsLayers.Environment;
            var envCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            var platform = scene.CreateEntity(obj.Name, center);
            platform.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.SaddleBrown);
            var collider = platform.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = envLayer;
            collider.CollidesWithLayers = envCollidesWith;

            if (obj.Rotation != 0)
                platform.RotationDegrees = obj.Rotation;
        }

        void CreateLeftRightLift(Scene scene, TmxObject obj)
        {
            CreateMovingPlatform(scene, obj, MovingPlatformAxis.Horizontal);
        }

        void CreateUpDownLift(Scene scene, TmxObject obj)
        {
            CreateMovingPlatform(scene, obj, MovingPlatformAxis.Vertical);
        }

        void CreateMovingPlatform(Scene scene, TmxObject obj, MovingPlatformAxis axis)
        {
            var center = GetCenter(obj);
            var platform = scene.CreateEntity(obj.Name, center);
            platform.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.SteelBlue);

            var collider = platform.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            platform.AddComponent(new MovingPlatform(
                axis,
                Constants.MovingPlatformDistance,
                Constants.MovingPlatformSpeed));
        }

        void CreateMushroom(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var mushroom = scene.CreateEntity("mushroom", center);
            mushroom.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Yellow);
            mushroom.AddComponent(new Mover());
            mushroom.AddComponent(new GravityBody());

            var body = mushroom.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = mushroom.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            mushroom.AddComponent(new Mushroom(_gameState));
        }

        void CreateFireFlower(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var fireFlower = scene.CreateEntity("fireflower", center);
            fireFlower.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.OrangeRed);
            fireFlower.AddComponent(new Mover());
            fireFlower.AddComponent(new GravityBody());

            var body = fireFlower.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = fireFlower.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            fireFlower.AddComponent(new FireFlower(_gameState));
        }

        void CreateOneUp(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var oneUp = scene.CreateEntity("oneup", center);
            oneUp.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Green);
            oneUp.AddComponent(new Mover());
            oneUp.AddComponent(new GravityBody());

            var body = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            oneUp.AddComponent(new OneUp(_gameState));
        }

        void CreateCoin(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var coin = scene.CreateEntity("coin", center);
            coin.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Gold);

            var pickup = coin.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            coin.AddComponent(new Coin(_gameState));
        }

        void CreateGoomba(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var goomba = scene.CreateEntity("goomba", center);
            goomba.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Brown);
            var mover = goomba.AddComponent(new Mover());
            goomba.AddComponent(new GravityBody());

            var body = goomba.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = goomba.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            goomba.AddComponent(new DamagePlayerTrigger());
            goomba.AddComponent(new Goomba());
            goomba.AddComponent(new EnemyWalker(mover, Constants.GoombaWalkSpeed));
        }

        void CreateSpiny(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var spiny = scene.CreateEntity("spiny", center);
            spiny.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.MediumPurple);
            var mover = spiny.AddComponent(new Mover());
            spiny.AddComponent(new GravityBody());

            var body = spiny.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = spiny.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            spiny.AddComponent(new DamagePlayerTrigger());
            spiny.AddComponent(new Spiny());
            spiny.AddComponent(new EnemyWalker(mover, Constants.SpinyWalkSpeed));
        }

        void CreateGreenKoopaTroopa(Scene scene, TmxObject obj)
        {
            CreateKoopaTroopa(scene, GetCenter(obj), obj.Width, obj.Height, KoopaColor.Green);
        }

        void CreateRedKoopaTroopa(Scene scene, TmxObject obj)
        {
            CreateKoopaTroopa(scene, GetCenter(obj), obj.Width, obj.Height, KoopaColor.Red);
        }

        public Entity CreateKoopaTroopa(Scene scene, Vector2 position, float width, float height, KoopaColor color)
        {
            var isRed = color == KoopaColor.Red;
            var koopa = scene.CreateEntity(isRed ? "redkoopatroopa" : "greenkoopatroopa", position);
            koopa.AddComponent(new PrototypeSpriteRenderer(width, height)).SetColor(isRed ? Color.Red : Color.Green);
            var mover = koopa.AddComponent(new Mover());
            koopa.AddComponent(new GravityBody());

            var body = koopa.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = koopa.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            koopa.AddComponent(new DamagePlayerTrigger());
            koopa.AddComponent(new KoopaTroopa(color));
            koopa.AddComponent(new EnemyWalker(
                mover,
                isRed ? Constants.RedKoopaTroopaWalkSpeed : Constants.GreenKoopaTroopaWalkSpeed));
            return koopa;
        }

        void CreateGreenKoopaParatroopa(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var para = scene.CreateEntity("greenkoopaparatroopa", center);
            para.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.LightGreen);
            var mover = para.AddComponent(new Mover());

            var body = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            para.AddComponent(new DamagePlayerTrigger());
            para.AddComponent(new KoopaParatroopa(
                this,
                mover,
                KoopaColor.Green,
                w,
                h,
                Constants.GreenKoopaParatroopaFlySpeed));
        }

        void CreateRedKoopaParatroopa(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var para = scene.CreateEntity("redkoopaparatroopa", center);
            para.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.IndianRed);
            var mover = para.AddComponent(new Mover());

            var body = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            para.AddComponent(new DamagePlayerTrigger());
            para.AddComponent(new KoopaParatroopa(
                this,
                mover,
                KoopaColor.Red,
                w,
                h,
                Constants.RedKoopaParatroopaFlySpeed));
        }

        void CreateBuzzyBeetle(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var buzzy = scene.CreateEntity("buzzybeetle", center);
            buzzy.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.DarkSlateBlue);
            var mover = buzzy.AddComponent(new Mover());
            buzzy.AddComponent(new GravityBody());

            var body = buzzy.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = buzzy.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            buzzy.AddComponent(new DamagePlayerTrigger());
            buzzy.AddComponent(new BuzzyBeetle());
            buzzy.AddComponent(new EnemyWalker(mover, Constants.BuzzyBeetleWalkSpeed));
        }

        void CreatePiranhaPlant(Scene scene, TmxObject obj)
        {
            var exposedCenter = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;
            var hiddenCenter = exposedCenter + new Vector2(0, h);

            var plant = scene.CreateEntity("piranhaplant", hiddenCenter);
            plant.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Green);

            var hit = plant.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            plant.AddComponent(new DamagePlayerTrigger());
            plant.AddComponent(new PiranhaPlant(exposedCenter));
        }

        void CreateHammerBro(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var bro = scene.CreateEntity("hammerbro", center);
            var renderer = bro.AddComponent(new PrototypeSpriteRenderer(w, h));
            renderer.SetColor(Color.DarkOliveGreen);
            bro.AddComponent(new Mover());
            bro.AddComponent(new GravityBody());

            var body = bro.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = bro.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            bro.AddComponent(new DamagePlayerTrigger());
            bro.AddComponent(new HammerBro(this, renderer));
        }

        void CreateBlooper(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var blooper = scene.CreateEntity("blooper", center);
            blooper.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.White);
            var mover = blooper.AddComponent(new Mover());

            var body = blooper.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = blooper.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            blooper.AddComponent(new DamagePlayerTrigger());
            blooper.AddComponent(new Blooper(mover));
        }

        void CreateBulletBillCannon(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var cannon = scene.CreateEntity("bulletbillcannon", center);
            cannon.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.DimGray);

            var body = cannon.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Environment;
            body.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            cannon.AddComponent(new BulletBillCannon(this));
        }

        void CreatePodoboo(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var podoboo = scene.CreateEntity("podoboo", center);
            podoboo.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.OrangeRed);
            podoboo.AddComponent(new Mover());

            var hit = podoboo.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            podoboo.AddComponent(new DamagePlayerTrigger());
            podoboo.AddComponent(new Podoboo());
        }

        void CreateGoalTrigger(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var goal = scene.CreateEntity("goaltrigger", center);
            goal.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.Gold);

            var collider = goal.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;

            goal.AddComponent(new GoalTrigger());
        }

        void CreateKillVolume(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var killVolume = scene.CreateEntity("killvolume", center);
            var collider = killVolume.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;
            killVolume.AddComponent(new KillVolume());
        }
    }
}
