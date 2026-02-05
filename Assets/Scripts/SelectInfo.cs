using UnityEngine;

public class SelectInfo
{
    public string name;

    // Tower info
    public float damage;
    public float attackRange;
    public float damageDone;
    public int killCount;

    // Enemy info
    public float maxHealth;
    public float health;
    public float speed;
    public float progress;
    public float reward;
    private bool multi;

    public override string ToString()
    {
        string infoText = "";
        infoText += $"{name}\n";

        // Tower
        if (damage > 0)
        {
            infoText += multi ? $"Avg Damage: {damage:F0}\n" : $"Damage: {damage:F0}\n";
            infoText += multi ? $"Max Attack Range: {attackRange:F0}\n" : $"Attack Range: {attackRange:F0}\n";
            infoText += multi ? $"Damage Done: {damageDone:F0}\n" : $"Damage Done: {damageDone:F0}\n";
            infoText += multi ? $"Kill Count: {killCount:F0}\n" : $"Kill Count: {killCount:F0}\n";
        }

        // enemy
        if (maxHealth > 0)
        {
            infoText += multi ? $"Max Health: {maxHealth:F0}\n" : $"Max Health: {maxHealth:F0}\n";
            infoText += multi ? $"Health: {health:F0} ({health / maxHealth * 100:F0}%) \n" : $"Health: {health:F0} ({health / maxHealth * 100:F0}%) \n";
            infoText += multi ? $"Avg Speed: {speed:F0}\n" : $"Speed: {speed:F0}\n";
            infoText += multi ? $"Avg Progress: {progress*100:F0}%\n" : $"Progress: {progress*100:F0}%\n";
            infoText += multi ? $"Reward: {reward:F0}\n" : $"Reward: {reward:F0}\n";
        }
        return infoText;
    }

    
    public static SelectInfo operator +(SelectInfo info1, SelectInfo info2)
    {
        SelectInfo combinedInfo = new SelectInfo();

        combinedInfo.name = "Multiple selections";
        combinedInfo.damage = (info1.damage + info2.damage) / 2f;
        combinedInfo.attackRange = Mathf.Max(info1.attackRange, info2.attackRange);
        combinedInfo.damageDone = info1.damageDone + info2.damageDone;
        combinedInfo.killCount = info1.killCount + info2.killCount;
        combinedInfo.maxHealth = info1.maxHealth + info2.maxHealth;
        combinedInfo.health = info1.health + info2.health;
        combinedInfo.speed = (info1.speed + info2.speed) / 2;
        combinedInfo.progress = (info1.progress + info2.progress) / 2;
        combinedInfo.reward = info1.reward + info2.reward;
        combinedInfo.multi = true;

        return combinedInfo;
    }
    
}
