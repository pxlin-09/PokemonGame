using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    [SerializeField] Image pokemonDisplay;

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
                pokemonDisplay.sprite = memberSlots[i].Pokemon.Base.FrontSprite;
            } else
            {
                memberSlots[i].GetComponent<Image>().color = Color.white;
            }
        }
    }
}
