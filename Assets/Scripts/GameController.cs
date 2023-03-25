using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Awake()
    {
        ConditionsDB.Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerController.onEncountered += StartBattle;
        battleSystem.onBattleOver += EndBattle;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        } else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().getRandomPokemon();
        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool playerWin)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
}
