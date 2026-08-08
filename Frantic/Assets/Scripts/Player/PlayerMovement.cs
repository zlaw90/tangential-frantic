using UnityEngine;

namespace Frantic.Networking
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 5f;

        private void FixedUpdate()
        {
            var input = InputManager.Move;

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input != Vector2.zero)
            {
                transform.position += new Vector3(input.x, input.y, 0f) * _moveSpeed * Time.fixedDeltaTime;
            }
        }
    }
}
