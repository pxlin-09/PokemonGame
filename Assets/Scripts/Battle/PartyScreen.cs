using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    [SerializeField] Image pokemonDisplay;

    [SerializeField] TextMeshProUGUI hp;
    [SerializeField] TextMeshProUGUI att;
    [SerializeField] TextMeshProUGUI def;
    [SerializeField] TextMeshProUGUI spAtt;
    [SerializeField] TextMeshProUGUI spDef;
    [SerializeField] TextMeshProUGUI speed;

    string selectedColor = "#BABABA";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        pokemonDisplay.sprite = pokemons[0].Base.FrontSprite;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].SetData(pokemons[i]);
            } else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                Color newCol = Color.green;
                if (ColorUtility.TryParseHtmlString(selectedColor, out newCol))
                {
                    memberSlots[i].GetComponent<Image>().color = newCol;
                }
                SetStats(memberSlots[i].Pokemon);
            } else
            {
                memberSlots[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    private void SetStats(Pokemon pokemon)
    {
        pokemonDisplay.sprite = pokemon.Base.FrontSprite;
        hp.text = $"HP: {pokemon.HP}";
        att.text = $"Att: {pokemon.Attack}";
        def.text = $"Def: {pokemon.Defense}";
        spAtt.text = $"SpAtt: {pokemon.SpAttack}";
        spDef.text = $"SpDef: {pokemon.SpDefense}";
        speed.text = $"Speed: {pokemon.Speed}";
    }
}
