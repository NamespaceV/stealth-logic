using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "Config", menuName = "ScriptableObjects/GameConfigSO", order = 1)]
    public class GameConfigSO : ScriptableObject
    {   
        
        [Header("Map")]
        public GameObject Tile2DPrefab;
        public GameObject FloorTilePrefab;
        public GameObject FloorWaterTilePrefab;
        public GameObject WallTilePrefab;
        public GameObject PlayerPrefab;
        public GameObject EnemyPrefab;
        public Sprite     ExitSprite;
        public GameObject ButtonPrefab;
    }
}