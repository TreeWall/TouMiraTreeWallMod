using System;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TreeWallMod.Modules;
using TreeWallMod.Options.Roles.Neutral;
using UnityEngine;

namespace TreeWallMod.Modifiers.Neutral
{
	public sealed class MarksmanSmokedModifier(PlayerControl marksman) : DisabledModifier, IDisposable
	{
		public static Color blindVision = new(0.83f, 0.83f, 0.83f, 1f);
        private readonly Color dimVision = new(0.83f, 0.83f, 0.83f, 0.2f);

		private readonly Color normalVision = new(0.83f, 0.83f, 0.83f, 0f);

		private ScreenFlash? flash;
		public override string ModifierName => "Smoked";
		public override bool HideOnUi => true;
		public override float Duration => OptionGroupSingleton<MarksmanOptions>.Instance.SmokebombDuration + 0.5f;
		public override bool AutoStart => true;
		public override bool CanBeInteractedWith => true;
		public override bool IsConsideredAlive => true;
		public override bool CanUseAbilities => true;
		public override bool CanUseConsoles => Player == marksman;
		public override bool CanOpenMap => Player == marksman;
		public override bool CanReport => false;
		public PlayerControl Marksman => marksman;

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

            var cloudAsset = Assets.Assets.Cloud_4;

            switch (randomCloudSprite)
            {
                case 1:
                    cloudAsset = Assets.Assets.Cloud_1;
                    break;
                case 2:
                    cloudAsset = Assets.Assets.Cloud_2;
                    break;
                case 3:
                    cloudAsset = Assets.Assets.Cloud_3;
                    break;
                case 4:
                    cloudAsset = Assets.Assets.Cloud_4;
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

			if (Player.AmOwner)
			{
				if (!Marksman.AmOwner)
				{
					var notif1 = Helpers.CreateAndShowNotification(
						$"A Smokebomb went off near you!",
						Color.white,
						spr: Assets.RoleIcons.Marksman.LoadAsset());

					notif1.AdjustNotification();
					notif1.transform.localPosition = new Vector3(0f, 1f, -150f);
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (Player.AmOwner)
			{
				CloudLogic();
			}

			if (Player != Marksman && PlayerControl.LocalPlayer == Marksman)
			{
				if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
				{
					Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Color.black);
				}
				else
				{
					Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor,
						Palette.VisorColor);
				}
			}

			if (Player.AmOwner)
			{
				if (TimeRemaining > Duration - 0.5f)
				{
					var fade = (TimeRemaining - Duration) * -2.0f;

					if (ShouldPlayerBeBlinded(Player))
					{
						SetFlash(Color.Lerp(normalVision, blindVision, fade));
					}
					else if (ShouldPlayerBeDimmed(Player))
					{
						SetFlash(Color.Lerp(normalVision, dimVision, fade));
					}
					else
					{
						SetFlash(normalVision);
					}
				}
				else if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
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
				else if (TimeRemaining < 0.5f)
				{
					var fade2 = TimeRemaining * -2.0f + 1.0f;

					if (ShouldPlayerBeBlinded(Player))
					{
						SetFlash(Color.Lerp(blindVision, normalVision, fade2));
					}
					else if (ShouldPlayerBeDimmed(Player))
					{
						SetFlash(Color.Lerp(dimVision, normalVision, fade2));
					}
					else
					{
						SetFlash(normalVision);
					}
				}
				else
				{
					SetFlash(normalVision);

					TimeRemaining = 0.0f;
				}

				if (MeetingHud.Instance)
				{
					SetFlash(normalVision);

					TimeRemaining = 0.0f;
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