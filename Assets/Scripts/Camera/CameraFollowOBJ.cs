using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowOBJ : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float flipYRotationTime = 0.5f;

    private PlayerController player;

    private bool isFacingRight;

    void Awake()
    {
        player = playerTransform.gameObject.GetComponent<PlayerController>();

        isFacingRight = player.isFacingRight;
    }

    void Update()
    {
        transform.position = playerTransform.position;
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseInOutSine();
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
        {
            return 150f;
        }
        else
        {
            return 0f;
        }
    }
}
