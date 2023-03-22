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
    [SerializeField] MoveCategory category;

    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;

    [SerializeField] GameObject vfxPrefab;
    [SerializeField] GameObject vfxUp;
    [SerializeField] GameObject vfxDown;

    public bool directShot;

    public bool launchToSky;

    public GameObject VfxPrefab { get { return vfxPrefab; } }

    public GameObject VfxUp { get { return vfxUp; } }

    public GameObject VfxDown { get { return vfxDown; } }

    public string Name { get { return _name; } }

    public int PP { get { return pp; } }

    public int Power { get { return power; } }

    public int Accuracy { get { return accuracy; } }

    public PokemonType Type {  get { return type; } }

    public MoveCategory Category { get { return category; } }

    public MoveEffects Effects { get { return effects; } }

    public MoveTarget Target {  get { return target; } }

    public bool DirectShot { get { return directShot; } }

    public bool LaunchToSky { get { return launchToSky; } }

}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts { get { return boosts; } }
};

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
};

public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoveTarget
{
    Foe, Self
}