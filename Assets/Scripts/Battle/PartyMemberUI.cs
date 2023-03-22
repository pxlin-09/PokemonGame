using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TypeSprites typeSprites;
    [SerializeField] Image type1;
    [SerializeField] Image type2;

    Pokemon _pokemon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(Pokemon pokemon, TypeSprites ts)
    {
        typeSprites = ts;
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        hpBar.SetColor((float)pokemon.HP / pokemon.MaxHp);

        Sprite type1Sprite = typeSprites.GetSprite(_pokemon.Base.Type1);
        Sprite type2Sprite = typeSprites.GetSprite(_pokemon.Base.Type2);

        if (type1Sprite != null)
        {
            type1.sprite = type1Sprite;
        } else
        {
            type1.gameObject.SetActive(false);
        }

        if (type2Sprite != null)
        {
            type2.sprite = type2Sprite;
        }
        else
        {
            type2.gameObject.SetActive(false);
        }
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp,
            (float)_pokemon.MaxHp);
    }

    public Pokemon Pokemon
    {
        get
        {
            return _pokemon;
        }
    }
}
