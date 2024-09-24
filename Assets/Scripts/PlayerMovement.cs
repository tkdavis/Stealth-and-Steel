using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Position.Value = new Vector3(transform.position.x, 1.0f, transform.position.z);
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;

            characterController.Move(movement);
            MoveServerRpc(direction);
        }
        else if (IsOwner)
        {
            MoveServerRpc(Vector3.zero);
        }
    }

    [Rpc(SendTo.Server)]
    void MoveServerRpc(Vector3 direction, RpcParams rpcParams = default)
    {
        if (direction.magnitude >= 0.1f)
        {
            Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
            Position.Value += movement;
        }

        UpdatePositionClientRpc(transform.position);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 newPosition)
    {
        if (!IsOwner)
        {
            float threshold = 0.1f;
            if (Vector3.Distance(transform.position, newPosition) > threshold)
            {
                transform.position = newPosition;
            }
        }
    }

    private void LateUpdate()
    {
        if (IsServer)
        {
            transform.position = Position.Value;
        }
    }
}
