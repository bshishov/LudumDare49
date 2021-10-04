using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Leagues", menuName = "Leagues", order = 0)]
public class LeaguesData : ScriptableObject
{
    [Serializable]
    public struct LeagueData
    {
        public string Id;
        public Sprite Icon;
        public string Name;
        public Color BackgroundColor;
    }

    [SerializeField]
    public LeagueData[] Leagues;

    public LeagueData GetLeagueById(string id)
    {
        foreach (var leagueData in Leagues)
        {
            if (leagueData.Id == id)
                return leagueData;
        }

        return new LeagueData
        {
            Id = "fixme",
            Icon = null,
            BackgroundColor = Color.black,
            Name = "FIXME",
        };
    }
}