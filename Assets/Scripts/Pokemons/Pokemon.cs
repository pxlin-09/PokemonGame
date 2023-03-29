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

    public Move CurrentMove { get; set; }

    public int HP { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }

    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public Condition Status { get; private set; }

    public Condition VolatileStatus { get; private set; }

    public int VolatileStatusTime { get; set; }

    public int StatusTime { get; set; }

    public bool HpChange { get; set; }

    public event System.Action OnStatusChange;

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
        CalculateStats();
        HP = MaxHp;
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
        StatusTime = 0;
        VolatileStatusTime = 0;
        
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f + 5));
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f + 5));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f + 5));
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f + 5));

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // TODO: Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        } else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }
        return statVal;
    }

    public int MaxHp { get; private set; }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
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

        float atk = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float def = (move.Base.Category == MoveCategory.Special) ? attacker.SpDefense : attacker.Defense;
        float modifiers = Random.Range(0.85f, 1f) * effect * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) atk/def) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        Debug.Log($"Dealt dmg: {damage}");
        UpdateHP(damage);

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

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost >= 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} has increased!");
            } else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} has decreased!");
            }
            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public void SetStatus (ConditionID conditionId)
    {
        if (Status != null)
        {
            StatusChanges.Enqueue($"The move has no effect!");
        }
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChange?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null)
        {
            StatusChanges.Enqueue($"The move has no effect!");
        }
        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void UpdateHP(int damage)
    {
        HpChange = true;
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
    }

    public void OnBattleOver()
    { 
        VolatileStatus = null;
        ResetStatBoosts();
    }

    public Condition OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); // Null condition operator
        VolatileStatus?.OnAfterTurn?.Invoke(this);
        return Status;
    }

    public ConditionDetails OnBeforeTurn()
    {
        if (Status?.OnBeforeMove != null)
        {
            ConditionDetails cond =
                new ConditionDetails
                {
                    status = Status,
                    move = Status.OnBeforeMove(this)
                };
            return cond;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            ConditionDetails cond =
                new ConditionDetails
                {
                    status = VolatileStatus,
                    move = VolatileStatus.OnBeforeMove(this)
                };
            return cond;
        }
        return null;
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChange?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float Effect { get; set; }
}

public class ConditionDetails
{
    public Condition status;
    public bool move;
}