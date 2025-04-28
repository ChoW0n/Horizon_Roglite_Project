using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 카메라랑 적AI를 위해 대충 만든거라 맘대로 수정하세요.

    public float speed = 5f;
    public float jumpPower = 5f;

    private Rigidbody2D rb;
    private Vector3 movement;

    private bool isJumping = false;

    [NonSerialized] public bool isFacingRight = false;

    public GameObject cameraFollowOBJ;
    private CameraFollowOBJ c_CameraFollowOBJ;

    private float _fallSpeedYDampingChangeThreshold;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        c_CameraFollowOBJ = cameraFollowOBJ.GetComponent<CameraFollowOBJ>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;

        StartDirectionCheck();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }

        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.isLerpingYDamping && !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.isLerpingYDamping && CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }
    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    private void Move()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0 || horizontalInput < 0)
        {
            TurnCheck();
        }

        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
    }

    private void StartDirectionCheck()
    {
        if (transform.localScale.x > 0)
        {
            isFacingRight = true;
        }
        else
        {
            isFacingRight = false;
        }
    }

    private void TurnCheck()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0 && !isFacingRight)
        {
            Turn();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            c_CameraFollowOBJ.CallTurn();
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            c_CameraFollowOBJ.CallTurn();
        }
    }

    private void Jump()
    {
        if (!isJumping)
            return;

        rb.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

        isJumping = false;
    }
}
