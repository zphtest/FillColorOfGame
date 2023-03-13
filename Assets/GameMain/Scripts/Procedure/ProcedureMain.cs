//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureMain : ProcedureBase
    {
        private const float GameOverDelayedSeconds = 2f;

        private readonly Dictionary<GameMode, GameBase> m_Games = new Dictionary<GameMode, GameBase>();
        private GameBase m_CurrentGame = null;
        private bool m_IsDrawing = false;
        private bool m_GotoMenu = false;
        private float m_GotoMenuDelaySeconds = 0f;
        private GameMode m_GameMode = GameMode.None;

        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        public void GotoMenu()
        {
            m_GotoMenu = true;
        }

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            procedureOwner.SetData<VarByte>("GameMode", (byte)GameMode.None);
            //m_Games.Add(GameMode.Survival, new SurvivalGame());
        }

        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);

            m_Games.Clear();
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_GotoMenu = false;
            m_GameMode = (GameMode)procedureOwner.GetData<VarByte>("GameMode").Value;
            if (m_GameMode == GameMode.DrawingState)
            {
                //GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
            }
            //m_CurrentGame = m_Games[gameMode];
            //m_CurrentGame.Initialize();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Shutdown();
                m_CurrentGame = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_GameMode == GameMode.DrawingState)
            {
                return;
            }

            m_GameMode = (GameMode)procedureOwner.GetData<VarByte>("GameMode").Value;

            if (!m_GotoMenu)
            {
                m_GotoMenu = true;
                m_GotoMenuDelaySeconds = 0;
            }

            m_GotoMenuDelaySeconds += elapseSeconds;
            if (m_GotoMenuDelaySeconds >= GameOverDelayedSeconds)
            {
                procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Menu"));
                procedureOwner.SetData<VarByte>("GameMode", (byte)GameMode.GameMenu);
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }
    }
}
