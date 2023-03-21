using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;

    PokemonParty pokemonParty;
    Pokemon wildPokemon;

    // Observer design pattern: gameController observes when battleSystem
    // encounters a pokemon and changes state to battle
    public event Action<bool> onBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    // Start is called before the first frame update
    public void StartBattle(PokemonParty pokemonParty, Pokemon wildPokemon)
    {
        this.pokemonParty = pokemonParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        } else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(pokemonParty.getFirstNotFaintedPokemon());
        playerHud.SetData(playerUnit.Pokemon);

        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        PlayerAction();
    }

    // Update is called once per frame
    

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose and action"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}" +
            $"" +
            $" used {move.Base.Name}");

        playerUnit.PlayerAttackAnimation(move, enemyUnit.transform);
        yield return new WaitForSeconds(0.5f);
        enemyUnit.PlayerHitAnimation();
        var damageDetails = enemyUnit.Pokemon.TakeDmg(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted.");
            enemyUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(1f);
            onBattleOver(true);
        } else
        {
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}" +
            $"" +
            $" used {move.Base.Name}");

        enemyUnit.PlayerAttackAnimation(move, playerUnit.transform);
        yield return new WaitForSeconds(0.5f);
        playerUnit.PlayerHitAnimation();
        var damageDetails = playerUnit.Pokemon.TakeDmg(move, playerUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted.");
            playerUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(1f);
            onBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }

        if (damageDetails.Effect > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }

        if (damageDetails.Effect < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1) ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0) --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0) 
            {
                //Debug.Log("Fight");
                PlayerMove();
            } else {
                onBattleOver(false);
                //Debug.Log("Run");
            }
        } 
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove + 1 < playerUnit.Pokemon.Moves.Count)
            {
                currentMove++;
            }
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove - 1 >= 0)
            {
                currentMove--;
            }
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove + 2 < playerUnit.Pokemon.Moves.Count)
            {
                currentMove += 2;
            }
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove - 2 >= 0)
            {
                currentMove-=2;
            }
        }

        dialogBox.UpdateMoveSelection(currentMove,
            playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (playerUnit.Pokemon.Moves[currentMove].PP != 0)
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(PerformPlayerMove());
            } else
            {
                Debug.Log("HERE");
            }
            
        }
    }
}
