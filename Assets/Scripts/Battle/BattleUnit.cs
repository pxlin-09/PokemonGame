using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;

public class BattleUnit : MonoBehaviour
{

    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    Image img;
    Vector3 origin;
    Color originCol;

    private void Awake()
    {
        img = GetComponent<Image>();
        origin = img.transform.localPosition;
        originCol = img.color;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            img.sprite = Pokemon.Base.BackSprite;
        } else {
            img.sprite = Pokemon.Base.FrontSprite;
        }
        img.color = originCol;
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

    public void PlayerAttackAnimation(Move move, Transform target)
    {
        var seq = DOTween.Sequence();
        if (isPlayerUnit)
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x + 50f, 0.25f));
        } else
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x - 50f, 0.25f));
        }
        GameObject vfx = null;
        if (move.Base.VfxPrefab != null)
        {
            vfx = Instantiate(move.Base.VfxPrefab, img.transform.position, Quaternion.identity);
            rotateVfx(vfx, target);
            Vector3 vec = new Vector3(target.position.x, target.position.y);
            seq.Append(vfx.transform.DOLocalMove(vec, 0.45f));
            seq.AppendCallback(() => Destroy(vfx));
        }
        seq.Append(img.transform.DOLocalMoveX(origin.x, 0.25f));
        
    }

    private void rotateVfx(GameObject vfx, Transform target)
    {
        Vector2 dir = target.position - vfx.transform.position;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
    }

    public void PlayerHitAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(img.DOColor(Color.gray, 0.1f));
        seq.Append(img.DOColor(originCol, 0.1f));
    }

    public void PlayerFaintAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(img.transform.DOLocalMoveY(origin.y - 150f, 0.25f));
        seq.Join(img.DOFade(0f, 0.25f));
    }
}
