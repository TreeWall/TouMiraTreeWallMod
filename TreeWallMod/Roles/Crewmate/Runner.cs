using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using System;
using System.Collections.Generic;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TreeWallMod.TWAssets;
using UnityEngine;


namespace TreeWallMod.Roles.Crewmate
{
    public sealed class RunnerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IUnguessable, IDoomable
    {
        public string IdPart => "Runner";
        string ICustomRole.IdPrefix => "TreeWallMod.Role";

        public bool IsGuessable => false;
        public bool IsPowerCrew => true;
        public Color RoleColor => Colors.Runner;
        public RoleBehaviour AppearAs => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MayorRole>());
        public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;
        public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
        public DoomableType DoomHintType => DoomableType.Trickster;

        public string GetAdvancedDescription()
        {
            return
                MiraLocaleManager.Get($"TreeWallMod.Role.{IdPart}.WikiDescription") +
                MiscUtils.AppendOptionsText(GetType());
        }

        [HideFromIl2Cpp]
        public List<CustomButtonWikiDescription> Abilities
        {
            get
            {
                var abilities = new List<CustomButtonWikiDescription>
                {
                    new(MiraLocaleManager.Get($"TreeWallMod.Role.{IdPart}Caffeine", "Caffeine"),
                        MiraLocaleManager.Get($"TreeWallMod.Role.{IdPart}Caffeine.WikiDescription"),
                        TWCrewAssets.RunnerCaffeineSprite),
                };

                return abilities;
            }
        }

        public float SpeedMultiplier { get; set; } = 1f;
        public bool SpeedActive { get; set; } = false;

        const float RPC_TIME_PERIOD = 1f;
        const float RPC_MIN_DELAY = 0.25f;
        float elapsed = 0f;
        bool moving = true;
        bool? prevMoving = null;
        int order = 0;

        public List<RunnerMoveData> RunnerMoves { get; set; } = new();

        public CustomRoleConfiguration Configuration => new(this)
        {
            IconTmp = TmpSpriteUtils.CreateSpriteAsset(TWRoleIcons.Runner.LoadAsset(), "TreeWallMod.Roles.Crewmate.Runner", 1.45f),
            Icon = TWRoleIcons.Runner,
            OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        };

        public void FixedUpdate()
        {
            if (Player == null || !Player.AmOwner)
            {
                return;
            }

            // send moving to be false even if the player stops moving just for a frame in a 1 sec period
            if (Player.MyPhysics.Velocity.sqrMagnitude == 0)
            {
                moving = false;
            }

            elapsed += Time.deltaTime;

            if ((moving == false && elapsed >= RPC_MIN_DELAY) || elapsed >= RPC_TIME_PERIOD)
            {
                // reset timer
                elapsed = 0f;

                // check if the state of player movement has changed, and if so send a new packet stating so
                if (prevMoving == null || prevMoving != moving)
                {
                    RpcUpdateRunnerMoving(Player, moving, order);
                    prevMoving = moving;
                    order++;
                }

                moving = true;
            }
        }

        public override void OnMeetingStart()
        {
            SpeedMultiplier = 1f;
            SpeedActive = false;
        }

        [MethodRpc((uint)TreeWallModRpcsEnum.SetRunnerSpeed)]
        public static void RpcSetRunnerSpeed(PlayerControl runner, float multiplier, bool active)
        {
            var runnerRole = runner.GetRole<RunnerRole>();

            if (runnerRole == null)
            {
                return;
            }

            runnerRole.SpeedMultiplier = multiplier;
            runnerRole.SpeedActive = active;
        }

        [MethodRpc((uint)TreeWallModRpcsEnum.RunnerUpdateMoving)]
        public static void RpcUpdateRunnerMoving(PlayerControl player, bool moving, int order)
        {
            var runner = player.GetRole<RunnerRole>();

            if (runner == null)
            {
                return;
            }

            runner.RunnerMoves.Add(new RunnerMoveData(player.PlayerId, moving, order));

            if (runner.RunnerMoves.Count > 5)
            {
                runner.RunnerMoves.RemoveAll(x => x.Order <= order-5);
            }

            //Message($"Runner Info Updated - player: {player.name} moving: {moving} Order: {order} RunnerMoves Count: {runner.RunnerMoves.Count}");
        }
    }

    public readonly struct RunnerMoveData(byte playerId, bool moving, int order)
    {
        public byte PlayerId { get; } = playerId;
        public bool Moving { get; } = moving;
        public int Order { get; } = order;
    }
}