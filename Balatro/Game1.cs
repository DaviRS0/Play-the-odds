using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Balatro
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _playButtonTexture;
        private Texture2D _quitButtonTexture;
        private Texture2D _playHandButtonTexture;
        private Texture2D _discardButtonTexture;
        private Rectangle _playButtonRectangle;
        private Rectangle _quitButtonRectangle;
        private Rectangle _playHandButtonRectangle;
        private Rectangle _discardButtonRectangle;
        private bool _isGameStarted;
        private bool _isGameOver;
        private bool _isWin;
        private bool _showWinMessage;
        private double _winMessageTimer;
        private double _gameOverTimer;

        private List<Card> _cards;
        private MouseState _previousMouseState;
        private Dictionary<string, Texture2D> _cardTextures;
        private int _score;
        private int _totalScore;
        private int _totalScoreForAllLevels;
        private int _roundsPlayed;
        private int _discardCount;
        private int _currentLevel;
        private int _requiredScore;
        private const int MaxDiscards = 3;
        private const int MaxLevels = 5;
        private const double WinMessageDuration = 5.0; // 5 seconds

        private Vector2 _youLosePosition = new Vector2(100, 100); // Adjust position as needed
        private Rectangle _quitButtonRect;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1080;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("ScoreFont");
            _playButtonTexture = Content.Load<Texture2D>("buttons/Play");
            _quitButtonTexture = Content.Load<Texture2D>("buttons/Quit");
            _playHandButtonTexture = Content.Load<Texture2D>("buttons/Play_hand");
            _discardButtonTexture = Content.Load<Texture2D>("buttons/Discard");

            int buttonWidth = Math.Max(_playButtonTexture.Width, _quitButtonTexture.Width);
            int buttonHeight = Math.Max(_playButtonTexture.Height, _quitButtonTexture.Height);

            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;

            int totalWidth = buttonWidth * 2 + 20;
            int startX = (screenWidth - totalWidth) / 2;
            int startY = (screenHeight - buttonHeight) / 2;

            _playButtonRectangle = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            _quitButtonRectangle = new Rectangle(startX + buttonWidth + 20, startY, buttonWidth, buttonHeight);

            _playHandButtonRectangle = new Rectangle((screenWidth - totalWidth) / 2, screenHeight - buttonHeight - 20, buttonWidth, buttonHeight);
            _discardButtonRectangle = new Rectangle((screenWidth + 20) / 2, screenHeight - buttonHeight - 20, buttonWidth, buttonHeight);
           
            _youLosePosition = new Vector2((GraphicsDevice.Viewport.Width - _font.MeasureString("You Lose").X) / 2, (GraphicsDevice.Viewport.Height - _font.MeasureString("You Lose").Y) / 2);
            _quitButtonRect = new Rectangle((GraphicsDevice.Viewport.Width - _quitButtonTexture.Width) / 2, GraphicsDevice.Viewport.Height - _quitButtonTexture.Height - 20, _quitButtonTexture.Width, _quitButtonTexture.Height);

            _cardTextures = new Dictionary<string, Texture2D>();
            foreach (string suit in CardUtils.Suits)
            {
                foreach (string rank in CardUtils.Ranks)
                {
                    string cardName = $"{suit}_{rank}";
                    _cardTextures[cardName] = Content.Load<Texture2D>($"{suit}/{cardName}");
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (_isGameOver)
            {
                _gameOverTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_gameOverTimer <= 0)
                {
                    _isGameStarted = false;
                    _isGameOver = false;
                }
            }
            else
            {
                MouseState mouseState = Mouse.GetState();
                KeyboardState keyboardState = Keyboard.GetState();

                if (_showWinMessage)
                {
                    _winMessageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    if (_winMessageTimer <= 0)
                    {
                        StartNextRound();
                    }
                    return;
                }

                if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (!_isGameStarted)
                    {
                        if (_playButtonRectangle.Contains(mouseState.Position))
                        {
                            StartGame();
                        }
                        else if (_quitButtonRectangle.Contains(mouseState.Position))
                        {
                            Exit();
                        }
                    }
                    else if (_isGameOver)
                    {
                        // No action needed for quit button as it is removed
                    }
                    else
                    {
                        if (_playHandButtonRectangle.Contains(mouseState.Position))
                        {
                            PlayHand();
                        }
                        else if (_discardButtonRectangle.Contains(mouseState.Position) && _discardCount < MaxDiscards)
                        {
                            DiscardCards();
                        }
                        else
                        {
                            Point mousePosition = new Point(mouseState.X, mouseState.Y);
                            int selectedCardCount = _cards.Count(card => card.IsSelected);

                            foreach (var card in _cards)
                            {
                                if (card.Bounds.Contains(mousePosition))
                                {
                                    if (card.IsSelected || selectedCardCount < 5)
                                    {
                                        card.ToggleSelection();
                                        _score = CardUtils.CalculateScore(_cards);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (_isGameStarted)
                {
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        _isGameStarted = false;
                    }

                    foreach (var card in _cards)
                    {
                        card.Update(gameTime);
                    }
                }

                _previousMouseState = mouseState;
            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (_isGameOver)
            {
                _spriteBatch.DrawString(_font, "You Lose", _youLosePosition, Color.Red, 0, Vector2.Zero, 2.0f, SpriteEffects.None, 0);
                string countdownText = $"Returning to start screen in {Math.Ceiling(_gameOverTimer)} seconds";
                Vector2 countdownPosition = new Vector2(_youLosePosition.X, _youLosePosition.Y + 50); // Adjust position as needed
                _spriteBatch.DrawString(_font, countdownText, countdownPosition, Color.White);
            }
            else
            {
                if (!_isGameStarted)
                {
                    _spriteBatch.Draw(_playButtonTexture, _playButtonRectangle, Color.White);
                    _spriteBatch.Draw(_quitButtonTexture, _quitButtonRectangle, Color.White);
                }
                else
                {
                    foreach (var card in _cards)
                    {
                        card.Draw(_spriteBatch);
                        card.DrawValue(_spriteBatch, _font);
                    }

                    int screenWidth = _graphics.PreferredBackBufferWidth;
                    int screenHeight = _graphics.PreferredBackBufferHeight;

                    _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
                    _spriteBatch.DrawString(_font, $"Total Score: {_totalScore}", new Vector2(10, 40), Color.White);
                    _spriteBatch.DrawString(_font, $"Required Score: {_requiredScore}", new Vector2(10, 70), Color.White);
                    _spriteBatch.DrawString(_font, $"Discards Left: {MaxDiscards - _discardCount}", new Vector2(10, 130), Color.White);
                    _spriteBatch.DrawString(_font, $"Level: {_currentLevel}", new Vector2(screenWidth - 150, 10), Color.White);
                    _spriteBatch.DrawString(_font, $"Rounds Played: {_roundsPlayed}/4", new Vector2(10, screenHeight - 40), Color.White);
                    _spriteBatch.DrawString(_font, $"Total Score for All Levels: {_totalScoreForAllLevels}", new Vector2(screenWidth - 250, 40), Color.White);

                    if (_showWinMessage)
                    {
                        string winMessage = "You win! Get ready for the next round";
                        Vector2 winMessageSize = _font.MeasureString(winMessage);
                        _spriteBatch.DrawString(_font, winMessage, new Vector2((screenWidth - winMessageSize.X) / 2, 10), Color.Yellow, 0, Vector2.Zero, 1.1f, SpriteEffects.None, 0);
                    }

                    if (!_isGameOver)
                    {
                        _spriteBatch.Draw(_playHandButtonTexture, _playHandButtonRectangle, Color.White);
                        _spriteBatch.Draw(_discardButtonTexture, _discardButtonRectangle, Color.White);
                    }
                    else
                    {
                        string resultMessage = _isWin ? "You Win!" : "You Lose!";
                        _spriteBatch.DrawString(_font, resultMessage, new Vector2(10, 190), Color.White);
                    }
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void StartGame()
        {
            _isGameStarted = true;
            _currentLevel = 1;
            _totalScore = 0;
            _totalScoreForAllLevels = 0;
            _roundsPlayed = 0;
            _discardCount = 0;
            UpdateScoreRequirement();
            _cards = GenerateRandomCards();
            _score = CardUtils.CalculateScore(_cards);
        }

        private void UpdateScoreRequirement()
        {
            _requiredScore = 300 + (_currentLevel - 1) * 450; // Increase required score by 450 for each level
        }

        private List<Card> GenerateRandomCards()
        {
            List<Card> generatedCards = new List<Card>();
            List<string> deck = CardUtils.CreateDeck();
            List<string> playerHand = CardUtils.DealHand(deck);

            for (int i = 0; i < playerHand.Count; i++)
            {
                string card = playerHand[i];
                var newCard = new Card(new Vector2(100 + i * 110, 200), card, _cardTextures[card]);
                generatedCards.Add(newCard);
            }

            return generatedCards;
        }

        private void PlayHand()
        {
            int selectedCardCount = _cards.Count(card => card.IsSelected);
            if (selectedCardCount > 0)
            {
                _totalScore += _score;
                _totalScoreForAllLevels += _score;
                _roundsPlayed++;

                if (_totalScore >= _requiredScore)
                {
                    if (_currentLevel < MaxLevels)
                    {
                        _showWinMessage = true;
                        _winMessageTimer = WinMessageDuration;
                    }
                    else
                    {
                        _isWin = true;
                        _isGameOver = true;
                    }
                }
                else if (_roundsPlayed >= 4)
                {
                    _isWin = false;
                    _isGameOver = true;
                }
                else
                {
                    List<Card> newCards = GenerateRandomCards();
                    for (int i = 0; i < _cards.Count; i++)
                    {
                        if (_cards[i].IsSelected)
                        {
                            _cards[i] = newCards[i];
                        }
                    }

                    _score = CardUtils.CalculateScore(_cards);
                    _discardCount = 0; // Reset discard count when a new hand is played
                }
            }
        }

        private void StartNextRound()
        {
            _showWinMessage = false;
            _currentLevel++;
            _totalScore = 0; // Reset total score when a new level starts
            _roundsPlayed = 0;
            _discardCount = 0;
            UpdateScoreRequirement();
            _cards = GenerateRandomCards();
            _score = CardUtils.CalculateScore(_cards);
        }

        private void DiscardCards()
        {
            int selectedCardCount = _cards.Count(card => card.IsSelected);
            if (selectedCardCount > 0 && selectedCardCount <= 4)
            {
                _discardCount++;
                List<Card> newCards = GenerateRandomCards();
                for (int i = 0; i < _cards.Count; i++)
                {
                    if (_cards[i].IsSelected)
                    {
                        _cards[i] = newCards[i];
                    }
                }

                _score = CardUtils.CalculateScore(_cards);
            }
        }

        private void CheckGameOver()
        {
            if (_score < 0)
            {
                _isGameOver = true;
            }
        }
        private void GameOver()
        {
            _isGameOver = true;
            _gameOverTimer = 10.0; // 10 seconds
        }
    }
}