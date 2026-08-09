using System.Linq;
using Frantic.Networking;
using UnityEngine;

namespace Frantic
{
    public class DungeonMusicController : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _ambientSource;

        [SerializeField]
        private AudioSource _franticSource;

        [SerializeField]
        private float _engagementRange = 8f;

        [SerializeField]
        private float _fadeSpeed = 1.5f;

        [SerializeField]
        private float _checkInterval = 0.25f;

        private float _lastCheckTime;
        private bool _engaged;

        private void Update()
        {
            if (Time.time - _lastCheckTime >= _checkInterval)
            {
                _engaged = IsEngagingEnemy();
                _lastCheckTime = Time.time;
            }

            var targetAmbientVolume = _engaged ? 0f : 1f;
            var targetFranticVolume = _engaged ? 1f : 0f;

            if (_ambientSource != null)
            {
                _ambientSource.volume = Mathf.MoveTowards(_ambientSource.volume, targetAmbientVolume, _fadeSpeed * Time.deltaTime);
            }

            if (_franticSource != null)
            {
                _franticSource.volume = Mathf.MoveTowards(_franticSource.volume, targetFranticVolume, _fadeSpeed * Time.deltaTime);
            }
        }

        private bool IsEngagingEnemy()
        {
            var player = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None).FirstOrDefault();
            if (player == null) return false;

            var enemies = FindObjectsByType<EnemyNetwork>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (Vector3.Distance(player.transform.position, enemy.transform.position) <= _engagementRange)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
