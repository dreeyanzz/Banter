using System.Text.RegularExpressions;

namespace Banter.Utilities
{
    /// <summary>
    /// Provides methods for detecting and censoring profane language in strings.
    /// </summary>
    public static class ProfanityChecker
    {
        /// <summary>
        /// Core list of profane words used for both simple and robust checking.
        /// </summary>
        private static readonly string[] ProfaneWordsArray =
        [
            // Core Profanities & Variants
            "fuck",
            "fucker",
            "fucks",
            "fucking",
            "fucked",
            "motherfucker",
            "faggot",
            "fag",
            "shit",
            "shitter",
            "shitting",
            "shits",
            "shitty",
            "shittiest",
            "asshole",
            "ass",
            "asses",
            "asshat",
            "asswipe",
            "arse",
            "arses",
            "bitch",
            "bitches",
            "bitching",
            "bitchy",
            "sonofabitch",
            "cunt",
            "cunts",
            "cunting",
            "damn",
            "damning",
            "damnit",
            "goddamn",
            "goddamnit",
            "hell",
            "hella",
            "hells",
            "piss",
            "pissing",
            "pissed",
            "pisses",
            // Insults & Derogatory Terms
            "bastard",
            "bastards",
            "wanker",
            "wankers",
            "bollocks",
            "bollockseds",
            "twat",
            "twats",
            "dickhead",
            "dumbass",
            "dumbasses",
            "moron",
            "idiot",
            "cock",
            "cocks",
            "cocker",
            "cockhead",
            "cockmaster",
            "dick",
            "dicks",
            "prick",
            "pricks",
            "jackass",
            "loser",
            "whore",
            "whores",
            "slut",
            "sluts",
            "slag",
            "tramp",
            "harlot",
            "scum",
            "lowlife",
            "trash",
            "filth",
            "weasel",
            "puke",
            "vomit",
            // Explicit Anatomical/Sexual
            "penis",
            "vagina",
            "clitoris",
            "breasts",
            "nipple",
            "erection",
            "ejaculate",
            "sperm",
            "semen",
            "semenal",
            "orgasm",
            "masturbate",
            "boner",
            "tits",
            "pussy",
            "coochie",
            "blowjob",
            "handjob",
            "rimjob",
            "sex",
            "sexting",
            "sexual",
            "porn",
            "pornography",
            "pedophile",
            "incest",
            "rape",
            "rapist",
            "childsex",
            "prostitute",
            "gay",
            "lesbian",
            "queer",
            "homo",
            "tranny", // Contextual - often used as slurs
            // Stronger Verbs and Actions
            "kill",
            "killing",
            "murder",
            "suicide",
            "stab",
            "shoot",
            "choke",
            "strangle",
            "bomb",
            "terrorist",
            "terrorism",
            "nuke",
            "genocide",
            "violence",
            "abuse",
            "abusing",
            "dope",
            "drug",
            "drugs",
            "cocaine",
            "heroin",
            "meth",
            "weed",
            "stoner",
            // Contextual/Ambiguous Insults (Use with caution due to high false positives)
            "retard",
            "retarded",
            "spastic",
            "cripple",
            "gook",
            "chink",
            "jap",
            "kike",
            "nigger",
            "spic", // **Hate Speech & Racial Slurs**
            "muslim",
            "jew",
            "christian", // Religious terms, included if contextually used for hate/slurs
            "autistic",
            "schizo", // Mental health terms often used offensively
            // Compound/Phrasal (Good for simple whole-word matching)
            "assclown",
            "douchebag",
            "cumbucket",
            "shithead",
            "scumbag",
            "motherfucking",
            "pieceofshit",
            "sonofabitch",
            "tosser",
            "shithole",
            // Core Bisaya Profanities (Often mixed with Tagalog/English)
            "yawa", // General strong curse, often translated as "devil" or "damn"
            "ywa", // Common abbreviation/slang for yawa
            "pisting yawa", // Stronger phrase meaning "damned devil" or "fucking devil"
            "pota", // Derived from Spanish/Tagalog "puta" (whore)
            "peste", // Meaning "pest," used as a curse
            "otin",
            // Insults and Derogatory Terms
            "libog", // Meaning "horny" or "lustful"
            "buang", // Meaning "crazy" or "insane," often used as a light insult
            "way buot", // Meaning "no sense" or "irresponsible"
            "bogo", // Meaning "stupid" or "dull"
            "baga", // Shorthand for "bagag nawng" (thick-faced/shameless)
            "atay", // Literally "liver," but used as a mild exclamatory curse
            "sagdi", // Slang version of a strong insult, similar to "damn it"
            // Anatomical/Sexual
            "otot", // Meaning "fart," sometimes used as a mild curse
            "kiki", // Slang for female genitalia (vagina)
            "otol", // Slang for male genitalia (penis)
            "titi", // Slang for male genitalia (penis)
            "jakol", // Derived from Tagalog/English slang for masturbation (handjob)
            "iyot", // Vulgar slang for sexual intercourse
            "kiat", // Meaning "flirtatious" or "promiscuous"
            "hubag", // Meaning "swollen" or "drunk," but can be used vulgarly
            // Mixing Filipino/Tagalog (often used interchangeably in Bisaya contexts)
            "ulol", // Tagalog for "crazy," also used in Bisaya areas
            "putangina", // Strong Tagalog curse (motherfucker)
            "tangina",
            "gago", // Tagalog for "stupid/idiot"
            "leche", // Spanish/Filipino for "milk," used as a mild curse
        ];

