using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string _name;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] PokemonType type;
    [SerializeField] int pp;
    [SerializeField] int power;
    [SerializeField] int accuracy;

    public string Name { get { return _name; } }

    public int PP { get { return pp; } }

    public int Power { get { return power; } }

    public int Accuracy { get { return accuracy; } }

    public PokemonType Type {  get { return type; } }
}
