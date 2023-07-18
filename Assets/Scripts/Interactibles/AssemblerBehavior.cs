using System.Collections.Generic;
using UnityEngine;
using Models;
using Utils;

public class AssemblerBehavior : MonoBehaviour
{

    [SerializeField] GameObject hologramPrefab;
    [SerializeField] GameObject dominoPrefab;

    private DominoGenerator dominoGenerator;

    private SpriteRenderer dominoSpriteRenderer;

    private Domino currentDomino;
    
    void Start()
    {
        dominoGenerator = FindObjectOfType<DominoGenerator>().GetComponent<DominoGenerator>();

        dominoSpriteRenderer = GetComponentsInChildren<SpriteRenderer>()[1];

        InitDomino();
    }

    private void InitDomino()
    {
        currentDomino = new Domino(DominoUtils.None);
        dominoSpriteRenderer.sprite = dominoGenerator.GenerateDominoSprite(currentDomino, false);
    }

    public bool IsEmpty()
    {
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                if (currentDomino.Blocks[i][j].Exists)
                    return false;
        
        return true;
    }

    public HologramBehavior AddHologram(Domino domino)
    {
        var hologram = Instantiate(hologramPrefab, transform);

        hologram.GetComponent<HologramBehavior>().SetDomino(domino);

        return hologram.GetComponent<HologramBehavior>();
    }

    public bool TryInsertHologram(HologramBehavior hologram)
    {
        if(CanAddHologram(hologram))
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    if (!currentDomino.Blocks[i][j].Exists)
                        currentDomino.Blocks[i][j] = hologram.domino.Blocks[i][j];

            dominoSpriteRenderer.sprite = dominoGenerator.GenerateDominoSprite(currentDomino, false);
            return true;
        }

        return false;
    }

    public GameObject TakeDominoOut()
    {
        var newDomino = Instantiate(dominoPrefab);
        newDomino.transform.position = new Vector2(100, 100);
        currentDomino.isAssembled = true;


        newDomino.GetComponent<DominoBehavior>().domino = currentDomino;

        InitDomino();
        return newDomino;
    }

    private bool CanAddHologram(HologramBehavior hologram)
    {
        
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                if (currentDomino.Blocks[i][j].Exists && hologram.domino.Blocks[i][j].Exists)
                    return false;
        
        return true;
    }
}
