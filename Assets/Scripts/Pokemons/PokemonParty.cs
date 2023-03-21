using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }    
    }

    public Pokemon getHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
    }
}
