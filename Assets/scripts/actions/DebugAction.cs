using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace actions
{
    public class DebugAction : Action
    {
        private int _id;
        private ActionHandler handler;
        private Color col;
        private ActionState state;
        private Renderer rend;
        private Priority _priority;
        public DebugAction(int id, ActionHandler _handler,Priority prio, Color _col )
        {

            _id = id;
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

        public void Tick()
        {
            rend.material.color = col;
            
            handler.Finish(_id);
        }

        public int GetId()
        {
            return _id;
        }
    }

    




}