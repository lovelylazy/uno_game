using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 打牌场景核心管理器（修复版，零越界）
public class CardGame : MonoBehaviour
{
    [Header("UI赋值（Inspector里拖入对应物体）")]
    public TextMeshProUGUI turnText;
    public Image topCardImage;
    public TextMeshProUGUI exitCountText;
    public Transform localPlayerHandParent;
    public Button playButton;
    public GameObject cardPrefab;

    [Header("对手手牌数量 (拖入3个对手的CardCountText)")]
    public TextMeshProUGUI[] opponentCardCounts = new TextMeshProUGUI[3];

    // 本地数据
    private List<PlayerData> aliveRoundPlayers;
    private int currentTurnIndex;
    private Card topPlayedCard;
    private Card selectedCard;

    void Start()
    {
        // 防御：GameManager不存在，直接退出
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager不存在！请从Main场景启动游戏");
            return;
        }

        // 防御：玩家列表为空，直接退出
        if (GameManager.Instance.players == null || GameManager.Instance.players.Count == 0)
        {
            Debug.LogError("玩家数据未初始化！请在Main场景点击开始游戏");
            return;
        }

        // 初始化小局数据
        InitRound();
        // 绑定出牌按钮事件
        playButton.onClick.AddListener(PlaySelectedCard);
    }

    // 初始化小局：重置玩家状态、发牌、生成手牌UI
    void InitRound()
    {
        // 筛选大局存活的玩家（未被炸淘汰的）
        aliveRoundPlayers = GameManager.Instance.players.Where(p => p.isAlive).ToList();

        // 防御：没有存活玩家，直接退出
        if (aliveRoundPlayers.Count == 0)
        {
            Debug.LogError("没有存活玩家！");
            return;
        }

        // 重置所有存活玩家的小局退出状态
        foreach (var p in aliveRoundPlayers)
        {
            p.isExitRound = false;
            p.handCards = new List<Card>();
        }

        // 发牌：每人7张UNO牌
        DealCards();
        // 生成本地玩家的手牌UI
        GenerateLocalPlayerHandUI();
        // 初始化桌上的第一张牌（随机生成）
        topPlayedCard = new Card()
        {
            color = (CardColor)Random.Range(0, 4),
            number = Random.Range(0, 10)
        };
        UpdateTopCardUI();
        // 初始化回合：从玩家0开始（安全赋值，不越界）
        currentTurnIndex = 0;
        UpdateTurnUI();
        // 刷新对手UI
        UpdateOpponentUI();
    }

    // 简化发牌逻辑：每人7张随机牌
    void DealCards()
    {
        foreach (var p in aliveRoundPlayers)
        {
            for (int i = 0; i < 7; i++)
            {
                p.handCards.Add(new Card()
                {
                    color = (CardColor)Random.Range(0, 4),
                    number = Random.Range(0, 10)
                });
            }
        }
        UpdateOpponentUI();
    }

    // 动态生成本地玩家的手牌UI
    void GenerateLocalPlayerHandUI()
    {
        // 防御：父物体为空，直接退出
        if (localPlayerHandParent == null) return;

        // 先清空旧的手牌UI
        foreach (Transform child in localPlayerHandParent)
        {
            Destroy(child.gameObject);
        }

        // 获取本地玩家（玩家ID为1，可根据你的逻辑调整）
        PlayerData localPlayer = aliveRoundPlayers.FirstOrDefault(p => p.playerId == 1);
        if (localPlayer == null) return;

        // 生成每张牌的UI
        foreach (var card in localPlayer.handCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, localPlayerHandParent);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                // 初始化卡牌UI数据
                cardUI.InitCard(card);
                // 绑定卡牌点击事件（选中牌）
                cardObj.GetComponent<Button>().onClick.AddListener(() => SelectCard(card));
            }
        }
    }

    // 玩家选中一张牌
    void SelectCard(Card card)
    {
        selectedCard = card;
        // 给选中的牌加高亮效果（可选，比如改变颜色或缩放）
        Debug.Log($"选中了牌：{card.color} {card.number}");
    }

    // 出牌按钮点击事件：打出选中的牌
    public void PlaySelectedCard()
    {
        if (selectedCard == null)
        {
            Debug.Log("请先选择一张牌！");
            return;
        }

        // 防御：回合索引越界，直接退出
        if (currentTurnIndex < 0 || currentTurnIndex >= aliveRoundPlayers.Count)
        {
            Debug.LogError("回合索引越界！");
            return;
        }

        // 简化出牌规则：必须和桌上的牌同色或同数字
        if (!CheckPlayRule(selectedCard))
        {
            Debug.Log("不符合出牌规则！请出同色或同数字的牌");
            return;
        }

        // 获取当前回合玩家
        PlayerData currentPlayer = aliveRoundPlayers[currentTurnIndex];
        // 移除手牌中的牌
        currentPlayer.handCards.Remove(selectedCard);
        // 更新桌上的牌
        topPlayedCard = selectedCard;
        UpdateTopCardUI();

        // 清空选中状态
        selectedCard = null;
        // 刷新本地玩家手牌UI
        GenerateLocalPlayerHandUI();
        // 刷新对手UI
        UpdateOpponentUI();

        // 检查玩家是否打完手牌（退出小局）
        if (currentPlayer.handCards.Count == 0)
        {
            currentPlayer.isExitRound = true;
            Debug.Log($"{currentPlayer.playerName} 打完手牌，退出小局！");
        }

        // 检查是否触发炸弹（3人退出小局）
        if (CheckTriggerBomb())
        {
            GameManager.Instance.GoToBombScene();
            return;
        }

        // 切换到下一回合
        NextTurn();
    }

    // 检查出牌规则（同色或同数字）
    bool CheckPlayRule(Card card)
    {
        if (topPlayedCard == null) return true; // 桌上没牌，随便出
        return card.color == topPlayedCard.color || card.number == topPlayedCard.number;
    }

    // 切换到下一回合（修复死循环+越界）
    void NextTurn()
    {
        // 防御：玩家列表为空，直接退出
        if (aliveRoundPlayers == null || aliveRoundPlayers.Count == 0) return;

        // 最多循环aliveRoundPlayers.Count次，防止死循环
        for (int i = 0; i < aliveRoundPlayers.Count; i++)
        {
            currentTurnIndex = (currentTurnIndex + 1) % aliveRoundPlayers.Count;
            if (!aliveRoundPlayers[currentTurnIndex].isExitRound)
            {
                break;
            }
        }

        // 更新回合UI
        UpdateTurnUI();
        // 如果是AI玩家，自动出牌（简化逻辑）
        if (aliveRoundPlayers[currentTurnIndex].playerId != 1)
        {
            Invoke(nameof(AIPlayCard), 1f); // 延迟1秒出牌，模拟思考
        }
    }

    // AI玩家自动出牌逻辑（简化版）
    void AIPlayCard()
    {
        // 防御：回合索引越界，直接退出
        if (currentTurnIndex < 0 || currentTurnIndex >= aliveRoundPlayers.Count) return;

        PlayerData aiPlayer = aliveRoundPlayers[currentTurnIndex];
        // 找一张符合规则的牌
        Card validCard = aiPlayer.handCards.FirstOrDefault(c => CheckPlayRule(c));
        if (validCard != null)
        {
            // 直接打出这张牌（复用出牌逻辑）
            selectedCard = validCard;
            PlaySelectedCard();
        }
        else
        {
            // 没牌出，抽一张牌（简化处理，直接加一张随机牌）
            aiPlayer.handCards.Add(new Card()
            {
                color = (CardColor)Random.Range(0, 4),
                number = Random.Range(0, 10)
            });
            UpdateOpponentUI();
            NextTurn();
        }
    }

    // 检查是否触发炸弹（3人退出小局）
    bool CheckTriggerBomb()
    {
        int exitCount = aliveRoundPlayers.Count(p => p.isExitRound);
        exitCountText.text = $"已退出小局：{exitCount}/3";
        return exitCount == 3;
    }

    // 更新回合显示UI（修复越界）
    void UpdateTurnUI()
    {
        // 防御：回合索引越界，直接退出
        if (currentTurnIndex < 0 || currentTurnIndex >= aliveRoundPlayers.Count)
        {
            Debug.LogError("回合索引越界！");
            return;
        }

        PlayerData currentPlayer = aliveRoundPlayers[currentTurnIndex];
        turnText.text = $"当前回合：{currentPlayer.playerName}";
    }

    // 更新桌上当前牌的UI
    void UpdateTopCardUI()
    {
        if (topPlayedCard == null) return;
        topCardImage.color = GetColorFromEnum(topPlayedCard.color);
    }

    // 辅助方法：把CardColor转成Unity Color
    Color GetColorFromEnum(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red: return Color.red;
            case CardColor.Blue: return Color.blue;
            case CardColor.Yellow: return Color.yellow;
            case CardColor.Green: return Color.green;
            default: return Color.white;
        }
    }

    // 安全刷新对手UI 零报错
    void UpdateOpponentUI()
    {
        if (aliveRoundPlayers == null || opponentCardCounts == null) return;

        for (int i = 0; i < 3; i++)
        {
            if (i + 1 < aliveRoundPlayers.Count && opponentCardCounts[i] != null)
            {
                int playerIndex = i + 1;
                opponentCardCounts[i].text = $"手牌：{aliveRoundPlayers[playerIndex].handCards.Count}";
            }
        }
    }
}