using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace Chaotx.Colorz {
    public delegate void GameOverHandler (Session obj, GameOverEventArgs args);
    public delegate void NewIndexHandler (Session obj, NewIndexEventArgs args);
    public class GameOverEventArgs : EventArgs {
        public int Turns {get;}
        public GameOverEventArgs(int turns) : base() {
            Turns = turns;
        }
    }

    public class NewIndexEventArgs : EventArgs {
        public int Index {get;}
        public NewIndexEventArgs(int index) : base() {
            Index = index;
        }
    }

    public class Session {
        public Random Rng {get; private set;}
        public int Width {get; private set;}
        public int Height {get; private set;}
        public bool IsTalking {get; private set;}
        public bool IsListening {get; private set;}
        public bool IsRunning {get; private set;}
        public bool Suspended {get; private set;}
        public int IdleTime {get; set;}

        public event GameOverHandler GameOver;
        public event NewIndexHandler NewIndex;

        private bool newIndex;
        private bool wasTalking, wasListening;
        private List<int> nextTiles;
        private int nextTileCount, index;
        private int timer;

        public Session(int width = 2, int height = 2, int activeTime = 1000) {
            Width = width;
            Height = height;
            Rng = new Random();
            nextTiles = new List<int>();
            nextTileCount = 0;
            IdleTime = activeTime;
        }

        public void EvaluateNextTiles() {
            ++nextTileCount;

            for(int i = nextTiles.Count; i < nextTileCount; ++i)
                nextTiles.Add(Rng.Next(Width*Height));

            index = timer = 0;
            IsTalking = newIndex = true;
            if(!IsRunning) IsRunning = true;
        }

        public int Say(GameTime gameTime) {
            if(!IsTalking) return -1;

            if(timer > IdleTime) {
                newIndex = true;
                timer = 0;
                ++index;
            }

            if(index >= nextTileCount) {
                IsTalking = false;
                IsListening = true;
                return index = 0;
            }

            if(newIndex) {
                OnNewIndex(nextTiles[index]);
                newIndex = false;
            }

            timer += gameTime.ElapsedGameTime.Milliseconds;
            return nextTiles[index];
        }

        public bool Listen(int tileIndex) {
            if(!IsListening || Suspended) return false;

            if(!tileIndex.Equals(nextTiles[index++])) {
                IsListening = false;
                OnGameOver(nextTileCount);
                return false;
            }

            if(index >= nextTileCount)
                return !(IsListening = false);

            return false;
        }

        public void Suspend() {
            Suspended = true;
            wasTalking = IsTalking;
            wasListening = IsListening;
        }

        public void Resume() {
            Suspended = false;

            if(!wasTalking || wasTalking && IsTalking)
                IsListening = wasListening;

            if(!IsTalking && !IsListening)
                EvaluateNextTiles();
        }

        public void Stop() {
            IsListening = IsTalking = false;
        }

        protected virtual void OnNewIndex(int index) {
            var handler = NewIndex;
            if(handler != null)
                handler(this, new NewIndexEventArgs(index));
        }

        protected virtual void OnGameOver(int turns) {
            var handler = GameOver;
            if(handler != null)
                handler(this, new GameOverEventArgs(turns));
        }
    }
}