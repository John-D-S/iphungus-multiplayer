using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootFall : MonoBehaviour
{
    [SerializeField] private Collider floor;
    private Material[] plateMats;
    [SerializeField] private Material baseMat;
    private readonly int invisibleDuration = 3;
    private Color[] startColours = new Color[3];
    private Color[] endColours = new Color[3];
    private MeshRenderer rend;
    private Color[] tempCol = new Color[3];

    private void Start()
    {
        tempCol[0] = new Color(0, 0, 10, 10);
        tempCol[1] = new Color(3, 3, 3, 10);
        tempCol[2] = new Color(6, 6, 6, 10);
        
        startColours[0] = Color.blue;
        startColours[1] = new Color(0.3f, 0.3f, 0.3f, 1);
        startColours[2] = new Color(0.588f, 0.588f, 0.588f, 1);

        for (int i = 0; i < tempCol.Length; i++)
        {
            tempCol[i].r /= 255f;
            tempCol[i].g /= 255f;
            tempCol[i].b /= 255f;
            tempCol[i].a /= 255f;
        }
        
        plateMats = new Material[3];

        rend = GetComponent<MeshRenderer>();

        for (int i = 0; i < plateMats.Length; i++)
        {
            plateMats[i] = new Material(baseMat);
            endColours[i] = Color.clear;
            plateMats[i].color = startColours[i];
        }
        rend.materials = plateMats;
    }
    
    public IEnumerator PlateAction()
    {
        yield return FadePlate(endColours, true);

        floor.enabled = false;
        yield return new WaitForSeconds(invisibleDuration);

        yield return FadePlate(startColours, false);
        floor.enabled = true;
    }

    private void ChangeColour(bool down)
    {
        if (down)
        {
            for (int i = 0; i < plateMats.Length; i++)
            {
                plateMats[i].color -= tempCol[i];
            }
        }
        else
        {
            for (int i = 0; i < plateMats.Length; i++)
            {
                plateMats[i].color += tempCol[i];
            }
        }
    }

    private IEnumerator FadePlate(Color[] finalColour, bool down)
    {
        if (down)
        {
            
            while (plateMats[0].color.a >= finalColour[0].a)
            {
                ChangeColour(down);
                yield return new WaitForFixedUpdate();

                if (plateMats[0].color.a <= finalColour[0].a)
                {
                    yield return null;
                }
            }
        }
        else
        {
            while (plateMats[1].color.b <= finalColour[1].b)
            {
                ChangeColour(down);
                yield return new WaitForFixedUpdate();
                    
                //Debug.Log(plateMats[1].color.b - finalColour[1].b);

                if (plateMats[1].color.b >= finalColour[1].b)
                {
                    yield return null;
                }
            }
        }

    }

}
    

