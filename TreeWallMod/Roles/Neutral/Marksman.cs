using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Rewired.Utils.Classes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.Crewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TreeWallMod.Assets;
using TreeWallMod.Buttons.Neutral.Marksman;
using TreeWallMod.Events.Crewmate;
using TreeWallMod.Modifiers.Crewmate;
using TreeWallMod.Modifiers.Neutral;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Roles.Neutral
{
	public sealed class MarksmanRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
	{
		public string IdPart => "Marksman";
		string ICustomRole.IdPrefix => "TreeWallMod.Role";

		public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SeerRole>());
		public Color RoleColor => Colors.Marksman;
		public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
		public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
		public DoomableType DoomHintType => DoomableType.Insight;

		public CustomRoleConfiguration Configuration => new(this)
		{
			MaxRoleCount = 1,
			IconTmp = TmpSpriteUtils.CreateSpriteAsset(RoleIcons.Marksman.LoadAsset(), "TreeWallMod.Roles.Neutral.Marksman", 1.45f),
			CanUseVent = OptionGroupSingleton<MarksmanOptions>.Instance.CanVent,
			Icon = RoleIcons.Marksman,
			OptionsScreenshot = TouBanners.NeutralRoleBanner,
			GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
		};

		public override void SpawnTaskHeader(PlayerControl playerControl)
		{
			if (!playerControl.AmOwner)
			{
				return;
			}
			ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
			orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
			orCreateTask.name = "NeutralRoleText";
		}

        public bool HasImpostorVision => true;

        public bool WinConditionMet()
        {
            var marksmanCount = CustomRoleUtils.GetActiveRolesOfType<MarksmanRole>().Count(x => !x.Player.HasDied());

            if (MiscUtils.KillersAliveCount > marksmanCount || MiscUtils.KillersAliveCount == 0)
            {
                return false;
            }

            return marksmanCount >= MiscUtils.GetImpactfulLivingPlayers().Count - marksmanCount;
        }

        public readonly List<MarksmanAbility> LockedAbilities   = Enum.GetValues<MarksmanAbility>().ToList();
		public readonly List<MarksmanAbility> UnlockedAbilities = new();

		public MarksmanWarpState WarpMarking { get; set; } = MarksmanWarpState.Marking;
		public PlayerControl? WarpMarkedPlayer { get; set; } = null;

		private MeetingMenu meetingMenu;

		public override void Initialize(PlayerControl player)
		{
			RoleBehaviourStubs.Initialize(this, player);

			if (Player.AmOwner)
			{
				meetingMenu = new MeetingMenu(
					this,
					ClickGuess,
					MeetingAbilityType.Click,
					TouAssets.Guess,
					null!,
					IsExempt);

				if (!LegacyAssets.IsLegacy)
				{
					HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(Colors.Marksman);
				}

				AddAbility(Player, MarksmanAbility.SharpenedBlade);
				AddAbility(Player, MarksmanAbility.SmokeBomb);
				AddAbility(Player, MarksmanAbility.Warp);
				AddAbility(Player, MarksmanAbility.Vanish);
				AddAbility(Player, MarksmanAbility.Dismantle);
				AddAbility(Player, MarksmanAbility.Supressor);
				AddAbility(Player, MarksmanAbility.Dualscover);
			}
		}

		public override void Deinitialize(PlayerControl targetPlayer)
		{
			RoleBehaviourStubs.Deinitialize(this, targetPlayer);
			TouRoleUtils.ClearTaskHeader(Player);

			if (Player.AmOwner)
			{
				meetingMenu?.Dispose();
				meetingMenu = null!;
			}
		}

        public override bool DidWin(GameOverReason gameOverReason)
        {
            return WinConditionMet();
        }

        public override bool CanUse(IUsable usable)
        {
            if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
            {
                return false;
            }

            var console = usable.TryCast<Console>()!;
            return console == null || console.AllowImpostor;
        }

        public void FixedUpdate()
		{
			if (PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.AmOwner)
			{
				return;
			}

			if (!PlayerControl.LocalPlayer.GetRole<MarksmanRole>())
			{
				return;
			}

			if (WarpMarkedPlayer != null && WarpMarking == MarksmanWarpState.Warp)
			{
				byte markedPlayerId = WarpMarkedPlayer.PlayerId;

				var data = GameData.Instance.GetPlayerById(markedPlayerId);
				if (!data)
				{
					WarpMarkedPlayer = null;
					WarpMarking = MarksmanWarpState.Marking;
					return;
				}

				var stoned = MiscUtils.GetFreshStonedPlayerById(markedPlayerId);
				if (stoned != null)
				{
					return;
				}

				var body = Helpers.GetBodyById(markedPlayerId);
				if (data.IsDead && body)
				{
					return;
				}

				var pc = data.Object;
				if (!pc)
				{
					WarpMarkedPlayer = null;
					WarpMarking = MarksmanWarpState.Marking;
					return;
				}

				if (pc.moveable || pc.inVent || (pc.TryGetModifier<DisabledModifier>(out var mod) &&
												 (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)))
				{
					return;
				}

				WarpMarkedPlayer = null;
				WarpMarking = MarksmanWarpState.Marking;
				return;
			}
			else if (WarpMarkedPlayer == null && WarpMarking == MarksmanWarpState.Warp)
			{
				WarpMarkedPlayer = null;
				WarpMarking = MarksmanWarpState.Marking;
			}
		}

		public int IncorrectGuesses { get; set; } = 0;
		public override void OnMeetingStart()
		{
			IncorrectGuesses = 0;
			RoleBehaviourStubs.OnMeetingStart(this);

			var meeting = MeetingHud.Instance;
			if (Player.AmOwner && meeting != null)
			{
				meetingMenu.GenButtons(meeting,
					Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());

				var discoverButton = CustomButtonSingleton<MarksmanDiscoverButton>.Instance;

				var firstTarget = discoverButton.FirstTarget;
				var secondTarget = discoverButton.SecondTarget;

				if (firstTarget == null && secondTarget == null)
				{
					MiscUtils.AddFakeChat(Player.Data, "Marksman Info", "No player was Marked", false, true);
				}

				if (firstTarget != null)
				{
					MiscUtils.AddFakeChat(Player.Data, "Marksman Info", GenReport(firstTarget), false, true);
					if (!OptionGroupSingleton<MarksmanOptions>.Instance.CanDiscoverTwice && OptionGroupSingleton<MarksmanOptions>.Instance.RoleHint == MarksmanRoleHintEnum.ListRoles)
					{
						discoverButton.AlreadyDiscovered.Add(firstTarget);
					}
				}

				if (secondTarget != null)
				{
					MiscUtils.AddFakeChat(Player.Data, "Marksman Info", GenReport(secondTarget), false, true);
					if (!OptionGroupSingleton<MarksmanOptions>.Instance.CanDiscoverTwice && OptionGroupSingleton<MarksmanOptions>.Instance.RoleHint == MarksmanRoleHintEnum.ListRoles)
					{
						discoverButton.AlreadyDiscovered.Add(secondTarget);
					}
				}
			}
		}

		public override void OnVotingComplete()
		{
			RoleBehaviourStubs.OnVotingComplete(this);

			if (Player.AmOwner)
			{
				meetingMenu.HideButtons();

				CustomButtonSingleton<MarksmanDiscoverButton>.Instance.FirstTarget = null;
				CustomButtonSingleton<MarksmanDiscoverButton>.Instance.SecondTarget = null;
			}
		}

		private string GenReport(PlayerControl target)
		{
			string ret = "";

			MarksmanRoleHintEnum marksmanRoleHint = OptionGroupSingleton<MarksmanOptions>.Instance.RoleHint;

			Info($"Generating Marksman report");

			var reportBuilder = new StringBuilder();

			if (marksmanRoleHint == MarksmanRoleHintEnum.Subalignment)
			{
				var role = target.Data.Role;
				var unGuessableType = role as IUnguessable;

				if (target.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
				{
					role = cachedMod.CachedRole;
				}

				if (unGuessableType != null)
				{
					role = unGuessableType.AppearAs;
				}

				string alignment = role.GetRoleAlignment() switch
				{
					RoleAlignment.NeutralEvil or RoleAlignment.NeutralKilling or RoleAlignment.NeutralOutlier => "Evil",
					_ => "Crew"
				};

				if (role.IsImpostor()) alignment = "Evil";

				reportBuilder.AppendLine($"{target.name} is {alignment}");
			}
			else if (marksmanRoleHint == MarksmanRoleHintEnum.ListRoles)
			{
				var role = target.Data.Role;
				var unGuessableType = role as IUnguessable;

				if (target.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
				{
					role = cachedMod.CachedRole;
				}

				if (unGuessableType != null)
				{
					role = unGuessableType.AppearAs;
				}

				reportBuilder.AppendLine($"{target.name} can be:");

				var roles = MiscUtils.GetPotentialRoles().Where(x => (x is not IUnguessable) && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x) && x != role).ToList();

				var allRoles = MiscUtils.AllRoles.Where(x => (x is not IUnguessable) && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x)).Where(x => x is IGuessable && !roles.Contains(x)).ToList();
				if (allRoles.Count > 0)
				{
					foreach (var addedRole in allRoles)
					{
						if (addedRole is IGuessable guessable && guessable.CanBeGuessed)
						{
							roles.Add(addedRole);
						}
					}
				}
				roles = roles.OrderBy(x => x.GetRoleName()).ToList();

				int chosenIndex = UnityEngine.Random.RandomRangeInt(
					0, (roles.Count() > OptionGroupSingleton<MarksmanOptions>.Instance.RoleAmount) ? OptionGroupSingleton<MarksmanOptions>.Instance.RoleAmount : roles.Count());

				for (int i = 0; i < OptionGroupSingleton<MarksmanOptions>.Instance.RoleAmount; i++)
				{
					if (i == chosenIndex)
					{
						reportBuilder.AppendLine(TownOfUsPlugin.Culture,
								$"{MiscUtils.GetHyperlinkText(role)}");
						continue;
					}

					int index = UnityEngine.Random.RandomRangeInt(0, roles.Count());
					reportBuilder.AppendLine(TownOfUsPlugin.Culture,
								$"{MiscUtils.GetHyperlinkText(roles[index])}");
					roles.RemoveAt(index);

					if (roles.Count() == 0)
					{
						break;
					}
				}
			}
			else
			{
				var role = target.Data.Role;
				var doomableRole = role as IDoomable;
				var undoomableRole = role as IUnguessable;
				var hintType = DoomableType.Default;

				if (target.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
				{
					role = cachedMod.CachedRole;
					doomableRole = role as IDoomable;
				}

				if (undoomableRole != null)
				{
					role = undoomableRole.AppearAs;
					doomableRole = role as IDoomable;
				}

				if (doomableRole != null)
				{
					hintType = doomableRole.DoomHintType;
				}

				var fallback = MiraLocaleManager.Get("TownOfUsMira.Role.DoomsayerRoleHintDefault");
				var hint = MiraLocaleManager.Get($"TownOfUsMira.Role.DoomsayerRoleHint{hintType}");

				if (hint.Contains("STRMISS"))
				{
					reportBuilder.AppendLine(TownOfUsPlugin.Culture,
						$"{fallback.Replace("<player>", target.name)}\n");
				}
				else
				{
					reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"{hint.Replace("<player>", target.name)}\n");
				}

				var roles = MiscUtils.GetPotentialRoles().Where(x => (x is IDoomable doomRole && doomRole.DoomHintType == DoomableType.Default &&
					x is not IUnguessable || x is not IDoomable) && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x)).ToList();

				var allRoles = MiscUtils.AllRoles.Where(x => (x is IDoomable doomRole && doomRole.DoomHintType == DoomableType.Default &&
					x is not IUnguessable || x is not IDoomable) && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x)).Where(x => x is IGuessable && !roles.Contains(x)).ToList();

				if (allRoles.Count > 0)
				{
					foreach (var addedRole in allRoles)
					{
						if (addedRole is IGuessable guessable && guessable.CanBeGuessed)
						{
							roles.Add(addedRole);
						}
					}
				}
				roles = roles.OrderBy(x => x.GetRoleName()).ToList();
				var lastRole = roles[^1];

				if (hintType != DoomableType.Default)
				{
					roles = MiscUtils.GetPotentialRoles().Where(x => x is IDoomable doomRole && doomRole.DoomHintType == hintType &&
						x is not IUnguessable && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x)).ToList();

					allRoles = MiscUtils.AllRoles.Where(x => x is IDoomable doomRole && doomRole.DoomHintType == hintType &&
						x is not IUnguessable && !x.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(x)).Where(x => x is IGuessable && !roles.Contains(x)).ToList();
					if (allRoles.Count > 0)
					{
						foreach (var addedRole in allRoles)
						{
							if (addedRole is IGuessable guessable && guessable.CanBeGuessed)
							{
								roles.Add(addedRole);
							}
						}
					}
					roles = roles.OrderBy(x => x.GetRoleName()).ToList();
					lastRole = roles[^1];
				}

				if (roles.Count != 0)
				{
					reportBuilder.Append(TownOfUsPlugin.Culture, $"(");
					foreach (var role2 in roles)
					{
						if (role2 == lastRole)
						{
							reportBuilder.Append(TownOfUsPlugin.Culture,
								$"{MiscUtils.GetHyperlinkText(lastRole)})");
						}
						else
						{
							reportBuilder.Append(TownOfUsPlugin.Culture,
								$"{MiscUtils.GetHyperlinkText(role2)}, ");
						}
					}
				}
			}
			ret = reportBuilder.ToString();

			return ret;
		}

		public bool IsExempt(PlayerVoteArea voteArea)
		{
			return voteArea.PlayerId == Player.PlayerId ||
				   Player.Data.IsDead || voteArea.AmDead ||
				   voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true ||
				   (voteArea.GetPlayer()?.Data.Role is MayorRole mayor && mayor.Revealed) ||
				   (Player.IsLover() && voteArea.GetPlayer()?.IsLover() == true);
		}

		private static bool IsRoleValid(RoleBehaviour role)
		{
			var unguessableRole = role as IUnguessable;
			if (role.IsDead || role is IGhostRole || (unguessableRole != null && !unguessableRole.IsGuessable))
			{
				return false;
			}

			return true;
		}

		public void ClickGuess(PlayerVoteArea voteArea, MeetingHud meetingHud)
		{
			if (meetingHud.state == MeetingHud.MeetingStates.Discussion)
			{
				return;
			}

			if (Minigame.Instance)
			{
				return;
			}

			var player = GameData.Instance.GetPlayerById(voteArea.PlayerId).Object;

			var shapeMenu = GuesserMenu.Create();
			shapeMenu.Begin(IsRoleValid, ClickRoleHandle);

			void ClickRoleHandle(RoleBehaviour role)
			{
				var realRole = player.Data.Role;


				var pickVictim = role.Role == realRole.Role;
				if (player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
				{
					pickVictim = cachedMod.GuessMode switch
					{
						// Checks for the role the player is at the moment
						CacheRoleGuess.ActiveRole => role.Role == realRole.Role,
						// Checks for the cached role itself (like Imitator or Traitor)
						CacheRoleGuess.CachedRole => role.Role == cachedMod.CachedRole.Role,
						// Checks if it's the cached or active role
						_ => role.Role == cachedMod.CachedRole.Role || role.Role == realRole.Role,
					};
				}
				var victim = pickVictim ? player : Player;

				ClickHandler(victim, voteArea.PlayerId);
			}

			void ClickHandler(PlayerControl victim, byte targetId)
			{
				var opts = OptionGroupSingleton<MarksmanOptions>.Instance;

				var firstTarget = CustomButtonSingleton<MarksmanDiscoverButton>.Instance.FirstTarget;
				var secondTarget = CustomButtonSingleton<MarksmanDiscoverButton>.Instance.SecondTarget;

				var playersAlive = PlayerControl.AllPlayerControls.ToArray()
				.Count(x => !x.HasDied() && !x.IsJailed() && x != Player);

				if (victim == Player)
				{
					IncorrectGuesses++;

					if (( firstTarget == null || targetId !=  firstTarget.PlayerId) &&
						(secondTarget == null || targetId != secondTarget.PlayerId) ||
						(IncorrectGuesses - (opts.MisguessAvailable ? 1 : 0) > 0))
					{
						var notifDeath1 = Helpers.CreateAndShowNotification(
							"That was an incorrect Guess NOW DIE",
							Color.red, new Vector3(0f, 1f, -20f), spr: RoleIcons.Marksman.LoadAsset());

						notifDeath1.AdjustNotification();

						Player.RpcMeetingMurder(victim, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
							causeOfDeath: "MarksmanMisguess");

						shapeMenu.Close();
						return;
					}

					var notif2 = Helpers.CreateAndShowNotification(
						$"That was an incorrect Guess, Incorrect Guesses Left: {(opts.MisguessAvailable ? 1 : 0) - IncorrectGuesses}",
						Color.white, new Vector3(0f, 1f, -20f), spr: RoleIcons.Marksman.LoadAsset());

					notif2.AdjustNotification();

					shapeMenu.Close();
					return;
				}
				else
				{
					if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
					{
						OracleRole.RpcOracleBlessNotify(PlayerControl.LocalPlayer, oracleMod.Oracle, victim);

						MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));

						meetingMenu?.HideSingle(targetId);

						shapeMenu.Close();
						return;
					}

					if (UnlockedAbilities.Contains(MarksmanAbility.Supressor))
					{
						victim.RpcAddModifier<MarksmanSuppressedModifier>(Player);
					}
					else
					{
						Player.RpcMeetingMurder(victim, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
							causeOfDeath: "Marksman");
					}
					for (int i = 0; i < opts.AbilitiesEachGuess; i++) { AddAbility(victim); }

					meetingMenu?.HideSingle(targetId);
				}

				shapeMenu.Close();
			}
		}

		public void AddAbility(PlayerControl target, MarksmanAbility? ability = null)
		{
			int index = UnityEngine.Random.RandomRangeInt(0, LockedAbilities.Count());
			var opts = OptionGroupSingleton<MarksmanOptions>.Instance;

			Message("Triggered Ability");

			if (index > LockedAbilities.Count() || LockedAbilities.Count() == 0)
			{
				Message("index more than count, or 0");
				return;
			}

			if (opts.AbilityCap <= UnlockedAbilities.Count)
			{
				return;
			}

			if (ability != null && LockedAbilities.Contains(ability.Value))
			{
				index = LockedAbilities.IndexOf(ability.Value);
			}

			switch (LockedAbilities[index])
			{
				case MarksmanAbility.SharpenedBlade:
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"Sharpened Blade was Unlocked! Your Kill Cd has been decreased by {opts.SharpenedBladeKillCdReduction} seconds.",
						Color.white, new Vector3(0f, 1f, -20f), spr: NeutAssets.MarksmanSharpenedBlade.LoadAsset());

					notif1.AdjustNotification();

					break;
				}

                case MarksmanAbility.SmokeBomb:
                {
                    var notif1 = Helpers.CreateAndShowNotification(
                        $"Smokebomb was Unlocked! You can now blind players in a set radius, this is not like grenadier, this has smoke",
                        Color.white, new Vector3(0f, 1f, -20f), spr: NeutAssets.MarksmanSmokeBomb.LoadAsset());

                    notif1.AdjustNotification();

                    break;
                }

                case MarksmanAbility.Warp:
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"Warp was Unlocked! You can now mark a player to Warp to them",
						Color.white, new Vector3(0f, 1f, -20f), spr: NeutAssets.MarksmanWarp.LoadAsset());

					notif1.AdjustNotification();

					break;
				}

				case MarksmanAbility.Vanish:
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"Vanish was Unlocked! You can now turn yourself mostly invisble",
						Color.white, new Vector3(0f, 1f, -20f), spr: TouImpAssets.SwoopSprite.LoadAsset());

					notif1.AdjustNotification();

					break;
				}

                case MarksmanAbility.Dismantle:
                {
                    var notif1 = Helpers.CreateAndShowNotification(
                        $"Dismantle was Unlocked! You can now make a Player unable to move, use abilities or SEE :D, but you only have 1 use!",
                        Color.white, new Vector3(0f, 1f, -20f), spr: NeutAssets.MarksmanDismantle.LoadAsset());

                    notif1.AdjustNotification();

                    break;
                }

                case MarksmanAbility.Supressor:
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"Supressor was Unlocked! You can now guess people without anyone else knowing.",
						Color.white, new Vector3(0f, 1f, -20f), spr: NeutAssets.MarksmanSuppressed.LoadAsset());

					notif1.AdjustNotification();

					break;
				}

				case MarksmanAbility.Dualscover:
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"Dualscover was Unlocked! You can now use Discover twice.",
						Color.white, new Vector3(0f, 1f, -20f), spr: RoleIcons.Marksman.LoadAsset());

					notif1.AdjustNotification();

					break;
				}
			}

			Message($"Unlocked {LockedAbilities[index]}");

			UnlockedAbilities.Add(LockedAbilities[index]);
			LockedAbilities.RemoveAt(index);
		}

		public bool IsModifierApplicable(BaseModifier modifier)
		{
			return modifier is not AssassinModifier;
		}

		[MethodRpc((uint)TreeWallModRpcsEnum.MarksmanWarp)]
		public static void RpcWarp(PlayerControl marksman, byte player2)
		{
			byte player1 = marksman.PlayerId;

			if (LobbyBehaviour.Instance)
			{
				MiscUtils.RunAnticheatWarning(marksman);
				return;
			}
			if (marksman.Data.Role is not MarksmanRole)
			{
				Error("RpcTransport - Invalid Marksman");
				return;
			}

			var t1 = GetTarget(player1);
			var t2 = GetTarget(player2);

			if (t1 == null || t2 == null)
			{
				if (marksman.AmOwner)
				{
					Coroutines.Start(MiscUtils.CoFlash(Color.red));
				}

				return;
			}

			var play2pos = GetAdjustedPosition(t2);

			Transport(t1, play2pos);

			MonoBehaviour? GetTarget(byte id)
			{
				var data = GameData.Instance.GetPlayerById(id);
				if (!data)
				{
					return null;
				}

				var stoned = MiscUtils.GetFreshStonedPlayerById(id);
				if (stoned != null)
				{
					return stoned;
				}

				var body = Helpers.GetBodyById(id);
				if (data.IsDead && body)
				{
					return body;
				}

				var pc = data.Object;
				if (!pc)
				{
					return null;
				}

				if (pc.moveable || pc.inVent || (pc.TryGetModifier<DisabledModifier>(out var mod) &&
												 (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)))
				{
					return pc;
				}

				return null;
			}

			Vector2 GetAdjustedPosition(MonoBehaviour transportable)
			{
				// assign dummy values so it doesnt error about returning unassigned variables
				Vector2 TPPosition = transportable.gameObject.transform.position;

				if (transportable.TryCast<DeadBody>() == null)
				{
					Error($"type: {transportable.GetIl2CppType().Name}");
					var TP = transportable.TryCast<PlayerControl>()!;
					var stoned = transportable.TryCast<StonedPlayer>();
					if (stoned == null)
					{
						TPPosition = TP.GetTruePosition();
						TPPosition = new Vector2(TPPosition.x, TPPosition.y + 0.3636f);
					}

					if (TP && TP.HasModifier<MiniModifier>() || stoned != null && stoned.IsMiniPlayer)
					{
						TPPosition = new Vector2(TPPosition.x, TPPosition.y + 0.2233912f * 0.75f);
					}
				}
				else if (transportable.TryCast<DeadBody>() != null)
				{
					var Player1Body = transportable.TryCast<DeadBody>()!;
					TPPosition = Player1Body.TruePosition;
					TPPosition = new Vector2(TPPosition.x, TPPosition.y + 0.3636f);
				}

				return (TPPosition);
			}
		}

		public static void Transport(MonoBehaviour mono, Vector3 position)
		{
			Message($"Transport({mono.name}, {position}) called");

			var player = mono.TryCast<PlayerControl>();
			if (player == null)
			{
				Error("Player is null");
				return;
			}

			if (player != null)
			{
				player.MyPhysics.ResetMoveState();
				player.transform.position = position;
				player.NetTransform.SnapTo(position);
			}

			mono.transform.position = position;
			Collider2D cd = mono.GetComponent<Collider2D>();

			var cnt = mono.TryCast<CustomNetworkTransform>();
			if (cnt != null)
			{
				cnt.SnapTo(position, (ushort)(cnt.lastSequenceId + 1));

				if (cnt.AmOwner && ModCompatibility.IsSubmerged())
				{
					ModCompatibility.ChangeFloor(cnt.myPlayer.GetTruePosition().y > -7);
					ModCompatibility.CheckOutOfBoundsElevator(cnt.myPlayer);
				}
			}

			if (player != null && player.AmOwner)
			{
				// If the transported player is a Puppeteer/Parasite controlling someone, snap camera to the victim instead
				MonoBehaviour? cameraTarget = null;

				if (player.Data?.Role is ITransportTrigger triggerRole)
				{
					cameraTarget = triggerRole.OnTransport();
				}

				MiscUtils.SnapPlayerCamera(cameraTarget ?? PlayerControl.LocalPlayer);
			}
		}
	}

	public enum MarksmanAbility
	{
		SharpenedBlade,
		SmokeBomb,
		Warp,
		Vanish,
		Dismantle,
		Supressor,
		Dualscover
	}

	public enum MarksmanWarpState
	{
		Marking,
		Warp,
		Used
	}
}
