namespace AlTouch {
    public enum SwipeResult {
        Non,
        Busy,
        Left,
        Right,
        Top,
        Bottom
    }

    internal class Swipe {
        //画面サイズ
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        //基準（開始）点
        private int _startX;
        private int _startY;

        //カウンター
        private int _right = -1;
        private int _left = -1;
        private int _top = -1;
        private int _bottom = -1;

        private const int Margin = 50;
        private const int Count = 10;


        public Swipe(int screenWidth, int screenHeight) {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public SwipeResult Start(int startX, int startY) {

            _startX = startX;
            _startY = startY;

            if (_screenWidth - Margin < startX) {
                _right = 0;
            }
            if (_startX < Margin) {
                _left = 0;
            }
            if (_screenHeight - Margin < _startY) {
                _bottom = 0;
            }
            if (_startY < Margin && _startY != 0) {
                _top = 0;
            }
            if (_startY == 0) {
                return SwipeResult.Busy;
            }
            return GetResult();
        }

        public void Stop() {
            _right = -1;
            _left = -1;
            _top = -1;
            _bottom = -1;
        }

        public SwipeResult GetResult() {
            if (_right != -1 || _left != -1 || _top != -1 || _bottom != -1) {
                return SwipeResult.Busy;
            }
            return SwipeResult.Non;
        }

        public SwipeResult Move(int x, int y) {

            //右に移動している場合
            if (_startX < x) {
                _right = -1;
                if (_left != -1) {
                    _left++;
                    if (_left > Count) {
                        _left = -1;
                        return SwipeResult.Left;
                    }
                }
            }
            //左に移動している場合
            if (x < _startX) {
                _left = -1;
                if (_right != -1) {
                    _right++;
                    if (_right > Count) {
                        _right = -1;
                        return SwipeResult.Right;
                    }
                }
            }
            //上に移動している場合
            if (y < _startY) {
                _top = -1;
                if (_bottom != -1) {
                    _bottom++;
                    if (_bottom > Count) {
                        _bottom = -1;
                        return SwipeResult.Bottom;
                    }
                }
            }

            //下に移動している場合
            if (_startY < y) {
                _bottom = -1;
                if (_top != -1) {
                    _top++;
                    if (_top > Count) {
                        _top = -1;
                        return SwipeResult.Top;
                    }
                }
            }
            return GetResult();
        }
    }
}
