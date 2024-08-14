namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class ElectrolyzerComponent : Component
    {
        /// <summary>
        /// Minimum amount of energy required for the electrolyzer to function.
        /// </summary>
        [DataField("minimumEnergyRequirement")]
        public float MinimumEnergyRequirement { get; set; } = 100f;

        /// <summary>
        /// List of effects that should be applied during electrolysis.
        /// </summary>
        [DataField("effects")]
        public List<IGasReactionEffect> Effects { get; set; } = new();
    }
}
