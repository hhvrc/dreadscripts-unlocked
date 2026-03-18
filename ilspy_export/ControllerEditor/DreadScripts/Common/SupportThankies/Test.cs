namespace DreadScripts.Common.SupportThankies;

internal static class Test
{
	internal static readonly string[] _Role = new string[13]
	{
		"Goofy Goobers", "Kofi Lovers", "Just People", "Pookies", "Friendos", "Frens", "Cultists", "Dreadlings", "Epic Gamers", "Y'all",
		"Pals", "Buddies", "Gang Gang"
	};

	internal static readonly string[] m_Model = new string[15]
	{
		"♡ Thanks to these lovely people ♡", "♡ You are the champions ♡", "♡ Couldn't do it without them ♡", "♡ Overwhelmed by their support ♡", "♡ Fueled by their love and support ♡", "♡ I have been thoroughly supported ♡", "♡ Overly caffeinated by these peeps ♡", "They sorta like my stuff", "♡ I'd learn shader code for them ♡", "♡ Literally the best people ever ♡",
		"I'm not crying, you're crying!", "Jokes on them, I'm a terrible person", "Slightly better than my cat", "XOXO", "pls support"
	};

	internal static readonly string[] m_Tokenizer = new string[14]
	{
		"Thanks to you too ♡", "You're cool too ♡", "Got Kofi?", "Join the cool kids club", "You better be on this list", "You look like you'd be a good supporter ;)", "Remember to support your local devs", "Support ya boi", "Eat your veggies", "Eat tight. Sleep healthy.",
		"HYDRATE. NOW.", "Check your posture", "Use code DREADSCRIPTS for 10% off (lie)", "You're now blinking and breathing manually"
	};

	internal static readonly string[] decorator = new string[23]
	{
		"The Goofiest Goober", "Chad", "Epic Gamer", "Top Tier", "Chat is this real?!", "Friendo of the Year", "Quality Human", "Real MVP", "Very very cool", "I threatened them to support me",
		"Had to bribe them with snacks", "Not a bot", "Can beat Goku", "Makes the best coffee", "Makes a mean applepie", "They're behind you.", "Why would they do this?", "They're cool at parties", "Might be the impostor", "The peak of human evolution",
		"The friend we make along the way", "OwO", "W"
	};

	private static Test ChangeIndexer;

	internal static bool CalculateIndexer()
	{
		return ChangeIndexer == null;
	}
}
