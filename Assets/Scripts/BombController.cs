using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
    public BombData bombData; // 炸弹数据
    public PlayerData bombPlayer; // 当前拆炸弹的玩家
    public Text countDownText; // 倒计时文本
    public Button[] lineButtons; // 4条线的按钮（0-3）
    private float currentTime;
    private bool isGameOver;

    private void Start()
    {
        bombPlayer = GameManager.Instance.GetBombPlayer();
        InitBomb(); // 初始化炸弹
    }

    // 初始化炸弹：随机生成炸弹线 + 绑定按钮
    void InitBomb()
    {
        isGameOver = false;
        currentTime = bombData.countDownTime;
        // 随机炸弹线（0-3）
        bombData.bombLineIndex = Random.Range(0, 4);
        // 绑定4条线的点击事件
        for (int i = 0; i < lineButtons.Length; i++)
        {
            int index = i;
            lineButtons[i].GetComponent<Image>().color = bombData.lineColors[i];
            lineButtons[i].onClick.AddListener(() => OnChooseLine(index));
        }
    }

    private void Update()
    {
        if (isGameOver) return;
        // 倒计时逻辑
        currentTime -= Time.deltaTime;
        countDownText.text = Mathf.Ceil(currentTime).ToString();
        // 超时判定
        if (currentTime <= 0) OnBombFail(true);
    }

    // 玩家点击拆线
    void OnChooseLine(int index)
    {
        if (isGameOver) return;
        // 保存本次拆线结果到玩家存档
        bombPlayer.bombSave = new BombSaveData
        {
            lastBombLineIndex = bombData.bombLineIndex,
            lastChooseLineIndex = index,
            lastIsSuccess = index != bombData.bombLineIndex
        };
        GameManager.Instance.SaveBombSave(bombPlayer.playerId, bombPlayer.bombSave);

        // 判定结果
        if (index == bombData.bombLineIndex) OnBombFail(false);
        else OnBombSuccess();
    }

    // 拆线成功：存活，回到打牌场景开新小局
    void OnBombSuccess()
    {
        isGameOver = true;
        bombPlayer.isExitRound = false; // 重置小局状态
        // 检查是否胜利
        if (GameManager.Instance.CheckGameWin()) ShowWinUI();
        else GameManager.Instance.GoToGameScene();
    }

    // 拆线失败/超时：炸了，大局淘汰
    void OnBombFail(bool isTimeOut)
    {
        isGameOver = true;
        bombPlayer.isAlive = false; // 标记大局淘汰
        ShowFailUI(isTimeOut); // 弹出失败界面
    }

    // 弹出失败界面（可选：退出/观战）
    void ShowFailUI(bool isTimeOut)
    {
        // 这里用Unity UI做弹窗，按钮：退出游戏 / 观战
        Debug.Log($"{bombPlayer.playerName} 被炸淘汰！原因：{(isTimeOut ? "超时" : "拆中炸弹线")}");
        // 观战逻辑：禁用操作，仅看其他玩家
        // 退出逻辑：GameManager.Instance.GoToMainScene();
    }

    // 弹出胜利界面
    void ShowWinUI()
    {
        Debug.Log($"{bombPlayer.playerName} 获得大局胜利！");
        // 二次元胜利特效+弹窗
    }
}