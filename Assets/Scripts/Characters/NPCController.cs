using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    Character character;

    int currentPattern = 0;
    NPCState state ;
    float idleTimer = 0f;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    void Update()
    {
        if (DialogManager.Instance.IsShowing) return;

        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    public void Interact()
    {
        if (state == NPCState.Idle)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
        }
    }

    public IEnumerator Walk()
    {
        state = NPCState.Walking;
        yield return character.Move(movementPattern[currentPattern]);

        currentPattern = (currentPattern + 1) % movementPattern.Count;
        state = NPCState.Idle;
    }
}

public enum NPCState
{
    Idle, Walking
}