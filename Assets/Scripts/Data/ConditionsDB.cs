using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var KVP in Conditions)
        {
            var id = KVP.Key;
            var cond = KVP.Value;
            cond.ID = id;
        }
    }

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
                    if (Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed and cannot move!");
                        return false;
                    }
                    return true;
                }
            }
        },

        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer frozen!");
                        return true;
                    }
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is frozen!");
                    return false;
                }
            }
        },

        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    // Sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"will sleep for {pokemon.StatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping!");
                    return false;
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