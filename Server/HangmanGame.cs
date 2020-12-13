using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class HangmanGame
    {
        public bool GameRunning { get; private set; }
        public bool GameStarting { get; private set; }
        public List<string> Players { get; private set; }

        private int _guessesLeft;
        private List<char> _correctLetters;
        private List<char> _incorrectLetters;
        private List<string> _words = new List<string>() { "computer", "hangman", "programming", "network", "protocol", "packet" };
        private string _word;
        private Random _random;

        private readonly List<char> cValidGuesses = new List<char>() {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };

        public HangmanGame()
        {
            GameRunning = false;
            GameStarting = false;
            _correctLetters = new List<char>();
            _incorrectLetters = new List<char>();
            _random = new Random();
            Players = new List<string>();
        }

        private void ResetPlayerList()
        {
            Players.Clear();
        }

        public void AddPlayer(string name)
        {
            Players.Add(name);
        }

        public void Starting()
        {
            GameStarting = true;
        }

        private bool ValidateGuess(string guess, out string response)
        {
            bool isValid = true;
            response = "";

            if (guess.Length == 0 || guess.Length > 1)
            {
                response = "That is not a valid guess";
                isValid = false;
            }
            else if (_incorrectLetters.Contains(guess[0]) || _correctLetters.Contains(guess[0]))
            {
                response = guess + " has already been guessed";
                isValid = false;
            }
            else if (!cValidGuesses.Contains(guess[0]))
            {
                response = guess + " is not a letter";
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
            GameStarting = false;
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

            guess = guess.ToLower();
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
                ResetPlayerList();
            }
            else if (_guessesLeft == 0)
            {
                response.Add("You lose");
                GameRunning = false;
                ResetPlayerList();
            }

            return response;
        }
    }
}
