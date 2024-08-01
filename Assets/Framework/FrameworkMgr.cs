using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface ISingleton
    {
        public void Init();
    }

    public abstract class Singleton<T> where T :  ISingleton, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new T();
                _instance.Init();
                return _instance;
            }
        }
    }

    //框架管理器
    public abstract class FrameworkMgr<T> : Singleton<T>, IMgr, ISingleton
        where T : ISingleton, new()
    {
        private Dictionary<Type, IModule> _moduleDic;
        private Dictionary<Type, Delegate> _delegateDic;

        void ISingleton.Init()
        {
            _moduleDic = new Dictionary<Type, IModule>();
            _delegateDic = new Dictionary<Type, Delegate>();
            OnInit();
        }

        protected abstract void OnInit();

        void IMgr.AddEvent<V>(Action<V> addEvent)
        {
            Type type = typeof(V);
            if (_delegateDic.TryGetValue(type, out var value))
                _delegateDic[type] = Delegate.Combine(value, addEvent);
            else _delegateDic.Add(type, addEvent);
        }

        void IMgr.RemoveEvent<V>(Action<V> remEvent)
        {
            Type type = typeof(V);
            if (_delegateDic.TryGetValue(type, out var value))
                _delegateDic[type] = Delegate.Remove(value, remEvent);
        }

        void IMgr.SendEvent<V>(V v)
        {
            if (_delegateDic.TryGetValue(typeof(V), out var value))
                (value as Action<V>)?.Invoke(v);
        }

        protected void RegisterSystem<S>(S system) where S : class, ISystem
        {
            if (_moduleDic.ContainsKey(typeof(S))) return;
            _moduleDic[typeof(S)] = system;
            system.Init(this);
        }

        protected void RegisterModel<M>(M model) where M : class, IModel
        {
            if (_moduleDic.ContainsKey(typeof(M))) return;
            _moduleDic[typeof(M)] = model;
            model.Init(this);
        }

        protected void RegisterUtility<U>(U utility) where U : class, IUtility
        {
            if (_moduleDic.ContainsKey(typeof(U))) return;
            _moduleDic[typeof(U)] = utility;
        }

        S IMgr.GetSystem<S>() => _moduleDic.TryGetValue(typeof(S), out IModule system)
            ? system as S
            : throw new Exception("找不到" + typeof(S) + "该模块");

        M IMgr.GetModel<M>() => _moduleDic.TryGetValue(typeof(M), out IModule module)
            ? module as M
            : throw new Exception("找不到" + typeof(M) + "该模块");

        U IMgr.GetUtility<U>() => _moduleDic.TryGetValue(typeof(U), out IModule system)
            ? system as U
            : throw new Exception("找不到" + typeof(U) + "该模块");

        void IMgr.SendCmd<C>()
        {
            C c = new C();
            c.Init(this);
            c.Do();
        }

        void IMgr.UnDoCmd<C>()
        {
            C c = new C();
            c.Init(this);
            c.UnDo();
        }

        void IMgr.SendCmd<C>(C c)
        {
            c.Init(this);
            c.Do();
        }

        void IMgr.SendCmd<C, V>(V v)
        {
            C c = new C();
            c.Init(this);
            c.Do(v);
        }
        
        void IMgr.UnDoCmd<C, V>(V v)
        {
            C c = new C();
            c.Init(this);
            c.UnDo(v);
        }
        
        R IMgr.SendQuery<Q, R>()
        {
            Q q = new Q();
            q.Init(this);
            return q.Do();
        }

        R IMgr.SendQuery<Q, R, V>(V v)
        {
            Q q = new Q();
            q.Init(this);
            return q.Do(v);
        }

        R IMgr.SendQuery<Q, R, V, K>(V v, K k)
        {
            Q q = new Q();
            q.Init(this);
            return q.Do(v, k);
        }


        public void DebugModuleKey()
        {
            foreach (Type moduleDicKey in _moduleDic.Keys)
            {
                Debug.LogWarning(moduleDicKey);
            }
        }
    }

    public abstract class AbsSendQuery<R> : IQuery<R>,
        ICanGetSystem, ICanGetModel, ICanGetUtility, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
        }

        public virtual R Do() => default;

        public virtual void ReDo()
        {
        }
    }


    public abstract class AbsSendQuery<R, V> : IQuery<R, V>,
        ICanGetSystem, ICanGetModel, ICanGetUtility, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
        }

        public abstract R Do(V v);
        public abstract void ReDo();
    }

    public abstract class AbsSendQuery<R, V, Q> : IQuery<R, V, Q>,
        ICanGetSystem, ICanGetModel, ICanGetUtility, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
        }

        public abstract R Do(V v, Q q);
        public abstract void ReDo();
    }

    public abstract class AbsCommand : ICommand,
        ICanGetModel, ICanGetSystem, ICanGetUtility, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
        }

        public virtual void Do()
        {
        }

        public virtual void UnDo()
        {
        }

        public virtual void ReDo()
        {
        }
    }

    public abstract class AbsCommand<V> : ICommand<V>,
        ICanGetModel, ICanGetSystem, ICanGetUtility, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
        }

        public virtual void Do(V v)
        {
        }

        public virtual void UnDo(V v)
        {
        }

        public virtual void ReDo()
        {
        }
    }

    public abstract class AbsSystem : ISystem, ICanGetSystem, ICanGetUtility, ICanGetModel, ICanAddEvent,
        ICanRemoveEvent, ICanSendEvent, ICanSendCmd, ICanRemCmd, ICanSendQuery
    {
        public IMgr Ins => _ins;
        private IMgr _ins;

        void INeedInit.Init(IMgr iMgr)
        {
            _ins = iMgr;
            OnInit();
        }

        protected abstract void OnInit();
    }

    public abstract class AbsModel : IModel
    {
        public void Init(IMgr iMgr) => OnInit();
        protected abstract void OnInit();
    }

    public static class ExpendFrameworkMethods
    {
        public static void AddEvent<V>(this ICanAddEvent iCanAddEvent, Action<V> addEvent) =>
            iCanAddEvent.Ins.AddEvent(addEvent);

        public static void RemoveEvent<V>(this ICanRemoveEvent iCanRemoveEvent, Action<V> remEvent) =>
            iCanRemoveEvent.Ins.RemoveEvent(remEvent);

        public static void SendEvent<V>(this ICanSendEvent iCanSendEvent, V v) => iCanSendEvent.Ins.SendEvent(v);

        public static void SendCmd<C>(this ICanSendCmd iCanSendCmd) where C : ICommand, new() =>
            iCanSendCmd.Ins.SendCmd<C>();

        public static void UndoCmd<C>(this ICanSendCmd iCanSendCmd) where C : ICommand, new() =>
            iCanSendCmd.Ins.UnDoCmd<C>();

        public static void SendCmd<C>(this ICanSendCmd iCanSendCmd, C c) where C : ICommand =>
            iCanSendCmd.Ins.SendCmd(c);

        public static void SendCmd<C, V>(this ICanSendCmd iCanSendCmd, V v) where C : ICommand<V>, new() =>
            iCanSendCmd.Ins.SendCmd<C, V>(v);
        
        public static void UnDoCmd<C, V>(this ICanSendCmd iCanSendCmd, V v) where C : ICommand<V>, new() =>
            iCanSendCmd.Ins.UnDoCmd<C, V>(v);

        public static R SendQuery<Q, R>(this ICanSendQuery iCanSendQuery) where Q : IQuery<R>, new() =>
            iCanSendQuery.Ins.SendQuery<Q, R>();

        public static R SendQuery<Q, R, V>(this ICanSendQuery iCanSendQuery, V v) where Q : IQuery<R, V>, new() =>
            iCanSendQuery.Ins.SendQuery<Q, R, V>(v);

        public static R SendQuery<Q, R, V, K>(this ICanSendQuery iCanSendQuery, V v, K k)
            where Q : IQuery<R, V, K>, new() =>
            iCanSendQuery.Ins.SendQuery<Q, R, V, K>(v, k);

        public static S GetSystem<S>(this ICanGetSystem iCanGetSystem) where S : class, ISystem =>
            iCanGetSystem.Ins.GetSystem<S>();

        public static M GetModel<M>(this ICanGetModel iCanGetModel) where M : class, IModel =>
            iCanGetModel.Ins.GetModel<M>();

        public static U GetUtility<U>(this ICanGetUtility iCanGetUtility) where U : class, IUtility =>
            iCanGetUtility.Ins.GetUtility<U>();
    }


    public interface IModule
    {
    }

    public interface IModel : IModule, INeedInit
    {
    }

    public interface ISystem : IModule, INeedInit
    {
    }

    public interface IUtility : IModule
    {
    }

    public interface ICommand : INeedInit
    {
        void Do();
        void UnDo();
        void ReDo();
    }

    public interface ICommand<V> : INeedInit
    {
        void Do(V v);
        void UnDo(V v);
        void ReDo();
    }

    public interface IQuery<R> : INeedInit
    {
        R Do();
        void ReDo();
    }

    public interface IQuery<R, V> : INeedInit
    {
        R Do(V value);
        void ReDo();
    }

    public interface IQuery<R, V, Q> : INeedInit
    {
        R Do(V value, Q value1);
        void ReDo();
    }

    public interface ICanGetModel : ICanGetMgr
    {
    }

    public interface ICanGetSystem : ICanGetMgr
    {
    }

    public interface ICanGetUtility : ICanGetMgr
    {
    }


    public interface ICanSendEvent : ICanGetMgr
    {
    }

    public interface ICanAddEvent : ICanGetMgr
    {
    }

    public interface ICanRemoveEvent : ICanGetMgr
    {
    }

    public interface ICanSendCmd : ICanGetMgr
    {
    }

    public interface ICanRemCmd : ICanGetMgr
    {
    }

    public interface ICanSendQuery : ICanGetMgr
    {
    }

    public interface ICanGetMgr
    {
        public IMgr Ins { get; }
    }

    public interface IController : ICanGetSystem, ICanGetUtility, ICanGetModel, ICanSendCmd, ICanRemCmd, ICanSendQuery,ICanAddEvent,ICanRemoveEvent,ICanSendEvent
    {
    }

    public interface INeedInit
    {
        public void Init(IMgr iMgr);
    }

    public interface INeedDeInit
    {
        public void DeInit();
    }

    public interface IMgr
    {
        public void DebugModuleKey();
        public void AddEvent<V>(Action<V> addEvent);
        public void RemoveEvent<V>(Action<V> remEvent);
        public void SendEvent<V>(V v);
        public S GetSystem<S>() where S : class, ISystem;
        public M GetModel<M>() where M : class, IModel;
        public U GetUtility<U>() where U : class, IUtility;
        public void SendCmd<C>() where C : ICommand, new();
        public void UnDoCmd<C>() where C : ICommand, new();
        public void SendCmd<C>(C c) where C : ICommand;
        public void SendCmd<C, V>(V v) where C : ICommand<V>, new();
        public void UnDoCmd<C, V>(V v) where C : ICommand<V>, new();
        public R SendQuery<Q, R>() where Q : IQuery<R>, new();
        public R SendQuery<Q, R, V>(V v) where Q : IQuery<R, V>, new();
        public R SendQuery<Q, R, V, K>(V v, K k) where Q : IQuery<R, V, K>, new();
    } 
}