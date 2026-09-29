using Reactor.Utilities;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TreeWallMod.Modules
{
    // Code Review: Should be using a MonoBehaviour (I have no idea what it means but TOU: mira had it so here you go :P)
    public sealed class DriftingCloud : IDisposable
    {
        private readonly SpriteRenderer _renderer;
        private readonly float _scale;

        static float z = -100f;

        private DriftingCloud(SpriteRenderer renderer, float scale)
        {
            _renderer = renderer;
            _scale = scale;
        }

        public static DriftingCloud Spawn(Sprite cloudSprite, Vector2 startPos, Vector2 endPos, float duration, bool flipX = false, float scale = 1f, bool offsetX = false)
        {
            var renderer = Object.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.FullScreen.transform.parent);
            renderer.sprite = cloudSprite;
            renderer.color = new Color(1f, 1f, 1f, 1f);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
            renderer.flipX = flipX;
            renderer.gameObject.SetActive(true);

            float correctingFactor = offsetX ? (258*scale)/renderer.sprite.pixelsPerUnit : 0f;

            Vector3 sPos = new(startPos.x + Mathf.Sign(startPos.x)*correctingFactor, startPos.y, z);
            Vector3 ePos = new(endPos.x + Mathf.Sign(endPos.x)*correctingFactor, endPos.y, z);
            renderer.transform.localPosition = sPos;

            var cloud = new DriftingCloud(renderer, scale);
            Coroutines.Start(cloud.CoDrift(sPos, ePos, duration));

            //Message($"New Cloud created at: {cloud._renderer.transform.localPosition} with start pos: {sPos} end pos: {ePos} duration: {duration}");

            return cloud;
        }

        private IEnumerator CoDrift(Vector3 startPos, Vector3 endPos, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (!_renderer)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                _renderer.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                _renderer.transform.localScale = new Vector3(_scale, _scale, 1f);
                yield return null;
            }

            Destroy();
        }

        public bool IsAlive()
        {
            return _renderer != null;
        }

        public void Destroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _renderer)
            {
                Object.Destroy(_renderer.gameObject);
            }
        }
    }
}
