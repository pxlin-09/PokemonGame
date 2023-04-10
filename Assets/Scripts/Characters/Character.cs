using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    CharacterAnimator animator;
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    public CharacterAnimator Animator
    {
        get => animator;
    }

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1, 1);
        animator.MoveY = Mathf.Clamp(moveVec.y,-1, 1);
        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos)) yield break;

        IsMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;
        if (Physics2D.BoxCast(transform.position + dir, new Vector2(.2f, .2f),
            0f, dir, diff.magnitude - 1,
            GameLayers.i.SolidObjectLayer |
            GameLayers.i.InteractableLayer |
            GameLayers.i.PlayerLayer) == true)
        {
            return false;
        }
        return true;
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f,
            GameLayers.i.SolidObjectLayer |
            GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }
}
