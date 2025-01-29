using UnityEngine;

namespace actions
{
    public class DebugAction : Action
    {
        private readonly int _id;
        private readonly ActionEnemy handler;
        private Color col;
        private ActionState state;
        private readonly Renderer rend;
        private readonly Priority _priority;

        public DebugAction(ActionEnemy _handler, Priority prio, Color _col)
        {
            _id = Hive.GetId();
            handler = _handler;
            col = _col;
            _priority = prio;

            rend = handler.GetComponent<Renderer>();
        }

        public ActionType GetActionType()
        {
            return ActionType.Idle;
        }

        public Priority GetPriority()
        {
            return _priority;
        }

        public ActionState GetState()
        {
            return state;
        }

        public void SetState(ActionState newState)
        {
            state = newState;
        }

        public void Tick() { }

        public int GetId()
        {
            return _id;
        }

        public void Finish()
        {
            if (state == ActionState.Running)
            {
                handler.actions.Finish(_id);
            }
        }

        public void Run()
        {
            rend.material.color = col;
            Finish();
        }
    }
}
