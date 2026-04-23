using UnityEngine;
using TMPro;

public class ReplaceAllTMPFonts : MonoBehaviour
{
    // 把你的思源黑体SDF字体拖到这个字段里
    public TMP_FontAsset targetFont;

    void Start()
    {
        if (targetFont == null)
        {
            Debug.LogError("请把你的思源黑体SDF字体拖到targetFont字段里！");
            return;
        }

        // 替换场景里所有TMP文本的字体
        TextMeshProUGUI[] allTMPTexts = FindObjectsOfType<TextMeshProUGUI>(includeInactive: true);
        foreach (var tmpText in allTMPTexts)
        {
            tmpText.font = targetFont;
            Debug.Log($"已替换字体：{tmpText.gameObject.name}");
        }

        Debug.Log("所有TMP文本字体替换完成！");
    }
}