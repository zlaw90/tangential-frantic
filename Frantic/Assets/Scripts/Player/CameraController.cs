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
                var player = FindPlayerInSameScene();
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

        private PlayerNetwork FindPlayerInSameScene()
        {
            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.gameObject.scene.name == gameObject.scene.name)
                {
                    return player;
                }
            }
            return null;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
