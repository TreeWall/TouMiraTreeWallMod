using TownOfUs.Modifiers;

namespace TreeWallMod.Modifiers
{
    public sealed class DisableButtonsModifier : DisabledModifier
    {
        public override string ModifierName => "Disable Buttons and Abilities";
        public override bool HideOnUi => true;
        public override bool AutoStart => false;

        public override bool CanBeInteractedWith => false;
        public override bool CanReport => false;
        public override bool IsConsideredAlive => false;
        public override bool CanUseAbilities => false;
        public override bool CanUseConsoles => true;
        public override bool CanOpenMap => true;

        public override void OnMeetingStart()
        {
            ModifierComponent!.RemoveModifier(this);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            var buttonsParent = HudManager.Instance.transform.Find("Buttons");

            if (buttonsParent != null)
            {
                var allButtons = buttonsParent.GetComponentsInChildren<ActionButton>(true);
                foreach (var button in allButtons)
                {
                    if (button == HudManager.Instance.SabotageButton)
                    {
                        button.SetDisabled();
                    }
                }
            }
        }

        public override void OnActivate()
        {
            base.OnActivate();

            var buttonsParent = HudManager.Instance.transform.Find("Buttons");

            if (buttonsParent != null)
            {
                var allButtons = buttonsParent.GetComponentsInChildren<ActionButton>(true);
                foreach (var button in allButtons)
                {
                    if (button != HudManager.Instance.PetButton && button != HudManager.Instance.UseButton)
                    {
                        button.SetDisabled();
                    }
                }
            }
        }
    }
}
