using PowerTools;
using Reactor.Utilities;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TreeWallMod.Modules
{
    // Code Review: Should be using a MonoBehaviour (I have no idea what it means but TOU: mira had it so here you go :P)
    public sealed class HeadlessPlayer : IDisposable
    {
        private readonly GameObject _gameObject;
        private readonly SpriteRenderer _renderer;
        private readonly SpriteAnim _spriteAnim;
        private readonly AnimationClip _idleAnim;
        private readonly AnimationClip _walkAnim;

        private bool _isWalking = false;

        public byte PlayerId { get; }
        public PlayerControl Player { get; }

        private HeadlessPlayer(GameObject gameObject, SpriteRenderer renderer, SpriteAnim spriteAnim, AnimationClip idleAnim, AnimationClip walkAnim, PlayerControl player)
        {
            _gameObject = gameObject;
            _renderer = renderer;
            _spriteAnim = spriteAnim;
            _idleAnim = idleAnim;
            _walkAnim = walkAnim;

            PlayerId = player.PlayerId;
            Player = player;
        }

        public static HeadlessPlayer Spawn(PlayerControl player, Sprite? playerSprite = null, AnimationClip? idleAnim = null, AnimationClip? walkAnim = null)
        {
            var bodyClone = Object.Instantiate(player.cosmetics.currentBodySprite.BodySprite.gameObject);
            bodyClone.transform.position = player.transform.position;
            bodyClone.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

            var renderer = bodyClone.GetComponent<SpriteRenderer>()!;
            if (playerSprite != null)
            {
                renderer.sprite = playerSprite;
            }

            var spriteAnim = bodyClone.GetComponent<SpriteAnim>()!;
            if (idleAnim != null)
            {
                spriteAnim.m_defaultAnim = idleAnim;
                spriteAnim.Play(idleAnim);
            }

            var headlessPlayer = new HeadlessPlayer(
                bodyClone, renderer, spriteAnim, spriteAnim.m_defaultAnim, walkAnim == null ? player.MyPhysics.Animations.group.RunAnim : walkAnim, player);

            Coroutines.Start(headlessPlayer.CoFollowPlayer());

            return headlessPlayer;
        }

        private IEnumerator CoFollowPlayer()
        {
            while (_gameObject != null && Player != null)
            {
                _gameObject.transform.position = Player.transform.position;
                _renderer.flipX = Player.cosmetics.currentBodySprite.BodySprite.flipX;

                bool isMoving = Player.MyPhysics.Velocity.sqrMagnitude != 0;

                if (!isMoving && _isWalking)
                {
                    _spriteAnim.Play(_idleAnim);
                    _isWalking = false;
                }
                else if (isMoving && !_isWalking)
                {
                    _spriteAnim.Play(_walkAnim);
                    _isWalking = true;
                }

                yield return null;
            }

            Destroy();
        }

        public void SetColor(float r, float g, float b, float a)
        {
            _renderer.color = new Color(r, g, b, a);
        }

        public void Destroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_gameObject)
                {
                    Object.Destroy(_gameObject);
                }

                if (_renderer)
                {
                    Object.Destroy(_renderer);
                }
            }
        }
    }
}
