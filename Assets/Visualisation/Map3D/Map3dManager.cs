using CoreLogic.Grid;
using DataFormats;
using Settings;
using UnityEngine;
using Visualisation.TileVisualisation;

namespace Visualisation.Map3D
{
    public class Map3dManager
    {
        private GameManager _mgr;

        private GameObject LevelParent;

        private readonly GameConfigSO _config;

        public Map3dManager(
            GameManager mgr,
            GameObject levelParent,
            GameConfigSO config)
        {
            _mgr = mgr;
            LevelParent = levelParent;
            _config = config;
        }

        public void Generate()
        {
            Clear();

            var grid = _mgr.GetGrid();
            for (int y = 0; y < _mgr.mapHeight; y++)
            {
                for (int x = 0; x < _mgr.mapWidth; x++)
                {
                    var tile = grid.GetTile(new Vector2Int(x, y));
                    var prefab = tile.FloorType == TileFloorType.WATER ? _config.FloorWaterTilePrefab : _config.FloorTilePrefab;
                    GameObject floor = Object.Instantiate(prefab, new Vector3(x, 0, y), Quaternion.identity);
                    floor.transform.SetParent(LevelParent.transform);
                    tile._tile3d.SetFloor(floor);
                    tile._tile3d.SetSelected(tile.IsSelected);
                    var buttonColor = tile.GetButtonColor();
                    if (buttonColor.HasValue)
                    {
                        var button = Object.Instantiate(_config.ButtonPrefab, new Vector3(x, 0.01f, y), Quaternion.identity, floor.gameObject.transform);
                        button.GetComponentInChildren<SpriteRenderer>().color = Wall2d.FromColor(buttonColor.Value);
                    }
                }
            }

            foreach (var tile in grid)
            {
                ProcessWalls(tile);

                switch (tile.GetOccupierTileType())
                {
                    case TileOccupierType.EMPTY:
                        break;
                    case TileOccupierType.ENEMY:
                        SpawnOccupier(tile._tile3d, _config.EnemyPrefab);
                        break;
                    case TileOccupierType.HERO:
                        SpawnOccupier(tile._tile3d, _config.PlayerPrefab);
                        break;
                }
            }
        }

        private void SpawnOccupier(Tile3d tileTile3d, GameObject prefab)
        {
            GameObject occupier = Object.Instantiate(prefab, tileTile3d.Pos, Quaternion.identity);
            occupier.transform.SetParent(LevelParent.transform);
            tileTile3d._occupier3d = occupier;
        }

        private void ProcessWalls(Tile2d tile2d)
        {
            SpawnWall3d(tile2d, Direction.Up, Quaternion.identity);
            SpawnWall3d(tile2d, Direction.Down, Quaternion.Euler(new(0, 180, 0)));
            SpawnWall3d(tile2d, Direction.Left, Quaternion.Euler(new(0, -90, 0)));
            SpawnWall3d(tile2d, Direction.Right, Quaternion.Euler(new(0, 90, 0)));
        }

        private void SpawnWall3d(Tile2d tile2d, Direction dir, Quaternion rotation)
        {
            if (!tile2d.HasWall(dir)) { return; }
            
            GameObject wall = Object.Instantiate(_config.WallTilePrefab, tile2d._tile3d.Pos, rotation);
            if (tile2d.GetWall(dir).IsExit)
            {
                wall.GetComponentInChildren<SpriteRenderer>().sprite = _config.ExitSprite;
            }
            wall.transform.SetParent(LevelParent.transform);
        }
        
        public void Clear()
        {
            foreach (Transform child in LevelParent.transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        public void Hide()
        {
            LevelParent.SetActive(false);
        }

        public void Show()
        {
            LevelParent.SetActive(true);
        }
    }
}