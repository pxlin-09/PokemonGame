using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")] 
public class PokemonBase : ScriptableObject
{

    [SerializeField] string _name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;



    // using c# properties
    public string Name { get { return _name; } }

    public string Description { get { return description; } }

    public int MaxHp { get { return maxHp; } }

    public int Attack { get { return attack; } }

    public int Defense { get { return defense; } }

    public int SpAttack { get { return spAttack; } }

    public int SpDefense { get { return SpDefense; } }

    public int Speed { get { return speed; } }

    public Sprite FrontSprite { get { return frontSprite; } }

    public Sprite BackSprite { get { return backSprite; } }

    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base { get { return moveBase; } }

    public int Level { get { return level; } }  
}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public class TypeChart
{
    float[][] chart =
    {
        //           nor fir wat ele gra ice fig poi gro fly psy bug roc gho dra 
        new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 0f, 1f}, // normal
        new float[] { 1f,.5f,.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f,.5f, 1f,.5f}, // fire
        new float[] { 1f, 2f,.5f, 1f,.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f,.5f}, // water
        new float[] { 1f, 1f, 2f,.5f,.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f,.5f}, // electric
        new float[] { 1f,.5f, 2f, 1f,.5f, 1f, 1f,.5f, 2f,.5f, 1f,.5f, 2f, 1f,.5f}, // grass
        new float[] { 1f,.5f,.5f, 1f, 2f,.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f}, // ice
        new float[] { 2f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 1f,.5f,.5f,.5f, 2f, 0f, 1f}, // fighting
        new float[] { 1f, 1f, 1f, 1f, 2f, 1f, 1f,.5f,.5f, 1f, 1f, 1f,.5f,.5f, 1f}, // poison
        new float[] { 1f, 2f, 1f, 2f,.5f, 1f, 1f, 2f, 1f, 0f, 1f,.5f, 2f, 1f, 1f}, // ground
        new float[] { 1f, 1f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f,.5f, 1f, 1f}, // flying
        new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f,.5f, 1f, 1f, 1f, 1f}, // psychic
        new float[] { 1f,.5f, 1f, 1f, 2f, 1f,.5f,.5f, 1f,.5f, 2f, 1f, 1f,.5f, 1f}, // bug
        new float[] { 1f, 2f, 1f, 1f, 1f, 2f,.5f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f}, // rock
        new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f}, // ghost
        new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f}  // dragon
    };
}