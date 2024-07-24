using Content.Shared.Item;
using Content.Shared.Chaplain;
using Content.Server.Chaplain.Components;
using Robust.Server.GameObjects;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Verbs;
using System.Numerics;
using Content.Server.Bible.Components;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Server.Popups;
using Robust.Shared.Player;
using Robust.Shared.Map;

namespace Content.Server.Chaplain;

public sealed class NullRodUpdateSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    private const int MaxSelectedSets = 1;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullRodComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<NullRodComponent, NullRodApproveMessage>(OnApprove);
        SubscribeLocalEvent<NullRodComponent, NullRodChangeSetMessage>(OnChangeSet);
    }

    private void OnUIOpened(Entity<NullRodComponent> rod, ref BoundUIOpenedEvent args)
    {
        UpdateUI(rod.Owner, rod.Comp);
    }

    private void OnApprove(Entity<NullRodComponent> backpack, ref NullRodApproveMessage args)
    {
        if (backpack.Comp.SelectedSets.Count != MaxSelectedSets)
            return;

        foreach (var i in backpack.Comp.SelectedSets)
        {
            var set = _proto.Index(backpack.Comp.PossibleSets[i]);
            foreach (var item in set.Content)
            {
                var ent = Spawn(item, _transform.GetMapCoordinates(backpack.Owner));
                if (TryComp<ItemComponent>(ent, out var itemComponent))
                    _transform.DropNextTo(ent, backpack.Owner);
            }
        }
        _audio.PlayPvs(backpack.Comp.ApproveSound, backpack.Owner);
        QueueDel(backpack);
    }
    private void OnChangeSet(Entity<NullRodComponent> backpack, ref NullRodChangeSetMessage args)
    {
        if (!backpack.Comp.SelectedSets.Remove(args.SetNumber))
            backpack.Comp.SelectedSets.Add(args.SetNumber);

        UpdateUI(backpack.Owner, backpack.Comp);
    }

    private void UpdateUI(EntityUid uid, NullRodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        Dictionary<int, NullRodSetInfo> data = new();

        for (int i = 0; i < component.PossibleSets.Count; i++)
        {
            var set = _proto.Index(component.PossibleSets[i]);
            var selected = component.SelectedSets.Contains(i);
            var info = new NullRodSetInfo(
                set.Name,
                set.Description,
                set.Sprite,
                selected);
            data.Add(i, info);
        }

        _ui.TrySetUiState(uid, NullRodUIKey.Key, new NullRodBoundUserInterfaceState(data, MaxSelectedSets));
    }
}
