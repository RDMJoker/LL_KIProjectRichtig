using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultNamespace.Enums;
using Generation.DungeonGeneration.DungeonGenerationScriptables;
using LL_Unity_Utils.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Generation.DungeonGeneration
{
    public class LevelGenerator
    {
        readonly int generationWidth;
        readonly int generationHeight;
        readonly int minRoomCount;
        readonly float roomCountIncrementPerLevel;
        int seed;
        readonly int level;
        readonly bool debug;
        readonly bool useStaticSeed;
        readonly float cellSize;
        
        int maxRoomCount => (int)Mathf.Min((generationWidth + generationHeight) * 0.25f, 60);
        ObjectGrid<ERoomTypes> grid;
        System.Random random;
        List<Vector2Int> endRooms;
        
        public LevelGenerator(LevelGenerationData _data,int _level = 1, bool _debug = false)
        {
            generationWidth = _data.GenerationGridWidth;
            generationHeight = _data.GenerationGridHeight;
            minRoomCount = _data.MinRoomCount;
            roomCountIncrementPerLevel = _data.RoomCountIncrement;
            seed = _data.Seed;
            useStaticSeed = _data.UseStaticSeed;
            level = _level;
            debug = _debug;
            cellSize = _data.GridCellSize;
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
        
        ObjectGrid<ERoomTypes> InitMap()
        {
            random = new System.Random(seed);
            endRooms = new List<Vector2Int>();
            grid = new ObjectGrid<ERoomTypes>(generationWidth, generationHeight, cellSize, new Vector3(0,0,0));
            ResetMap();
            GenerateLevelMap(level);
            if (debug) Debug.Log(GetLevelLog());
            return grid;
        }

        void ResetMap()
        {
            endRooms.Clear();
            for (int y = 0; y < generationHeight; y++)
            {
                for (int x = 0; x < generationWidth; x++)
                {
                    grid.SetValue(x, y, ERoomTypes.Free);
                }
            }
        }

        void GenerateLevelMap(int _level)
        {
            int roomCountToGenerate = (int)(random.Next(0, 2) + minRoomCount + Mathf.Min(_level * roomCountIncrementPerLevel, maxRoomCount));
            var center = new Vector2Int(generationWidth, generationHeight) / 2;
            bool isValid = false;
            const int maxIterations = 100;
            int currentIterations = 0;
            while (!isValid && currentIterations < maxIterations)
            {
                ResetMap();
                int rooms = GenerateRooms(roomCountToGenerate, center);
                isValid = ValidateMap(roomCountToGenerate, rooms);
                ++currentIterations;
            }

            if (currentIterations >= maxIterations) Debug.LogError("Something went wrong!!" + seed);
            GenerateSpecialRooms();
        }

        bool ValidateMap(int _roomCountToGenerate, int _generatedRooms)
        {
            return _generatedRooms == _roomCountToGenerate && endRooms.Count >= 2;
        }

        void GenerateSpecialRooms()
        {
            var lastRoom = endRooms.Last();
            grid.SetValue(lastRoom, ERoomTypes.Boss);
        }

        int GenerateRooms(int _roomsToGenerate, Vector2Int _center)
        {
            grid.SetValue(_center, ERoomTypes.Normal);

            Queue<Vector2Int> discoveryQueue = new();
            discoveryQueue.Enqueue(_center);

            int generatedRooms = 1;
            while (discoveryQueue.Count > 0)
            {
                var currentCoord = discoveryQueue.Dequeue();
                var neighbourCoords = new[]
                {
                    currentCoord + Vector2Int.right,
                    currentCoord + Vector2Int.left,
                    currentCoord + Vector2Int.up,
                    currentCoord + Vector2Int.down,
                };
                bool hasGenerated = false;

                foreach (var currentNeighbourCoords in neighbourCoords)
                {
                    if (grid.IsOutsideBounds(currentNeighbourCoords)) continue;
                    if (generatedRooms >= _roomsToGenerate) break;
                    if (grid.GetValue(currentNeighbourCoords) != ERoomTypes.Free) continue;
                    if (random.Next(0, 2) == 0) continue;
                    if (GetNeighbourCount(currentNeighbourCoords) > 1) continue;

                    grid.SetValue(currentNeighbourCoords, ERoomTypes.Normal);
                    ++generatedRooms;
                    discoveryQueue.Enqueue(currentNeighbourCoords);
                    hasGenerated = true;
                }

                if (hasGenerated == false) endRooms.Add(currentCoord);
            }

            return generatedRooms;
        }

        int GetNeighbourCount(Vector2Int _coord)
        {
            var neighbourCoords = new Vector2Int[]
            {
                _coord + Vector2Int.right,
                _coord + Vector2Int.left,
                _coord + Vector2Int.up,
                _coord + Vector2Int.down,
            };

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