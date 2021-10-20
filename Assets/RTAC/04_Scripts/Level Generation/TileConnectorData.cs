using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltarChase.LevelGen
{
    public enum TileShape
    {
        Cap,
        L,
        I,
        T,
        FourWay
    }
    
    [System.Serializable]
    public class TileConnectorData
    {
        [SerializeField, Tooltip("What Shape Is This Tile?")] private TileShape tileShape;
        public TileShape ThisTileShape => tileShape;

        public TileConnectorData(TileShape _tileShape)
        {
            tileShape = _tileShape;
        }
        
        public int NumberOfConnections()
        {
            switch(tileShape)
            {
                case TileShape.Cap:
                    return 1;
                case TileShape.L:
                    return 2;
                case TileShape.I:
                    return 2;
                case TileShape.T:
                    return 3;
                case TileShape.FourWay:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public List<Vector3Int> DefaultConnectorPositions()
        {
            List<Vector3Int> returnVal = new List<Vector3Int>();
            switch(tileShape)
            {
                case TileShape.Cap:
                    returnVal.Add(Vector3Int.forward);
                    break;
                case TileShape.L:
                    returnVal.Add(Vector3Int.forward);
                    returnVal.Add(Vector3Int.right);
                    break;
                case TileShape.I:
                    returnVal.Add(Vector3Int.forward);
                    returnVal.Add(Vector3Int.back);
                    break;
                case TileShape.T:
                    returnVal.Add(Vector3Int.forward);
                    returnVal.Add(Vector3Int.right);
                    returnVal.Add(Vector3Int.back);
                    break;
                case TileShape.FourWay:
                    returnVal.Add(Vector3Int.forward);
                    returnVal.Add(Vector3Int.right);
                    returnVal.Add(Vector3Int.back);
                    returnVal.Add(Vector3Int.left);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return returnVal;
        }
    }
}
