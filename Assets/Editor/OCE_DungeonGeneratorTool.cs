using Generation.DungeonGeneration;
using Generation.DungeonGeneration.DungeonGenerationScriptables;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class OCE_DungeonGeneratorTool : EditorWindow
    {
        [SerializeField] VisualTreeAsset uxmlRef;
        DungeonGenerator dungeonGenerator;
        SerializedObject generator;
        SerializedObject levelGenData;
        ScriptableObject creationInstance;

        Label filePathLabel;
        HelpBox errorBox;

        bool overwriteStaticSeedSetting;

        string generationDataName;

        static OCE_DungeonGeneratorTool window;

        string savePath = "Assets/Scripts/Generation/DungeonGeneration/DungeonGenerationScriptables/";

        [MenuItem("Window/OCE2026/GenerationTool %#t")]
        public static void ShowWindow()
        {
            window = GetWindow<OCE_DungeonGeneratorTool>();
            window.minSize = new Vector2(650, 450);
            window.titleContent = new GUIContent("OCE_GenerationTool 2026 Pro+");
        }

        void CreateGUI()
        {
            var root = rootVisualElement;
            uxmlRef.CloneTree(root);

            var staticSeedToggle = root.Q<Toggle>("StaticSeedToggle");
            staticSeedToggle.RegisterValueChangedCallback((onValueChanged) =>
            {
                overwriteStaticSeedSetting = onValueChanged.newValue;
            });
            var generateButton = root.Q<Button>("GenerationButton");
            generateButton.RegisterCallback<ClickEvent>((onClick => StartGeneration()));

            // Get and Register all Visual Elements related to creating new LevelData ScriptableObjects
            var editModeToggle = root.Q<Toggle>("EditModeToggle");
            var editObjectField = root.Q<ObjectField>("EditObjectField");
            var openFolderPathChoosing = root.Q<ToolbarButton>("OpenFolderPathChoosingButton");

            filePathLabel = root.Q<Label>("FilePathLabel");
            string cutString = savePath.Split("Assets")[1];
            savePath = "Assets" + cutString;
            filePathLabel.text = "Chosen save path: " + savePath;

            var generateLevelDataButton = root.Q<Button>("GenerateLevelDataButton");
            var generationDataNameField = root.Q<TextField>("GenerationDataNameField");
            generationDataNameField.RegisterValueChangedCallback((onValueChanged) =>
                generationDataName = onValueChanged.newValue
            );

            editModeToggle.RegisterValueChangedCallback((onValueChange) =>
            {
                editObjectField.SetEnabled(onValueChange.newValue);
                if (onValueChange.newValue)
                {
                    generationDataNameField.style.display = DisplayStyle.None;
                    editObjectField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    generationDataNameField.style.display = DisplayStyle.Flex;
                    editObjectField.style.display = DisplayStyle.None;
                }

                generateLevelDataButton.SetEnabled(!onValueChange.newValue);
                if (onValueChange.newValue) CopyDataFromGenDataToInstance((ScriptableObject)editObjectField.value);
                else ResetToEmptyInstance();
            });
            generationDataNameField.style.display = DisplayStyle.Flex;
            editObjectField.style.display = DisplayStyle.None;
            editObjectField.RegisterValueChangedCallback((_onValueChange =>
                CopyDataFromGenDataToInstance((ScriptableObject)_onValueChange.newValue)));
            openFolderPathChoosing.RegisterCallback<ClickEvent>((_ => SetFilePath()));
            generateLevelDataButton.RegisterCallback<ClickEvent>((_ => CreateLevelGenData()));
        }

        void SetFilePath()
        {
            savePath = EditorUtility.OpenFolderPanel("Choose save folder", "Assets", "");
            string cutString = savePath.Split("Assets")[1];
            savePath = "Assets" + cutString + "/";
            filePathLabel.text = "Chosen save path: " + savePath;
        }

        void ResetToEmptyInstance()
        {
            if (creationInstance == null)
            {
                creationInstance = CreateInstance(typeof(LevelGenerationData));
                levelGenData = new SerializedObject(creationInstance);
                rootVisualElement.Bind(levelGenData);
                return;
            }

            string json = JsonUtility.ToJson((LevelGenerationData)creationInstance);
            creationInstance = CreateInstance(typeof(LevelGenerationData));
            JsonUtility.FromJsonOverwrite(json, creationInstance);
            levelGenData = new SerializedObject(creationInstance);
            rootVisualElement.Bind(levelGenData);
        }

        void CopyDataFromGenDataToInstance(ScriptableObject _object)
        {
            if (_object == null) return;
            creationInstance = _object;
            levelGenData = new SerializedObject(creationInstance);
            rootVisualElement.Bind(levelGenData);
        }

        void CreateLevelGenData()
        {
            if (creationInstance == null)
            {
                ResetToEmptyInstance();
            }

            AssetDatabase.CreateAsset((LevelGenerationData)creationInstance, savePath + generationDataName + ".asset");
        }


        void StartGeneration()
        {
            errorBox = new HelpBox("The generator has no valid data! Please add atleast one generation data!",
                HelpBoxMessageType.Error);
            if (dungeonGenerator.GenerationData.Count == 0) rootVisualElement.Add(errorBox);
            else
            {
                if (overwriteStaticSeedSetting)
                {
                    foreach (var data in dungeonGenerator.GenerationData)
                    {
                        data.UseStaticSeed = overwriteStaticSeedSetting;
                    }
                }
                if(rootVisualElement.Contains(errorBox)) rootVisualElement.Remove(errorBox);
                dungeonGenerator.GenerateDungeon();
            }
        }

        void OnEnable()
        {
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
            errorBox = new HelpBox("", HelpBoxMessageType.Error);
            if (dungeonGenerator == null)
            {
                errorBox.text =
                    "No DungeonGenerator Object found! Please switch to the correct scene and reopen the window!";
                rootVisualElement.Add(errorBox);
            }

            generator = new SerializedObject(dungeonGenerator);
            rootVisualElement.Bind(generator);
            ResetToEmptyInstance();
        }
    }
}