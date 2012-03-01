using System.Collections.Generic;
using TShockAPI;

namespace ExtendedBans
{
    
    public class EBPlayer
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }

        public EBPlayer(int index)
        {
            Index = index;
        }

        public static List<EBPlayer> GetPlayersByName(string plrName)
        {
            List<EBPlayer> players = new List<EBPlayer>();
            foreach (EBPlayer plrs in ExtendedBans.EBPlayers)
            {
                if(plrs.TSPlayer.Name.ToLower() == plrName.ToLower()) {
                    players.Clear();
                    players.Add(plrs);
                    break;
                }
                else if(plrs.TSPlayer.Name.ToLower().Contains(plrName.ToLower()))
                {
                    players.Add(plrs);
                }
            }
            return players;
        }

        public static List<EBPlayer> GetPlayersByIPMask(string IP)
        {
            List<EBPlayer> players = new List<EBPlayer>();
            foreach (EBPlayer plrs in ExtendedBans.EBPlayers)
            {
                string[] plrIP = plrs.TSPlayer.IP.Split('.');
                string[] argIP = IP.Split('.');
                if (argIP[0] == plrIP[0] || argIP[0] == "*")
                {
                    if (argIP[1] == plrIP[1] || argIP[1] == "*")
                    {
                        if (argIP[2] == plrIP[2] || argIP[2] == "*")
                        {
                            if (argIP[3] == plrIP[3] || argIP[3] == "*")
                            {
                                players.Add(plrs);
                            }
                        }
                    }
                }
            }
            return players;
        }
    }

}
