using UnityEngine;

namespace Frantic
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PulseLight : MonoBehaviour
    {
        [SerializeField]
        private float _bpm = 120f;

        [SerializeField]
        private float _minScale = 0.8f;

        [SerializeField]
        private float _maxScale = 1.2f;

        private SpriteRenderer _renderer;
        private Vector3 _baseScale;
        private float _baseAlpha;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _baseAlpha = _renderer.color.a;
        }

        private void Update()
        {
            var beatsPerSecond = _bpm / 60f;
            var pulse = (Mathf.Sin(Time.time * beatsPerSecond * Mathf.PI * 2f) + 1f) * 0.5f;

            transform.localScale = _baseScale * Mathf.Lerp(_minScale, _maxScale, pulse);

            var color = _renderer.color;
            color.a = Mathf.Lerp(_baseAlpha * 0.5f, _baseAlpha, pulse);
            _renderer.color = color;
        }
    }
}
