//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace StarForce
{
    /// <summary>
    /// 游戏模式。
    /// </summary>
    public enum GameMode : byte
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        None,
        /// <summary>
        /// 游戏Logo
        /// </summary>
        GameLogo,
        /// <summary>
        /// 游戏菜单
        /// </summary>
        GameMenu,
        /// <summary>
        /// 开始画画。
        /// </summary>
        DrawingState,
    }
}
