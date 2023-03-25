using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TypeSprites typeSprites;
    [SerializeField] Image type1;
    [SerializeField] Image type2;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color slpColor;
    Dictionary<ConditionID, Color> statusColors;

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
        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor },
            { ConditionID.brn, brnColor },
            { ConditionID.par, parColor },
            { ConditionID.frz, frzColor },
            { ConditionID.slp, slpColor }
        };

        typeSprites = ts;
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        hpBar.SetColor((float)pokemon.HP / pokemon.MaxHp);
        SetStatusText();
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
    public void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        } else
        {
            statusText.text = _pokemon.Status.ID.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.ID];
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
