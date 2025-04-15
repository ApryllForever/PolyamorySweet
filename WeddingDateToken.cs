using StardewModdingAPI;
using StardewValley;

namespace PolyamorySweetLove

{ 
/// <summary>A token which returns the wedding date for an NPC
internal class WeddingDateToken
{
    ///Dictionary will be from ModEntry.SpouseWeddingDate

    // private Dictionary<string, int> NPCWeddingDate = new Dictionary<string, int>(); 

    private string weddingDate = String.Empty;    

    /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
    public bool AllowsInput()
    {
        return true;
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public bool CanHaveMultipleValues(string input = null)
    {
        return false;
    }

    /****
    ** State
    ****/
    /// <summary>Update the values when the context changes.</summary>
    /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
    public bool UpdateContext()
    {
            if(SaveGame.loaded?.player != null || Context.IsWorldReady)
            {
                return false;
            }

            if (Game1.weddingToday)
            {
                return true;
            }
            return false;
    }

    /// <summary>Get whether the token is available for use.</summary>
    public bool IsReady()
    {
            return (SaveGame.loaded?.player != null || Context.IsWorldReady);
        }

    /// <summary>Get the current values.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public IEnumerable<string> GetValues(string input)
    {
        // get name
        string name = input;
           // string weddingdate;
        if (string.IsNullOrWhiteSpace(name))
            yield break;
          //  int goat = Game1.Date.TotalDays;
            if (Game1.player.previousActiveDialogueEvents.ContainsKey("married_"+name))
            {
                Game1.player.previousActiveDialogueEvents.TryGetValue("married_"+name, out int weddingdate);

               // weddingdate = goat -= weddingdate;
                yield return weddingdate.ToString();
            }
           /*  
       NPC babe = Game1.getCharacterFromName(name);
            if(babe == null) yield break;
            if (babe.modData.ContainsKey("ApryllForever.PolyamorySweetLove/WeddingDate"))
            {
                babe.modData.TryGetValue("ApryllForever.PolyamorySweetLove/WeddingDate", out string weddingDate);
                

                yield return weddingDate;
            }
           */
        yield return "";
    }
}
}