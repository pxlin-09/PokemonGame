using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;

    [SerializeField] PartyScreen partyScreen;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    // Observer design pattern: gameController observes when battleSystem
    // encounters a pokemon and changes state to battle
    public event Action<bool> onBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    // Start is called before the first frame update
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
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
        } else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    public IEnumerator SetupBattle()
    {
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        playerUnit.Setup(playerParty.getHealthyPokemon());
        playerHud.SetData(playerUnit.Pokemon);

        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        PlayerAction();
    }

    // Update is called once per frame
    

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialogBox("Choose and action");
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
            $" used {move.Base.Name}!");

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
            $" used {move.Base.Name}!");

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

            var nextPokemon = playerParty.getHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            } else
            {
                onBattleOver(false);
            }
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
        // temp fix for double display bug ***
        dialogBox.SetDialogBox("Choose an action!");
        dialogBox.EnableMoveSelector(false);
        // ***

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAction--;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0) 
            {
                //Debug.Log("Fight");
                PlayerMove();
            } else if (currentAction == 1) {
                onBattleOver(false);
                //Debug.Log("Bag");
            } else if (currentAction == 2)
            {
                OpenPartyScreen();
            } else if (currentAction == 3)
            {
                onBattleOver(false);
                //Debug.Log("Run");
            }
        } 
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMove--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMove++;
        }

        currentMove = Mathf.Clamp(currentMove, 0, 3);

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
                Debug.Log("NO PP");
            }
            
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }    
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember++;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember--;
        }

        if (currentMember >= playerParty.Pokemons.Count)
        {
            currentMember = 0;
        } else if (currentMember < 0)
        {
            currentMember = playerParty.Pokemons.Count - 1;
        }

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedPokemon = playerParty.Pokemons[currentMember];
            if (selectedPokemon.HP <= 0 || selectedPokemon == playerUnit.Pokemon)
            {
                // Play "cannot sound"
                return;
            }
            partyScreen.gameObject.SetActive(false);
            StartCoroutine(SwitchPokemon(selectedPokemon));
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        dialogBox.EnableActionSelector(false);
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        yield return
            dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Base.Name}!");
        playerUnit.PlayerFaintAnimation();
        yield return new WaitForSeconds(0.7f);
        playerUnit.Setup(newPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"Go {playerUnit.Pokemon.Base.Name}!"));

        StartCoroutine(EnemyMove());
    }
}
