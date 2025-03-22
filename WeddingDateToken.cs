using StardewModdingAPI;
using StardewValley;

namespace PolyamorySweetLove

{ 
/// <summary>A token which returns the wedding date for an NPC
internal class WeddingDateToken
{
    ///Dictionary will be from ModEntry.SpouseWeddingDate

    // private Dictionary<string, int> NPCWeddingDate = new Dictionary<string, int>(); 

    private int weddingDate = new int();    

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
            return (SaveGame.loaded?.player != null || Context.IsWorldReady);
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
            int weddingdate;
        if (string.IsNullOrWhiteSpace(name))
            yield break;

       NPC babe = Game1.getCharacterFromName(name);

            weddingdate =  babe.WeddingDate();

      
            {
                yield return weddingDate.ToString();
            }

        yield return "";
    }
}
}