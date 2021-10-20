using Mirror;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Random = UnityEngine.Random;

namespace AltarChase.LevelGen
{
    public class LevelGenerator : NetworkBehaviour
    {
        [SerializeField] private float tileSize = 10;
        [SerializeField] private int numerOfTiles = 100;
        [SerializeField] private List<LevelTile> levelTiles;

        //use network server.spawn for instantiation.
        //all modules should have a network identity and all module prefabs should be in registered spawnable prefabs in the network manager. 
        private Dictionary<Vector3Int, TempLevelTileData> levelTilesByGridPos = new Dictionary<Vector3Int, TempLevelTileData>();
        
        private Dictionary<TileShape, List<LevelTile>> levelTilesByConnectorData = new Dictionary<TileShape, List<LevelTile>>();

        private List<GameObject> spawnedTiles = new List<GameObject>();

    #region GenerateTiles
        
        private void GenerateTiles()
        {
            if(levelTiles == null || levelTiles.Count == 0)
            {
                Debug.Log("No level tiles to spawn from.");
                return;
            }
            
            //add all the leveltiles in levelTiles to the list of available tempLevelTileDatas
            foreach(LevelTile levelTile in levelTiles)
            {
                if(!levelTilesByConnectorData.ContainsKey(levelTile.ConnectorData.ThisTileShape))
                    levelTilesByConnectorData[levelTile.ConnectorData.ThisTileShape] = new List<LevelTile>();
                levelTilesByConnectorData[levelTile.ConnectorData.ThisTileShape].Add(levelTile);
            }

            List<TileConnectorData> availableConnectorDatas = new List<TileConnectorData>();
            foreach(TileShape tileShape in Enum.GetValues(typeof(TileShape)))
            {
                if(levelTilesByConnectorData.ContainsKey(tileShape))
                    availableConnectorDatas.Add(new TileConnectorData(tileShape));
            }
            
            Queue<Vector3Int> openPositions = new Queue<Vector3Int>();
            List<Vector3Int> closedPositions = new List<Vector3Int>();

            //set the tiletype of the tile at 000 to the one with the most avilable connections out of availableConnectorDatas.
            levelTilesByGridPos[Vector3Int.zero] = new TempLevelTileData(availableConnectorDatas[availableConnectorDatas.Count - 1], Vector3Int.zero, 0);
            closedPositions.Add(Vector3Int.zero);
            foreach(Vector3Int connectorPosition in levelTilesByGridPos[closedPositions[0]].ConnectorPositionsOnGrid())
                openPositions.Enqueue(connectorPosition);
            
            //this loop adds all the tiles that make up the bulk of the maze until it reaches all the tiles that will end up being placed
            while(closedPositions.Count + openPositions.Count < numerOfTiles)
            {
                Vector3Int positionToFill = openPositions.Dequeue();
                //add all available connector datas except for the first one (which should be the cap) to this new list
                List<TileConnectorData> tileDatasToTryPlaceInOrder = availableConnectorDatas.GetRange(1, availableConnectorDatas.Count - 1);
                //shuffle the list so that it will try to place down a random tile in the given position
                tileDatasToTryPlaceInOrder = tileDatasToTryPlaceInOrder.OrderBy(x => Guid.NewGuid()).ToList();
                //add a cap tile at the end so that if no other tile can be placed here, a cap tile can be.
                tileDatasToTryPlaceInOrder.Add(availableConnectorDatas[0]);

                //initialise the new tile that will be placed.
                TempLevelTileData newTile = new TempLevelTileData(new TileConnectorData(TileShape.FourWay), positionToFill, 0);
                //this is the loop that runs over all the all the tile types and rotations until it finds one that fits in the open space.
                foreach(TileConnectorData tileConnectorData in tileDatasToTryPlaceInOrder)
                {
                    bool tilePlaced = false;
                    //set the connector data to tileConnectorData
                    newTile.connectorData = tileConnectorData;
                    //give the tile rotation a random offset to eliminate too much of the same rotation in tile placement.
                    int randomRotOffset = Random.Range(0, 4);
                    for(int j = 0; j < 4; j++)
                    {
                        newTile.quaterRots = (randomRotOffset + j) % 4;
                        if(newTile.TileFitsInPosition(levelTilesByGridPos))
                        {
                            //set the tile at positionToFill to newTile
                            levelTilesByGridPos[positionToFill] = newTile;
                            //add the new tile's position to the list of closed positions.
                            closedPositions.Add(positionToFill);
                            //when the tile is placed, set this bool to true so that the foreach loop will not need to continue
                            tilePlaced = true;
                        }
                        else
                            continue;
                        break;
                    }

                    //if the tile has been placed, break this loop
                    if(tilePlaced)
                        break;
                }
                
                foreach(Vector3Int closedPosition in closedPositions)
                    foreach(Vector3Int connectorPosition in levelTilesByGridPos[closedPosition].ConnectorPositionsOnGrid())
                        //if the connectorPosition is not already in the list of open or closed positions, add it to the open positions lis
                        if(!openPositions.Contains(connectorPosition) && !closedPositions.Contains(connectorPosition))
                            openPositions.Enqueue(connectorPosition);
            }
            
            //this loop closes off all the open tiles left by the last loop
            while(openPositions.Count > 0)
            {
                Vector3Int positionToFill = openPositions.Dequeue();
                //add all available connector datas in order of the number of connectors each one has
                List<TileConnectorData> tileDatasToTryPlaceInOrder = availableConnectorDatas.OrderBy(x => x.NumberOfConnections()).ToList();

                //initialise the new tile that will be placed.
                TempLevelTileData newTile = new TempLevelTileData(new TileConnectorData(TileShape.Cap), positionToFill, 0);
                //this is the loop that runs over all the all the tile types and rotations until it finds one that fits in the open space.
                foreach(TileConnectorData tileConnectorData in tileDatasToTryPlaceInOrder)
                {
                    bool tilePlaced = false;
                    //set the connector data to tileConnectorData
                    newTile.connectorData = tileConnectorData;
                    //give the tile rotation a random offset to eliminate too much of the same rotation in tile placement.
                    int randomRotOffset = Random.Range(0, 4);
                    for(int j = 0; j < 4; j++)
                    {
                        newTile.quaterRots = (randomRotOffset + j) % 4;
                        if(newTile.TileFitsInPositionWithNoEmptyConnectors(levelTilesByGridPos))
                        {
                            //set the tile at positionToFill to newTile
                            levelTilesByGridPos[positionToFill] = newTile;
                            //add the new tile's position to the list of closed positions.
                            closedPositions.Add(positionToFill);
                            //when the tile is placed, set this bool to true so that the foreach loop will not need to continue
                            tilePlaced = true;
                        }
                        else
                            continue;
                        break;
                    }

                    //if the tile has been placed, break this loop
                    if(tilePlaced)
                        break;
                }
            }

            //set the gameobject of each TempLevelTileData to a be a random one with the appropriate connectionData
            foreach(KeyValuePair<Vector3Int,TempLevelTileData> tileByGridPos in levelTilesByGridPos)
            {
                List<LevelTile> possibleTiles = levelTilesByConnectorData[tileByGridPos.Value.connectorData.ThisTileShape];
                tileByGridPos.Value.tileGameObject = possibleTiles[Random.Range(0, possibleTiles.Count)].gameObject;
            }
        }
    #endregion