        /// <summary>
        /// Regex for simple whole-word profanity matching.
        /// </summary>
        private static readonly Regex SimpleProfanityRegex = new(
            @"\b(" + string.Join("|", ProfaneWordsArray) + @")\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Maps common leetspeak characters back to standard letters for normalization.
        /// </summary>
        private static readonly Dictionary<char, char> LeetMap = new()
        {
            { '0', 'o' },
            { '1', 'l' },
            { '3', 'e' },
            { '4', 'a' },
            { '5', 's' },
            { '6', 'g' },
            { '7', 't' },
            { '8', 'b' },
            { '9', 'g' },
            { '!', 'i' },
            { '$', 's' },
            { '@', 'a' },
            { 'o', 'u' },
            { 'u', 'o' },
            { 'i', 'e' },
            { 'e', 'i' },
        };

        /// <summary>
        /// Replaces whole, clearly defined profane words with asterisks of the same length.
        /// This method is fast but can be easily bypassed.
        /// </summary>
        /// <param name="text">The input text to censor.</param>
        /// <returns>The censored text.</returns>
        public static string CensorTextSimple(string text)
        {
            return SimpleProfanityRegex.Replace(text, match => new string('*', match.Length));
        }

        /// <summary>
        /// Attempts to censor messages even when characters are substituted (leetspeak) or separated by non-alphanumeric symbols.
        /// This method is more thorough but slower than <see cref="CensorTextSimple"/>.
        /// </summary>
        /// <param name="text">The input text to censor.</param>
        /// <returns>The censored text.</returns>
        public static string CensorTextRobust(string text)
        {
            string originalText = text;

            // --- Step A: Normalize the text for matching ---
            // 1. Remove non-alphanumeric characters (like spaces, punctuation, etc.)
            // 2. Convert to lowercase.
            // 3. Convert leetspeak/symbols (like '0', '4', '@') back to standard letters.
            string normalizedText = new(
                [
                    .. text.Where(c => char.IsLetterOrDigit(c) || LeetMap.ContainsKey(c)) // Only keep relevant chars
                        .Select(c =>
                        {
                            char lowerC = char.ToLower(c);
                            return LeetMap.TryGetValue(lowerC, out char mappedChar)
                                ? mappedChar
                                : lowerC;
                        }),
                ]
            );

            // --- Step B: Find profane substrings in the normalized text ---
            // This is O(N*M) where N is normalizedText length and M is ProfaneWords count,
            // but necessary to find split/leetspeak words.
            foreach (string word in ProfaneWordsArray)
            {
                int startIndex = -1;
                while ((startIndex = normalizedText.IndexOf(word, startIndex + 1)) != -1)
                {
                    // Match found in normalized text (e.g., 'f u c k' became 'fuck')
                    int matchLength = word.Length;

                    // --- Step C: Map the match back to the original text and censor ---

                    // We need to find the original start index in the *original* text
                    int originalStart = -1;
                    int originalEnd = -1;

                    // Count how many alphanumeric/leet characters are *before* the match
                    int preMatchCharCount = 0;
                    for (int i = 0; i < startIndex; i++)
                    {
                        if (
                            char.IsLetterOrDigit(originalText[i])
                            || LeetMap.ContainsKey(originalText[i])
                        )
                            preMatchCharCount++;
                    }

                    // Find the original start index by counting only the relevant characters
                    // until we hit preMatchCharCount + 1 (start of the match).
                    int relevantCount = 0;
                    for (int i = 0; i < originalText.Length; i++)
                    {
                        if (
                            char.IsLetterOrDigit(originalText[i])
                            || LeetMap.ContainsKey(originalText[i])
                        )
                        {
                            if (relevantCount == startIndex)
                                originalStart = i;
                            relevantCount++;

                            if (relevantCount == startIndex + matchLength)
                            {
                                originalEnd = i;
                                break;
                            }
                        }
                    }

                    // If we found a range, censor the original string
                    if (originalStart != -1 && originalEnd != -1)
                    {
                        // Create a replacement string of asterisks covering the original characters
                        string replacement = new('*', originalEnd - originalStart + 1);

                        // Replace the segment in the original text
                        originalText = originalText
                            .Remove(originalStart, originalEnd - originalStart + 1)
                            .Insert(originalStart, replacement);
                    }

                    // This is a complex mapping, but it handles split/leetspeak censorship correctly.
                }
            }

            return originalText;
        }

        // Renamed your original public method for clarity (optional)
        // public static string CensorText(string text) => CensorTextSimple(text);
    }
}
