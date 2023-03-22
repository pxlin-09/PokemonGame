using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove,
    Busy, PartyScreen, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;

    [SerializeField] BattleUnit enemyUnit;

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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        } else if (state == BattleState.MoveSelection)
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
        playerUnit.IsPlayerUnit = true;

        enemyUnit.Setup(wildPokemon);
        enemyUnit.IsPlayerUnit = false;

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        ActionSelection();
    }

    // Update is called once per frame
    

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialogBox("Choose and action");
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        onBattleOver(won);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var move = enemyUnit.Pokemon.GetRandMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
        
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}" +
            $"" +
            $" used {move.Base.Name}!");

        sourceUnit.PlayerAttackAnimation(move, targetUnit);
        yield return new WaitForSeconds(0.5f);
        targetUnit.PlayerHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            var effects = move.Base.Effects;
            if (effects.Boosts != null)
            {
                if (move.Base.Target == MoveTarget.Self)
                {
                    sourceUnit.Pokemon.ApplyBoosts(effects.Boosts);
                } else if (move.Base.Target == MoveTarget.Foe)
                {
                    targetUnit.Pokemon.ApplyBoosts(effects.Boosts);
                }
                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return ShowStatusChanges(targetUnit.Pokemon);
            }
        } else
        {
            var damageDetails = targetUnit.Pokemon.TakeDmg(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        
        if (targetUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted.");
            targetUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(1f);

            CheckBattleOver(targetUnit);
        }
    }

    void CheckBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.getHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        } else
        {
            BattleOver(true);
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
                MoveSelection();
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

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove,
            playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (playerUnit.Pokemon.Moves[currentMove].PP != 0)
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(PlayerMove());
            } else
            {
                Debug.Log("NO PP");
            }
            
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
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
            ActionSelection();
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
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return
                dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(0.7f);
        }
        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"Go {playerUnit.Pokemon.Base.Name}!"));

        StartCoroutine(EnemyMove());
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
}
