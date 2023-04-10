using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask longGrassLayer;
    [SerializeField] LayerMask playerLayer;

    public static GameLayers i { get; private set; }

    public LayerMask SolidObjectLayer
    {
        get => solidObjectLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask LongGrassLayer
    {
        get => longGrassLayer;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    private void Awake()
    {
        i = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
