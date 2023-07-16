using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models;
using Enums;
using Utils;

public class AssemblerBehavior : MonoBehaviour
{
    
    private DominoGenerator dominoGenerator;

    private SpriteRenderer dominoSpriteRenderer;
    private SpriteRenderer hologramSpriteRenderer;

    private Domino currentDomino;
    
    void Start()
    {
        dominoGenerator = FindObjectOfType<DominoGenerator>().GetComponent<DominoGenerator>();

        dominoSpriteRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
        hologramSpriteRenderer = GetComponentsInChildren<SpriteRenderer>()[2];

        currentDomino = new Domino(DominoUtils.None);
        dominoSpriteRenderer.sprite = dominoGenerator.GenerateDominoSprite(currentDomino);
    }
}
