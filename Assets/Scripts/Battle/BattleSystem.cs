using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn,
    Busy, PartyScreen, BattleOver}

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;

    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] BattleDialogBox dialogBox;

    [SerializeField] PartyScreen partyScreen;

    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    // Observer design pattern: gameController observes when battleSystem
    // encounters a pokemon and changes state to battle
    public event Action<bool> onBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    // Start is called before the first frame update
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        StartCoroutine(SetupBattle());
    }

    // Start is called before the first frame update
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
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
        //dialogBox.EnableActionSelector(false);
        //dialogBox.EnableMoveSelector(false);
        //dialogBox.EnableDialogText(true);
        playerUnit.Clear();
        enemyUnit.Clear();
        playerUnit.IsPlayerUnit = true;
        enemyUnit.IsPlayerUnit = false;
        if (!isTrainerBattle) // wild pokemon encounter
        {
            playerUnit.Setup(playerParty.getHealthyPokemon());
            enemyUnit.Setup(wildPokemon);
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        }
        else // trainer battle
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.gameObject.SetActive(true);
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} has challenged you to a battle!");

            // trainer send out pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.getHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Base.Name}!");

            // player send out pokemon
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.getHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        
        partyScreen.Init();
        
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
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            // Check who goes first

            bool playerGoesFirst = true;
            if (playerMovePriority > enemyMovePriority)
            {
                playerGoesFirst = true;
            } else if (playerMovePriority < enemyMovePriority)
            {
                playerGoesFirst = false;
            } else
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
            
        } else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            // enemy turn
            var enemyMove = enemyUnit.Pokemon.GetRandMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();

        }
    }

    
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        ConditionDetails cond = sourceUnit.Pokemon.OnBeforeTurn();

        if (cond != null)
        {
            if (!cond.move)
            {
                sourceUnit.PlayerHitAnimation(cond.status);

                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return sourceUnit.Hud.UpdateHP();
                yield break;
            }
            
        }

        yield return ShowStatusChanges(sourceUnit.Pokemon);
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}" +
            $"" +
            $" used {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayerAttackAnimation(move, targetUnit);
            yield return new WaitForSeconds(0.5f);
            targetUnit.PlayerHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit, targetUnit, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDmg(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null
                && move.Base.Secondaries.Count > 0
                && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit, targetUnit, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted.");
                targetUnit.PlayerFaintAnimation();

                yield return new WaitForSeconds(1f);

                CheckBattleOver(targetUnit);

            }
        } else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
        }
        

        
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // status can decrease pokemon hp
        Condition status = sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        if (sourceUnit.Pokemon.HpChange)
        {
            sourceUnit.PlayerHitAnimation(status);
        }
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted.");
            sourceUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(1f);

            CheckBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, BattleUnit sourceUnit, BattleUnit targetUnit, MoveTarget moveTarget)
    {
        Pokemon source = sourceUnit.Pokemon;
        Pokemon target = targetUnit.Pokemon;


        // Stats boosting
        if (effects.Boosts != null)
        {
            if (moveTarget== MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
                foreach (var boosts in effects.Boosts)
                {
                    if (boosts.boost > 0)
                    {
                        sourceUnit.PlayStatusChangeAnimation(true);
                    } else
                    {
                        sourceUnit.PlayStatusChangeAnimation(false);
                    }
                }
            }
            else if (moveTarget == MoveTarget.Foe)
            {
                target.ApplyBoosts(effects.Boosts);
                foreach (var boosts in effects.Boosts)
                {
                    if (boosts.boost > 0)
                    {
                        targetUnit.PlayStatusChangeAnimation(true);
                    }
                    else
                    {
                        targetUnit.PlayStatusChangeAnimation(false);
                    }
                }
            }
            
        }

        // Stats condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);

    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        } else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        } else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                enemyUnit.Clear();
                var nextPokemon = trainerParty.getHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(SendNextTrainerPokemon(nextPokemon));
                } else
                {
                    BattleOver(true);
                }
            }
            
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
                prevState = state;
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
                StartCoroutine(RunTurns(BattleAction.Move));
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

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            } else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedPokemon));
            }
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

        state = BattleState.RunningTurn;
        
    }

    IEnumerator SendNextTrainerPokemon(Pokemon nextPokemon)
    {
        state = BattleState.Busy;

        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
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
