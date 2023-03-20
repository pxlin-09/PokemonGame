using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{

    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    Image img;
    Vector3 origin;

    private void Awake()
    {
        img = GetComponent<Image>();
        origin = img.transform.localPosition;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);
        if (isPlayerUnit)
        {
            img.sprite = Pokemon.Base.BackSprite;
        } else {
            img.sprite = Pokemon.Base.FrontSprite;
        }
        PlayerEnterAnimation();
    }

    public void PlayerEnterAnimation()
    {
        if (isPlayerUnit)
        {
            img.transform.localPosition = new Vector3(-500f, origin.y);
        } else
        {
            img.transform.localPosition = new Vector3(500f, origin.y);
        }

        img.transform.DOLocalMoveX(origin.x, 0.7f);
    }

    public void PlayerAttackAnimation()
    {
        var seq = DOTween.Sequence();
        if (isPlayerUnit)
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x + 50f, 0.25f));
        } else
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x - 50f, 0.25f));
        }
        seq.Append(img.transform.DOLocalMoveX(origin.x, 0.25f));
    }
}
