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
    [SerializeField] GameObject vfxPrefab;

    [SerializeField] GameObject vfxUp;
    [SerializeField] GameObject vfxDown;

    public GameObject VfxPrefab { get { return vfxPrefab; } }

    public GameObject VfxUp { get { return vfxUp; } }

    public GameObject VfxDown { get { return vfxDown; } }

    public bool directShot;

    public bool launchToSky;

    public string Name { get { return _name; } }

    public int PP { get { return pp; } }

    public int Power { get { return power; } }

    public int Accuracy { get { return accuracy; } }

    public PokemonType Type {  get { return type; } }

    public bool DirectShot { get { return directShot; } }

    public bool LaunchToSky { get { return launchToSky; } }

    public bool IsSpecial
    {
        get
        {
            if (type == PokemonType.Fire ||
                type == PokemonType.Water ||
                type == PokemonType.Grass ||
                type == PokemonType.Ice ||
                type == PokemonType.Electric ||
                type == PokemonType.Dragon)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
