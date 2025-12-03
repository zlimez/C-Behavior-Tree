using System;
using System.Collections.Generic;
using Utils;

namespace BehaviorTree
{
    public class BehaviorTree
    {
#if UNITY_EDITOR
        private int _idCounter;
#endif

        private readonly Node _root;
        public readonly Deque<Node> Scheduled = new();
        // NOTE: Directly subscription to blackboard events prohibited to prevent mid-tick changes and multiple reevaluates per composite,
        // a buffer is used to store events and then processed at the start of the next tick
        private readonly Dictionary<string, System.Action> _trackedVars = new();
        private readonly Dictionary<string, (IObservableStore, Action<object>)> _trackedVarsActions = new();

        private Queue<System.Action> _eventFrontBuffer = new(), _eventBackBuffer = new();
        private Queue<System.Action> _eventWriteBuffer;

        private readonly IObservableStore[] _observedStores; // By contract should possess or have reference to external vars (not written by this BT instance) that are tracked which can cause composite restarts
        private readonly Dictionary<string, object> _internalStore = new(); // Can contain null values

        public BehaviorTree(Node firstNode, IObservableStore[] observedStores = null)
        {
            _root = new Root(firstNode);
            _observedStores = observedStores;
            _eventWriteBuffer = _eventBackBuffer;
            Setup();
        }

        private void Setup()
        {
            Queue<Node> q = new();
            q.Enqueue(_root);
            while (q.Count > 0)
            {
                var node = q.Dequeue();
#if UNITY_EDITOR
                node.Id = _idCounter++;
#endif
                node.Setup(this);
                var children = node.GetChildren();
                if (children == null) continue;
                foreach (var child in children)
                    q.Enqueue(child);
            }

            if (_observedStores == null && _trackedVars.Count > 0) throw new Exception("No store configured to be observed");

            foreach (var (varName, changeEvent) in _trackedVars)
            {
                var sourceFound = false;
                foreach (var observedStore in _observedStores)
                {
                    if (!observedStore.Has(varName)) continue;
                    sourceFound = true;
                    observedStore.Subscribe(varName, Listener);
                    _trackedVarsActions[varName] = (observedStore, Listener);
                    break;
                }

                if (!sourceFound) throw new Exception($"No store found for {varName}, problem with setup");
                continue;

                void Listener(object datum)
                {
                    _internalStore[varName] = datum;
                    _eventWriteBuffer.Enqueue(changeEvent);
                }
            }
        }

        #region APIs
        public void Teardown()
        {
            foreach (var (varName, (observedStore, action)) in _trackedVarsActions)
                observedStore.Unsubscribe(varName, action);

            Queue<Node> q = new();
            q.Enqueue(_root);
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                node.Teardown();
                var children = node.GetChildren();
                if (children == null) continue;
                foreach (var child in children)
                    q.Enqueue(child);
            }
        }

        public void AddTracker(string observedVar, System.Action func)
        {
            if (!_trackedVars.TryAdd(observedVar, func))
                _trackedVars[observedVar] += func;
        }

        public void RemoveTracker(string observedVar, System.Action func)
        {
            if (!_trackedVars.ContainsKey(observedVar)) throw new Exception($"No tracker for {observedVar}, problem with setup");
            _trackedVars[observedVar] -= func;
        }

        public State Tick(float deltaTime)
        {
            foreach (var e in _eventFrontBuffer) e.Invoke();
            _eventFrontBuffer.Clear();
            (_eventFrontBuffer, _eventBackBuffer) = (_eventBackBuffer, _eventFrontBuffer);
            _eventWriteBuffer = _eventBackBuffer;

            if (Scheduled.Count == 0) Scheduled.AddFirst(_root);
            Scheduled.AddLast(null); // End of turn marker, the last element should be root
            while (Step(deltaTime)) ;
            Scheduled.RemoveFirst(); // Remove null marker

            var execState = _root.State;
            if (execState != State.Running) _root.Done();
            return execState;
        }

        // TODO: For observed var change that causes reevaluation and potentially aborts starting from parent will be more efficient
        // from children many alt routes in children subtree may be taken only for parent to restart and abort the alt path
        private bool Step(float deltaTime)
        {
            var node = Scheduled.PeekFirst();
            if (node == null) return false;

            var ogState = node.State;
            var state = node.Tick(deltaTime);
            // Non-leaf control nodes with children first traversed, child node pushed to Scheduled, do not pop this node until children evaluated (Action node always popped)
            // Active controls nodes that require condition recheck every turn do not pop until recheck is done (Reevaluated toggles back to false)
            if (node is ActiveSelector { Restarted: true } or ActiveSequence { Restarted: true }
                or ObserveSelector { WillRestart: true } or ObserveSequence { WillRestart: true }) return true;
            if (ogState == State.Inactive) return true;

            Scheduled.RemoveFirst();
            // INVARIANT: End of each turn, only running / aborted nodes by parent are in Scheduled + at most one action node in Running state
            if (state == State.Running) Scheduled.AddLast(node);
            else if (node.Completed) node.Parent?.OnChildComplete(node, state);

            return true;
        }
        #endregion

        #region Data Getters and Setters
        public object[] GetData(string[] varNames, bool nullable = false)
        {
            var data = new object[varNames.Length];
            for (var i = 0; i < varNames.Length; ++i)
            {
                var varName = varNames[i];
                if (!_internalStore.TryGetValue(varName, out var value))
                {
                    if (nullable) data[i] = null;
                    else throw new Exception($"Var {varName} not found in board");
                }
                else data[i] = value;
            }
            return data;
        }

        public T GetDatum<T>(string name, bool nullable = false)
        {
            if (_internalStore.TryGetValue(name, out var value)) return (T)value;
            if (nullable) return default;
            throw new Exception($"Var {name} not found in board");
        }

        public void SetDatum<T>(string name, T val) => _internalStore[name] = val;
        public void ClearDatum(string name) => _internalStore[name] = null;
        #endregion
    }
}