        private void PlaceTilesOffline()
        {
            foreach(KeyValuePair<Vector3Int,TempLevelTileData> tileByGridPos in levelTilesByGridPos)
            {
                TempLevelTileData tileToPlace = tileByGridPos.Value;
                Vector3 position = transform.position + tileSize * (Vector3)tileByGridPos.Key;
                Quaternion rotation = Quaternion.Euler(0, tileToPlace.quaterRots * 90, 0);
                Instantiate(tileToPlace.tileGameObject, position, rotation, transform);
            }
        }

        private void ClearTiles()
        {
            levelTilesByGridPos.Clear();
            levelTilesByConnectorData.Clear();
            foreach(GameObject spawnedTile in spawnedTiles)
            {
                NetworkServer.Destroy(spawnedTile);
            }
            spawnedTiles.Clear();
        }
        
        private void PlaceTilesOnline()
        {
            foreach(KeyValuePair<Vector3Int,TempLevelTileData> tileByGridPos in levelTilesByGridPos)
            {
                TempLevelTileData tileToPlace = tileByGridPos.Value;
                Vector3 position = transform.position + tileSize * (Vector3)tileByGridPos.Key;
                Quaternion rotation = Quaternion.Euler(0, tileToPlace.quaterRots * 90, 0);
                GameObject tileGO = Instantiate(tileToPlace.tileGameObject, position, rotation, transform);
                spawnedTiles.Add(tileGO);
                NetworkServer.Spawn(tileGO);
            }
        }

        public void RegenerateLevelOnline()
        {
            ClearTiles();
            GenerateTiles();
            PlaceTilesOnline();
        }

        public override void OnStartServer()
        {
            RegenerateLevelOnline();
        }
    }    
}