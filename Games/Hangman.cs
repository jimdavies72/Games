using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
// I had to use NuGet to install. --> install-package System.Data.SqlClient
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace Games
{
   
    class Games
   {
        public class Hangman
        {

            public static void RunHangmanGame(string versionNumber)
            {
                /*
                 // Export the game wording to csv file for import into database
                 WordingController exportWording = new(true);
                 exportWording.ExportWording(@"C:\CS_Extract\");
                */


                // main game code starts here...
                int defaultGuesses = 10;
                string yN;
                bool continueGame = true;

                DataController dataController = new("GamesWindowsAuth");
                WordingController gameWording = new(dataController);

                Introduction(versionNumber, gameWording);

                while (continueGame)
                {
                    SettingsController gameSettings = new(defaultGuesses, gameWording);
                    Console.Clear();
                    PhraseController phraseController = new(gameSettings.Phrase);
                    gameWording.SetGameRulesText(gameSettings.PhraseWordCount, gameSettings.NumberOfGuesses, gameSettings.Clue, phraseController.DisplayCharacterList);
                    DisplayStartText(gameWording);
                    GuessController.RunGuesses(gameSettings, phraseController, gameWording);

                    Tools.WriteBlankLines(2);
                    Console.Write(gameWording.GetInGameText(11));
                    yN = string.Format(Console.ReadLine()).ToUpper();
                    if (yN != "Y")
                    {
                        continueGame = false;
                        Console.Clear();
                        Tools.WriteBlankLines(3);
                        Console.WriteLine(gameWording.GetInGameText(12));
                        Tools.Sleep();
                        Menu menu = new();
                        menu.DisplayMenu();
                    }
                    else 
                    {
                        Console.Clear();
                    }
                }

            }

            private static void Introduction(string versionNumber, WordingController gameWording, int optionalSplashLength = 2000)
            {
                Console.Clear();
                
                StringBuilder welcomeText = new();
                welcomeText.AppendLine(gameWording.GetInGameText(1));
                welcomeText.AppendLine(gameWording.GetInGameText(2));
                welcomeText.AppendLine("_____");
                welcomeText.AppendLine("|    |");
                welcomeText.AppendLine("|    |");
                welcomeText.AppendLine("|   \\@/");
                welcomeText.AppendLine("|    |");
                welcomeText.AppendLine("|    |");
                welcomeText.AppendLine("|   / \\");
                welcomeText.AppendLine("---");

                Console.WriteLine(Convert.ToString(welcomeText), versionNumber);

                if (optionalSplashLength > 0)
                {
                    if (optionalSplashLength <= 1999)
                    {
                        // The minimum splash time is 2 seconds to stop the screen 'flashing on'
                        Tools.Sleep();
                        Console.Clear();
                    }
                    else
                    {
                        Tools.Sleep(optionalSplashLength);
                        Console.Clear();
                    }

                }
                else
                {
                    // keep intro screen in view and put 5 blank lines between
                    Tools.WriteBlankLines(5);
                }
            }

            private static void DisplayStartText(WordingController gameWording)
            {
                StringBuilder gameWordingText = new();

                gameWordingText.AppendLine(gameWording.GetInGameText(10));
                gameWordingText.AppendLine(Tools.CreateUnderline(gameWording.GetInGameText(10)));
                gameWordingText.AppendLine(" ");
                gameWordingText.AppendLine(gameWording.SinglePluralPhraseText);
                gameWordingText.AppendLine(" ");
                gameWordingText.AppendLine(gameWording.NumberOfGuessesText);
                gameWordingText.AppendLine(" ");
                gameWordingText.AppendLine(gameWording.ClueIsText);
                gameWordingText.AppendLine(" ");
                gameWordingText.AppendLine(gameWording.StartText);
                Console.WriteLine(Convert.ToString(gameWordingText));
                Tools.WriteBlankLines(2);
            }
        }

        class SettingsController
        {
            private int numberOfGuesses;
            private string phrase;
            private int phraseWordCount;
            private string clue;

            public SettingsController(int defaultGuesses, WordingController gameWording)
            {
                bool gameSet = true;

                // Get number of guesses. loop until we have a correct number.
                while (gameSet)
                {
                    Console.Write(gameWording.GetInGameText(20), defaultGuesses);
                    string returnValue = Console.ReadLine().Trim();

                    if (returnValue.Length == 0)
                    {
                        NumberOfGuesses = defaultGuesses;
                        gameSet = false;
                    }
                    else
                    {
                        if (returnValue.All(Char.IsDigit))
                        {
                            NumberOfGuesses = Convert.ToInt32(returnValue);
                            if (NumberOfGuesses == 0)
                            {
                                // cant have 0 guesses!
                                Console.WriteLine(gameWording.GetInGameText(21)); //need to enter a legit number
                                gameSet = true;
                            }
                            else
                            {
                                gameSet = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine(gameWording.GetInGameText(21)); //need to enter a legit number
                            gameSet = true;
                        }
                    }
                }

                // Get phrase and clue
                gameSet = true;
                do
                {
                    Console.Write(gameWording.GetInGameText(22));
                    Phrase = Console.ReadLine().Trim();

                    if (Tools.HasLettersOrSpacesOnly(Phrase, true))
                    {
                        gameSet = false;
                    }
                    else
                    {
                        gameSet = true;
                    }
                } while (gameSet);

                Console.Write(gameWording.GetInGameText(23));
                Clue = Console.ReadLine().Trim();
                PhraseWordCount = Tools.GetPhraseWordCount(Phrase);
            }

            public int NumberOfGuesses
            {
                get
                {
                    return numberOfGuesses;
                }

                private set
                {
                    this.numberOfGuesses = value;

                }
            }

            public string Phrase
            {
                get
                {
                    return phrase;
                }

                private set
                {
                    this.phrase = value;

                }
            }

            public int PhraseWordCount
            {
                get
                {
                    return phraseWordCount;
                }

                private set
                {
                    this.phraseWordCount = value;

                }
            }

            public string Clue
            {
                get
                {
                    return clue;
                }

                private set
                {
                    this.clue = value;

                }
            }

            public override string ToString()
            {
                return String.Format("Game Settings:  Guesses: {0} - Phrase: {1} - Clue: {2}", NumberOfGuesses, Phrase, Clue);
            }
        }

        class WordingController
        {

            private string singlePluralPhraseText;
            private string numberOfGuessesText;
            private string clueIsText;
            private string startText;
            private List<GameText> inGameTextList = new();

            public WordingController(DataController dataController, bool setInGameTextLiterals = false)
            {
                InGameText = SetInGameText(dataController, setInGameTextLiterals);
            }

            public string SinglePluralPhraseText
            {
                get
                {
                    return singlePluralPhraseText;
                }

                private set
                {
                    singlePluralPhraseText = value;
                }
            }

            public string NumberOfGuessesText
            {
                get
                {
                    return numberOfGuessesText;
                }
                private set
                {
                    this.numberOfGuessesText = value;
                }
            }

            public string ClueIsText
            {
                get
                {
                    return clueIsText;
                }
                private set
                {
                    this.clueIsText = value;
                }
            }

            public string StartText
            {
                get
                {
                    return startText;
                }
                private set
                {
                    this.startText = value;
                }
            }

            public List<GameText> InGameText
            {
                get
                {
                    return inGameTextList;
                }
                set
                {
                    this.inGameTextList = value;
                }
            }

            public void SetGameRulesText(int wordCount, int numberOfGuesses, string clue, List<string> displayCharacterList)
            {
                SinglePluralPhraseText = SetSinglePuralPhraseText(wordCount);
                NumberOfGuessesText = SetNumberOfGuessesText(numberOfGuesses);
                ClueIsText = SetClueIsText(clue);
                StartText = SetStartText(displayCharacterList, wordCount);
            }

            private string SetSinglePuralPhraseText(int wordCount)
            {
                string singlePluralPhraseText;

                if (wordCount > 1)
                {
                    singlePluralPhraseText = String.Format(this.GetInGameText(91), Tools.NumberToWords(wordCount));
                }
                else
                {
                    singlePluralPhraseText = String.Format(this.GetInGameText(92), Tools.NumberToWords(wordCount));
                }

                return singlePluralPhraseText;
            }

            private string SetNumberOfGuessesText(int numberOfGuesses)
            {
                string numberOfGuessesText;

                if (numberOfGuesses == 1)
                {
                    numberOfGuessesText = String.Format(this.GetInGameText(93), Tools.NumberToWords(numberOfGuesses));
                }
                else
                {
                    numberOfGuessesText = String.Format(this.GetInGameText(94), Tools.NumberToWords(numberOfGuesses));
                }

                return numberOfGuessesText;
            }

            private string SetClueIsText(string clue)
            {
                string clueIsText;

                if (clue.Length > 0)
                {
                    clueIsText = String.Format(this.GetInGameText(95), char.ToUpper(clue[0]) + clue[1..]);
                }
                else
                {
                    clueIsText = this.GetInGameText(96);
                }

                return clueIsText;
            }

            private string SetStartText(List<string> displayCharacterList, int wordCount)
            {
                string displayText = Tools.ConvertListToString(displayCharacterList);

                string singlePluralText;

                if (wordCount > 1)
                {
                    singlePluralText = this.GetInGameText(97);
                }
                else
                {
                    singlePluralText = this.GetInGameText(98);
                }

                string startText = String.Format(this.GetInGameText(99), singlePluralText, displayText);


                return startText;
            }

            private List<GameText> SetInGameText(DataController dataController, bool writeEscapeChars = false)
            {

                /*
                writeEscapeChars = false by default
                writeEscapeChars = true IF you wish to add escape chars to the text as literals.
                wrap the text in an if () statement when/where you use escape chars in in the string.
                for example:

                if (writeEscapeChars == true)
                {
                    gameTextList.Add(new GameText(201, @"Sorry, You Lose! The correct answer was: '{0}'. \n\nBetter Luck Next Time!"));
                }
                else 
                {
                    gameTextList.Add(new GameText(201, "Sorry, You Lose! The correct answer was: '{0}'. \n\nBetter Luck Next Time!"));
                }

                */

                List<GameText> gameTextList = new();

                if (dataController !=null)
                {
                    StoredProcedure storedProcedure = new();
                    storedProcedure.StoredProcedureName = "GetInGameWording";
                    storedProcedure.ParamList.Add(new Tuple<string, SqlDbType, dynamic>("@LanguageID", SqlDbType.NChar, "Eng"));
                    storedProcedure.ParamList.Add(new Tuple<string, SqlDbType, dynamic>("@MenuItem", SqlDbType.NVarChar, "Hangman"));
                    DataTable dataTable = dataController.ExecuteStoredProc(storedProcedure);
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        gameTextList.Add(new GameText(Convert.ToInt32(dataTable.Rows[i]["WordingID"]), dataTable.Rows[i]["Wording"].ToString()));
                    }
                    

                }
                else
                {
                    // intro text
                    gameTextList.Add(new GameText(1, "***Welcome to the Hangman Game!***"));
                    gameTextList.Add(new GameText(2, "***Version {0} by James Davies***"));

                    // general game text
                    gameTextList.Add(new GameText(10, "The game is..."));
                    gameTextList.Add(new GameText(11, "Would you like to play another game? Y/N: "));
                    gameTextList.Add(new GameText(12, "Thank you for playing the Hangman Game! Goodbye!"));
                    gameTextList.Add(new GameText(13, "There has been an error, please contact your System Administrator for assistance."));
                    gameTextList.Add(new GameText(14, "Directory {0} Not Found, Do you want to create it Y/N? "));
                    gameTextList.Add(new GameText(15, "General Exception: {0} Caught."));
                    gameTextList.Add(new GameText(16, "Wording Export Complete."));

                    // game setting text
                    gameTextList.Add(new GameText(20, "How many guesses shall we have (default is {0})? "));
                    gameTextList.Add(new GameText(21, "You need to enter a number here (greater than 0)."));
                    gameTextList.Add(new GameText(22, "Enter your word/phrase without any numbers: "));
                    gameTextList.Add(new GameText(23, "Enter a clue if you wish: "));
                    gameTextList.Add(new GameText(24, "How many guesses shall we have(default is {0})?"));

                    // game rules text
                    gameTextList.Add(new GameText(91, "There are {0} words."));
                    gameTextList.Add(new GameText(92, "There is {0} word."));
                    gameTextList.Add(new GameText(93, "Guesses remaining: {0}"));
                    gameTextList.Add(new GameText(94, "Guesses remaining: {0}"));
                    gameTextList.Add(new GameText(95, "The clue is: {0}."));
                    gameTextList.Add(new GameText(96, "There is no clue for this game."));
                    gameTextList.Add(new GameText(97, "phrase"));
                    gameTextList.Add(new GameText(98, "word"));
                    gameTextList.Add(new GameText(99, "The {0} is: {1} (Type *quit at any time to quit playing)"));

                    // guessing text
                    gameTextList.Add(new GameText(100, "Guesses remaining: {0}"));
                    gameTextList.Add(new GameText(101, "Guesses remaining: {0}, Letters Attempted: {1} "));
                    gameTextList.Add(new GameText(102, "Enter your guess: "));
                    gameTextList.Add(new GameText(103, "You have already tried the guess '{0}' before, please try again!"));
                    gameTextList.Add(new GameText(104, "Incorrect guess, try again."));

                    // ending text
                    gameTextList.Add(new GameText(200, "Congratulations! You Win! The answer was: '{0}'."));
                    if (writeEscapeChars == true)
                    {
                        gameTextList.Add(new GameText(201, @"Sorry, You Lose! The correct answer was: '{0}'. \n\nBetter Luck Next Time!"));
                    }
                    else
                    {
                        gameTextList.Add(new GameText(201, "Sorry, You Lose! The correct answer was: '{0}'. \n\nBetter Luck Next Time!"));
                    }
                    gameTextList.Add(new GameText(202, "Sorry To See You Go!"));

                }

                return gameTextList;
            }
            public string GetInGameText(int id)
            {
                GameText text = inGameTextList.FirstOrDefault(z => z.ID == id);
                string result;

                if (text == null)
                {
                    //result = this.GetInGameText(13);
                    result = "There has been an error, please contact your System Administrator for assistance.";
                }
                else
                {
                    result = Regex.Unescape(text.TextValue);
                }

                return result;
            }

            public void ExportWording(string fileFolderLocation)
            {

                DateTime dateToday = DateTime.Now;
                string dateFormated = String.Format("{0:yyyyMMdd}", dateToday);

                if (fileFolderLocation.Substring(fileFolderLocation.Length - 1) != @"\")
                {
                    fileFolderLocation += @"\";
                }
                string fileLocation = string.Format(@fileFolderLocation + "WordingExtract_{0}.txt", dateFormated);

                bool retry = true;
                while (retry)
                {
                    try
                    {
                        string line = "";
                        using (TextWriter textWriter = new StreamWriter(fileLocation))
                        {
                            // as text will contains commas we can set the csv delimitter to be a ';' 
                            //textWriter.WriteLine("sep=;");

                            foreach (var item in InGameText)
                            {
                                // GameID (Hardcoded for now), Lang (Hard coded for now), Wording ID, Wording
                                line = string.Format("2; Eng; {0}; {1}", item.ID, item.TextValue);
                                textWriter.WriteLine(line);
                            }
                            retry = false;
                            textWriter.Close();
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        string returnValue = "";
                        Console.Clear();
                        Tools.WriteBlankLines(3);
                        Console.Write(this.GetInGameText(14), Path.GetDirectoryName(fileLocation));

                        returnValue = Console.ReadLine().ToUpper();
                        if (returnValue == "Y")
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
                            retry = true;
                        }
                        else
                        {
                            retry = false;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Tools.WriteBlankLines(3);
                        Console.WriteLine(this.GetInGameText(15), ex.Message);
                        retry = false;
                    }
                    finally
                    {
                        if (retry == false)
                        {
                            Console.Clear();
                            Tools.WriteBlankLines(3);
                            Console.WriteLine(this.GetInGameText(16));
                        }
                    }
                }
            }

            public override string ToString()
            {
                return String.Format(
                    "Single/Plural text: {0} \n" +
                    "Number of guesses text: {1} \n" +
                    "Clue text: {2}", singlePluralPhraseText, numberOfGuessesText, clueIsText);
            }
        }

        class GameText
        {
            private int id;
            private string textValue;

            public GameText(int _id, string _textValue)
            {
                id = _id;
                textValue = _textValue;
            }

            public int ID
            {
                get
                {
                    return id;
                }

                init
                {
                    id = value;
                }
            }

            public string TextValue
            {
                get
                {
                    return textValue;
                }

                init
                {
                    textValue = value;
                }
            }

        }

        class PhraseController
        {

            private List<string> phraseList = new();
            private List<string> guessedCharactersList = new();
            private List<string> displayCharactersList = new();
            private List<string> usedCharactersList = new();

            public PhraseController(string _phrase)
            {
                phraseList = SetPhraseList(_phrase);
                guessedCharactersList = SetClonedList(phraseList, ClonedListType.guessedList);
                displayCharactersList = SetClonedList(phraseList, ClonedListType.displayList);

            }

            public List<string> PhraseList // List of characters that make up the game word or phrase
            {
                get
                {
                    return phraseList;
                }

                init
                {
                    phraseList = value;
                }
            }

            public List<string> GuessedCharacterList
            {
                get
                {
                    return guessedCharactersList;
                }

                init
                {
                    guessedCharactersList = value;
                }
            } // List of characters that have YET to be correctly guessed

            public List<string> DisplayCharacterList
            {
                get
                {
                    return displayCharactersList;
                }

                init
                {
                    displayCharactersList = value;
                }
            } // List of how the phrase is displayed --> _ _ _ K / O _ _ 

            public List<string> UsedCharactersList
            {
                get
                {
                    return usedCharactersList;
                }

                private set
                {
                    usedCharactersList = value;
                }
            } // List of used (correct or incorrectly guessed) characters

            private List<string> SetPhraseList(string phrase)
            {
                List<string> phraseList = new();

                foreach (char character in phrase)
                {
                    string letter = String.Format(Convert.ToString(character)).ToUpper();
                    phraseList.Add(letter);
                }

                return phraseList;
            }

            private List<string> SetClonedList(List<string> phraseList, ClonedListType clonedListType)
            {
                // Cloning the original phrase list,swapping characters for '_'s
                // and removing spaces from it as spaces are not guessed within the game.

                List<string> clonedList = new(phraseList);

                for (int i = 0; i < clonedList.Count; i++)
                {
                    if (clonedListType == ClonedListType.guessedList)
                    {
                        //Guessed list
                        if (clonedList[i] == " ")
                        {
                            clonedList.Remove(clonedList[i]);
                        }
                    }
                    else
                    {
                        //Display List
                        if (clonedList[i] == " ")
                        {
                            clonedList[i] = "/ ";
                        }
                        else
                        {
                            clonedList[i] = "_ ";
                        }
                    }

                }
                return clonedList;

            }

            public void UpdateUsedCharacterList(string usedLetter)
            {
                UsedCharactersList.Add(usedLetter);
            }

            public string ShowUsedCharacters()
            {
                List<string> usedList = usedCharactersList;
                string result = "";

                usedList.Sort();
                foreach (var item in usedList)
                {

                    result += item.ToString() + " ";
                }

                return result;
            }

            public override string ToString()
            {
                return string.Join("", PhraseList.ToArray());
            }

            private enum ClonedListType
            {
                guessedList,
                displayList,
            }

        }

        class GuessController
        {
            public static void RunGuesses(SettingsController gameSettings, PhraseController phraseController, WordingController gameWording)
            {

                EndState endState = EndState.Null;
                int numberOfGuesses = gameSettings.NumberOfGuesses;
                GoodGuess goodGuess = GoodGuess.Null;

                while (numberOfGuesses > 0 && endState != EndState.Quit && endState != EndState.Win)
                {
                    Tools.WriteBlankLines(2);

                    if (phraseController.UsedCharactersList.Count == 0)
                    {
                        Console.WriteLine(gameWording.GetInGameText(100), numberOfGuesses);
                    }
                    else
                    {
                        Console.WriteLine(gameWording.GetInGameText(101), numberOfGuesses, phraseController.ShowUsedCharacters());
                    }

                    Tools.WriteBlankLines(1);
                    Console.Write(gameWording.GetInGameText(102));
                    string currentGuess = String.Format(Console.ReadLine().Trim().ToUpper());

                    //Player can either make a single letter or full answer guess.
                    // We need to establish which
                    if (currentGuess.Length > 1) // Word entered
                    {
                        if (currentGuess == "*QUIT")
                        {
                            endState = EndState.Quit;
                        }
                        else if (currentGuess == phraseController.ToString())
                        {
                            // player has correctly guessed the phrase outwright so win
                            goodGuess = GoodGuess.True;
                            endState = EndState.Win;
                        }
                        else
                        {
                            // bad word guess
                            goodGuess = GoodGuess.False;
                        }
                    }
                    else // single letter guess
                    {
                        if (currentGuess.Length == 0) //accidental return key hit (or ' ')
                        {
                            goodGuess = GoodGuess.Null;
                        }
                        else
                        {
                            // Check legal character entered
                            if (!Tools.HasLettersOrSpacesOnly(currentGuess)) //letters only
                            {
                                goodGuess = GoodGuess.Null;
                            }
                            else if (!phraseController.UsedCharactersList.Contains(currentGuess))
                            {
                                // legitiamate single character entry
                                // guess letter has not already been used.

                                phraseController.UpdateUsedCharacterList(currentGuess);

                                // do we have a match? start by assuming that we havent.
                                goodGuess = GoodGuess.False;

                                foreach (var (letter, index) in phraseController.PhraseList.Select((value, i) => (value, i)))
                                {
                                    if (letter.ToString() == currentGuess)
                                    {
                                        // we have a match
                                        phraseController.GuessedCharacterList.Remove(letter);
                                        phraseController.DisplayCharacterList[index] = letter + " ";
                                        goodGuess = GoodGuess.True;
                                    }

                                }

                            }
                            else
                            {
                                goodGuess = GoodGuess.Null;
                                Console.WriteLine(gameWording.GetInGameText(103), currentGuess);
                            }
                        }
                    }

                    Console.Clear();
                    Tools.WriteBlankLines(2);
                    Console.WriteLine(Tools.ConvertListToString(phraseController.DisplayCharacterList));

                    if (phraseController.GuessedCharacterList.Count == 0)
                    {
                        // All characters guessed, player is a winner!
                        endState = EndState.Win;
                    }

                    if (goodGuess == GoodGuess.False)
                    {
                        numberOfGuesses--;
                        if (numberOfGuesses > 0)
                        {
                            Tools.WriteBlankLines(2);
                            // incorrect guess
                            Console.WriteLine(gameWording.GetInGameText(104));
                        }
                        else
                        {
                            // no more guesses allowed
                            endState = EndState.Lose;
                        }

                    }

                }

                DisplayWinState(endState, phraseController, gameWording);

            }

            private static void DisplayWinState(EndState endState, PhraseController phraseController, WordingController gameWording)
            {
                Console.Clear();
                Tools.WriteBlankLines(3);
                switch (endState)
                {
                    case EndState.Win:
                        Console.WriteLine(gameWording.GetInGameText(200), phraseController);
                        break;
                    case EndState.Lose:
                        Console.WriteLine(gameWording.GetInGameText(201), phraseController);
                        break;
                    case EndState.Quit:
                        Console.WriteLine(gameWording.GetInGameText(202));
                        break;
                }

            }

            enum GoodGuess
            {
                Null,
                True,
                False,
            }

            enum EndState
            {
                Null,
                Win,
                Lose,
                Quit,
            }

        }
    }

    class DataController
    {
        // https://www.connectionstrings.com/

        private string dbConnectionString;

        public DataController(string configConnName)
        {
            DbConnectionString = Tools.GetDbConnectionSettings(configConnName);
        }

        public string DbConnectionString
        {
            get
            {
                return dbConnectionString;
            }

            init
            {
                dbConnectionString = value;
            }
        }
        
        public DataTable ExecuteStoredProc(StoredProcedure storedProcedure)
        {

            DataTable dataTable = new();

            using (SqlConnection conn = new(this.DbConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new(storedProcedure.StoredProcedureName, conn))
                {
                    try
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        foreach (Tuple<string, SqlDbType, dynamic> tuple in storedProcedure.ParamList)
                        {
                            cmd.Parameters.AddWithValue(tuple.Item1, tuple.Item2).Value = tuple.Item3;
                        }
                        
                        dataTable.Load(cmd.ExecuteReader());
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                        
                    }

                    return dataTable;
                }
            }
        }
    }

    class StoredProcedure
    {
        string storedProcedureName = "";
        List<Tuple<string, SqlDbType, dynamic>> paramList = new();

        public string StoredProcedureName
        {
            get
            {
                return storedProcedureName;
            }

            set
            {
                storedProcedureName = value;
            }
        }

        public List<Tuple<string, SqlDbType, dynamic>> ParamList
        {
            get
            {
                return paramList;
            }

            set
            {
                paramList = value;
            }
        }
    }

    class Menu
    {
        private string menuTitle = "";
        private List<MenuItem> menuItemList = new();

        public Menu()
        {
            DataController dataController = new("GamesWindowsAuth");
            menuItemList = SetMenuItems(dataController);
        }

        public string MenuTitle
        {
            get
            {
                return menuTitle;
            }
            private set
            {
                this.menuTitle = value;
            }
        }

        public List<MenuItem> MenuItemList
        {
            get
            {
                return menuItemList;
            }
            private set
            {
                this.menuItemList = value;
            }
        }

        private List<MenuItem> SetMenuItems(DataController dataController) 
        {
            List<MenuItem> menuItemList = new();

            if (dataController != null)
            {
                StoredProcedure storedProcedure = new();
                storedProcedure.StoredProcedureName = "GetMenu";
                DataTable dataTable = dataController.ExecuteStoredProc(storedProcedure);
                
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    int menuLevel = Convert.ToInt32(dataTable.Rows[i]["MenuLevel"]);

                    if (menuLevel == 0)
                    {
                        // Level 0 is the Menu Title
                        menuTitle = dataTable.Rows[i]["MenuItem"].ToString();
                    }
                    else
                    {
                        // Level >=1 are menu items

                        menuItemList.Add(new MenuItem(
                        i,
                        Convert.ToInt32(dataTable.Rows[i]["Parent"]),
                        dataTable.Rows[i]["MenuItem"].ToString(),
                        dataTable.Rows[i]["Description"].ToString(),
                        dataTable.Rows[i]["VersionNumber"].ToString(),
                        Convert.ToInt32(dataTable.Rows[i]["MenuLevel"]),
                        dataTable.Rows[i]["ExecuteTypeName"].ToString(),
                        dataTable.Rows[i]["ExecuteMethodName"].ToString(),
                        dataTable.Rows[i]["ExecuteMethodParams"].ToString()
                        ));
                    }
                }
            }

            return menuItemList;
        }

        public void DisplayMenu() 
        {
            string userChoice;
            bool menuSet;

            do
            {
                Console.Clear();
                Tools.WriteBlankLines(3);
                Console.WriteLine(MenuTitle);
                Console.WriteLine(Tools.CreateUnderline(MenuTitle));
                Tools.WriteBlankLines(2);
                foreach (var item in menuItemList)
                {
                    if (item.MenuItemDescription != "")
                    {
                        Console.WriteLine("{0})  {1}. - {2}", item.MenuItemNumber, item.MenuItemTitle, item.MenuItemDescription);
                    }
                    else 
                    {
                        Console.WriteLine("{0})  {1}.", item.MenuItemNumber, item.MenuItemTitle);
                    }
                    
                }

                //
                Tools.WriteBlankLines(2);
                Console.Write("Enter choice: ");

                userChoice = Console.ReadLine().Trim();
                if (Tools.HasDigitsOnly(userChoice))
                {
                    menuSet = false;
                }
                else
                {
                    menuSet = true;
                }
            } while (menuSet);

            ExecuteMenu(menuItemList, userChoice);

        }
        private void ExecuteMenu(List<MenuItem> menuItemList, string userChoice) 
        {
            MenuItem selectedMenuItem = menuItemList.Where(z => z.MenuItemNumber == Convert.ToInt32(userChoice)).FirstOrDefault();

            if (selectedMenuItem != null)
            {
                if (selectedMenuItem.TypeName != "")
                {
                    Tools.InvokeStringMethod(selectedMenuItem.TypeName, selectedMenuItem.MethodName, selectedMenuItem.MethodParams);
                }
                else
                {
                    DisplayMenu();
                }
            }
            else 
            {
                DisplayMenu(); 
            }
        }

        

    }

    class MenuItem
    {
        private int menuItemNumber;
        private int parentItem;
        private string menuItemTitle;
        private string menuItemDescription;
        private string itemVersionNumber;
        private int menuLevel;
        private string typeName;
        private string methodName;
        private string methodParams;


        public MenuItem(int _menuItemNumber, int _parentItem, string _menuItemTitle, string _menuItemDescription,
                        string _itemVersionNumber, int _menuLevel, string _typeName,
                        string _methodName, string _methodParams)
        {
            menuItemNumber = _menuItemNumber;
            parentItem = _parentItem;
            menuItemTitle = _menuItemTitle;
            menuItemDescription = _menuItemDescription;
            itemVersionNumber = _itemVersionNumber;
            menuLevel = _menuLevel;
            typeName = _typeName;
            methodName = _methodName;
            methodParams = _methodParams;
        }

        public int MenuItemNumber
        {
            get
            {
                return menuItemNumber;
            }
            init
            {
                this.menuItemNumber = value;
            }
        }

        public int ParentItem
        {
            get
            {
                return parentItem;
            }
            init
            {
                this.parentItem = value;
            }
        }

        public string MenuItemTitle
        {
            get
            {
                return menuItemTitle;
            }
            init
            {
                this.menuItemTitle = value;
            }
        }

        public string MenuItemDescription
        {
            get
            {
                return menuItemDescription;
            }
            init
            {
                this.menuItemDescription = value;
            }
        }

        public string ItemVersionNumber
        {
            get
            {
                return itemVersionNumber;
            }
            init
            {
                this.itemVersionNumber = value;
            }
        }

        public int MenuLevel
        {
            get
            {
                return menuLevel;
            }
            init
            {
                this.menuLevel = value;
            }
        }

        public string TypeName
        {
            get
            {
                return typeName;
            }
            init
            {
                this.typeName = value;
            }
        }

        public string MethodName
        {
            get
            {
                return methodName;
            }
            init
            {
                this.methodName = value;
            }
        }

        public string MethodParams
        {
            get
            {
                return methodParams;
            }
            init
            {
                this.methodParams = value;
            }
        }

    }

    class PlayerController 
    {
        //Player Controller code here...
    
    }

    class Player 
    {
        int playerID;
        string nickname;
        string language;

        public Player(int _playerID, string _nickname, string _language) 
        {
            playerID = _playerID;
            nickname = _nickname;
            language = _language;
        }

        public int PlayerID
        {
            get 
            { 
                return playerID;
            } 
            set
            {
                playerID = value;
            }



        }

        public string Nickname
        {
            get
            {
                return nickname;
            }
            init
            {
                nickname = value;
            }
        }

        public string Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
            }
        }

    }

    class Tools 
    {
        public static bool HasLettersOrSpacesOnly(string phrase, bool includeSpaces = false) 
        {
            Regex regex;

            if (includeSpaces == true)
            {
                regex = new Regex("^[A-Za-z ]+$");
            }
            else 
            {
                regex = new Regex("^[A-Za-z]+$");
            }
            
            if (regex.IsMatch(phrase))
            {
                return true;
            }
            else 
            {
                return false;            
            }
           
        }

        public static bool HasDigitsOnly(string value) 
        {
            Regex regex = new(@"^\d+$");

            if (regex.IsMatch(value))
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        
        public static string NumberToWords(int number)
        {
            /* 
                This code was provided by MSDN at the following location:
                https://social.msdn.microsoft.com/Forums/vstudio/en-US/0b1dec9c-61e9-4544-8134-bda1264a21a4/how-to-convert-number-into-words-in-c?forum=csharpgeneral

            */
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public static int GetPhraseWordCount(string Phrase)
        {
            // how many words in the phrase
            int count = 1;

            foreach (char letter in Phrase)
            {
                if (Convert.ToString(letter) == " ")
                {
                    count++;
                }
            }
            return count;
        }

        public static string ConvertListToString(List<string> list) 
        {
            string returnString = string.Join("", list.ToArray());

            return returnString;

        }

        public static void WriteBlankLines(int numberOfLines = 1) 
        {
            Console.Write(new string('\n', numberOfLines));
        }

        public static string Alphabetise(string sourceString)
        {
            // adapted from https://www.dotnetperls.com/alphabetize-string

            // Convert to char array.
            char[] charArray = sourceString.ToCharArray();

            // Sort letters.
            Array.Sort(charArray);
            
            // Return modified string.
            return new string(charArray);
        }

        public static string CreateUnderline(string wordToUnderline) 
        {
            string underline = "";

            for (int i = 0; i < wordToUnderline.Length; i++)
            {
                underline += "_";
            }

            return underline;
        
        }

        public static void GetConnectionStrings()
        {
            // retrieve the current connection string settings from app.config.
            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    Console.WriteLine(cs.Name);
                    Console.WriteLine(cs.ConnectionString);
                }
            }
        }

        public static string GetDbConnectionSettings(string configConnName)
        {
            string connectionString = null;
            // Connection String stored in the app Config File....
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[configConnName];

            if (settings != null)
            {
                connectionString = settings.ConnectionString;
            }

            return connectionString;
        }

        public static string InvokeStringMethod(string typeName, string methodName, string stringParam = "")
        {
            //Revised from from:
            //https://www.codeproject.com/Articles/19911/Dynamically-Invoke-A-Method-Given-Strings-with-Met

            try
            {
                // Get the Type for the class
                Type calledType = Type.GetType(typeName);

                String s;

                // Invoke the method itself. The string returned by the method winds up in s
                if (calledType.GetMethod(methodName).GetParameters().Length > 0)
                {
                    s = (String)calledType.InvokeMember(
                                methodName,
                                BindingFlags.InvokeMethod | BindingFlags.Public |
                                    BindingFlags.Static,
                                null,
                                null,
                                new Object[] { stringParam });
                } 
                else 
                {
                    s = (String)calledType.InvokeMember(
                                    methodName,
                                    BindingFlags.InvokeMethod | BindingFlags.Public |
                                        BindingFlags.Static,
                                    null,
                                    null,
                                    null);
                }

                // Return the string that was returned by the called method.
                return s;
            }
            catch (Exception ex)
            {
                string exitmessage = String.Format("An exception occured: {0}.\nPlease report this to your System Administrator. \nThe application will now close.", ex.Message);
                Tools.ExitConsole(exitmessage);
                return ex.Message;
            }
        }

        public static void ExitConsole(string exitWording = "")
        {
            Console.Clear();
            Tools.WriteBlankLines(2);
            if (exitWording !="")
            {
                // no wording to read so just exit without wait
                Console.WriteLine(exitWording);
                Tools.Sleep(3000);
                
            }
            Environment.Exit(0);
        }

        public static void Sleep(int sleepLengthInMs = 2000) 
        {
            System.Threading.Thread.Sleep(sleepLengthInMs);

        }
    }
}
