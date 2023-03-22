using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    
    public List<Move> Moves { get; set; }
    public int HP { get; set; }

    public void Init()
    {
        HP = MaxHp;

        Moves = new List<Move>(); 
        foreach (var move in Base.LearnableMoves)
        {
            if (Moves.Count >= 4) break;
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
        }
    }

    public int MaxHp
    {
        // original:
        // get { return Mathf.FloorToInt((Base.MaxHp * Level) / 70f) + 10; }
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 70f) + 10; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f + 5); }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f + 5); }
    }

    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f + 5); }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f + 5); }
    }

    public DamageDetails TakeDmg(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25)
        {
            critical = 2f;
        }

        float effect = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1)
            * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        float atk = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float def = (move.Base.IsSpecial) ? attacker.SpDefense : attacker.Defense;
        float modifiers = Random.Range(0.85f, 1f) * effect * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) atk/def) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0) { HP = 0; }

        var damageDetails = new DamageDetails()
        {
            Fainted = HP <= 0,
            Critical = critical,
            Effect = effect

        };
        return damageDetails;
    }

    public Move GetRandMove()
    {
        int random = Random.Range(0, Moves.Count);
        return Moves[random];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float Effect { get; set; }
}