using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PlayerData
{
    public int playerId; // 1-4号玩家
    public string playerName; // 玩家昵称
    public List<Card> handCards; // 手牌（UNO牌）
    public bool isExitRound; // 【小局状态】是否退出当前小局
    public bool isAlive; // 【大局状态】是否存活（未被炸）
    public BombSaveData bombSave; // 独立炸弹存档
}
[System.Serializable]
public class BombSaveData
{
    public int lastBombLineIndex; // 上一次炸弹线索引（0-3）
    public int lastChooseLineIndex; // 上一次点击的线索引
    public bool lastIsSuccess; // 上一次是否拆线成功
}
public class BombData
{
    public Color[] lineColors = { Color.red, Color.blue, Color.yellow, Color.green }; // 4条线颜色
    public int bombLineIndex; // 随机生成的炸弹线（0-3）
    public float countDownTime = 10f; // 倒计时10秒
}
[System.Serializable]
public class Card
{
    public CardColor color; // 红/蓝/黄/绿
    public int number; // 0-9（简化UNO，去掉功能牌）
    public Card() { }
}
public enum CardColor { Red, Blue, Yellow, Green }