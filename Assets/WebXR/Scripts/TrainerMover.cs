using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerMover : MonoBehaviour
{
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        transform.position += move * speed * Time.deltaTime;
    }
}
