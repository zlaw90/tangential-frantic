using UnityEngine;

namespace Frantic.Networking
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private float _smoothSpeed = 10f;

        [SerializeField]
        private Vector3 _offset = new Vector3(0f, 0f, -10f);

        private void LateUpdate()
        {
            if (_target == null)
            {
                var player = FindPlayer();
                if (player != null)
                {
                    _target = player.transform;
                }
                else
                {
                    return;
                }
            }

            var desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }

        private PlayerNetwork FindPlayer()
        {
            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
            if (players.Length > 0) return players[0];
            return null;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
