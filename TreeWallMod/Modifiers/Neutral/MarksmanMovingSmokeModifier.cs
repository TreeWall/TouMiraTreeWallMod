using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using System;
using System.Collections.Generic;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Neutral;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace TreeWallMod.Modifiers.Neutral
{
	public sealed class MarksmanMovingSmokeModifier(PlayerControl marksman) : DisabledModifier, IDisposable
	{
		public static Color blindVision = new(0.83f, 0.83f, 0.83f, 1f);
		private readonly Color dimVision = new(0.83f, 0.83f, 0.83f, 0.2f);

		private readonly Color normalVision = new(0.83f, 0.83f, 0.83f, 0f);

		private ScreenFlash? flash;
		public override string ModifierName => "MovingSmoked";
		public override bool HideOnUi => true;
		public override float Duration => OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombDuration + 0.5f;
		public override bool AutoStart => true;
		public override bool CanBeInteractedWith => true;
		public override bool IsConsideredAlive => true;
		public override bool CanUseAbilities => true;
		public override bool CanUseConsoles => Player == marksman || !Blind;
		public override bool CanOpenMap => Player == marksman || !Blind;
		public override bool CanReport => false || !Blind;
		public PlayerControl Marksman => marksman;

		public bool Blind = false;
		private float blindedTime = 0f;
		private float eyeNotHurtyTime = 0f;
		private Color currFlashColor = new(0.83f, 0.83f, 0.83f, 0f);
		private Color prevFlashColor = new(0.83f, 0.83f, 0.83f, 0f);

		private int maxClouds = 15;
		private List<DriftingCloud> cloudObjects = new();
		private float elapsed = 0f;
		private float randomSpawnTime = UnityEngine.Random.RandomRangeInt(1, 201)/100;

		private DriftingCloud SpawnCloud()
		{
			var camera = Camera.main;

			float y = camera.orthographicSize;
			float x = y*camera.aspect;

			int   randomCloudSprite = UnityEngine.Random.RandomRangeInt(0, 4);
			int   random = UnityEngine.Random.RandomRangeInt(0, 2);
			int   randomFlip = UnityEngine.Random.RandomRangeInt(0, 2);
			float randomY = UnityEngine.Random.RandomRange(-y, y);
			float randomDuration = UnityEngine.Random.RandomRangeInt(3, 15);
			float randomScale = UnityEngine.Random.RandomRange(1f, 2.5f);

			if (Marksman.AmOwner)
			{
				randomScale = 1f;
			}

			var cloudAsset = TWAssets.TWAssets.Cloud_4;

			switch (randomCloudSprite)
			{
				case 1:
					cloudAsset = TWAssets.TWAssets.Cloud_1;
					break;
				case 2:
					cloudAsset = TWAssets.TWAssets.Cloud_2;
					break;
				case 3:
					cloudAsset = TWAssets.TWAssets.Cloud_3;
					break;
				case 4:
					cloudAsset = TWAssets.TWAssets.Cloud_4;
					break;
			}

			var cloud = DriftingCloud.Spawn(
				cloudAsset.LoadAsset(), new Vector2((random*2 - 1)*x, randomY),
				new Vector2((1 - random*2)*x, randomY), randomDuration, randomFlip == 0, randomScale, true);

			return cloud;
		}

		private void CloudLogic()
		{
			cloudObjects.RemoveAll(c => !c.IsAlive());

			elapsed += Time.deltaTime;

			if (cloudObjects.Count >= maxClouds)
			{
				return;
			}

			if (cloudObjects.Count < 2)
			{
				cloudObjects.Add(SpawnCloud());

				return;
			}
			else if (elapsed >= randomSpawnTime)
			{
				elapsed -= randomSpawnTime;
				randomSpawnTime = UnityEngine.Random.RandomRangeInt(1, 201)/100;

				cloudObjects.Add(SpawnCloud());

				return;
			}
		}

		public void Dispose()
		{
			flash?.Dispose();
			foreach (var cloud in cloudObjects)
			{
				cloud?.Dispose();
			}
		}

		public override void OnActivate()
		{
			base.OnActivate();

			flash = new ScreenFlash();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (Helpers.GetClosestPlayers(Marksman, OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombRadius * ShipStatus.Instance.MaxLightRadius).Contains(Player) && TimeRemaining > 0.5f)
			{
				Blind = true;
				if (eyeNotHurtyTime != 0f)
				{
					prevFlashColor = currFlashColor;
				}
				eyeNotHurtyTime = 0f;
			}
			else
			{
				Blind = false;
				if (blindedTime != 0f)
				{
					prevFlashColor = currFlashColor;
				}
				blindedTime = 0f;
			}

			if (Player != Marksman && PlayerControl.LocalPlayer == Marksman)
			{
				if (blindedTime >= 0.5f && Blind)
				{
					Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Color.black);
				}
				else
				{
					Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor,
						Palette.VisorColor);
				}
			}

			if (Player.AmOwner && (Blind || Marksman.AmOwner))
			{
				CloudLogic();
			}

			if (!Blind)
			{
				eyeNotHurtyTime += Time.deltaTime;

				if (flash == null || !flash.IsActive())
				{
					return;
				}

				if (Player.AmOwner)
				{
					if (eyeNotHurtyTime < 0.5f)
					{
						var fade2 = eyeNotHurtyTime * 2.0f;

						if (ShouldPlayerBeBlinded(Player))
						{
							SetFlash(Color.Lerp(prevFlashColor, normalVision, fade2));
						}
						else if (ShouldPlayerBeDimmed(Player))
						{
							SetFlash(Color.Lerp(prevFlashColor, normalVision, fade2));
						}
						else
						{
							SetFlash(normalVision);
						}
					}
				}

				return;
			}
			else
			{
				blindedTime += Time.deltaTime;

				if (Player.AmOwner)
				{
					if (blindedTime < 0.5f)
					{
						var fade = blindedTime * 2.0f;

						if (ShouldPlayerBeBlinded(Player))
						{
							SetFlash(Color.Lerp(prevFlashColor, blindVision, fade));
						}
						else if (ShouldPlayerBeDimmed(Player))
						{
							SetFlash(Color.Lerp(prevFlashColor, dimVision, fade));
						}
						else
						{
							SetFlash(prevFlashColor);
						}
					}
					else if (blindedTime >= 0.5f)
					{
						if (ShouldPlayerBeBlinded(Player))
						{
							SetFlash(blindVision);
						}
						else if (ShouldPlayerBeDimmed(Player))
						{
							SetFlash(dimVision);
						}
						else
						{
							SetFlash(normalVision);
						}
					}

					if (MeetingHud.Instance)
					{
						SetFlash(normalVision);

						TimeRemaining = 0.0f;
					}
				}
			}
		}

		public override void OnDeactivate()
		{
			if (Player.AmOwner)
			{
				SetFlash(normalVision);

				flash?.Destroy();
			}

			if (Player != Marksman && PlayerControl.LocalPlayer == Marksman)
			{
				Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
			}
		}

		public override void OnMeetingStart()
		{
			ModifierComponent?.RemoveModifier(this);
		}

		private void SetFlash(Color color)
		{
			if (flash != null)
			{
				flash.SetColour(color);
				flash.SetActive(true);
				currFlashColor = color;

				if (color == normalVision)
				{
					flash.SetActive(false);
				}
			}
		}

		private bool ShouldPlayerBeDimmed(PlayerControl player)
		{
			return (player == Marksman || player.HasDied()) && !MeetingHud.Instance;
		}

		private bool ShouldPlayerBeBlinded(PlayerControl player)
		{
			return (player != Marksman) && !player.HasDied() && !MeetingHud.Instance;
		}
	}
}