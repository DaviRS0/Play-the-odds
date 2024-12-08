using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Balatro
{
    public class Card
    {
        public Vector2 Position { get; private set; }
        public bool IsSelected { get; private set; }
        public string TextureName { get; }
        public Texture2D Texture { get; }
        public string Name { get; }
        public int HighCardBonus { get; set; } // This property can remain for other purposes

        private Vector2 _targetPosition;
        private float _animationProgress;
        private const float AnimationSpeed = 0.3f; // Adjusted speed (3 times faster)

        public event Action OnSelectionChanged; // Event to notify selection change

        public Card(Vector2 position, string name, Texture2D texture, int highCardBonus = 5)
        {
            Position = position;
            _targetPosition = position;
            Name = name;
            TextureName = name;
            Texture = texture;
            IsSelected = false;
            _animationProgress = 1.0f; // Start fully animated
            HighCardBonus = highCardBonus; // Initialize the high card bonus
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public void SetTargetPosition(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            _animationProgress = 0.0f; // Reset animation progress
        }

        public void ToggleSelection()
        {
            IsSelected = !IsSelected;
            var targetPosition = Position;
            targetPosition.Y += IsSelected ? -10 : 10; // Move card up or down
            SetTargetPosition(targetPosition);
            OnSelectionChanged?.Invoke(); // Trigger the event
        }

        public void Update(GameTime gameTime)
        {
            if (_animationProgress < 1.0f)
            {
                _animationProgress += AnimationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_animationProgress > 1.0f)
                {
                    _animationProgress = 1.0f;
                }
                Position = Vector2.Lerp(Position, _targetPosition, _animationProgress);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }

        public void DrawValue(SpriteBatch spriteBatch, SpriteFont font)
        {
            var cardValue = CardUtils.CardValues[Name.Split('_')[1]];
            var valuePosition = new Vector2(Position.X + Texture.Width - 20, Position.Y);
            spriteBatch.DrawString(font, cardValue.ToString(), valuePosition, Color.Black);
        }
    }
}