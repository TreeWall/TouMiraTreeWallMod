using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Roles;
using TreeWallMod.TWAssets;
using UnityEngine;

namespace TreeWallMod.Roles.Crewmate
{
	public sealed class PsychicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
	{
		public string IdPart => "Psychic";
		string ICustomRole.IdPrefix => "TreeWallMod.Role";

		public bool IsPowerCrew => false;
		public Color RoleColor => Colors.Psychic;
		public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
		public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
		public DoomableType DoomHintType => DoomableType.Relentless;

		private List<PsychicKillerInfo> killers = new();

		float elapsed = 0f;
		float totalDeltaTime;

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
					new(MiraLocaleManager.Get($"TreeWallMod.Role.{IdPart}GuessKiller", "Guess Killer"),
						MiraLocaleManager.Get($"TreeWallMod.Role.{IdPart}GuessKiller.WikiDescription"),
						TWCrewAssets.PsychicKillGuessSprite),
				};

                return abilities;
            }
        }

        public CustomRoleConfiguration Configuration => new(this)
        {
            IconTmp = TmpSpriteUtils.CreateSpriteAsset(TWRoleIcons.Psychic.LoadAsset(), "TreeWallMod.Roles.Crewmate.Psychic", 1.45f),
            Icon = TWRoleIcons.Psychic,
            OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        };

        public void FixedUpdate()
		{
			if (Player == null || !Player.AmOwner)
			{
				return;
			}

			totalDeltaTime += Time.deltaTime;

			if (totalDeltaTime > 0.1)
			{
				elapsed += totalDeltaTime;
				totalDeltaTime = 0.0f;

				for (int i = 0; i < killers.Count; i++)
				{
					if (killers[i].Time < elapsed - 15f)
					{
						Message($"Removed {MiscUtils.PlayerById(killers[i].KillerId).name}, current count: {killers.Count-1}");
						killers.RemoveAt(i);
					}
				}
			}
		}

		public void AddKiller(PlayerControl killer)
		{
			killers.Add(new PsychicKillerInfo(killer.PlayerId, elapsed));
		}

		public bool HasKiller(byte killerId)
		{
			return killers.Any(x => x.KillerId == killerId);
		}
	}

	public readonly struct PsychicKillerInfo(byte killerId, float time)
	{
		public byte KillerId { get; } = killerId;
		public float Time { get; } = time;
	}
}
