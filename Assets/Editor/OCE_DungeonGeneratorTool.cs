using System;
using Generation.DungeonGeneration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
            overwriteStaticSeedSetting = onValueChanged.newValue
        })
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
            //Scriptable Object logic für Laden und Übertragen der Daten
            
        });
        generationDataNameField.style.display = DisplayStyle.Flex;
        editObjectField.style.display = DisplayStyle.None;
        // 3 Callbacks kommen hier noch :) l
    }

    void StartGeneration()
    {
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

        // Hier fehlt noch was :)
    }
}