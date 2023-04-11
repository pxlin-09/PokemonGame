using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string _name;
    [SerializeField] Sprite sprite;
    private Vector2 input;
    private Character character;

    // Observer design pattern: gameController observes when playController
    // encounters a pokemon and changes state to battle
    public event Action onEncountered;
    public event Action<Collider2D> onEnterTrainersView;

    private void Awake()
    {
        character = GetComponent<Character>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public string Name
    {
        get => _name;
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0) input.y = 0;
            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }
        character.HandleUpdate();
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckForInTrainersView();
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.LongGrassLayer) != null)
        {
            Debug.Log("GRASS");
            if (UnityEngine.Random.Range(1,101) <= 15)
            {
                character.Animator.IsMoving = false;
                onEncountered();
            }
        }
    }

    private void CheckForInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
            onEnterTrainersView?.Invoke(collider);
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, .3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
}
