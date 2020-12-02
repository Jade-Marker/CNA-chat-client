using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class HangmanGame
    {
        public bool GameRunning { get; private set; }
        private int _guessesLeft;
        private List<char> _correctLetters;
        private List<char> _incorrectLetters;
        private List<string> _words = new List<string>() { "computer", "hangman", "programming", "network", "protocol", "packet" };
        private string _word;
        private Random _random;

        public HangmanGame()
        {
            GameRunning = false;
            _correctLetters = new List<char>();
            _incorrectLetters = new List<char>();
            _random = new Random();
        }

        private bool ValidateGuess(string guess, out string response)
        {
            bool isValid = true;
            response = "";

            if (guess.Length != 1)
            {
                if (guess.Length == 0)
                    response = "That is not a valid guess";
                else if (_incorrectLetters.Contains(guess[0]) || _correctLetters.Contains(guess[0]))
                    response = guess + " is not a valid guess";

                isValid = false;
            }

            return isValid;
        }

        private bool WordGuessed()
        {
            for (int i = 0; i < _word.Length; i++)
            {
                if (!_correctLetters.Contains(_word[i]))
                    return false;
            }

            return true;
        }

        public void StartGame()
        {
            GameRunning = true;
            _guessesLeft = 6;

            _correctLetters.Clear();
            _incorrectLetters.Clear();

            _word = _words[_random.Next(0, _words.Count)];

            if (_word.Contains(' '))
                _correctLetters.Add(' ');
        }

        public List<string> GetBoard()
        {
            List<string> response = new List<string>();

            response.Add("     |------+  ");
            response.Add("     |          |  ");
            response.Add("     |         " + (_guessesLeft < 6 ? "O" : ""));
            response.Add("     |         "  + (_guessesLeft < 4 ? "/" : "") + (_guessesLeft < 5 ? "|" : "") + (_guessesLeft < 3 ? @"\" : ""));
            response.Add("     |         "  + (_guessesLeft < 2 ? "/" : "") + " " + (_guessesLeft < 1 ? @"\" : ""));
            response.Add("     |         ");
            response.Add("===============");

            string word = "";
            foreach (char letter in _word)
            {
                if (_correctLetters.Contains(letter))
                {
                    if (letter == ' ')
                        word += "  ";
                    else
                        word += letter;
                }
                else
                    word += "_ ";
            }
            response.Add(word);

            return response;
        }

        public List<string> Guess(string guess, string name)
        {
            List<string> response = new List<string>();

            response.Add(name + " guessed " + guess);

            string sResponse;
            if (ValidateGuess(guess, out sResponse))
            {
                if (_word.Contains(guess[0]))
                {
                    response.Add("The word contains a " + guess[0]);
                    _correctLetters.Add(guess[0]);
                }
                else 
                {
                    response.Add("The word does not contain a " + guess[0]);
                    _incorrectLetters.Add(guess[0]);
                    _guessesLeft--;
                }

                response.AddRange(GetBoard());
            }
            else
                response.Add(sResponse);

            if (WordGuessed())
            {
                response.Add("You win!");
                GameRunning = false;
            }
            else if (_guessesLeft == 0)
            {
                response.Add("You lose");
                GameRunning = false;
            }

            return response;
        }
    }
}
