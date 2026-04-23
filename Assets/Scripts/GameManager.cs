using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 全局游戏管理器（核心）
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 单例
    public List<PlayerData> players = new List<PlayerData>(); // 4名玩家
    public int currentRound; // 当前小局数
    public bool isBombPhase; // 是否处于拆炸弹阶段

    private void Awake()
    {
        // 单例模式，防止重复创建
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    // 1. 初始化4名玩家（大局开始）
    public void InitPlayers()
    {
        players.Clear();
        for (int i = 1; i <= 4; i++)
        {
            players.Add(new PlayerData
            {
                playerId = i,
                playerName = "玩家" + i,
                isExitRound = false,
                isAlive = true,
                bombSave = LoadBombSave(i) // 加载本地存档
            });
        }
    }

    // 2. 检查是否触发炸弹（3人退出小局）
    public bool CheckTriggerBomb()
    {
        int exitCount = 0;
        foreach (var p in players)
        {
            if (p.isExitRound && p.isAlive) exitCount++;
        }
        return exitCount == 3; // 3人退出 → 触发
    }

    // 3. 获取当前需要拆炸弹的玩家
    public PlayerData GetBombPlayer()
    {
        foreach (var p in players)
        {
            if (!p.isExitRound && p.isAlive) return p;
        }
        return null;
    }

    // 4. 检查大局胜利条件（只剩1人存活）
    public bool CheckGameWin()
    {
        int aliveCount = 0;
        foreach (var p in players)
        {
            if (p.isAlive) aliveCount++;
        }
        return aliveCount == 1;
    }

    // 5. 加载炸弹存档（每人独立）
    public BombSaveData LoadBombSave(int playerId)
    {
        BombSaveData save = new BombSaveData();
        save.lastBombLineIndex = PlayerPrefs.GetInt($"Player{playerId}_BombLine", -1);
        save.lastChooseLineIndex = PlayerPrefs.GetInt($"Player{playerId}_ChooseLine", -1);
        save.lastIsSuccess = PlayerPrefs.GetInt($"Player{playerId}_Success", 0) == 1;
        return save;
    }

    // 6. 保存炸弹存档（每人独立）
    public void SaveBombSave(int playerId, BombSaveData save)
    {
        PlayerPrefs.SetInt($"Player{playerId}_BombLine", save.lastBombLineIndex);
        PlayerPrefs.SetInt($"Player{playerId}_ChooseLine", save.lastChooseLineIndex);
        PlayerPrefs.SetInt($"Player{playerId}_Success", save.lastIsSuccess ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 7. 切换到炸弹场景
    public void GoToBombScene() => SceneManager.LoadScene("Bomb");
    // 8. 切换到打牌场景
    // 原来的GoToGameScene()，改成下面这样
    public void GoToGameScene()
    {
        // 关键：进入游戏前，必须初始化玩家数据
        InitPlayers();
        SceneManager.LoadScene("Game");
    }
    // 9. 切换到主界面
    public void GoToMainScene() => SceneManager.LoadScene("Main");
}