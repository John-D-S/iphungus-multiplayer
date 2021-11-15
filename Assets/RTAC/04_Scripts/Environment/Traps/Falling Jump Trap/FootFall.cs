using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootFall : MonoBehaviour
{
    [SerializeField] private MeshCollider floor;
    [SerializeField] private Material[] plateMats;
    private readonly int invisibleDuration = 5;
    private Color32[] startColours = new Color32[3];
    private Color32[] endcolours = new Color32[3];
    [SerializeField] private MeshRenderer rend;

    private void Start()
    {

        startColours[0] = new Color32(150, 150, 150, 255);
        startColours[1] = new Color32(150, 150, 150, 255);
        startColours[2] = new Color32(75, 75, 75, 255);

        for (int i = 0; i < endcolours.Length; i++)
        {
            endcolours[i] = Color.clear;
        }
        

        for (int i = 0; i < startColours.Length; i++)
        {
            plateMats[i].color = startColours[i];
        }
    }

    public IEnumerator PlateAction()
    {
        void ChangeColour(Color32[] finalColour)
        {
            for (int i = 0; i < plateMats.Length; i++)
            {
                plateMats[i].color = Color32.Lerp(plateMats[i].color, finalColour[i], Time.fixedDeltaTime);
            }
        }

        while (plateMats[0].color != Color.clear)
        {
            ChangeColour(endcolours);
            yield return new WaitForFixedUpdate();

            if (plateMats[0].color == Color.clear)
            {
                yield return null;
            }
        }

        floor.enabled = false;
        yield return new WaitForSeconds(invisibleDuration);

        while (plateMats[0].color != Color.white)
        {
            ChangeColour(startColours); 
            yield return new WaitForFixedUpdate();
            if (plateMats[0].color == Color.white)
            {
                yield return null;
            }
        }
        
    }
}
    

