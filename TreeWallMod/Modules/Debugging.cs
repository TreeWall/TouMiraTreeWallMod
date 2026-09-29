using MiraAPI.Hud;
using Reactor.Utilities;
using System;
using System.Collections;
using System.Text;
using TownOfUs.Buttons;
using TreeWallMod.Options.Modifiers;
using UnityEngine;

namespace TreeWallMod.Modules
{
    public static class Debugging
    {
        public static string DebugCamera()
        {
            var cam = Camera.main;
            var z = -89f;
            var bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.WorldToScreenPoint(new Vector3(0, 0, z)).z));
            var topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.WorldToScreenPoint(new Vector3(0, 0, z)).z));

            return ($"Orthographic: {cam.orthographic}, Size: {cam.orthographicSize}, Aspect: {cam.aspect}, " +
                $"Position: {cam.transform.position}, Bottom Left: {bottomLeft}, Top Right: {topRight}");
        }

        public static string DebugKillButton()
        {
            StringBuilder sb = new();

            if (CustomButtonManager.Buttons.Count == 0 || CustomButtonManager.Buttons == null)
            {
                return "No Buttons";
            }

            foreach (var button in CustomButtonManager.Buttons)
            {
                if (button != null && button.Button != null && button is IKillButton && button.Button.isActiveAndEnabled)
                {
                    sb.Append($"{button.Name} is Kill type and present\n");
                }
            }

            return sb.ToString();
        }

        public static void DebugStrech(PlayerControl Player, WEEEStrechEnum strechMode, float duration, float scale, int cycles, bool rotate, int rotCycles)
        {
            Coroutines.Start(CoStrech(Player, strechMode, duration, scale, cycles, rotate, rotCycles));
        }

        private static IEnumerator CoStrech(PlayerControl Player, WEEEStrechEnum strechMode, float duration, float scale, int cycles, bool rotate, int rotCycles)
        {
            if (Player == null || Player.HasDied())
            {
                yield break;
            }

            var body = Player.cosmetics.currentBodySprite.BodySprite.gameObject;
            var cosmetics = Player.cosmetics.gameObject;

            if (body == null || cosmetics == null)
            {
                yield break;
            }

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
                strechMode = (WEEEStrechEnum)UnityEngine.Random.RandomRangeInt(1, 5);
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
                    strechMode == WEEEStrechEnum.Both)
                {
                    float scalingFactor = (float)(Math.Sin(elapsed*2*Math.PI*cycles/duration + correctingFactor)*scale);

                    float scaleX = scalingFactor;

                    body.transform.localScale = new Vector3(scaleX*ogBodyScales.x, ogBodyScales.y, 1*ogBodyScales.z);
                    cosmetics.transform.localScale = new Vector3(scaleX*ogCosmeticsScales.x, ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
                }
                if (strechMode == WEEEStrechEnum.inY ||
                    strechMode == WEEEStrechEnum.Both)
                {
                    float scalingFactor = (float)(Math.Sin(elapsed*2*Math.PI*cycles/duration + correctingFactor)*scale);

                    float scaleY = scalingFactor;

                    body.transform.localScale = new Vector3(ogBodyScales.x, scaleY*ogBodyScales.y, 1*ogBodyScales.z);
                    cosmetics.transform.localScale = new Vector3(ogCosmeticsScales.x, scaleY*ogCosmeticsScales.y, 1*ogCosmeticsScales.z);
                }

                if (strechMode == WEEEStrechEnum.Squish)
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
