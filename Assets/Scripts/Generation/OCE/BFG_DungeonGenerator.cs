using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultNamespace.Enums;
using UnityEngine;
using LL_Unity_Utils.Generic;
using NaughtyAttributes;

namespace DefaultNamespace.Generation.OCE
{
    public class BFG_DungeonGenerator : MonoBehaviour
    {
        ObjectGrid<ERoomTypes> grid;

        readonly int generationWidth = 10;
        readonly int generationHeight = 10;
        readonly int minRoomCount = 2;
        readonly float roomCountIncrementPerLevel = 1.3f;
        readonly int level = 10;
        readonly float cellSize = 1;
        System.Random random;
        
        int seed;
        [SerializeField] bool useStaticSeed;
        int maxRoomCount => (int)Mathf.Min((generationWidth * generationHeight) * 0.25f, hardMaxRoomCount);

        List<Vector2Int> endRooms;
        
        
        [SerializeField][Range(0,100)] int randomCancelChance;
        [SerializeField] int hardMaxRoomCount;


        void Awake()
        {
            var currentGrid = GenerateLevel();
            Debug.Log(GetLevelLog());
        }

        public ObjectGrid<ERoomTypes> GenerateLevel()
        {
            ResetSeed();
            return InitMap();
        }

        void ResetSeed()
        {
            if (useStaticSeed && seed != 0) return;
            seed = DateTime.Now.Millisecond;
        }

        public ObjectGrid<ERoomTypes> InitMap()
        {
            random = new System.Random(seed);
            endRooms = new List<Vector2Int>();
            grid = new ObjectGrid<ERoomTypes>(generationWidth, generationHeight, cellSize, new Vector3(0, 0, 0));
            ResetMap();
            GenerateLevelMap(level);
            return grid;
        }

        void ResetMap()
        {
            endRooms.Clear();
            for (int y = 0; y < generationHeight; y++)
            {
                for (int x = 0; x < generationWidth; x++)
                {
                    grid.SetValue(x,y, ERoomTypes.Free);
                }
            }
        }

        void GenerateLevelMap(int _level)
        {
            int roomCountToGenerate = (int)(random.Next(0, 2) + minRoomCount + Mathf.Min(_level * roomCountIncrementPerLevel, maxRoomCount));
            var center = new Vector2Int(generationWidth, generationHeight) / 2;
            bool isValid = false;
            const int maxIteration = 100;
            int currentIteration = 0;
            while (!isValid && currentIteration < maxIteration)
            {
                ResetMap();
                int rooms = GenerateRooms(roomCountToGenerate, center);
                isValid = ValidateMap(roomCountToGenerate, rooms);
                ++currentIteration;
            }

            GenerateSpecialRooms();
        }

        void GenerateSpecialRooms()
        {
            var lastRoom = endRooms.Last();
            grid.SetValue(lastRoom, ERoomTypes.Boss);
        }

        bool ValidateMap(int _roomCountToGenerate, int _generatedRooms)
        {
            return _generatedRooms == _roomCountToGenerate && endRooms.Count >= 2;
        }

        int GenerateRooms(int _roomCountToGenerate, Vector2Int _center)
        {
            grid.SetValue(_center, ERoomTypes.Normal);
            Queue<Vector2Int> discoveryQueue = new();
            discoveryQueue.Enqueue(_center);
            int generatedRooms = 1;

            while (discoveryQueue.Count > 0)
            {
                var currentCoord = discoveryQueue.Dequeue();
                var neighbourCoords = GetNeighbourCoords(currentCoord);

                bool hasGenerated = false;

                foreach (var currentNeighbourCoord in neighbourCoords)
                {
                    if (generatedRooms >= _roomCountToGenerate) break;
                    if (random.Next(0, 100) >= randomCancelChance) continue;
                    if(grid.IsOutsideBounds(currentNeighbourCoord)) continue;
                    if(grid.GetValue(currentNeighbourCoord) != ERoomTypes.Free) continue;
                    if (GetNeighbourCount(currentNeighbourCoord) > 2) continue;
                    
                    grid.SetValue(currentNeighbourCoord, ERoomTypes.Normal);
                    ++generatedRooms;
                    hasGenerated = true;
                    discoveryQueue.Enqueue(currentNeighbourCoord);
                }
                if (!hasGenerated) endRooms.Add(currentCoord);
            }
            return generatedRooms;
        }

        Vector2Int[] GetNeighbourCoords(Vector2Int _currentCoord)
        {
            return new[]
            {
                _currentCoord + Vector2Int.right,
                _currentCoord + Vector2Int.left,
                _currentCoord + Vector2Int.up,
                _currentCoord + Vector2Int.down,
            };
        }

        int GetNeighbourCount(Vector2Int _coord)
        {
            var neighbourCoords = GetNeighbourCoords(_coord);
            return neighbourCoords.Count(_currentCoord => grid.GetValue(_currentCoord) != ERoomTypes.Free);
        }
        
        string GetLevelLog()
        {
            var stringBuilder = new StringBuilder();
            for (int y = 0; y < generationHeight; y++)
            {
                stringBuilder.Append("|");
                for (int x = 0; x < generationWidth; x++)
                {
                    int value = (int)grid.GetValue(x, y);
                    if (value >= 0) stringBuilder.Append(" ");
                    stringBuilder.Append(value.ToString() + "|");
                }

                stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
        }
    }
}