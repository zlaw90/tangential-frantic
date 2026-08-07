using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 5f;

        private PlayerNetwork _playerNetwork;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _playerNetwork = GetComponent<PlayerNetwork>();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            var input = Vector2.zero;
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input != Vector2.zero)
            {
                MoveServerRpc(input);
            }
        }

        [ServerRpc]
        private void MoveServerRpc(Vector2 input)
        {
            if (!IsServer) return;

            transform.position += new Vector3(input.x, input.y, 0f) * _moveSpeed * Time.fixedDeltaTime;
        }
    }
}
