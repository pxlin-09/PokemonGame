using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

public class BattleUnit : MonoBehaviour
{

    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    [SerializeField] StatusEffects statusEffects;


    public Pokemon Pokemon { get; set; }

    public bool IsPlayerUnit { get; set; }

    public BattleHud Hud { get { return hud; } }

    Image img;
    Vector3 origin;
    Vector3 absOrigin;
    Color originCol;

    private void Awake()
    {
        img = GetComponent<Image>();
        origin = img.transform.localPosition;
        absOrigin = img.transform.position;
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
        hud.SetData(pokemon);
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

    public void PlayerAttackAnimation(Move move, BattleUnit unit)
    {
        var seq = DOTween.Sequence();
        if (isPlayerUnit)
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x + 50f, 0.25f));
        } else
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x - 50f, 0.25f));
        }
        
        if (move.Base.DirectShot)
        {
            Transform target = unit.transform;
            GameObject vfx = Instantiate(move.Base.VfxPrefab, img.transform.position, Quaternion.identity);
            rotateVfx(vfx, target);
            Vector3 vec = new Vector3(target.position.x, target.position.y);
            seq.Join(vfx.transform.DOLocalMove(vec, 0.45f));
            seq.AppendCallback(() => Destroy(vfx));
            seq.Append(img.transform.DOLocalMoveX(origin.x, 0.25f));
        } else if (move.Base.LaunchToSky)
        {
            //Debug.Log("Launch to sky!");
            Vector3 downPos = new Vector3(unit.img.transform.position.x,
                unit.img.transform.position.y+10f,
                unit.img.transform.position.z);
            GameObject vfxUp = Instantiate(move.Base.VfxUp, img.transform.position, Quaternion.identity);
            GameObject vfxDown = Instantiate(move.Base.VfxDown, downPos, Quaternion.identity);
            seq.Join(vfxUp.transform.DOLocalMoveY(img.transform.position.y+10f, 0.45f));
            seq.AppendCallback(() => Destroy(vfxUp));
            seq.Append(img.transform.DOLocalMoveX(origin.x, 0.25f));
            seq.Append(vfxDown.transform.DOLocalMoveY(unit.img.transform.position.y, 0.45f));
            seq.AppendCallback(() => Destroy(vfxDown));
        } else
        {
            seq.Append(img.transform.DOLocalMoveX(origin.x, 0.25f));
        }
        
        
    }

    public void PlayStatusChangeAnimation(bool increase)
    {
        var seq = DOTween.Sequence();
        Vector3 start;
        GameObject arrow = null;
        float end;
        Debug.Log($"x coord is:{absOrigin.x}");
        Debug.Log($"y coord is:{absOrigin.y}");
        if (increase)
        {
            
            end = absOrigin.y + 1f;
            if (isPlayerUnit)
            {
                start = new Vector3(absOrigin.x - 2f,
                    absOrigin.y - 0.5f,
                    absOrigin.z);
                
            } else
            {
                start = new Vector3(absOrigin.x + 2f,
                    absOrigin.y - 0.5f,
                    absOrigin.z);
                
            }
            arrow = Instantiate(statusEffects.UpArrow, start, Quaternion.identity);
        } else
        {
            end = absOrigin.y - 0.5f;
            if (isPlayerUnit)
            {
                start = new Vector3(absOrigin.x - 1.5f,
                    absOrigin.y + 1f,
                    absOrigin.z);
            } else
            {
                start = new Vector3(absOrigin.x + 1.5f,
                    absOrigin.y + 1f,
                    absOrigin.z);
                
            }
            arrow = Instantiate(statusEffects.DownArrow, start, Quaternion.identity);
            
        }
        seq.Append(arrow.transform.DOLocalMoveY(end, 0.7f));
        seq.AppendCallback(() => Destroy(arrow));
    }

    private void rotateVfx(GameObject vfx, Transform target)
    {
        Vector2 dir = target.position - vfx.transform.position;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
    }

    public void PlayerHitAnimation(Condition status = null)
    {
        var seq = DOTween.Sequence();
        if (status == null)
        {
            seq.Append(img.DOColor(Color.gray, 0.1f));
        }
        else if (status.Name == "Poison")
        {
            seq.Append(img.DOColor(Color.magenta, 0.1f));
        }
        else if (status.Name == "Burn")
        {
            seq.Append(img.DOColor(Color.red, 0.1f));
        }
        else if (status.Name == "Paralyze")
        {
            seq.Append(img.DOColor(Color.yellow, 0.1f));
            Shake(seq);
        }
        else if (status.Name == "Freeze")
        {
            seq.Append(img.DOColor(Color.blue, 0.1f));
            Shake(seq);
        }
        else if (status.Name == "Sleep")
        {
            seq.Append(img.DOColor(Color.gray, 0.1f));
            Shake(seq);
        }
        
        seq.Append(img.DOColor(originCol, 0.1f));
    }

    private void Shake(DG.Tweening.Sequence seq)
    {
        seq.Append(img.transform.DOLocalMoveX(origin.x + 5f, 0.06f));
        seq.Append(img.transform.DOLocalMoveX(origin.x - 5f, 0.06f));
        seq.Append(img.transform.DOLocalMoveX(origin.x + 5f, 0.06f));
        seq.Append(img.transform.DOLocalMoveX(origin.x - 5f, 0.06f));
        seq.Append(img.transform.DOLocalMoveX(origin.x, 0.03f));
    }

    public void PlayerFaintAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(img.transform.DOLocalMoveY(origin.y - 150f, 0.25f));
        seq.Join(img.DOFade(0f, 0.25f));
    }
}
