using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody2D rb;

    private MasterInput inputSystem = null;

    private Vector2 playerInput = Vector2.zero;
    private void Awake()
    {
        inputSystem = new MasterInput();
        inputSystem.Enable();
    }

    private void Start()
    {
        inputSystem.Player.Movement.performed += MovementInputStart;
        inputSystem.Player.Movement.canceled += MovementInputStop;
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (playerInput != Vector2.zero)
        {
            //Vector2 bufferPosition = playerInput.normalized * (moveSpeed * Time.deltaTime);
            //transform.position += new Vector3(bufferPosition.x, bufferPosition.y, 0);
            
            rb.MovePosition(rb.position + playerInput * (moveSpeed * Time.deltaTime));
        }
    }

    private void MovementInputStart(InputAction.CallbackContext a_context)
    {
        playerInput = a_context.ReadValue<Vector2>();
    }

    private void MovementInputStop(InputAction.CallbackContext a_context)
    {
        playerInput = Vector2.zero;
    }
}
