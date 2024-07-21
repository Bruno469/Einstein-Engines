using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server.Administration;
using Content.Shared.Curse;
using Content.Server.Bible.Components;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Chat;
using Content.Shared.Verbs;
using Content.Shared.Actions;
using Robust.Server.GameObjects;
using Robust.Server.Physics;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Content.Server.Administration.Logs.Converters;
using System.Runtime.CompilerServices;

namespace Content.Server.Curse;
public sealed class DarkDugeonCreate : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly SharedActionSystem _actionSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public EntityUid DugeonArenaMap { get; private set; } = new();
    public EntityUid? DugeonArenaGrid { get; private set; } = new();
    public List<string> Dungeons = new List<string>
    {
        "/Maps/Dungeon/Old_Libary2.yml",
        "/Maps/Dungeon/Old_Apartament.yml"
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkDugeonCreateComponent, GetVerbsEvent<ActivationVerb>>(Tryteleport);
    }

    private void Tryteleport(EntityUid uid, DarkDugeonCreateComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        // if it doesn't have an actor and we can't reach it then don't add the verb
        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        // this is to prevent ghosts from using it
        if (!args.CanInteract)
            return;

        var prayerVerb = new ActivationVerb
        {
            Text = Loc.GetString(comp.Verb),
            Icon = comp.VerbImage,
            Act = () =>
            {
                if (comp.BibleUserOnly && !EntityManager.TryGetComponent<BibleUserComponent>(args.User, out var bibleUser))
                {
                    _popupSystem.PopupEntity(Loc.GetString("prayer-popup-notify-try-tp"), uid, actor.PlayerSession, PopupType.Large);
                    return;
                }
                _popupSystem.PopupEntity(Loc.GetString("prayer-chat-notify-Teleport"), uid, actor.PlayerSession, PopupType.Large);
                var (mapUid, gridUid) = LoadDungeon(uid, comp);
                _actionSystem.StartUseDelay(args.User, args);
                _transform.SetCoordinates(args.User, new EntityCoordinates(gridUid ?? mapUid, Vector2.One));
                return;
            },
            Impact = LogImpact.Low,
        };
        prayerVerb.Impact = LogImpact.Low;
        args.Verbs.Add(prayerVerb);
    }

    public (EntityUid Map, EntityUid? Grid) LoadDungeon(EntityUid uid, DarkDugeonCreateComponent comp)
    {
        DugeonArenaMap = _mapManager.GetMapEntityId(_mapManager.CreateMap());
        _metaDataSystem.SetEntityName(DugeonArenaMap, $"Dungeon-{DugeonArenaMap}");
        var DugeonArenaMapId = Comp<MapComponent>(DugeonArenaMap).MapId;

        // Pega uma dungeon aleatoria no prototype e depois da spawn na grid dela
        // TODO: PreLoad DugeonArenaMap

        var grids = _map.LoadMap(DugeonArenaMapId, _random.Pick(Dungeons));

        if (grids.Count != 0)
        {
            _metaDataSystem.SetEntityName(grids[0], $"Dungeon-{DugeonArenaMap}");
            DugeonArenaGrid = grids[0];
        }
        else
        {
            DugeonArenaGrid = null;
        }

        _mapManager.SetMapPaused(DugeonArenaMapId, false);
        return (DugeonArenaMap, DugeonArenaGrid);
    }
}
