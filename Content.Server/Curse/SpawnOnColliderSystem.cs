
using Content.Shared.Spider;
using Content.Shared.Maps;
using Content.Shared.Curse;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Vector2 = System.Numerics.Vector2;

namespace Content.Server.Curse
{
    public sealed class SpawnOnColliderSystem : SharedSpawnOnColliderSystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SpawnOnColliderComponent, ProjectileHitEvent>(OnProjectileCollide);
        }

        private void OnProjectileCollide(EntityUid uid, SpawnOnColliderComponent component, ProjectileHitEvent args)
        {
            var transform = Transform(uid);

            if (transform.GridUid == null)
            {
                return;
            }

            var coords = transform.Coordinates;
            var result = false;

            if (!IsTileBlockedByWeb(coords))
            {
                Spawn(component.Prototype, coords);
                result = true;
            }

            // Gerar teias em torno do ponto de colisão de forma uniforme
            Generate(coords, component.radius, component.points, component);
        }
        private void Generate(EntityCoordinates center, int radius, int points, SpawnOnColliderComponent component)
        {
            var angleStep = 360.0f / points;
            var angle = 0.0f;

            for (int i = 0; i < points; i++)
            {
                var offsetX = radius * Math.Cos(angle * Math.PI / 180.0f);
                var offsetY = radius * Math.Sin(angle * Math.PI / 180.0f);
                var offset = new Vector2((float)offsetX, (float)offsetY);
                var newCoords = center.Offset(offset);

                if (!IsTileBlockedByWeb(newCoords))
                {
                    Spawn(component.Prototype, newCoords);
                }

                angle += angleStep;
            }
        }

        private bool IsTileBlockedByWeb(EntityCoordinates coords)
        {
            foreach (var entity in coords.GetEntitiesInTile())
            {
                if (HasComp<SpiderWebObjectComponent>(entity))
                    return true;
            }
            return false;
        }
    }
}
