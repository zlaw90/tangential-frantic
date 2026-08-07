using UnityEngine;

namespace Frantic.Networking
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private float _smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (_target == null)
            {
                var player = FindObjectOfType<PlayerNetwork>();
                if (player != null)
                {
                    _target = player.transform;
                }
                return;
            }

            var desiredPosition = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
