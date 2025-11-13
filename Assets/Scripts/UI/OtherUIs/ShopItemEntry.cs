using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OtherUIs
{
    public class ShopItemEntry : MonoBehaviour
    {
        [Header("Shop UI Components")] 
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemRarityText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text costText;
    }
}

