using UnityEngine;

namespace Frantic
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbookAnimator : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] _frames;

        [SerializeField]
        private float _framesPerSecond = 10f;

        private SpriteRenderer _renderer;
        private float _timer;
        private int _frameIndex;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0 || _framesPerSecond <= 0f) return;

            var frameDuration = 1f / _framesPerSecond;
            _timer += Time.deltaTime;

            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _frameIndex = (_frameIndex + 1) % _frames.Length;
                _renderer.sprite = _frames[_frameIndex];
            }
        }
    }
}
