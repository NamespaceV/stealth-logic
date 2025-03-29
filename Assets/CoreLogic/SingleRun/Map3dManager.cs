using Assets.Common.Scripts;
using Assets.Gameplay.Manager;
using Settings;
using UnityEngine;

namespace Gameplay.Manager.SingleRun
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
                    tile.SetFloor3D(floor);
                    var buttonColor = tile.GetButtonColor();
                    if (buttonColor.HasValue)
                    {
                        var button = Object.Instantiate(_config.ButtonPrefab, new Vector3(x, 0.01f, y), Quaternion.identity, floor.gameObject.transform);
                        button.GetComponentInChildren<SpriteRenderer>().color = Wall.FromColor(buttonColor.Value);
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
                        GameObject enemy = Object.Instantiate(_config.EnemyPrefab, tile.Pos, Quaternion.identity);
                        enemy.transform.SetParent(LevelParent.transform);
                        tile.currentObject = enemy;
                        break;
                    case TileOccupierType.HERO:
                        GameObject player = Object.Instantiate(_config.PlayerPrefab, tile.Pos, Quaternion.identity);
                        player.transform.SetParent(LevelParent.transform);
                        tile.currentObject = player;
                        break;
                }
            }
        }
        
        
        private void ProcessWalls(Tile tile)
        {
            SpawnWall3d(tile, Direction.Up, Quaternion.identity);
            SpawnWall3d(tile, Direction.Down, Quaternion.Euler(new(0, 180, 0)));
            SpawnWall3d(tile, Direction.Left, Quaternion.Euler(new(0, -90, 0)));
            SpawnWall3d(tile, Direction.Right, Quaternion.Euler(new(0, 90, 0)));
        }

        private void SpawnWall3d(Tile tile, Direction dir, Quaternion rotation)
        {
            if (tile.HasWall(dir))
            {
                GameObject wall = Object.Instantiate(_config.WallTilePrefab, tile.Pos, rotation);
                if (tile.GetWall(dir).IsExit)
                    wall.GetComponentInChildren<SpriteRenderer>().sprite = _config.ExitSprite;
                wall.transform.SetParent(LevelParent.transform);
            }
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