using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace TreeWallMod.Options
{
    public sealed class TWGeneralOptions : AbstractOptionGroup
    {
        public override string GroupName => "General";
        public override uint GroupPriority => 1;

        public ModdedToggleOption FartKill { get; set; } = new("Replace the normal Imposter kill sounds with farts :D", false);
    }

    // Dunno why tf this class is needed but the above one doesnt work without it
    public sealed class DumbNeeded : AbstractOptionGroup
    {
        public override string GroupName => "General";
        public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    }
}
