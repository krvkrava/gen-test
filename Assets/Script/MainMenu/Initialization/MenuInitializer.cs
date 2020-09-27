using MainMenu.UiElements.LevelSelector;
using Strx.Expansions.Extensions.Algebraic;
using UnityEngine;

namespace MainMenu.Initialization
{
    public class MenuInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject _levelSelectorContentRoot;
    
        private void Awake()
        {
            InitLevelSelector();      
        }

        private async void InitLevelSelector()
        {
            await LevelSelectionController.Create(_levelSelectorContentRoot)
                .SpecifyAsync(() => "Cannot create level selector")
                .ThrowIfErrorAsync();
        }
    }
}
