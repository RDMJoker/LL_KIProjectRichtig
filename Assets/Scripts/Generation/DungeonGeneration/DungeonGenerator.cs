using System.Collections.Generic;
using Generation.DungeonGeneration.DungeonGenerationScriptables;
using NaughtyAttributes;
using UnityEngine;

namespace Generation.DungeonGeneration
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] public List<LevelGenerationData> GenerationData;
        [SerializeField] DungeonBuilder dungeonBuilder;
        [SerializeField] bool debugGeneration;
        [SerializeField] Transform rootParent;
        GenerationTileset currentTileset;

        LevelGenerator levelGenerator;
        List<GameObject> parentObjects;

        
        [Button]
        public void GenerateDungeon()
        {
            if (parentObjects == null) parentObjects = new List<GameObject>();
            else ResetParentObjects();
            dungeonBuilder.ResetDungeon();
            for (int i = 0, x = 1; i < GenerationData.Count; i++, x++)
            {
                levelGenerator = new LevelGenerator(GenerationData[i], x, debugGeneration);
                var generatedLevel = levelGenerator.GenerateLevel();
                currentTileset = GenerationData[i].Tileset;
                var currentLevelParentTransform = new GameObject("DungeonLevel: " + x)
                {
                    transform =
                    {
                        parent = rootParent
                    }
                };
                parentObjects.Add(currentLevelParentTransform);
                dungeonBuilder.BuildDungeon(generatedLevel, currentTileset, currentLevelParentTransform.transform,i * 5);
            }
        }

        void ResetParentObjects()
        {
            foreach (var parentObject in parentObjects)
            {
                DestroyImmediate(parentObject.gameObject);
            }
            parentObjects.Clear();
        }
    }
}