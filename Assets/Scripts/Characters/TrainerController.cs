using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject exclamation;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
}
