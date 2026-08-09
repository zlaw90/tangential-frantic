using UnityEngine;

namespace Frantic.Networking
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 5f;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            var input = InputManager.Move;

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input != Vector2.zero)
            {
                _animator.SetBool("walking", true);
                transform.position += new Vector3(input.x, input.y, 0f) * _moveSpeed * Time.fixedDeltaTime;
            }
            else
            {
                _animator.SetBool("walking", false);
            }
        }
    }
}
