using Content.Server.PsycUpdate.Components;
using Content.Shared.Item;
using Content.Shared.PsycUpdate;
using Robust.Server.GameObjects;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.PsycUpdate.Systems;

/// <summary>
/// <see cref="UpdateCristalComponent"/>
/// This system links the interface to the logic, and will output to the player a set of items selected by them in the interface
/// </summary>
public sealed class UpdateCristalSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private const int MaxSelectedSets = 2;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UpdateCristalComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<UpdateCristalComponent, UpdateCristalApproveMessage>(OnApprove);
        SubscribeLocalEvent<UpdateCristalComponent, UpdateCristalChangeSetMessage>(OnChangeSet);
    }

    private void OnUIOpened(EntityUid uid, UpdateCristalComponent component, BoundUIOpenedEvent args)
    {
        UpdateUI(uid, component);
    }

    private void OnApprove(EntityUid uid, UpdateCristalComponent component, UpdateCristalApproveMessage args)
    {
        if (component.SelectedSets.Count != MaxSelectedSets)
            return;

        foreach (var i in component.SelectedSets)
        {
            var set = _proto.Index(component.PossibleSets[i]);
            foreach (var item in set.Content)
            {
                var ent = Spawn(item, _transform.GetMapCoordinates(uid));
                if (TryComp<ItemComponent>(ent, out var itemComponent))
                    _transform.DropNextTo(ent, uid);
            }
        }
        _audio.PlayPvs(component.ApproveSound, uid);
        QueueDel(uid);
    }

    private void OnChangeSet(EntityUid uid, UpdateCristalComponent component, UpdateCristalChangeSetMessage args)
    {
        // Switch selecting set
        if (!component.SelectedSets.Remove(args.SetNumber))
            component.SelectedSets.Add(args.SetNumber);

        UpdateUI(uid, component);
    }

    private void UpdateUI(EntityUid uid, UpdateCristalComponent component)
    {
        Dictionary<int, UpdateCristalSetInfo> data = new();

        for (int i = 0; i < component.PossibleSets.Count; i++)
        {
            var set = _proto.Index(component.PossibleSets[i]);
            var selected = component.SelectedSets.Contains(i);
            var info = new UpdateCristalSetInfo(
                set.Name,
                set.Description,
                set.Sprite,
                selected);
            data.Add(i, info);
        }

        _ui.TrySetUiState(uid, UpdateCristalUIKey.Key, new UpdateCristalBoundUserInterfaceState(data, MaxSelectedSets));
    }
}
