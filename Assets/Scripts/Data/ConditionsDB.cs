using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    int damage = Mathf.Max(1, pokemon.MaxHp/8);
                    pokemon.UpdateHP(damage);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt due to poison!");
                }
            }
        },

        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    int damage = Mathf.Max(1, pokemon.MaxHp/16);
                    pokemon.UpdateHP(damage);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt due to burn!");
                }
            }
        },

        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyze",
                StartMessage = "has been paralyzed!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(0,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed and cannot move!");
                        return false;
                    }
                    return true;
                }
            }
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}