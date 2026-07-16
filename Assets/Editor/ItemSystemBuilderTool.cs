using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class ItemSystemBuilderTool : EditorWindow
    {
        [SerializeField] VisualTreeAsset uxmlRef;

        UniqueItemPrefabBuilder builder;
        SerializedObject serializedBuilderObject;

        DropdownField uniqueDropDown;
        DropdownField typeDropDown;
        TextField textField;
        ObjectField meshObjectField;
        ObjectField materialObjectField;
        VisualElement firstTab;
        VisualElement secondTab;
        ToolbarButton firstTabButton;
        ToolbarButton secondTabButton;

        ScriptableObject dropTableInstance;
        SerializedObject dropTableObject;

        HelpBox errorBox;

        string itemSaveFilePath = "Assets/MyPrefabs/ItemSystem/UniqueItems/";
        string tableSaveFilePath = "Assets/ItemSystem/ItemSystemScriptables/ItemTypes/DropTables/";

        static ItemSystemBuilderTool window;

        const string SavePath = "Assets/ItemSystem/ItemSystemScriptables/ItemTypes/DropTables/";
        string dropTableName = "";

        List<Type> typeList = new();
        // ObjectField uniqueItemHolderObjectField = null;
        // Label uniqueItemHolderLabel = null;
        // ObjectField tierDataObjectField = null;
        // Label tierDataLabel = null;

        Dictionary<EItemStat, int> itemStats = new Dictionary<EItemStat, int>()
        {
            { EItemStat.Health, 0 },
            { EItemStat.Mana, 0 },
            { EItemStat.AttackSpeed, 0 },
            { EItemStat.DamageFlat, 0 },
            { EItemStat.DamagePercent, 0 },
            { EItemStat.MovementSpeed, 0 }
        };

        int baseDamage = 0;
        int baseAttackSpeed = 0;
        int baseArmor = 0;

        [MenuItem("Window/ItemSystemBuilderTool %#i")]
        public static void ShowWindow()
        {
            window = GetWindow<ItemSystemBuilderTool>();
            window.minSize = new Vector2(650, 450);
            window.titleContent = new GUIContent("ItemSystemBuilderTool");
        }

        void OnEnable()
        {
            errorBox = new HelpBox("", HelpBoxMessageType.Error);
            builder = FindObjectOfType<UniqueItemPrefabBuilder>();
            if (builder == null)
            {
                errorBox.text =
                    "No Item Builder Found! Please switch to the correct scene and reopen the window! Correct scene: 'ItemSystem' ";
                rootVisualElement.Add(errorBox);
                return;
            }

            typeList = new List<Type>(builder.itemTypes);
            serializedBuilderObject = new SerializedObject(builder);

            dropTableInstance = CreateInstance(typeof(ItemTypeDropTable));
            dropTableObject = new SerializedObject(dropTableInstance);
            rootVisualElement.Bind(dropTableObject);
        }

        void CreateGUI()
        {
            var root = rootVisualElement;
            uxmlRef.CloneTree(root);
            if (errorBox.text ==
                "No Item Builder Found! Please switch to the correct scene and reopen the window! Correct scene: 'ItemSystem' ")
                return;
            root.Bind(serializedBuilderObject);

            // Register the Visual Elements related to the tab system.
            firstTab = root.Q<VisualElement>("ItemBuilderTab");
            secondTab = root.Q<VisualElement>("DroptableBuilderTab");

            firstTabButton = root.Q<ToolbarButton>("ItemBuilderTabButton");
            firstTabButton.style.color = Color.green;
            secondTabButton = root.Q<ToolbarButton>("DroptableBuilderTabButton");

            firstTabButton.RegisterCallback<ClickEvent>(_onClick => SwitchTabs(1));
            secondTabButton.RegisterCallback<ClickEvent>(_onClick => SwitchTabs(2));

            #region FirstTab

            // Register all Visual Elements related to the first tab.
            textField = root.Q<TextField>("UniqueNameTextField");
            meshObjectField = root.Q<ObjectField>("MeshField");
            materialObjectField = root.Q<ObjectField>("MaterialField");
            var weaponStatsFoldout = root.Q<Foldout>("WeaponStats");
            var armorStatsFoldout = root.Q<Foldout>("ArmorStats");
            var healthField = root.Q<IntegerField>("HealthTextField");
            var manaField = root.Q<IntegerField>("ManaTextField");
            var attackSpeedField = root.Q<IntegerField>("AttackSpeedField");
            var damageFlatField = root.Q<IntegerField>("FlatDamageField");
            var damagePercentField = root.Q<IntegerField>("PercentDamageField");
            var movementSpeedField = root.Q<IntegerField>("MovementSpeedField");
            var baseArmorField = root.Q<IntegerField>("BaseArmor");
            var baseDamageField = root.Q<IntegerField>("BaseDamage");
            var baseAttackSpeedField = root.Q<IntegerField>("BaseAttackSpeed");
            var itemLabel = root.Q<Label>("FilePathLabelItem");
            var savePathButtonItem = root.Q<ToolbarButton>("OpenFolderPathChoosingButtonItem");

            // Cut the savePath to fit the required format for the AssetDatabase function
            string cutString = itemSaveFilePath.Split("Assets")[1];
            itemSaveFilePath = "Assets" + cutString;
            itemLabel.text = "Chosen save path: " + itemSaveFilePath;

            // Continue with registration
            savePathButtonItem.RegisterCallback<ClickEvent>((_onClick => SetFilePath(ref itemSaveFilePath, itemLabel)));
            healthField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.Health, _onValueChange.newValue));
            manaField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.Mana, _onValueChange.newValue));
            attackSpeedField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.AttackSpeed, _onValueChange.newValue));
            damageFlatField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.DamageFlat, _onValueChange.newValue));
            damagePercentField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.DamagePercent, _onValueChange.newValue));
            movementSpeedField.RegisterValueChangedCallback((_onValueChange) =>
                ChangeDictionaryValue(EItemStat.MovementSpeed, _onValueChange.newValue));
            baseArmorField.RegisterValueChangedCallback((_onValueChange) => baseArmor = _onValueChange.newValue);
            baseDamageField.RegisterValueChangedCallback((_onValueChange) => baseDamage = _onValueChange.newValue);
            baseAttackSpeedField.RegisterValueChangedCallback((_onValueChange) =>
                baseAttackSpeed = _onValueChange.newValue);


            uniqueDropDown = root.Q<DropdownField>("UniqueTierDropDown");

            string[] test = AssetDatabase.FindAssets("t: UniqueItemHolder");
            for (int i = 0; i < test.Length; i++)
            {
                uniqueDropDown.choices.Add("Tier: " + i);
            }


            typeDropDown = root.Q<DropdownField>("ScriptTypeDropDown");
            foreach (var type in typeList)
            {
                typeDropDown.choices.Add(type.ToString().Replace("ItemSystem.", ""));
            }

            typeDropDown.RegisterValueChangedCallback((_onValueChange) =>
            {
                if (ItemTypeDictionaries.IsWeapon(typeList[typeDropDown.index]))
                {
                    ToggleSpecificGroup(armorStatsFoldout, false);
                    ToggleSpecificGroup(weaponStatsFoldout, true);
                }
                else
                {
                    ToggleSpecificGroup(weaponStatsFoldout, false);
                    ToggleSpecificGroup(armorStatsFoldout, true);
                }
            });
            var generationButton = root.Q<Button>("GenerationButton");
            generationButton.RegisterCallback<ClickEvent>((_onClick) => Generate());
            // Setup so none of the script specific groups are shown at the start
            ToggleSpecificGroup(armorStatsFoldout, false);
            ToggleSpecificGroup(weaponStatsFoldout, false);

            #endregion

            #region SecondTab

            // Register all Visual Elements related to the second tab.
            var droptableButton = root.Q<Button>("DroptableButton");
            droptableButton.RegisterCallback<ClickEvent>((_onClick => CreateDropTable()));
            var deleteDroptableButton = root.Q<Button>("DeleteDroptableButton");
            deleteDroptableButton.RegisterCallback<ClickEvent>((_onClick => DeleteDropTable()));
            var droptableNameField = root.Q<TextField>("DroptableName");
            droptableNameField.RegisterValueChangedCallback((_onValueChange =>
                dropTableName = _onValueChange.newValue));
            var droptableLabel = root.Q<Label>("FilePathLabelTable");
            var savePathButtonTable = root.Q<ToolbarButton>("OpenFolderPathChoosingButtonTable");

            // Cut the savePath to fit the required format for the AssetDatabase function
            string cutStringTable = tableSaveFilePath.Split("Assets")[1];
            tableSaveFilePath = "Assets" + cutStringTable;
            droptableLabel.text = "Chosen save path: " + tableSaveFilePath;
            savePathButtonTable.RegisterCallback<ClickEvent>((_onClick =>
                SetFilePath(ref tableSaveFilePath, droptableLabel)));

            #endregion
        }

        /// <summary>
        /// Function to change the value of a specific entry within the overhead dictionary
        /// </summary>
        /// <param name="_stat"></param>
        /// <param name="_value"></param>
        void ChangeDictionaryValue(EItemStat _stat, int _value)
        {
            itemStats[_stat] = _value;
        }


        /// <summary>
        /// Function to generate the unique item
        /// </summary>
        void Generate()
        {
            #region ErrorChecks

            List<string> errorStringList = new();
            if (textField.value == string.Empty || textField.value.Trim() == string.Empty)
            {
                errorStringList.Add("Item name cannot be empty or just consist of spaces!");
            }

            if (meshObjectField.value == null)
            {
                errorStringList.Add("Mesh cannot be empty!");
            }

            if (materialObjectField.value == null)
            {
                errorStringList.Add("Material cannot be empty!");
            }

            if (typeDropDown.index < 0)
            {
                errorStringList.Add("Type cannot be empty!");
            }

            if (uniqueDropDown.index < 0)
            {
                errorStringList.Add("Unique Tier cannot be empty!");
            }

            #endregion

            // Checks the amount of errors. If any error exists, prints a string built from the individual error messages
            if (errorStringList.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Please fix the following errors: \n");
                foreach (string error in errorStringList)
                {
                    if (error == errorStringList.Last())
                    {
                        stringBuilder.Append(error);
                    }
                    else
                    {
                        stringBuilder.Append(error + "\n");
                    }
                }

                if (rootVisualElement.Children().Contains(errorBox)) rootVisualElement.Remove(errorBox);
                errorBox = new HelpBox(stringBuilder.ToString(), HelpBoxMessageType.Error);
                rootVisualElement.Add(errorBox);
            }
            else
            {
                builder.CreateItemData(typeList[typeDropDown.index], itemStats);
                // Checks script type and creates the item based on the script type. (Currently only 2 script types available, which is why this function is just a bool check.
                if (ItemTypeDictionaries.IsWeapon(typeList[typeDropDown.index]))
                {
                    builder.CreatePrefab(textField.value, typeList[typeDropDown.index], (Mesh)meshObjectField.value,
                        (Material)materialObjectField.value, uniqueDropDown.index, itemSaveFilePath,
                        _baseDamage: baseDamage, _baseAttackSpeed: baseAttackSpeed);
                }
                else
                {
                    builder.CreatePrefab(textField.value, typeList[typeDropDown.index], (Mesh)meshObjectField.value,
                        (Material)materialObjectField.value, uniqueDropDown.index, itemSaveFilePath,
                        _baseDefence: baseArmor);
                }
            }
        }

        /// <summary>
        /// Creates the drop table ScriptableObject
        /// </summary>
        void CreateDropTable()
        {
            AssetDatabase.CreateAsset(dropTableInstance, tableSaveFilePath + dropTableName + ".asset");
        }

        /// <summary>
        /// Deletes a DropTable. Uses the currently used dropTableName to find the correct table to delete.
        /// </summary>
        void DeleteDropTable()
        {
            rootVisualElement.Unbind();
            dropTableInstance = CreateInstance(typeof(ItemTypeDropTable));
            dropTableObject = new SerializedObject(dropTableInstance);
            rootVisualElement.Bind(dropTableObject);
            AssetDatabase.DeleteAsset(tableSaveFilePath + dropTableName + ".asset");
        }

        /// <summary>
        /// Helper function to toggle the display of any Visual Element.
        /// </summary>
        /// <param name="_element"></param>
        /// <param name="_show"></param>
        void ToggleSpecificGroup(VisualElement _element, bool _show)
        {
            _element.style.display = _show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Function called to switch from one tab to another. Uses self defined index.
        /// </summary>
        /// <param name="_targetTab"></param>
        void SwitchTabs(int _targetTab)
        {
            switch (_targetTab)
            {
                case 1:
                    ToggleSpecificGroup(secondTab, false);
                    ToggleSpecificGroup(firstTab, true);
                    secondTabButton.style.color = Color.white;
                    firstTabButton.style.color = Color.green;
                    break;
                case 2:
                    ToggleSpecificGroup(firstTab, false);
                    ToggleSpecificGroup(secondTab, true);
                    firstTabButton.style.color = Color.white;
                    secondTabButton.style.color = Color.green;
                    break;
            }
        }

        /// <summary>
        /// Function that prompts the user to choose a save file path. This path is then used to save created Assets.
        /// </summary>
        /// <param name="_overrideString"></param>
        /// <param name="_label"></param>
        void SetFilePath(ref string _overrideString, Label _label)
        {
            string tempString = EditorUtility.OpenFolderPanel("Choose folder", "Assets", "");
            if (string.IsNullOrEmpty(tempString)) return;
            _overrideString = tempString;
            string cutString = _overrideString.Split("Assets")[1];
            _overrideString = "Assets" + cutString + "/";
            _label.text = "Chosen save path: " + _overrideString;
        }
    }
}