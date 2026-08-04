// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/SupporterStrings.cs
//
//   _Role       -> WindowTitles,      line 5
//   m_Model     -> HeaderTexts,       line 11
//   m_Tokenizer -> HeaderTooltips,    line 17
//   decorator   -> SupporterTooltips, line 23
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// Audit status: PARTIAL -- all four arrays were compared entry for entry against decompiled/ on
// 2026-08-05 and match.

namespace DreadScripts.Common
{
    /// <summary>
    /// The randomised copy for the support window. One entry from each list is picked per window,
    /// so the thank-you screen reads slightly differently every time it is opened.
    /// </summary>
    internal static class SupporterStrings
    {
        /// <summary>Window titles.</summary>
        internal static readonly string[] WindowTitles =
        {
            "Goofy Goobers", "Kofi Lovers", "Just People", "Pookies", "Friendos", "Frens", "Cultists", "Dreadlings", "Epic Gamers", "Y'all",
            "Pals", "Buddies", "Gang Gang"
        };

        /// <summary>Headings shown above the supporter grid.</summary>
        internal static readonly string[] HeaderTexts =
        {
            "♡ Thanks to these lovely people ♡", "♡ You are the champions ♡", "♡ Couldn't do it without them ♡", "♡ Overwhelmed by their support ♡", "♡ Fueled by their love and support ♡", "♡ I have been thoroughly supported ♡", "♡ Overly caffeinated by these peeps ♡", "They sorta like my stuff", "♡ I'd learn shader code for them ♡", "♡ Literally the best people ever ♡",
            "I'm not crying, you're crying!", "Jokes on them, I'm a terrible person", "Slightly better than my cat", "XOXO", "pls support"
        };

        /// <summary>Tooltips for the heading.</summary>
        internal static readonly string[] HeaderTooltips =
        {
            "Thanks to you too ♡", "You're cool too ♡", "Got Kofi?", "Join the cool kids club", "You better be on this list", "You look like you'd be a good supporter ;)", "Remember to support your local devs", "Support ya boi", "Eat your veggies", "Eat tight. Sleep healthy.",
            "HYDRATE. NOW.", "Check your posture", "Use code DREADSCRIPTS for 10% off (lie)", "You're now blinking and breathing manually"
        };

        /// <summary>Fallback tooltip for a supporter card that carries no <c>tooltip</c> attribute.</summary>
        internal static readonly string[] SupporterTooltips =
        {
            "The Goofiest Goober", "Chad", "Epic Gamer", "Top Tier", "Chat is this real?!", "Friendo of the Year", "Quality Human", "Real MVP", "Very very cool", "I threatened them to support me",
            "Had to bribe them with snacks", "Not a bot", "Can beat Goku", "Makes the best coffee", "Makes a mean applepie", "They're behind you.", "Why would they do this?", "They're cool at parties", "Might be the impostor", "The peak of human evolution",
            "The friend we make along the way", "OwO", "W"
        };
    }
}
