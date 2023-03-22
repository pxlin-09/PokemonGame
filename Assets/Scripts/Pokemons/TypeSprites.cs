using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeSprites : MonoBehaviour
{
    [SerializeField] List<Sprite> typeSprites;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Sprite> Sprites
    {
        get {
            return typeSprites;
        }
    }

    public Sprite GetSprite(PokemonType type)
    {
        if (type == PokemonType.None) return null;
        int idx = (int)type - 1;
        return typeSprites[idx];
    }
}
