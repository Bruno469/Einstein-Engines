using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.Flash;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Heretic.Components;
using Content.Server.Station.Components;
using Content.Server.Anomaly;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Roles;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server.GameTicking.Rules;
public sealed class HereticsRuleSystem : GameRuleSystem<HereticsRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly AnomalySystem _anomaly = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public readonly ProtoId<NpcFactionPrototype> HereticNpcFaction = "Heretics";
    public readonly ProtoId<NpcFactionPrototype> HereticPrototypeId = "Heretic";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayerJobAssigned);
        //SubscribeLocalEvent<HereticComponent, MobStateChangedEvent>(OnHeadRevMobStateChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        SubscribeLocalEvent<HereticsRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    //Set miniumum players
    protected override void Added(EntityUid uid, HereticsRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        gameRule.MinPlayers = component.MinPlayers;
    }

    protected override void Started(EntityUid uid, HereticsRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;

        var grid = _station.GetLargestGrid(stationData);

        if (grid is null)
            return;

        // 12 / 3 = 4 i dont know how it horking in ss13 but 4 for 3 heretics :)
        var amountToSpawn = 12;
        for (var i = 0; i < amountToSpawn; i++)
        {
            _anomaly.SpawnOnRandomGridLocation(grid.Value, component.RealitySmashSpawnerPrototype);
        }
    }

    //protected override void ActiveTick(EntityUid uid, HereticsRuleComponent component, GameRuleComponent gameRule, float frameTime)
    //{
    //    base.ActiveTick(uid, component, gameRule, frameTime);
    //}

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var hereticsLost = CheckHereticsLose();
        var query = AllEntityQuery<HereticsRuleComponent>();
        while (query.MoveNext(out var heretic))
        {
            ev.AddLine(Loc.GetString("heretic-count", ("initialCount", heretic.Heretics.Count)));
            foreach (var player in heretic.Heretics)
            {
                var count = CompOrNull<HereticsRoleComponent>(player.Value)?.Sacrifices ?? 0;

                _mind.TryGetSession(player.Value, out var session);
                var username = session?.Name;
                if (username != null)
                {
                    ev.AddLine(Loc.GetString("heretic-name-user",
                    ("name", player.Key),
                    ("username", username), ("count", count)));
                }
                else
                {
                    ev.AddLine(Loc.GetString("heretic-name",
                    ("name", player.Key), ("count", count)));
                }
            }
        }
    }

    private void OnGetBriefing(EntityUid uid, HereticsRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        var head = HasComp<HereticComponent>(mind.OwnedEntity);
        args.Append(Loc.GetString(head ? "heretic-briefing" : "heretic-briefing"));
    }

    //Check for enough players to start rule
    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        TryRoundStartAttempt(ev, Loc.GetString("roles-antag-heretic-name"));
    }

    private void OnPlayerJobAssigned(RulePlayerJobsAssignedEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out var activeGameRule, out var comp, out var gameRule))
        {
            var eligiblePlayers = _antagSelection.GetEligiblePlayers(ev.Players, comp.HereticPrototypeId);

            if (eligiblePlayers.Count == 0)
                continue;

            var hereticCount = _antagSelection.CalculateAntagCount(ev.Players.Length, comp.PlayersPerHeretic, comp.MaxHeretics);

            var heretic = _antagSelection.ChooseAntags(hereticCount, eligiblePlayers);

            GiveHeretic(heretic, comp.HereticPrototypeId, comp);
        }
    }

    private void GiveHeretic(IEnumerable<EntityUid> chosen, ProtoId<AntagPrototype> antagProto, HereticsRuleComponent comp)
    {
        foreach (var heretic in chosen)
            GiveHeretic(heretic, antagProto, comp);
    }
    private void GiveHeretic(EntityUid chosen, ProtoId<AntagPrototype> antagProto, HereticsRuleComponent comp)
    {
        RemComp<CommandStaffComponent>(chosen);

        var inCharacterName = MetaData(chosen).EntityName;

        if (!_mind.TryGetMind(chosen, out var mind, out _))
            return;

        if (!_role.MindHasRole<HereticsRoleComponent>(mind))
        {
            _role.MindAddRole(mind, new HereticsRoleComponent { PrototypeId = antagProto }, silent: true);
        }

        comp.Heretics.Add(inCharacterName, mind);

        // _inventory.SpawnItemsOnEntity(chosen, comp.StartingGear);

        var hereticComp = EnsureComp<HereticComponent>(chosen);
        EnsureComp<HereticComponent>(chosen);

        _antagSelection.SendBriefing(chosen, Loc.GetString("heretic-role-greeting"), Color.CornflowerBlue, hereticComp.HereticStartSound);
    }

    public void OnHeadRevAdmin(EntityUid entity)
    {
        if (HasComp<HereticsRoleComponent>(entity))
            return;

        var hereticsRule = EntityQuery<HereticsRuleComponent>().FirstOrDefault();
        if (hereticsRule == null)
        {
            GameTicker.StartGameRule("Heretics", out var ruleEnt);
            hereticsRule = Comp<HereticsRuleComponent>(ruleEnt);
        }

        GiveHeretic(entity, hereticsRule.HereticPrototypeId, hereticsRule);
    }


    //private void OnCommandMobStateChanged(EntityUid uid, CommandStaffComponent comp, MobStateChangedEvent ev)
    //{
    //    if (ev.NewMobState == MobState.Dead || ev.NewMobState == MobState.Invalid)
    //        CheckCommandLose();
    //}


    //private void OnHeadRevMobStateChanged(EntityUid uid, HeadRevolutionaryComponent comp, MobStateChangedEvent ev)
    //{
    //    if (ev.NewMobState == MobState.Dead || ev.NewMobState == MobState.Invalid)
    //        CheckHereticsLose();
    //}
    private bool CheckHereticsLose()
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var hereticsList = new List<EntityUid>();

        var heretics = AllEntityQuery<HereticComponent, MobStateComponent>();
        while (heretics.MoveNext(out var uid, out _, out _))
        {
            hereticsList.Add(uid);
        }

        if (IsHereticsDead(hereticsList, false))
        {
            var heretic = AllEntityQuery<HereticComponent, MindContainerComponent>();
            while (heretic.MoveNext(out var uid, out _, out var mc))
            {
                if (HasComp<HereticComponent>(uid))
                    continue;
            }
            return true;
        }

        return false;
    }

    private bool IsHereticsDead(List<EntityUid> list, bool checkOffStation)
    {
        var dead = 0;
        foreach (var entity in list)
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                if (state.CurrentState == MobState.Dead || state.CurrentState == MobState.Invalid)
                {
                    dead++;
                }
                else if (checkOffStation && _stationSystem.GetOwningStation(entity) == null && !_emergencyShuttle.EmergencyShuttleArrived)
                {
                    dead++;
                }
            }
            else
            {
                dead++;
            }
        }

        return dead == list.Count || list.Count == 0;
    }

    private static readonly string[] Outcomes =
    {
        // heretics survived and crewn survived... how
        "heretics-reverse-stalemate",
        // heretic won and crewn died
        "heretic-won",
        // heretic lost and crewn survived
        "heretic-lost",
        // heretic lost and crewn died
        "heretic-stalemate"
    };
}
