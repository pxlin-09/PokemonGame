using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject fov;
    [SerializeField] GameObject exclamation;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        // show exclaimation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(.75f);
        exclamation.SetActive(false);
        yield return new WaitForSeconds(.25f);

        // walk to player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        yield return character.Move(moveVec);

        // show dialog
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
        {
            Debug.Log("Start battle!");
        }));
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Left)
        {
            angle = 270f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirection.Down)
        {
            angle = 0f;
        }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
