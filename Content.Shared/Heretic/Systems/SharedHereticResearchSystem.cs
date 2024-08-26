using System.Linq;
using Content.Shared.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Systems;

public abstract class SharedHereticResearchSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticResearchDatabaseComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, HereticResearchDatabaseComponent component, MapInitEvent args)
    {
        UpdateResearch(uid, component);
        //UpdateResearch(uid, component);
    }

    public void UpdateResearch(EntityUid uid, HereticResearchDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var availableResearch = GetAvailableResearch(uid, component);
        _random.Shuffle(availableResearch);

        component.CurrentResearch.Clear();
        foreach (var discipline in component.SupportedDisciplines)
        {
            var selected = availableResearch.FirstOrDefault(p => p.Discipline == discipline);
            if (selected == null)
                continue;

            component.CurrentResearch.Add(selected.ID);
        }
        Dirty(uid, component);
    }

    public List<HereticResearchPrototype> GetAvailableResearch(EntityUid uid, HereticResearchDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new List<HereticResearchPrototype>();

        var availableResearchs = new List<HereticResearchPrototype>();
        var disciplineTiers = GetDisciplineTiers(component);
        foreach (var research in PrototypeManager.EnumeratePrototypes<HereticResearchPrototype>())
        {
            if (IsResearchAvailable(component, research, disciplineTiers))
                availableResearchs.Add(research);
        }

        return availableResearchs;
    }

    public bool IsResearchAvailable(HereticResearchDatabaseComponent component, HereticResearchPrototype research, Dictionary<string, int>? disciplineTiers = null)
    {
        disciplineTiers ??= GetDisciplineTiers(component);

        if (research.Hidden)
            return false;

        if (!component.SupportedDisciplines.Contains(research.Discipline))
            return false;

        if (research.Tier > disciplineTiers[research.Discipline])
            return false;

        if (component.UnlockedReasearchs.Contains(research.ID))
            return false;

        foreach (var prereq in research.ResearchPrerequisites)
        {
            if (!component.UnlockedReasearchs.Contains(prereq))
                return false;
        }

        return true;
    }

    public Dictionary<string, int> GetDisciplineTiers(HereticResearchDatabaseComponent component)
    {
        var tiers = new Dictionary<string, int>();
        foreach (var discipline in component.SupportedDisciplines)
        {
            tiers.Add(discipline, GetHighestDisciplineTier(component, discipline));
        }

        return tiers;
    }

    public int GetHighestDisciplineTier(HereticResearchDatabaseComponent component, string disciplineId)
    {
        return GetHighestDisciplineTier(component, PrototypeManager.Index<PathPrototype>(disciplineId));
    }

    public int GetHighestDisciplineTier(HereticResearchDatabaseComponent component, PathPrototype researchDiscipline)
    {
        var allresearch = PrototypeManager.EnumeratePrototypes<HereticResearchPrototype>()
            .Where(p => p.Discipline == researchDiscipline.ID && !p.Hidden).ToList();
        var allUnlocked = new List<HereticResearchPrototype>();
        foreach (var recipe in component.UnlockedReasearchs)
        {
            var proto = PrototypeManager.Index<HereticResearchPrototype>(recipe);
            if (proto.Discipline != researchDiscipline.ID)
                continue;
            allUnlocked.Add(proto);
        }

        var highestTier = researchDiscipline.TierPrerequisites.Keys.Max();
        var tier = 2; //tier 1 is always given

        // todo this might break if you have hidden Researchs. i'm not sure

        while (tier <= highestTier)
        {
            // we need to get the research for the tier 1 below because that's
            // what the percentage in TierPrerequisites is referring to.
            var unlockedTierresearch = allUnlocked.Where(p => p.Tier == tier - 1).ToList();
            var allTierresearch = allresearch.Where(p => p.Discipline == researchDiscipline.ID && p.Tier == tier - 1).ToList();

            if (allTierresearch.Count == 0)
                break;

            var percent = (float) unlockedTierresearch.Count / allTierresearch.Count;
            if (percent < researchDiscipline.TierPrerequisites[tier])
                break;

            if (tier >= researchDiscipline.LockoutTier &&
                component.Path != null &&
                researchDiscipline.ID != component.Path)
                break;
            tier++;
        }

        return tier - 1;
    }

    public FormattedMessage GetResearchDescription(
        HereticResearchPrototype researchnology,
        bool includeCost = true,
        bool includeTier = true,
        bool includePrereqs = false,
        PathPrototype? disciplinePrototype = null)
    {
        var description = new FormattedMessage();
        if (includeTier)
        {
            disciplinePrototype ??= PrototypeManager.Index(researchnology.Discipline);
            description.AddMarkup(Loc.GetString("research-console-tier-discipline-info",
                ("tier", researchnology.Tier), ("color", disciplinePrototype.Color), ("discipline", Loc.GetString(disciplinePrototype.Name))));
            description.PushNewline();
        }

        if (includeCost)
        {
            description.AddMarkup(Loc.GetString("research-console-cost", ("amount", researchnology.Cost)));
            description.PushNewline();
        }

        if (includePrereqs && researchnology.ResearchPrerequisites.Any())
        {
            description.AddMarkup(Loc.GetString("research-console-prereqs-list-start"));
            foreach (var recipe in researchnology.ResearchPrerequisites)
            {
                var researchProto = PrototypeManager.Index(recipe);
                description.PushNewline();
                description.AddMarkup(Loc.GetString("research-console-prereqs-list-entry",
                    ("text", Loc.GetString(researchProto.Name))));
            }
            description.PushNewline();
        }

        description.AddMarkup(Loc.GetString("research-console-unlocks-list-start"));
        foreach (var recipe in researchnology.RecipeUnlocks)
        {
            var recipeProto = PrototypeManager.Index(recipe);
            description.PushNewline();
            description.AddMarkup(Loc.GetString("research-console-unlocks-list-entry",
                ("name",recipeProto.Name)));
        }
        foreach (var generic in researchnology.GenericUnlocks)
        {
            description.PushNewline();
            description.AddMarkup(Loc.GetString("research-console-unlocks-list-entry-generic",
                ("text", Loc.GetString(generic.UnlockDescription))));
        }

        return description;
    }

    /// <summary>
    ///     Returns whether a researchnology is unlocked on this database or not.
    /// </summary>
    /// <returns>Whether it is unlocked or not</returns>
    public bool IsresearchnologyUnlocked(EntityUid uid, HereticResearchPrototype researchnology, HereticResearchDatabaseComponent? component = null)
    {
        return Resolve(uid, ref component) && IsresearchnologyUnlocked(uid, researchnology.ID, component);
    }

    /// <summary>
    ///     Returns whether a researchnology is unlocked on this database or not.
    /// </summary>
    /// <returns>Whether it is unlocked or not</returns>
    public bool IsresearchnologyUnlocked(EntityUid uid, string researchnologyId, HereticResearchDatabaseComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.UnlockedReasearchs.Contains(researchnologyId);
    }

    public void TrySetPath(HereticResearchPrototype prototype, EntityUid uid, HereticResearchDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var discipline = PrototypeManager.Index(prototype.Discipline);
        if (prototype.Tier < discipline.LockoutTier)
            return;
        component.Path = prototype.Discipline;
        Dirty(uid, component);
    }

    /// <summary>
    /// Removes a researchnology and its recipes from a researchnology database.
    /// </summary>
    public bool TryRemoveresearchnology(Entity<HereticResearchDatabaseComponent> entity, ProtoId<HereticResearchPrototype> research)
    {
        return TryRemoveresearchnology(entity, PrototypeManager.Index(research));
    }

    /// <summary>
    /// Removes a researchnology and its recipes from a researchnology database.
    /// </summary>
    [PublicAPI]
    public bool TryRemoveresearchnology(Entity<HereticResearchDatabaseComponent> entity, HereticResearchPrototype research)
    {
        if (!entity.Comp.UnlockedReasearchs.Remove(research.ID))
            return false;

        // check to make sure we didn't somehow get the recipe from another research.
        // unlikely, but whatever
        var recipes = research.RecipeUnlocks;
        foreach (var recipe in recipes)
        {
            var hasresearchElsewhere = false;
            foreach (var unlockedresearch in entity.Comp.UnlockedReasearchs)
            {
                var unlockedresearchProto = PrototypeManager.Index<HereticResearchPrototype>(unlockedresearch);

                if (!unlockedresearchProto.RecipeUnlocks.Contains(recipe))
                    continue;
                hasresearchElsewhere = true;
                break;
            }

            if (!hasresearchElsewhere)
                entity.Comp.UnlockedRecipes.Remove(recipe);
        }
        Dirty(entity, entity.Comp);
        UpdateResearch(entity, entity);
        return true;
    }

    /// <summary>
    /// Clear all unlocked Researchs from the database.
    /// </summary>
    [PublicAPI]
    public void Clearresearchs(EntityUid uid, HereticResearchDatabaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || comp.UnlockedReasearchs.Count == 0)
            return;

        comp.UnlockedReasearchs.Clear();
        Dirty(uid, comp);
    }
}
