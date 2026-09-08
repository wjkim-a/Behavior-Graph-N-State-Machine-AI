using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpSpeed;
    private Rigidbody2D _rigid;
    private bool _isGrounded;
    private Vector2 _inputValue;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _rigid.linearVelocity = new Vector2((_inputValue * _moveSpeed).x, _rigid.linearVelocity.y);
    }

    public void OnMoveAction(InputAction.CallbackContext ctx)
    {
        _inputValue = ctx.ReadValue<Vector2>();        
    }

    public void OnJumpAction(InputAction.CallbackContext ctx)
    {
        if(_isGrounded)
            _rigid.AddForce(Vector2.up *  _jumpSpeed, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _isGrounded = false;
    }

}
