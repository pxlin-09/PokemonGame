using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectLayer;
    public LayerMask interactableLayer;
    public LayerMask longGrassLayer;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;

    // Observer design pattern: gameController observes when playController
    // encounters a pokemon and changes state to battle
    public event Action onEncountered;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0) input.y = 0;
            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (isWalkable(targetPos)) StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, longGrassLayer) != null)
        {
            Debug.Log("GRASS");
            if (UnityEngine.Random.Range(1,101) <= 15)
            {
                animator.SetBool("isMoving", false);
                onEncountered();
            }
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, .3f, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }
}
