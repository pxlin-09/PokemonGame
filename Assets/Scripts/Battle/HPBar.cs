using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    string greenHex = "#73D670";
    string yellowHex = "#F3EA6B";
    string redHex = "#C62501";
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHp, float maxHp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;

        while (curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            SetColor(curHp);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
    }

    private void SetColor(float percent)
    {
        string col = "";
        if (0.5 <= percent)
        {
            col = greenHex;
        } else if (0.3 <= percent)
        {
            col = yellowHex;
        } else
        {
            col = redHex;
        }
        Color newCol = Color.green;
        if(ColorUtility.TryParseHtmlString(col, out newCol))
        {
            health.GetComponent<Image>().color = newCol;
        }
    }

    public void ResetColorToGreen()
    {
        Color newCol = Color.green;
        if (ColorUtility.TryParseHtmlString(greenHex, out newCol))
        {
            health.GetComponent<Image>().color = newCol;
        }
    }
}
