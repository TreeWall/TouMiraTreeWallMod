using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Modifiers.Game;
using TreeWallMod.Modules;
using TreeWallMod.Options.Modifiers;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.UIElements.Columns;

namespace TreeWallMod.Modifiers.GameModifiers
{
	public sealed class WEEEModifier : UniversalGameModifier, IWikiDiscoverable
	{
		public override string ModifierName => "WEEE";
		public override bool ShowInFreeplay => true;
		public override Color FreeplayFileColor => Colors.WEEEModifer;

		public GameObject? body { get; set; }
		public GameObject? cosmetics { get; set; }

		private List<WEEEStrechEnum> randomBucket = new();

		public override string GetDescription()
		{
			return "STRECH when you want to :D";
		}

		public string GetAdvancedDescription()
		{
			return "You STRECH. Thats it, thats the description";
		}

		public override int GetAmountPerGame()
		{
			return (int)OptionGroupSingleton<TWUniversalModifierOptions>.Instance.WEEEAmount;
		}

		public override int GetAssignmentChance()
		{
			return (int)OptionGroupSingleton<TWUniversalModifierOptions>.Instance.WEEEChance;
		}

		public override void OnActivate()
		{
			body = Player.cosmetics.currentBodySprite.BodySprite.gameObject;
			cosmetics = Player.cosmetics.gameObject;
		}

		private void fillBucket()
		{
			randomBucket = new List<WEEEStrechEnum>()
            {
				WEEEStrechEnum.Both  , WEEEStrechEnum.Both ,
				WEEEStrechEnum.inX   , WEEEStrechEnum.inX  ,
				WEEEStrechEnum.inY   , WEEEStrechEnum.inY  ,
				WEEEStrechEnum.Squish, WEEEStrechEnum.Squish
			};
		}

		private WEEEStrechEnum getFromBucket()
		{
			if (randomBucket.Count == 0)
			{
				fillBucket();
			}

			int random = UnityEngine.Random.RandomRangeInt(0, randomBucket.Count);
			var ret = randomBucket[random];

			randomBucket.RemoveAt(random);

			return ret;
		}

		[MethodRpc((uint)TreeWallModRpcsEnum.WEEEStrech)]
		public static void RpcWEEEStrech(PlayerControl player, WEEEStrechEnum strechMode, float duration = 10f, float scale = 2.5f, int cycles = 5, bool rotate = false, int rotCycles = 10)
		{
			if (!player.TryGetModifier<WEEEModifier>(out var mod))
			{
				return;
			}

			Coroutines.Start(mod.CoStrech(strechMode, duration, scale, cycles, rotate, rotCycles));
		}

		private IEnumerator CoStrech(WEEEStrechEnum strechMode, float duration, float scale, int cycles, bool rotate, int rotCycles)
		{
			if (body == null || cosmetics == null)
			{
				yield break;
			}

			Vector3 ogBodyScales = body.transform.localScale;
			Vector3 ogCosmeticsScales = cosmetics.transform.localScale;

			Vector3 ogBodyRot = body.transform.localEulerAngles;
			Vector3 ogCosmeticsRot = cosmetics.transform.localEulerAngles;

			var elapsed = 0f;
			float correctingFactor = (float)Math.Asin(1/scale);

			if (cycles<1) cycles = 1;
			if (rotCycles<1) rotCycles = 1;

			if (strechMode == WEEEStrechEnum.Random)
			{
				strechMode = getFromBucket();
			}

			while (elapsed < duration)
			{
				if (Player == null || Player.HasDied() || body == null || cosmetics == null)
				{
					if (body != null)
					{
						body.transform.localScale = ogBodyScales;
						body.transform.localEulerAngles = ogBodyRot;
					}

					if (cosmetics != null)
					{
						cosmetics.transform.localScale = ogBodyScales;
						cosmetics.transform.localEulerAngles = ogBodyRot;
					}

					yield break;
				}

				elapsed += Time.deltaTime;

				float rot = (float)(elapsed/duration * 360 * rotCycles);

				if (strechMode == WEEEStrechEnum.inX ||
					strechMode == WEEEStrechEnum.inY ||
					strechMode == WEEEStrechEnum.Both)
				{
					float scalingFactor = (float)(Math.Sin(elapsed*2*Math.PI*cycles/duration + correctingFactor)*scale);

					float scaleX = 1f;
					float scaleY = 1f;

					if (strechMode == WEEEStrechEnum.inX || strechMode == WEEEStrechEnum.Both)
					{
						scaleX = scalingFactor;
					}
					if (strechMode == WEEEStrechEnum.inY || strechMode == WEEEStrechEnum.Both)
					{
						scaleY = scalingFactor;
					}

                    body.transform.localScale = new Vector3(scaleX*ogBodyScales.x, scaleY*ogBodyScales.y, 1*ogBodyScales.z);
					cosmetics.transform.localScale = new Vector3(scaleX*ogCosmeticsScales.x, scaleY*ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
				}
				else if (strechMode == WEEEStrechEnum.Squish)
				{
					if (elapsed < 0.05*duration)
					{
						float t = (elapsed*20)/duration;
						float s = Mathf.Lerp(1, scale, t);

						body.transform.localScale = new Vector3(s*ogBodyScales.x, s*ogBodyScales.y, 1*ogBodyScales.z);
						cosmetics.transform.localScale = new Vector3(s*ogCosmeticsScales.x, s*ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
					}
					else if (elapsed >= 0.95f*duration)
					{
						float t = ((elapsed - 0.95f*duration)*20)/duration;
						float s = Mathf.Lerp(scale, 1, t);

						body.transform.localScale = new Vector3(s*ogBodyScales.x, s*ogBodyScales.y, 1*ogBodyScales.z);
						cosmetics.transform.localScale = new Vector3(s*ogCosmeticsScales.x, s*ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
					}
					else
					{
						float t = ((elapsed - 0.05f*duration)/(0.9f*duration))*cycles;
						float theta = (float)(Math.PI/4 + t*2*Math.PI);

						float x = (float)Math.Cos(theta) * 1.4142f;
						float y = (float)Math.Sin(theta) * 1.4142f;

						body.transform.localScale = new Vector3(x*scale*ogBodyScales.x, y*scale*ogBodyScales.y, 1*ogBodyScales.z);
						cosmetics.transform.localScale = new Vector3(x*scale*ogCosmeticsScales.x, y*scale*ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
					}
				}

				if (rotate)
				{
					body.transform.localEulerAngles = new Vector3(ogBodyRot.x, ogBodyRot.y, ogBodyRot.z + rot);
					cosmetics.transform.localEulerAngles = new Vector3(ogCosmeticsRot.x, ogCosmeticsRot.y, ogCosmeticsRot.z + rot);
				}


				yield return null;
			}

			body.transform.localScale = ogBodyScales;
			cosmetics.transform.localScale = ogCosmeticsScales;

			body.transform.localEulerAngles = ogBodyRot;
			cosmetics.transform.localEulerAngles = ogCosmeticsRot;
		}
	}
}
