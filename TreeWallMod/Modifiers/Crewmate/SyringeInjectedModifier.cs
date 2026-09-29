using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Networking;
using TownOfUs.Roles.Impostor;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Crewmate;

namespace TreeWallMod.Modifiers.Crewmate
{
    public sealed class SyringeInjectedModifier(PlayerControl syringe) : BaseModifier
    {
        public override string ModifierName => "SyringeInjectedModifier";
        public override bool HideOnUi => false;

        public List<SyringeModItem> SyringeItems { get; set; } = new();

        public bool RemovePlayer(PlayerControl player)
        {
            bool ret = 0 < SyringeItems.RemoveAll(x => x.Syringe == player);
            //if (ret) Message($"Removed {player.name} to {Player.name} Length: {SyringeItems.Count}");

            return ret;
        }

        public bool AddPlayer(PlayerControl player)
        {
            bool ret = SyringeItems.Any(x => x.Syringe == player);
            if (ret) SyringeItems.RemoveAll(x => x.Syringe == player);

            SyringeItems.Add(new SyringeModItem(player, !Player.IsCrewmate()));

            ret = SyringeItems.Any(x => x.Syringe == player);
            //if (ret) Message($"Added {player.name} to {Player.name} with Evil set to {SyringeItems.FirstOrDefault(x => x.Syringe == player).Evil}");

            CrewmateCheck();

            return ret;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            AddPlayer(syringe);
            //Message($"Added {syringe.name} to {Player.name}");
            //         if (Player.AmOwner)
            //{
            //	this.RpcAddPlayerSyringeInject(Player, syringe);
            //}
        }

        void CrewmateCheck()
        {
            if (Player.IsCrewmate())
            {
                if (Player.AmOwner)
                {
                    int random = UnityEngine.Random.Range(0, 99);

                    int AliveCrew = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.IsCrewmate());

                    if (random < OptionGroupSingleton<SyringeOptions>.Instance.CrewmateDeath.Value)
                    {
                        Player.RpcSpecialMurder(Player, causeOfDeath: "SyringeMisCured");
                        SyringeItems.Last().Syringe.RpcSpecialMurder(SyringeItems.Last().Syringe, causeOfDeath: "SyringeSued");
                    }
                    else if (random < OptionGroupSingleton<SyringeOptions>.Instance.CrewmateChangeTraitor.Value + OptionGroupSingleton<SyringeOptions>.Instance.CrewmateDeath.Value)
                    {
                        if (AliveCrew < OptionGroupSingleton<SyringeOptions>.Instance.MinCrewForTraitor)
                        {
                            if (OptionGroupSingleton<SyringeOptions>.Instance.syringeTraitorFail == SyringeTraitorFail.BothSyringeAndTargetDie)
                            {
                                Player.RpcSpecialMurder(Player, causeOfDeath: "SyringeMisCured");
                                SyringeItems.Last().Syringe.RpcSpecialMurder(SyringeItems.Last().Syringe, causeOfDeath: "SyringeSued");
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }

                        Player.RpcChangeRole(RoleId.Get<TraitorRole>());
                        if (OptionGroupSingleton<SyringeOptions>.Instance.InjectedCrewmateBecomesAssasin && !Player.HasModifier<AssassinModifier>())
                        {
                            Player.RpcAddModifier<AssassinModifier>();
                        }
                    }
                }
            }
        }

        public override void OnDeath(DeathReason reason)
        {
            ModifierComponent!.RemoveModifier(this);
        }

        public static void UpdateSyringe(PlayerControl target, PlayerControl syringe)
        {
            if (!target.TryGetModifier<SyringeInjectedModifier>(out var syringeInjectedMod))
            {
                target.RpcAddModifier<SyringeInjectedModifier>(syringe);
                return;
            }

            target.RpcAddPlayerSyringeInject(syringe);
        }
    }

    public readonly struct SyringeModItem(PlayerControl syringe, bool evil)
    {
        public PlayerControl Syringe { get; } = syringe;
        public bool Evil { get; } = evil;
    }
}
